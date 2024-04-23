using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshManager : MonoBehaviour {
    [SerializeField] GameObject meshPrefab;
    [SerializeField] Dictionary<string, Color> typeColors = new();
    [SerializeField] Color[] ColorList = Array.Empty<Color>();
    [SerializeField] Color defaultColor = Color.gray; // should actually never be used; just for safety

    readonly List<MeshGenerator> meshList = new();
    readonly Dictionary<int, Dictionary<string, HashSet<int>>> typesPresentAtVertex = new();

    public void UpdateTypes(IEnumerable<string> types) {
        typeColors = new(Enumerable.Zip(
            types,
            ColorList.Extend(
                () => {
                    var res = Random.ColorHSV(0, 1, 0.9f, 1);
                    res.a = defaultColor.a;
                    return res;
                }), 
            (type, color) => new KeyValuePair<string, Color>(type, color)
        ));
    }

    public bool AddMesh(IEnumerable<Vertex> vertices, Transform parent, string type = "")
    {
        if (vertices == null)
            return false;
        type ??= "";
        var vertexIds = vertices.Select(vertex => vertex.Id).ToList();
        if (vertexIds.Count < 3) // meshes with less than 3 vertices would be invisible
            return false;

        if (!string.IsNullOrWhiteSpace(type)) {
            var previousMeshIdsAtVertices = (
                from vertexId in vertexIds
                let isPresent = typesPresentAtVertex.ContainsKey(vertexId) && typesPresentAtVertex[vertexId].ContainsKey(type)
                select isPresent ? typesPresentAtVertex[vertexId][type] : new());

            if (!previousMeshIdsAtVertices.intersectAll().IsEmpty())
                return false;
            // If all vertices already have a (joint!) mesh of the same type, don't add a new one
            // I had to save the meshIDs and compare them because else it might leave a mesh empty if the vertices all already have some different relator meshes of this type (happened with one row of meshes in the torus example)
        }

        MeshGenerator meshGen = GameObject.Instantiate(meshPrefab, parent.position, Quaternion.identity, parent).GetComponent<MeshGenerator>();
        meshGen.Initialize(vertices, typeColors.GetValueOrDefault(type, defaultColor), this, type);
        meshGen.gameObject.name = type + " @ " + vertices.First().name;
        meshList.Add(meshGen);
        foreach (var vertexId in vertexIds)
        {
            typesPresentAtVertex.TryAdd(vertexId, new());
            typesPresentAtVertex[vertexId].TryAdd(type, new());
            typesPresentAtVertex[vertexId][type].Add(meshGen.GetInstanceID());
        }
        return true;
    }

    public void ResetMeshes()
    {
        foreach (MeshGenerator mesh in meshList) 
            GameObject.Destroy(mesh.gameObject);
        meshList.Clear();
        typesPresentAtVertex.Clear();
    }

    public void RemoveMesh(MeshGenerator mesh) {
        meshList.Remove(mesh);
        foreach (var vertex in mesh.vertexElements)
        {
            if (!typesPresentAtVertex.TryGetValue(vertex.Id, out var types)) continue; // should not happen
            if (!types.TryGetValue(mesh.type, out var meshIds)) continue; // should not happen
            meshIds.Remove(mesh.GetInstanceID());
        }
        
    }
}
