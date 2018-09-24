
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryObject
{
    //this class stores variables of MonoBehaviours that have memories to be saved

    public bool found = false;//whether this memory has been obtained by Merky yet
    public string objectName;
    public string sceneName;

    public MemoryObject() { }

    public MemoryObject(MemoryMonoBehaviour mmb, bool foundYet)
    {
        objectName = mmb.gameObject.name;
        sceneName = mmb.gameObject.scene.name;
        this.found = foundYet;
    }

    public bool isFor(MemoryMonoBehaviour mmb)
    {
        bool nameMatch = objectName == mmb.gameObject.name;
        bool sceneMatch = sceneName == mmb.gameObject.scene.name;
        return nameMatch && sceneMatch;
    }

    /// <summary>
    /// Finds the GameObject that this MemoryObject is for
    /// </summary>
    /// <returns></returns>
    public GameObject findGameObject()
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            foreach (GameObject sceneGo in scene.GetRootGameObjects())
            {
                if (sceneGo.name.Equals(objectName))
                {
                    return sceneGo;
                }
                foreach (Transform childTransform in sceneGo.GetComponentsInChildren<Transform>())
                {
                    GameObject childGo = childTransform.gameObject;
                    if (childGo.name.Equals(objectName))
                    {
                        return childGo;
                    }
                }
            }
        }
        return null;
    }
}
