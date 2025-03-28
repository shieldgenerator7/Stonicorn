using UnityEngine;

public class CreditsRoller : MonoBehaviour
{
    private new Camera camera;
    [SerializeField]
    private RectTransform rectTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //camera = Managers.Camera.cam;   
    }

    // Update is called once per frame
    void Update()
    {
        //rectTransform.localScale= new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
        //rectTransform.position = Managers.Camera.transform.position;
    }
}
