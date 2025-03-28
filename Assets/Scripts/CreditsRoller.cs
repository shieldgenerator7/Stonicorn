using UnityEngine;

public class CreditsRoller : MonoBehaviour
{
    private new Camera camera;
    [SerializeField]
    private RectTransform rectTransform;

    public float scrollSpeed = 1;
    public RectTransform creditsContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //camera = Managers.Camera.cam;   
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = creditsContainer.position;
        pos.y += scrollSpeed * Time.unscaledDeltaTime;
        creditsContainer.position = pos;

    }
}
