﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainData
{
    public Vector2[] vectorPath;
    public GameObject parent;

    public TerrainData(Vector2[] vectorPath, GameObject parent)
    {
        this.vectorPath = vectorPath;
        this.parent = parent;
    }

    public static implicit operator List<Vector2>(TerrainData td)
        => new List<Vector2>(td.vectorPath);
}
