using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public CubeSphere attractor;
    public AstronautManager astronautManager;

    private int gridSize;

    GameObject[] astronauts;

    private float cameraAltitude = 40;
    private float initialAltitude;
    private float zoom = 0;
    private float minZoom = -40;
    private float maxZoom = 100;
    private float minZoomChangeAmount = 80f;    //Zoom change if zoom is lower than 0
    private float maxZoomChangeAmount = 80f;
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
        gridSize = DataBetweenScenes.getSize();
        switch (gridSize)
        {
            case 100: minZoom = -40; maxZoom = 100; maxZoomChangeAmount = 80; break;
            case 200: minZoom = -40; maxZoom = 150; maxZoomChangeAmount = 120; break;
            case 400: minZoom = -70; maxZoom = 300; maxZoomChangeAmount = 240; zoom = 30; break;
            default: minZoom = -70; maxZoom = 300; maxZoomChangeAmount = 240; zoom = 30; break;
        }
        initialAltitude = gridSize / 2f + CubeSphere.heightMultiplier + cameraAltitude + zoom;
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
        switch(gridSize)
        {
            case 100: dragSpeed *= 0.8f; break;
            case 400: dragSpeed *= 1.75f; break;
        }

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
        float zoomChangeAmount = (zoom < 0) ? minZoomChangeAmount : maxZoomChangeAmount;
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

        float zoomChangeAmount = (zoom < 0) ? minZoomChangeAmount : maxZoomChangeAmount;

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
        if(PauseMenu.GamePaused)
        {
            return;
        }
        bool pointing = GameObject.Find("Mouse").GetComponent<MouseSkinManager>().isPointingAstronaut || GameObject.Find("Mouse").GetComponent<MouseSkinManager>().isPointingButton;
        if (astronautMovement)
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
            if(!pointing)
            {
                PerformHorizontalTranslation();
            }
            //UPDATE VERTICAL TRANSLATION
            PerformZoom();
        }
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(0, (gridSize / 2f + CubeSphere.heightMultiplier + zoom), 0), 0.5f);
    }*/
}
