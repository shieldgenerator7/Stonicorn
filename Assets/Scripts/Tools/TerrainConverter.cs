using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class TerrainConverter : MonoBehaviour
{

    [Tooltip("The list of terrain objects to convert")]
    public List<GameObject> inputTerrains;

    public enum TerrainType
    {
        FERR_2D,
        SPRITE_SHAPE
    }
    public TerrainType outputType = TerrainType.SPRITE_SHAPE;

    public GameObject spriteShapePrefab;
    public bool flipX = false;//true to flip the result horizontally
    public bool flipY = false;//true to flip the result vertically
    public bool reverseDirection = false;//reverse the list of points

    public void addAllTerrains()
    {
        //Clear the list
        inputTerrains.Clear();
        //Add Ferr2D terrains
        foreach (Ferr2DT_PathTerrain f2dtpt in GameObject.FindObjectsOfType<Ferr2DT_PathTerrain>())
        {
            inputTerrains.Add(f2dtpt.gameObject);
        }
        //Add SpriteShape terrains
        foreach (SpriteShapeController ssc in GameObject.FindObjectsOfType<SpriteShapeController>())
        {
            inputTerrains.Add(ssc.gameObject);
        }
    }

    public void convertTerrains()
    {
        convertTerrains(outputType);
    }

    public void convertTerrains(TerrainType terrainType)
    {
        if (outputType == TerrainType.FERR_2D)
        {
            Debug.LogError("Terrain Converter: Converting to Ferr2D is not yet supported.");
            return;
        }
        if (outputType == TerrainType.SPRITE_SHAPE)
        {
            List<TerrainData> terrainShapes = new List<TerrainData>();
            //Remove terrains that don't need to be converted
            removeTerrainsOfType(inputTerrains, typeof(SpriteShapeController));
            //Get vector paths from the terrains that need converted
            foreach (GameObject go in inputTerrains)
            {
                Ferr2DT_PathTerrain f2dtpt = go.GetComponent<Ferr2DT_PathTerrain>();
                if (f2dtpt != null)
                {
                    terrainShapes.Add(convertToVectorPath(f2dtpt));
                }
            }
            //Turn the vector paths into new terrains
            foreach (TerrainData td in terrainShapes)
            {
                GameObject go = convertToSpriteShapeTerrain(td);
                go.transform.parent = td.parent.transform.parent;
            }
        }
    }

    private List<GameObject> removeTerrainsOfType(List<GameObject> objects, System.Type typeToRemove)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            GameObject go = objects[i];
            if (go.GetComponent(typeToRemove.ToString()))
            {
                objects.RemoveAt(i);
            }
        }
        return objects;
    }

    private TerrainData convertToVectorPath(Ferr2DT_PathTerrain ferrTerrain)
    {
        return new TerrainData(
            ferrTerrain.PathData.GetPoints(1).ToArray(),
            ferrTerrain.gameObject
            );
    }


    private GameObject convertToSpriteShapeTerrain(TerrainData terrainData)
    {
        GameObject newObject = GameObject.Instantiate(spriteShapePrefab);
        newObject.name = terrainData.parent.name + " (SpriteShape)";
        Utility.copyTransform(terrainData.parent.transform, ref newObject);
        SpriteShapeController newSSC = newObject.GetComponent<SpriteShapeController>();
        //Remove existing points
        Spline spline = newSSC.spline;
        spline.Clear();
        if (reverseDirection)
        {
            Array.Reverse(terrainData.vectorPath);
        }
        for (int i = 0; i < terrainData.vectorPath.Length; i++)
        {
            Vector2 v = terrainData.vectorPath[i];
            //Flip if necessary
            v.x *= (flipX) ? -1 : 1;
            v.y *= (flipY) ? -1 : 1;
            //Insert point
            spline.InsertPointAt(i, v);
        }
        return newObject;
    }
}
#endif
