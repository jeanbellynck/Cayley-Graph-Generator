using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager {
    readonly List<MeshGenerator> meshList = new();
    readonly GameObject meshPrefab;

    public MeshManager(GameObject meshPrefab) {
        this.meshPrefab = meshPrefab;
    }

    public void AddMesh(IEnumerable<Vertex> vertices, Transform parent)
    {
        MeshGenerator meshGen = GameObject.Instantiate(meshPrefab, parent.position, Quaternion.identity, parent).GetComponent<MeshGenerator>();
        meshGen.Initialize(vertices);
        meshList.Add(meshGen);
    }

    public void resetMeshes()
    {
        foreach (MeshGenerator mesh in meshList) {
            GameObject.Destroy(mesh.gameObject);
        }
        meshList.Clear();
    }
}
