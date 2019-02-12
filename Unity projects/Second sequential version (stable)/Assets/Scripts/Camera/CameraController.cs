using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float moveSpeed = 15;
    public CubeSphere attractor;

    private float cameraAltitude = 40;
    private float initialAltitude;
    private float zoom = 0;
    private float minZoom = -40;
    private float maxZoom = 100;
    private float zoomChangeAmount = 80f;

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

    void Update()
    {
        //PERFORM ROTATIONS
        PerformGravityRotation();

        //UPDATE HORIZONTAL TRANSLATION
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        transform.position += transform.TransformDirection(moveDir) * moveSpeed * Time.deltaTime;
        //UPDATE VERTICAL TRANSLATION

        /*if(Input.GetMouseButton(0))
        {
            xDisplacement = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            yDisplacement = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        }*/

        PerformZoom();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(0, (attractor.gridSize / 2f + CubeSphere.heightMultiplier + zoom), 0), 0.5f);
    }
}
