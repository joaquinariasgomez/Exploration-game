using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public CubeSphere attractor;

    private float cameraAltitude = 40;
    private float initialAltitude;
    private float zoom = 0;
    private float minZoom = -40;
    private float maxZoom = 100;
    private float zoomChangeAmount = 80f;

    private bool astronautMovement = false;

    //Mouse
    private float dragSpeed;
    private Vector3 dragOrigin;
    //private float lastMouseY = 0;     FUTURE IMPLEMENTATION

    private Vector3 moveDir;

    void Awake()
    {
        initialAltitude = attractor.gridSize / 2f + CubeSphere.heightMultiplier + cameraAltitude;
        transform.position += new Vector3(0, initialAltitude, 0);
    }

    private void PerformGravityRotation()
    {
        Vector3 gravityUp = (transform.position - attractor.transform.position).normalized;
        Vector3 cameraUp = transform.up;

        Quaternion targetRotation = Quaternion.FromToRotation(cameraUp, gravityUp) * transform.rotation;
        transform.rotation = targetRotation;
    }

    private void PerformHorizontalTranslation()
    {
        float zoomTravel = maxZoom + Mathf.Abs(minZoom);
        float distanceToMinZoom = zoom - minZoom;
        float zoomPercentage = (distanceToMinZoom / zoomTravel) * 100;
        dragSpeed = 0.7f + zoomPercentage / 6f;

        //Keyboard
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        transform.position += transform.TransformDirection(moveDir) * dragSpeed * 2 * Time.deltaTime;

        //Mouse
        /*if (Input.GetMouseButton(1))
        {
            transform.RotateAround(transform.position, transform.up, lastMouseY);
            lastMouseY = -Input.GetAxis("Mouse Y") * dragSpeed;
        }*/

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }
        else
        {
            if(Input.GetMouseButton(0))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                pos = -pos;
                Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

                transform.position += transform.TransformDirection(move);
            }
        }
    }

    private void PerformZoom()
    {
        Vector3 gravityUp = (transform.position - attractor.transform.position).normalized;
        if (Input.mouseScrollDelta.y > 0)   //Zoom in
        {
            zoom -= zoomChangeAmount * Time.deltaTime;
        }
        if (Input.mouseScrollDelta.y < 0)   //Zoom out
        {
            zoom += zoomChangeAmount * Time.deltaTime;
        }
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        float distanceFromCenterToCamera = Vector3.Distance(attractor.transform.position, transform.position);
        float difference = distanceFromCenterToCamera - initialAltitude;
        transform.position -= gravityUp * difference;
        transform.position += gravityUp * zoom;
    }

    public void SetAstronautMovement()
    {
        this.astronautMovement = true;
    }

    public void SetCameraMovement()
    {
        this.astronautMovement = false;
    }

    void Update()
    {
        if(astronautMovement)
        {
            //OTHER actions (like letting camera not roll)
            //UPDATE VERTICAL TRANSLATION
            PerformZoom();
        }
        else
        {
            //PERFORM ROTATIONS
            PerformGravityRotation();
            //UPDATE HORIZONTAL TRANSLATION
            PerformHorizontalTranslation();
            //UPDATE VERTICAL TRANSLATION
            PerformZoom();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(0, (attractor.gridSize / 2f + CubeSphere.heightMultiplier + zoom), 0), 0.5f);
    }
}
