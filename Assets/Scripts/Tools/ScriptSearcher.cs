using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class ScriptSearcher : MonoBehaviour
{
    public string scriptName;

    public void findAllObjectsWithScript()
    {
        //2020-12-16: copied from http://www.zuluonezero.net/2020/02/04/unity-script-find-all-assets-and-prefabs-with-a-given-component-by-type/
        Debug.Log("Finding all Prefabs that have component: " + scriptName);
        List<string> directories = new List<string>()
        {
            "Assets/Enemies",
            "Assets/Prefabs",
            "Assets/Resources/Prefabs"
        };
        List<string> guids = new List<string>();
        directories.ForEach(
            dir => guids.AddRange(AssetDatabase.FindAssets("t:Object", new[] { dir }))
            );

        foreach (string guid in guids)
        {
            //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] myObjs = AssetDatabase.LoadAllAssetsAtPath(myObjectPath);

            //Debug.Log("printing myObs now...");
            foreach (Object thisObject in myObjs)
            {
                if (thisObject == null)
                {
                    Debug.LogError("Object is null! " + thisObject + " in " + myObjectPath);
                    continue;
                }
                //Debug.Log(thisObject.name);
                //Debug.Log(thisObject.GetType().Name); 
                string myType = thisObject.GetType().Name;
                if (myType == scriptName)
                {
                    Debug.Log(
                        scriptName + " Found in...  " + thisObject.name
                        + " at " + myObjectPath
                        );
                }
            }
        }

        // Find all object that have an Audio component in the current Scene
        Debug.Log("Finding all Assets in the Current Scene that have a " + scriptName);
        System.Type scriptType = typeof(GameManager).Assembly.GetType(scriptName);
        MonoBehaviour[] myScripts = FindObjectsByType(scriptType, FindObjectsSortMode.None) as MonoBehaviour[];
        Debug.Log("Found " + myScripts.Length + " objects with a " + scriptName + " attached");
        foreach (MonoBehaviour item in myScripts)
        {
            Debug.Log(item.gameObject.name, item.gameObject);
        }
    }
}
#endif
