using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CheckPointChecker : MemoryMonoBehaviour
{

    public static GameObject current = null;//the current checkpoint
    public bool generateGhostImage = false;//set true to write the picture to a file

    public bool activated = false;
    public Sprite ghostSprite;
    private GameObject ghost;
    public CheckPointGhostMover cpGhostMover;
    public GameObject ghostPrefab;
    private GameObject player;
    private PlayerController plyrController;
    private static Camera checkpointCamera;

    // Use this for initialization
    void Start()
    {
        if (ghost == null)
        {
            initializeGhost();
        }
        player = GameManager.getPlayerObject();
        plyrController = player.GetComponent<PlayerController>();
        if (checkpointCamera == null)
        {
            GameObject cpBgCamera = GameObject.Find("CP BG Camera");
            if (cpBgCamera)
            {
                checkpointCamera = cpBgCamera.GetComponent<Camera>();
                checkpointCamera.gameObject.SetActive(false);
            }
        }
    }
    void initializeGhost()
    {
        ghost = (GameObject)Instantiate(ghostPrefab);
        ghost.SetActive(false);
        cpGhostMover = ghost.GetComponent<CheckPointGhostMover>();
        cpGhostMover.parentCPC = this;
        ghost.GetComponent<SpriteRenderer>().sprite = ghostSprite;
    }

    //When a player touches this checkpoint, activate it
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == GameManager.playerTag)
        {
            activate();
        }
    }

    /**
    * When the player is inside, show the other activated checkpoints
    */
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag(GameManager.playerTag))
        {
            trigger();
        }
    }
    public void activate()
    {
        activated = true;
        GameManager.saveMemory(this);
        GameManager.saveCheckPoint(this);
        //Start the particles
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        //Initialize ghost sprite (if necessary)
    }
    public void trigger()
    {
        if (current == this.gameObject)
        {
            //don't trigger it twice
            return;
        }
        current = this.gameObject;
        if (ghostSprite == null || generateGhostImage)
        {
            grabCheckPointCameraData();
            ghost.GetComponent<SpriteRenderer>().sprite = ghostSprite;
        }
        activate();
        ghost.SetActive(false);
        plyrController.setIsInCheckPoint(true);
        foreach (CheckPointChecker cpc in GameManager.getActiveCheckPoints())
        {
            if (cpc != this)
            {
                cpc.showRelativeTo(this.gameObject);
            }
        }
    }
    public static void readjustCheckPointGhosts(Vector2 epicenter)
    {
        foreach (CheckPointChecker cpc in GameManager.getActiveCheckPoints())
        {
            if (cpc.gameObject != current)
            {
                cpc.cpGhostMover.readjustPosition(epicenter);
            }
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (current == this.gameObject)
        {
            plyrController.setIsInCheckPoint(false);
            activate();
            clearPostTeleport(true);
            current = null;
        }
    }

    public string grabCheckPointCameraData()//2016-12-06: grabs image data from the camera designated for checkpoints
    {
        if (checkpointCamera == null)
        {
            checkpointCamera = GameObject.Find("CP BG Camera").GetComponent<Camera>();
            checkpointCamera.gameObject.SetActive(false);
        }
        checkpointCamera.gameObject.SetActive(true);
        checkpointCamera.gameObject.transform.position = gameObject.transform.position + new Vector3(0, 0, -10);
        //2016-12-06: The following code copied from an answer by jashan: http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
        int resWidth = 300;
        int resHeight = 300;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        checkpointCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        checkpointCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();
        checkpointCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        Color transparentColor = new Color(1, 1, 1, 0);
        for (int x = 0; x < screenShot.width; x++)
        {
            for (int y = 0; y < screenShot.height; y++)
            {
                float dx = Mathf.Abs(x - screenShot.width / 2);
                float dy = Mathf.Abs(y - screenShot.height / 2);
                //If the pixel is outside the circle inscribed in the rect,
                if (dx * dx + dy * dy > screenShot.width * screenShot.width / 4)
                {
                    //set the pixel to transparent
                    screenShot.SetPixel(x, y, transparentColor);
                }
            }
        }
        Sprite createdSprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f));
        ghostSprite = createdSprite;
        if (ghost != null)
        {
            ghost.GetComponent<SpriteRenderer>().sprite = createdSprite;
        }
        checkpointCamera.gameObject.SetActive(false);
        string filename = gameObject.name + ".png";
        ES2.SaveImage(screenShot, filename);
        return filename;
    }

    /**
    * Displays this checkpoint relative to currentCheckpoint
    */
    public void showRelativeTo(GameObject currentCheckpoint)
    {
        if (activated)
        {
            cpGhostMover.showRelativeTo(currentCheckpoint);
        }
    }
    /// <summary>
    /// Checks to see if this checkpoint's ghost contains the targetPos
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool checkGhostActivation(Vector3 targetPos)
    {
        return ghost.GetComponent<CircleCollider2D>().bounds.Contains(targetPos);
    }
    /// <summary>
    /// So now the player has teleported out and the checkpoint ghosts need to go away
    /// </summary>
    /// <param name="first"></param>
    public void clearPostTeleport(bool first)
    {
        cpGhostMover.goHome();
        if (first)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Checkpoint_Root"))
            {
                if (!go.Equals(this.gameObject))
                {
                    go.GetComponent<CheckPointChecker>().clearPostTeleport(false);
                }
            }
        }
    }

    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, activated);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        if (memObj.found)
        {
            activate();
        }
    }
}

