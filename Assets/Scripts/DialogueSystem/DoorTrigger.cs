using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Triggers going through a door to another area
/// </summary>
public class DoorTrigger : EventTrigger
{
    public class Door
    {
        public int id;
        /// <summary>
        /// The id of the door this door is connected to
        /// </summary>
        public int connectId;

        public Door(int id, int connectId)
        {
            this.id = id;
            this.connectId = connectId;
        }
    }

    public int connectedId = -1;
    public string connectScene;

    public Door door;


    protected override void Start()
    {
        base.Start();
        door = new Door(id, connectedId);
        
        if (connectedId < 0)
        {
            Debug.LogError(
                "DoorTrigger Connected Id is invalid on object " + gameObject.name
                + " in scene " + gameObject.scene.name + ". "
                + "Connected Id must be 0 or greater (Connect two DoorTriggers together). "
                + "Invalid value: " + connectedId
                , this);
        }
        if (this.connectScene == null || this.connectScene == "")
        {
            Debug.LogError(
                "DoorTrigger Connect Scene is invalid on object " + gameObject.name
                + " in scene " + gameObject.scene.name + ". "
                + "Connected Scene must be a valid scene name. "
                + "Invalid value: " + connectScene
                , this);
        }
        
    }

    protected override void triggerEvent()
    {
        FindObjectOfType<AreaManager>().jumpToDoor(connectScene, door);
    }

    public void connectTo(DoorTrigger door)
    {
        this.connectedId = door.id;
        this.connectScene = door.gameObject.scene.name;
    }
}
