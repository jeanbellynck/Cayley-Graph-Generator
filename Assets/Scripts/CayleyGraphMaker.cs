using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public abstract class CayleyGraphMaker : MonoBehaviour
{
    protected VertexManager knotenverwalter;
    protected EdgeManager kantenverwalter;
    protected MeshManager meshManager;
    protected GameObject knotenPrefab;
    protected GameObject kantenPrefab;
    protected Physik physik;
    protected int complexSize;
    protected Color[] colourList = new Color[]{new Color(255, 0, 0), new Color(0,0,255), new Color(0, 255, 0), new Color(255, 255,0 )};
    protected char[] generators;// = new char[]{'a', 'b', 'c'};
    protected string[] relators;// = new string[]{"abAB"};


    public void InitializeCGMaker(VertexManager knotenverwalter, EdgeManager kantenverwalter, MeshManager meshManager, GameObject knotenPrefab, GameObject kantenPrefab, Color[] colourList, char[] generators, string[] relators, int complexSize) {
        this.knotenverwalter = knotenverwalter;
        this.kantenverwalter = kantenverwalter;
        this.meshManager = meshManager;
        this.knotenPrefab = knotenPrefab;
        this.kantenPrefab = kantenPrefab;
        this.colourList = colourList;
        this.generators = generators;
        this.relators = relators;
        this.complexSize = complexSize;
        InitializeCGMaker();
    }

    public abstract void InitializeCGMaker();

    public void setPhysics(Physik physik)
    {
        this.physik = physik;
    }

    internal abstract void setVertexNumber(int v);
}
