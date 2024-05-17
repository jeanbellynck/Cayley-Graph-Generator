using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;


public class GAPClient
{
    readonly string url = "http://49.13.201.239:63910/aosijfoaisdoifnCodnifaoGsinf";
    // readonly string apiKey = "wpqopgasdsefpwqe4"; 
    // Todo: add to header (not possible with UnityWebRequest, which is needed for cross-platform)
    // Todo: Store secretly. This seems to be extremely difficult, so we will just ignore the problem for now, but only allow requests coming from where we officially host this application. For your personal use, host the GAP server yourself and change the url.

    public GAPClient(string url = "") {
        if (!string.IsNullOrWhiteSpace(url))
            this.url = url;
    }

    async Task<string?> CallClient(string body)
    {
        using UnityWebRequest request = UnityWebRequest.Post(url, body, "text/plain");
        try {
            await request.SendWebRequest();
            var response = request.result;

            if (response == UnityWebRequest.Result.Success) 
                return request.downloadHandler.text;

            Debug.LogWarning($"Error when calling GAP-server at {url}: {request.responseCode} - {request.error}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error when calling GAP-server at {url}: {e}, {request.error}");
        }
        return null;
    }

    readonly Dictionary<(string[], string[]), (string[], string[], Dictionary<string, string>)> cache = new();
    public async Task<(bool, string[], string[], Dictionary<string, string>)> OptimizePresentation(string[] generators, string[] relators)
    {
        if (cache.TryGetValue((generators, relators), out var cachedValue))
            return (true, cachedValue.Item1, cachedValue.Item2, cachedValue.Item3);

        var relatorsInGAPFormat =  from relator in relators select ToGAPNotation(relator);
        string requestBody = $@"    F := FreeGroup({string.Join(',', from gen in generators select $"\"{gen}\"")});;
    AssignGeneratorVariables(F);;
    G := F/[{string.Join(", ", relatorsInGAPFormat)}];;
    iso := IsomorphismSimplifiedFpGroup(G);
    H := Range(iso);;
    gens := GeneratorsOfGroup(H);
    rels := RelatorsOfFpGroup(H);"
            .Replace("\n", "\"&\";");

        string? result = await CallClient(requestBody);
        if (result == null) 
            return (false, generators, relators, new Dictionary<string, string>(from g in generators select new KeyValuePair<string, string>(g, g)));

        string[] resultArray = (
            from line in result.Replace("\n", "").Replace("\"", "").Split("&")
            where !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#I")
            select line.Trim().TrimEnd(']').TrimStart('[').Trim()
        ).SkipWhile(line => !line.Contains("->")).ToArray();

        if (resultArray.Length < 3) {
            Debug.LogWarning($"The result of the server didn't have the expected format: {string.Join(", ", resultArray)}");
            return (false, generators, relators, new Dictionary<string, string>(from g in generators select new KeyValuePair<string, string>(g, g)));
        }

        string[] optimizedGenerators = resultArray[1].Split(", ");
        string[] optimizedRelators = (
            from relatorInGAPFormat in resultArray[2].Split(", ")
            select ToOurNotation(relatorInGAPFormat)
        ).ToArray();
        
        Dictionary<string, string> optimizedMap;
        var a = resultArray[0].Split("->");

        if (a.Length < 2) 
            optimizedMap = new(from g in optimizedGenerators select new KeyValuePair<string, string>(g, g));
        else {
            var generatorString = a[0].Trim().TrimEnd(']').TrimStart('[').Trim();
            var imagesOfGeneratorsString = a[1].Trim().TrimEnd(']').TrimStart('[').Trim();
            optimizedMap = new(
                generatorString.Split(",").Zip(
                    imagesOfGeneratorsString.Split(","),
                    (gen, img) => new KeyValuePair<string, string>(
                        gen.Trim(),
                        ToOurNotation(img.Trim())
                    )
                )
            );
        }
        cache.Add((generators, relators), (optimizedGenerators, optimizedRelators, optimizedMap));
        return (true, optimizedGenerators, optimizedRelators, optimizedMap);
    }

    static string ToGAPNotation(string relator)
    {
        return string.Join('*',
                from gen in relator
                select char.IsUpper(gen) ? char.ToLower(gen) + "^-1" : gen.ToString()
            );
    }

    static string ToOurNotation(string relatorInGAPFormat)
    {
        return string.Join("",
            from gen in relatorInGAPFormat.Split('*')
            select gen.Length > 2 &&  gen[2] == '-' ? char.ToUpper(gen[0]) + gen.Replace("-","").Replace("^1", "").Remove(0,1) : gen
        );
    }
}
