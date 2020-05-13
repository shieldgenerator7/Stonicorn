
#if UNITY_EDITOR

using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TugBoat : MonoBehaviour
{
    public Vector3 targetPosition;

    public void move()
    {
        Vector3 direction = targetPosition - transform.position;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                foreach (GameObject go in s.GetRootGameObjects())
                {
                    go.transform.position += direction;
                }
                EditorSceneManager.MarkSceneDirty(s);
            }
        }
    }
}

#endif
