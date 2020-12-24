using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ObjectState
{
    //Transform
    public Vector3 position;//2017-10-10: actually stores the localPosition
    public Vector3 localScale;
    public Quaternion rotation;//2017-10-10: actually stores the localRotation
    //RigidBody2D
    public Vector2 velocity;
    public float angularVelocity;
    //Saveable Object
    public List<SavableObject> soList = new List<SavableObject>();
    //Name
    public string objectName;
    public string sceneName;
    public string prefabName;

    public ObjectState() { }
    public ObjectState(GameObject go)
    {
        objectName = go.name;
        sceneName = go.scene.name;
        prefabName = go.GetComponent<ObjectInfo>().PrefabName;
        saveState(go);
    }

    private void saveState(GameObject go)
    {
        position = go.transform.localPosition;
        localScale = go.transform.localScale;
        rotation = go.transform.localRotation;
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            velocity = rb2d.velocity;
            angularVelocity = rb2d.angularVelocity;
        }
        soList = new List<SavableObject>();
        foreach (SavableMonoBehaviour smb in go.GetComponents<SavableMonoBehaviour>())
        {
            this.soList.Add(smb.CurrentState);
        }
    }
    public void loadState()
    {
        GameObject go = getGameObject();//finds and sets the game object
        if (go == null)
        {
            return;//don't load the state if go is null
        }
        go.transform.localPosition = position;
        go.transform.localScale = localScale;
        go.transform.localRotation = rotation;
        Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.velocity = velocity;
            rb2d.angularVelocity = angularVelocity;
        }
        foreach (SavableObject so in this.soList)
        {
            SavableMonoBehaviour smb =
                (SavableMonoBehaviour)go.GetComponent(so.ScriptType);
            if (smb == null)
            {
                if (so.isSpawnedScript)
                {
                    smb = (SavableMonoBehaviour)so.addScript(go);
                }
                else
                {
                    throw new UnityException("Object " + go + " is missing non-spawnable script " + so.scriptType);
                }
            }
            smb.CurrentState = so;
        }
    }

    //
    //Retrieves the variable, go,
    //and if go is null, it finds the correct GameObject and sets go
    //
    public GameObject getGameObject()
    {
        GameObject go = null;
        if (go == null || ReferenceEquals(go, null))//2016-11-20: reference equals test copied from an answer by sindrijo: http://answers.unity3d.com/questions/13840/how-to-detect-if-a-gameobject-has-been-destroyed.html
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                //First Pass: get GO from ObjectManager list
                go = Managers.Object.getObject(objectName);
                //Second Pass: try spawning it
                if (go == null || ReferenceEquals(go, null))
                {
                    foreach (SavableObject so in soList)
                    {
                        //Make it
                        GameObject spawned = so.spawnObject(objectName, prefabName);
                        if (spawned.scene.name != sceneName)
                        {
                            SceneManager.MoveGameObjectToScene(spawned, scene);
                        }
                        foreach (Transform t in spawned.transform)
                        {
                            if (t.gameObject.isSavable())
                            {
                                Managers.Object.addObject(t.gameObject);
                                if (t.gameObject.name == this.objectName)
                                {
                                    go = t.gameObject;
                                }
                            }
                        }
                        if (go == null)
                        {
                            go = spawned;
                            go.name = this.objectName;
                        }
                        Managers.Object.addObject(spawned);
                        break;
                    }
                }
                //Third Pass: get GO by searching all the scene objects
                if (go == null)
                {
                    foreach (GameObject sceneGo in scene.GetRootGameObjects())
                    {
                        if (sceneGo.name == objectName)
                        {
                            go = sceneGo;
                            break;
                        }
                        else
                        {
                            foreach (Transform childTransform in sceneGo.GetComponentsInChildren<Transform>())
                            {
                                GameObject childGo = childTransform.gameObject;
                                if (childGo.name == objectName)
                                {
                                    go = childGo;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            //Else if the scene is not loaded,
            else
            {
                //Don't find the object
                go = null;
            }
        }
        return go;
    }
}
