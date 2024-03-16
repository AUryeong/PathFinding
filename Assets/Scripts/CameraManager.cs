using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : SingletonBehavior<CameraManager>
{
    [FormerlySerializedAs("cam")]
    public Camera mainCamera;
    
    public RectInt screenRectInt;
    [HideInInspector] public Rect screenRect;

    private Vector3 prevMousePosition;

    private const float CAMERA_ZOOM_VALUE = 1;
    private const float CAMERA_MIN_SIZE = 1;
    private const float CAMERA_MAX_SIZE = 30;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public override void Init()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateScreenRect();
        NodeManager.Instance.Init();
        InputManager.Instance.Init();
    }

    private void UpdateScreenRect()
    {
        float sizeY = mainCamera.orthographicSize * 2;
        float sizeX = sizeY * Screen.width / Screen.height;

        float posX = mainCamera.transform.position.x - sizeX / 2;
        float posY = mainCamera.transform.position.y - sizeY / 2;

        screenRect.Set(posX, posY, sizeX, sizeY);

        screenRectInt.size = new Vector2Int(Mathf.FloorToInt(sizeX), Mathf.CeilToInt(sizeY));
        screenRectInt.position = new Vector2Int(Mathf.FloorToInt(posX), Mathf.CeilToInt(posY));
    }

    private void Update()
    {
        CheckMoveCameraPos();
        CheckCameraZoom();
    }


    private void CheckCameraZoom()
    {
        if (Input.mouseScrollDelta.y == 0) return;

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - Input.mouseScrollDelta.y * CAMERA_ZOOM_VALUE, CAMERA_MIN_SIZE, CAMERA_MAX_SIZE);
        UpdateScreenRect();
    }

    private void CheckMoveCameraPos()
    {
        if (!Input.GetMouseButton(2)) return;
        if (Input.GetMouseButtonDown(2))
            prevMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0) return;

        mainCamera.transform.position -= mainCamera.ScreenToWorldPoint(Input.mousePosition) - prevMousePosition;
        UpdateScreenRect();
    }
}