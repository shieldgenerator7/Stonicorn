using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class StopSpriteShapeColliderUpdating : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Find all SpriteShapes now
        FindObjectsByType<SpriteShapeController>(FindObjectsSortMode.None).ToList()
            .ForEach(ssc => ssc.autoUpdateCollider = false);
        //Find all SpriteShapes when a scene is loaded
        SceneManager.sceneLoaded += (scene, loadMode) =>
        FindObjectsByType<SpriteShapeController>(FindObjectsSortMode.None).ToList()
            .ForEach(ssc => ssc.autoUpdateCollider = false);
        Destroy(this);
    }
}
