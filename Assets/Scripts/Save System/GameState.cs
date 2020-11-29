using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameState
{
    public int id;
    public List<ObjectState> states = new List<ObjectState>();
    private ObjectState merky;//the object state in the list specifically for Merky
    public ObjectState Merky
    {
        get
        {
            if (merky == null)
            {
                merky = states.Find(os => os.objectName == "merky");
            }
            return merky;
        }
        private set => merky = value;
    }

    public static int nextid = 0;
    private GameObject representation;//the player ghost that represents this game state
    public GameObject Representation
    {
        get
        {
            if (representation == null)
            {
                try
                {
                    representation = GameObject.Instantiate(Managers.Game.playerGhostPrefab);
                    representation.transform.position = Merky.position;
                    representation.transform.localScale = Merky.localScale;
                    representation.transform.rotation = Merky.rotation;
                    //If this is the first game state,
                    if (id == 0)
                    {
                        //make its representation slightly bigger
                        representation.transform.localScale *= 2f;
                    }
                }
                catch (System.NullReferenceException nre)
                {
                    Debug.LogError("GameState (" + id + ") does not have a Merky! merky: " + Merky);
                }
            }
            return representation;
        }
    }

    //Instantiation
    public GameState()
    {
        id = nextid;
        nextid++;
    }
    public GameState(ICollection<GameObject> list) : this()
    {
        //Object States
        foreach (GameObject go in list)
        {
            ObjectState os = new ObjectState(go);
            os.saveState();
            states.Add(os);
            if (go.name == "merky")
            {
                Merky = os;
            }
        }
    }
    //Loading
    public void load()
    {
        states.ForEach(os => os.loadState());
    }
    public bool loadObject(GameObject go)
    {
        ObjectState state = states.Find(
            os => os.sceneName == go.scene.name && os.objectName == go.name
            );
        if (state != null)
        {
            state.loadState();
            return true;
        }
        return false;
    }
    //
    //Spawned Objects
    //

    //
    //Gets the list of the game objects that have object states in this game state
    //
    public List<GameObject> getGameObjects()
        => states.ConvertAll(os => os.getGameObject());

    //Returns true IFF the given GameObject has an ObjectState in this GameState
    public bool hasGameObject(GameObject go)
    {
        if (go == null)
        {
            throw new System.ArgumentNullException("GameState.hasGameObject() cannot accept null for go! go: " + go);
        }
        return states.Any(
            os => os.objectName == go.name && os.sceneName == go.scene.name
            );
    }
    //Representation (check point ghost)
    public void showRepresentation(int mostRecentId)
    {
        Representation.SetActive(true);
        //Set the Alpha Value
        SpriteRenderer sr = Representation.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        ParticleSystem ps = Representation.GetComponentInChildren<ParticleSystem>();

        if (mostRecentId - id < 10)
        {
            sr.color = new Color(c.r, c.g, c.b, 1.0f);
            ps.Play();
        }
        else if (mostRecentId - id < 100)
        {
            sr.color = new Color(c.r, c.g, c.b, 0.9f);
            ps.Stop();
        }
        else
        {
            sr.color = new Color(c.r, c.g, c.b, 0.5f);
            ps.Stop();
        }
        //Do special processing for the first one
        if (id == 0)
        {
            //Make sure it's always on screen
            if (!Managers.Camera.inView(Representation.transform.position))
            {
                Representation.transform.position =
                    Managers.Camera.getInViewPosition(
                        Merky.position,
                        0.9f
                    );
            }
            else
            {
                Representation.transform.position = Merky.position;
            }
        }
    }
    public bool checkRepresentation(Vector3 touchPoint, bool checkSprite = true)
    {
        if (checkSprite)
        {
            return Representation.GetComponent<SpriteRenderer>().bounds.Contains(touchPoint);
        }
        else
        {
            return Representation.GetComponent<Collider2D>().OverlapPoint(touchPoint);
        }
    }
    public void hideRepresentation()
    {
        if (representation != null)
        {
            representation.SetActive(false);
        }
    }
}
