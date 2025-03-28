using UnityEngine;

public class CreditsRoller : MonoBehaviour
{
    private new Camera camera;
    [SerializeField]
    private RectTransform rectTransform;

    public float scrollSpeed = 1;
    public RectTransform creditsContainer;
    public Fader backdropFader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //camera = Managers.Camera.cam;   
    }

    private void OnEnable()
    {
        Managers.Camera.onZoomLevelChanged -= listenForZoom;
        Managers.Camera.onZoomLevelChanged += listenForZoom;
    }
    private void OnDisable()
    {
        Managers.Camera.onZoomLevelChanged -= listenForZoom;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = creditsContainer.position;
        pos.y += scrollSpeed * Time.unscaledDeltaTime;
        creditsContainer.position = pos;

    }

    private void listenForZoom(float zoom, float delta)
    {
        //restart the backdrop fading
        backdropFader.enabled = false;
        //if not in menu or time rewinding,
        if (zoom >= Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.PORTRAIT)
            && zoom < Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.TIMEREWIND))
        {
            //start backdrop fade again
            backdropFader.enabled = true;
        }
    }
}
