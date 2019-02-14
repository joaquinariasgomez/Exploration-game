using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCameraButton : MonoBehaviour {

    public AstronautManager astronautManager;
    public CameraController cameraController;
    public AstronautSelector astronautSelector;
    public Button ChangeCameraButton;

    GameObject[] astronauts;
    private List<PlayerController> astronautControllers = new List<PlayerController>();
    private bool globalCamera = true;

    public void Start()
    {
        this.astronauts = astronautManager.astronauts;
        foreach (GameObject astronaut in astronauts)
        {
            astronautControllers.Add(astronaut.GetComponent<PlayerController>());
        }
    }

    public void ChangeToGlobalCamera()
    {
        astronautSelector.SetSelectedAstronaut(-1);
        cameraController.SetCameraMovement();
        //transform.position = cameraController.position;
        //transform.parent = cameraController.transform;
    }

    public void ChangeToAstronautCamera(int selectedAstronaut)
    {
        cameraController.SetAstronautMovement();
        transform.position = astronauts[selectedAstronaut].transform.position;
        transform.parent = astronauts[selectedAstronaut].transform;
    }

    public void ChangeCamera()
    {
        //Change camera to selected astronaut's ubication, so that he will be his parent
        globalCamera = !globalCamera;

        if(globalCamera)
        {
            ChangeToGlobalCamera();
        }
        else
        {
            int selectedAstronaut = astronautSelector.GetSelectedAstronaut();
            if (selectedAstronaut == -1)
            {
                ChangeToGlobalCamera();
            }
            else
            {
                ChangeToAstronautCamera(selectedAstronaut);
            }
        }
    }
}
