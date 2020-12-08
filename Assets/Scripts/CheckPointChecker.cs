using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CheckPointChecker : MemoryMonoBehaviour
{

    public static CheckPointChecker current = null;//the current checkpoint

    public Sprite ghostSprite;
    private GameObject ghost;
    private CheckPointGhostMover cpGhostMover;
    public CheckPointGhostMover GhostMover => cpGhostMover;
    public GameObject ghostPrefab;
    private static Camera checkpointCamera;

    public int defaultTelepadIndex = 0;
    public List<Transform> telepads;
    public int telepadIndex { get; private set; } = -1;//the current telepad the player is in

    // Use this for initialization
    void Start()
    {
        initializeGhost();
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
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ghost, gameObject.scene);
        ghost.SetActive(false);
        cpGhostMover = ghost.GetComponent<CheckPointGhostMover>();
        cpGhostMover.parentCPC = this;
        ghost.GetComponent<SpriteRenderer>().sprite = ghostSprite;
    }

    //When a player touches this checkpoint, activate it
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.isPlayerSolid())
        {
            Discovered = true;
        }
    }

    public void activate()
    {
        //Not already activated, go ahead and activate
        Managers.saveCheckPoint(this);
        //Get the list of active checkpoints
        List<CheckPointChecker> activeCPCs = Managers.ActiveCheckPoints;
        //If there's two or more active checkpoints,
        if (activeCPCs.Count > 1)
        {
            //Start the particles
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }
        }
        //Start the fade-in effect
        foreach (Fader f in GetComponentsInChildren<Fader>())
        {
            f.enabled = true;
        }
    }
    public void trigger()
    {
        //If this checkpoint is already the current one,
        if (current == this)
        {
            //don't trigger it
            return;
        }
        current = this;
        if (ghostSprite == null)
        {
            Debug.LogError("CheckPointChecker " + gameObject.name + " needs to have a ghost sprite!");
        }
        activate();
        ghost.SetActive(false);
        InCheckPoint = true;
        foreach (CheckPointChecker cpc in Managers.ActiveCheckPoints)
        {
            if (cpc != this)
            {
                cpc.showRelativeTo(this.gameObject);
            }
        }
        calculateTelepadIndex();
    }

    public Vector2 getTelepadPosition(CheckPointChecker fromCP)
    {
        fromCP.calculateTelepadIndex();
        int currentTelepad = fromCP.telepadIndex;
        if (this.telepads[currentTelepad] != null)
        {
            return this.telepads[currentTelepad].position;
        }
        else
        {
            return this.telepads[defaultTelepadIndex].position;
        }
    }

    public void calculateTelepadIndex()
    {
        //Set current telepad
        telepadIndex = -1;
        float minDist = float.MaxValue;
        int minTP = -1;
        //Find the telepad
        for (int i = 0; i < telepads.Count; i++)
        {
            Transform tp = telepads[i];
            if (tp)
            {
                float dist = Vector2.Distance(
                    tp.position,
                    Managers.Player.transform.position
                    );
                //That is closest to the player
                if (dist < minDist)
                {
                    minDist = dist;
                    minTP = i;
                }
            }
        }
        telepadIndex = minTP;
    }

    public static void readjustCheckPointGhosts(Vector2 epicenter)
    {
        foreach (CheckPointChecker cpc in Managers.ActiveCheckPoints)
        {
            if (cpc != current)
            {
                cpc.cpGhostMover.snapToNewPosition(epicenter);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            if (current != this)
            {
                trigger();
            }
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            if (current == this)
            {
                InCheckPoint = false;
                activate();
                clearPostTeleport(true);
                current = null;
            }
        }
    }
    /// <summary>
    /// Whether or not the player is inside a checkpoint
    /// </summary>
    public static bool InCheckPoint
    {
        set
        {
            if (value)
            {
                Managers.Player.Teleport.overrideTeleportPosition -= checkCheckPointGhosts;
                Managers.Player.Teleport.overrideTeleportPosition += checkCheckPointGhosts;
                Managers.Player.Teleport.onTeleport -= updateCheckPointCheckers;
                Managers.Player.Teleport.onTeleport += updateCheckPointCheckers;
            }
            else
            {
                Managers.Player.Teleport.overrideTeleportPosition -= checkCheckPointGhosts;
                Managers.Player.Teleport.onTeleport -= updateCheckPointCheckers;
            }
        }
    }
    private static Vector2 checkCheckPointGhosts(Vector2 pos, Vector2 tapPos)
    {
        CheckPointChecker checkPoint = Managers.ActiveCheckPoints
            .Find(cpc => cpc.checkGhostActivation(pos));
        if (checkPoint)
        {
            Vector2 telepadPos = checkPoint.getTelepadPosition(current);
            Vector2 foundPos = Managers.Player.Teleport
                .findTeleportablePosition(telepadPos, telepadPos);
            if (checkPoint.GetComponent<Collider2D>().OverlapPoint(foundPos))
            {
                return foundPos;
            }
        }
        return Vector2.zero;
    }
    private static void updateCheckPointCheckers(Vector2 oldPos, Vector2 newPos)
    {
        //If teleport to other checkpoint,
        if ((oldPos - newPos).magnitude > Managers.Player.Teleport.Range * 2)
        {
            //Move the camera to Merky's center
            Managers.Camera.recenter();
        }
        //If teleport within same checkpoint,
        else
        {
            //Reposition checkpoint previews
            readjustCheckPointGhosts(Managers.Player.transform.position);
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
        //Orient the camera to the gravity collider it's in
        foreach (GravityZone gz in FindObjectsOfType<GravityZone>())
        {
            if (gz.GetComponent<Collider2D>().OverlapPoint(
                checkpointCamera.gameObject.transform.position
                ))
            {
                checkpointCamera.gameObject.transform.up =
                    (Vector2)checkpointCamera.gameObject.transform.position - (Vector2)gz.transform.position;
                break;
            }
        }
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
        ES3.SaveImage(screenShot, filename);
        return filename;
    }

    /**
    * Displays this checkpoint relative to currentCheckpoint
    */
    public void showRelativeTo(GameObject currentCheckpoint)
    {
        if (Discovered)
        {
            cpGhostMover.showRelativeTo(currentCheckpoint);
        }
    }
    /// <summary>
    /// Checks to see if this checkpoint's ghost contains the targetPos
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool checkGhostActivation(Vector2 targetPos)
    {
        return ghost.GetComponent<Collider2D>().OverlapPoint(targetPos);
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
    protected override void nowDiscovered()
    {
        trigger();
    }
    protected override void previouslyDiscovered()
    {
        activate();
    }
}

