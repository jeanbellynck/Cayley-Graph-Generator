using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class GAPClient
{
    readonly string url = "http://localhost:63910/aosijfoaisdoifnCodnifaoGsinf"; 

    public GAPClient(string url = "") {
        if (!string.IsNullOrWhiteSpace(url))
            this.url = url;
    }

    async Task<string?> CallClient(string body)
    {
        using HttpClient client = new();
        try{
            HttpResponseMessage response = await client.PostAsync(url, new StringContent(body));

            if (!response.IsSuccessStatusCode){
                Debug.WriteLine($"Error {response.StatusCode}: {response.ReasonPhrase}");
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Debug.WriteLine("Error: " + e);
            return null;
        }
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

        if (resultArray.Length < 3) 
            return (false, generators, relators, new Dictionary<string, string>(from g in generators select new KeyValuePair<string, string>(g, g)));

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
