using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRewindController : MonoBehaviour
{
    [Header("Objects")]
    public GameObject playerGhostPrefab;//this is to show Merky in the past (prefab)
    public GameObject ghostFolder;//object that the preview ghosts will be parented under
    public GameObject selectionHighlighter;//the object that visually points at the future projection

    /// <summary>
    /// The list of past merky representations
    /// Indexed into by the game state id
    /// </summary>
    private List<GameObject> representations = new List<GameObject>();

    private void Start()
    {
        Managers.Rewind.onRewindState += hideOldRepresentations;
    }

    #region Player Ghosts
    /// <summary>
    /// Shows the game state representations
    /// </summary>
    public void showPlayerGhosts(bool show)
    {
        this.enabled = show;
        //If the game state representations should be shown,
        if (show)
        {
            //Loop through all game states
            foreach (GameState gs in Managers.Rewind.GameStates)
            {
                //Update its representation
                updateRepresentation(gs);
                //Show a sprite to represent them on screen
                showRepresentation(gs, Managers.Rewind.GameStateId);
            }
        }
        //Else, they should be hidden
        else
        {
            //And hide all game states representations
            representations.ForEach(
                rep => rep?.SetActive(false)
                );
#if UNITY_EDITOR
            int count = representations.FindAll(rep => rep == null).Count;
            if (count > 0)
            {
                Debug.LogError("PlayerRewindController has " + count + " null representations in it!");
            }
#endif
        }
    }

    private void updateRepresentation(GameState gs)
    {
        //If the representation is not already in the list,
        if (gs.id >= representations.Count || representations[gs.id] == null)
        {
            //Populate the list with null values
            while (gs.id >= representations.Count)
            {
                representations.Add(null);
            }
            //Put a new rep into the list
            representations[gs.id] = GameObject.Instantiate(playerGhostPrefab);
            representations[gs.id].transform.parent = ghostFolder.transform;
        }
        //Retrieve the rep from the list
        GameObject rep = representations[gs.id];
        //Set the rep's transform
        try
        {
            var merky = gs.Merky;
            rep.transform.position = merky.position;
            rep.transform.localScale = merky.localScale;
            rep.transform.rotation = merky.rotation;
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("GameState (" + gs.id + ") does not have a Merky! merky: " + gs.Merky);
        }
        //If this is the first game state,
        if (gs.id == 0)
        {
            //make its representation slightly bigger
            rep.transform.localScale *= 2f;
        }
    }

    private GameObject getRepresentation(GameState gs)
        => representations[gs.id];

    //Representation (check point ghost)
    private void showRepresentation(GameState gs, int mostRecentId)
    {
        GameObject rep = getRepresentation(gs);
        rep.SetActive(true);
        //Set the Alpha Value
        SpriteRenderer sr = rep.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        ParticleSystem ps = rep.GetComponentInChildren<ParticleSystem>();

        if (mostRecentId - gs.id < 10)
        {
            sr.color = new Color(c.r, c.g, c.b, 1.0f);
            ps.Play();
        }
        else if (mostRecentId - gs.id < 100)
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
        if (gs.id == 0)
        {
            //Make sure it's always on screen
            if (!Managers.Camera.inView(rep.transform.position))
            {
                rep.transform.position =
                    Managers.Camera.getInViewPosition(
                        gs.Merky.position,
                        0.9f
                    );
            }
            else
            {
                rep.transform.position = gs.Merky.position;
            }
        }
    }

    /// <summary>
    /// Returns the player ghost that is closest to the given position
    /// </summary>
    /// <param name="pos">The ideal position of the closest ghost</param>
    /// <returns>The player ghost that is closest to the given position</returns>
    public GameObject getClosestPlayerGhost(Vector2 pos)
    {
        float closestDistance = float.MaxValue;
        GameObject closestObject = null;
        foreach (GameState gs in Managers.Rewind.GameStates)
        {
            GameObject rep = getRepresentation(gs);
            Vector2 gsPos = rep.transform.position;
            float gsDistance = Vector2.Distance(gsPos, pos);
            if (gsDistance < closestDistance)
            {
                closestDistance = gsDistance;
                closestObject = rep;
            }
        }
        return closestObject;
    }
    /// <summary>
    /// Used specifically to highlight last saved Merky after the first death
    /// for tutorial purposes
    /// </summary>
    /// <returns></returns>
    public Vector2 getLatestSafeRewindGhostPosition()
        => Managers.Rewind.GameStates[
            Managers.Rewind.GameStateId - 1
            ]
            .Merky.position;

    public bool checkRepresentation(GameState gs, Vector3 touchPoint, bool checkSprite = true)
    {
        if (checkSprite)
        {
            return getRepresentation(gs).GetComponent<SpriteRenderer>().bounds.Contains(touchPoint);
        }
        else
        {
            return getRepresentation(gs).GetComponent<Collider2D>().OverlapPoint(touchPoint);
        }
    }
    private void hideOldRepresentations(List<GameState> gameStates, int gameStateId)
    {
        //Hide representations that got rewound out of
        for (int i = gameStateId; i < representations.Count; i++)
        {
            representations[i]?.SetActive(false);
        }
        //Update position of first representation
        if (gameStateId >= 0)
        {
            updateRepresentation(gameStates[0]);
        }
    }

    private List<GameState> getGameStatesAtPosition(Vector2 pos)
    {
        //We have to do 2 passes to allow for
        //both precision clicking and fat-fingered tapping

        //Sprite detection pass
        List<GameState> states = Managers.Rewind.GameStates
            .FindAll(gs => checkRepresentation(gs, pos))
            .OrderBy(gs => gs.id).ToList();

        //Collider detection pass
        if (states.Count == 0)
        {
            states = Managers.Rewind.GameStates
                .FindAll(gs => checkRepresentation(gs, pos, false))
                .OrderBy(gs => gs.id).ToList();
        }

        return states;
    }

    private GameState getGameStateAtPosition(Vector2 pos)
    {
        List<GameState> states = getGameStatesAtPosition(pos);
        if (states.Count > 0)
        {
            GameState final = states.Last();
            //If the tapped one is already the current one,
            if (final.id == Managers.Rewind.GameStateId
                //And if the current one overlaps a previous one,
                && states.Count > 1)
            {
                //Choose the previous one
                states.Remove(final);
                return states.Last();
            }
            //Return the most recent one
            return final;
        }
        //Return nothing
        return null;
    }

    #endregion

    #region Input Processing

    public void processHoverGesture(Vector3 pos)
    {
        GameState gs = getGameStateAtPosition(pos);
        if (gs != null)
        {
            //Show selection highlighter
            selectionHighlighter.SetActive(true);
            selectionHighlighter.transform.localScale = transform.localScale;
            selectionHighlighter.transform.position = gs.Merky.position;
        }
        else
        {
            //Hide selection highlighter
            selectionHighlighter.SetActive(false);
        }
    }
    /// <summary>
    /// Processes the tap gesture at the given position
    /// </summary>
    /// <param name="curMPWorld">The position of the tap in world coordinates</param>
    public void processTapGesture(Vector3 curMPWorld)
    {
        GameState final = getGameStateAtPosition(curMPWorld);
        if (final != null)
        {
            //Process tapped game state
            //If the tapped one is already the current one,
            if (final.id == Managers.Rewind.GameStateId)
            {
                //Reload the current one
                Managers.Rewind.Load(final.id);
            }
            //Else if a past one was tapped,
            else
            {
                //Rewind back to it
                Managers.Rewind.RewindTo(final.id);
            }
            //Update Stats
            Managers.Stats.addOne("RewindPlayer");
        }

        //Leave this zoom level even if no past merky was chosen
        float defaultZoomLevel = Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.DEFAULT);
        Managers.Camera.ZoomLevel = defaultZoomLevel;
        Managers.Gesture.switchGestureProfile(GestureManager.GestureProfileType.MAIN);
        showPlayerGhosts(false);
        selectionHighlighter.SetActive(false);

        //Process tapProcessed delegates
        tapProcessed?.Invoke(curMPWorld);
    }
    public delegate void TapProcessed(Vector2 curMPWorld);
    public event TapProcessed tapProcessed;
    #endregion
}
