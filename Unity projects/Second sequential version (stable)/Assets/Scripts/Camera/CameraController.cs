using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public CubeSphere attractor;
    public AstronautManager astronautManager;

    GameObject[] astronauts;

    private float cameraAltitude = 40;
    private float initialAltitude;
    private float zoom = 0;
    private float minZoom = -40;
    private float maxZoom = 100;
    private float zoomChangeAmount = 80f;
    [HideInInspector]
    public float zoomPercentage;

    private bool astronautMovement = false;
    private int selectedAstronaut;

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

    private void Start()
    {
        this.astronauts = astronautManager.astronauts;
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
        zoomPercentage = (distanceToMinZoom / zoomTravel) * 100;
        dragSpeed = 0.7f + zoomPercentage / 6f;

        //Keyboard
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        transform.position += transform.TransformDirection(moveDir) * dragSpeed * 2 * Time.deltaTime;

        //Mouse
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

    private void PerformGravityRotationFromAstronaut()
    {
        int childCount = astronauts[selectedAstronaut].transform.childCount;

        Vector3 gravityUp = (astronauts[selectedAstronaut].transform.GetChild(childCount - 1).position - attractor.transform.position).normalized;
        Vector3 cameraUp = astronauts[selectedAstronaut].transform.GetChild(childCount - 1).up;

        Quaternion targetRotation = Quaternion.FromToRotation(cameraUp, gravityUp) * astronauts[selectedAstronaut].transform.GetChild(childCount - 1).rotation;
        astronauts[selectedAstronaut].transform.GetChild(childCount - 1).rotation = targetRotation;
    }

    private void PerformZoomFromAstronaut()
    {
        //ONLY FOR SHARED DATA
        float zoomTravel = maxZoom + Mathf.Abs(minZoom);
        float distanceToMinZoom = zoom - minZoom;
        zoomPercentage = (distanceToMinZoom / zoomTravel) * 100;
        //END ONLY FOR SHARED DATA

        int childCount = astronauts[selectedAstronaut].transform.childCount;

        Vector3 gravityUp = (astronauts[selectedAstronaut].transform.GetChild(childCount - 1).position - attractor.transform.position).normalized;
        if (Input.mouseScrollDelta.y > 0)   //Zoom in
        {
            zoom -= zoomChangeAmount * Time.deltaTime;
        }
        if (Input.mouseScrollDelta.y < 0)   //Zoom out
        {
            zoom += zoomChangeAmount * Time.deltaTime;
        }
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        float distanceFromCenterToCamera = Vector3.Distance(attractor.transform.position, astronauts[selectedAstronaut].transform.GetChild(childCount - 1).position);
        float difference = distanceFromCenterToCamera - initialAltitude;

        astronauts[selectedAstronaut].transform.GetChild(childCount - 1).position -= gravityUp * difference;
        astronauts[selectedAstronaut].transform.GetChild(childCount - 1).position += gravityUp * zoom;
    }

    public void SetAstronautMovement(int selectedAstronaut)
    {
        this.astronautMovement = true;
        this.selectedAstronaut = selectedAstronaut;
    }

    public void SetCameraMovement()
    {
        this.astronautMovement = false;
    }

    void Update()
    {
        if(astronautMovement)
        {
            //OTHER actions (like not letting camera roll)
            //UPDATE VERTICAL TRANSLATION
            PerformGravityRotationFromAstronaut();
            PerformZoomFromAstronaut();
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
