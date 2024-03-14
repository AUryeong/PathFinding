using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera cam;
    public Rect screenRect;

    private Vector3 prevMousePosition;

    private const float CAMERA_ZOOM_VALUE = 1;
    private const float CAMERA_MIN_SIZE = 1;
    private const float CAMERA_MAX_SIZE = 30;
    
    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        UpdateScreenRect();
    }

    private void UpdateScreenRect()
    {
        float screenY = cam.orthographicSize * 2;
        screenRect.size = new Vector2(screenY * Screen.width / Screen.height, screenY);
        screenRect.position = new Vector2(cam.transform.position.x - screenRect.width / 2, cam.transform.position.y - screenRect.height / 2);
    }

    private void Update()
    {
        UpdateCameraPos();
        UpdateCameraZoom();
    }

    private void UpdateCameraZoom()
    {
        if (Input.mouseScrollDelta.y == 0) return;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y * CAMERA_ZOOM_VALUE, CAMERA_MIN_SIZE, CAMERA_MAX_SIZE);
        UpdateScreenRect();
    }

    private void UpdateCameraPos()
    {
        if (!Input.GetMouseButton(2)) return;
        if (Input.GetMouseButtonDown(2))
            prevMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0) return;

        cam.transform.position -= cam.ScreenToWorldPoint(Input.mousePosition) - prevMousePosition;
        UpdateScreenRect();
    }
}