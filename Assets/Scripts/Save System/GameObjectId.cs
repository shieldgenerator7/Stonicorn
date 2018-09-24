using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectId {
    //2017-02-07: the purpose of this class is to uniquely identify a GameObject

    public string name;//the name of the GameObject
    public string sceneName;//the name of the scene that the GameObject is in

    GameObjectId(string name, string sceneName)
    {
        this.name = name;
        this.sceneName = sceneName;
    }
    public GameObjectId() {		
	}

    public bool matches(GameObject go)
    {
        return this.name == go.name && this.sceneName == go.scene.name;
    }

    public bool equals(GameObjectId goId)
    {
        return this.name == goId.name && this.sceneName == goId.sceneName;
    }

    public static GameObjectId createId(GameObject go)
    {
        return new GameObjectId(go.name, go.scene.name);
    }
}
