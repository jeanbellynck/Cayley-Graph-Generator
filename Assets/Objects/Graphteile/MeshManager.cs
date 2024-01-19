using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager {

    public ICollection<MeshGenerator> meshList = new List<MeshGenerator>();

    public void AddMesh(MeshGenerator meshGen)
    {
        meshList.Add(meshGen);
    }

    // Start is called before the first frame update


    public ICollection<MeshGenerator> GetMeshes()
    {
        return meshList;
    }

    public void resetMeshes()
    {
        meshList.Clear();
    }
}
