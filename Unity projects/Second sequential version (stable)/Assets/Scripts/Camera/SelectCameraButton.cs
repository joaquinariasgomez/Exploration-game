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
    private int latestAstronautCamera = -1;

    public void Start()
    {
        this.astronauts = astronautManager.astronauts;
    }

    public void ChangeToGlobalCamera()
    {
        if(latestAstronautCamera == -1) { return; }

        cameraController.SetCameraMovement();

        int childCount = astronauts[latestAstronautCamera].transform.childCount;
        astronauts[latestAstronautCamera].transform.GetChild(childCount - 1).parent = GameObject.Find("Father Camera").transform;

        latestAstronautCamera = -1;
    }

    public void ChangeToAstronautCamera(int selectedAstronaut)
    {
        latestAstronautCamera = selectedAstronaut;

        cameraController.SetAstronautMovement(selectedAstronaut);
        transform.position = astronauts[selectedAstronaut].transform.position;
        transform.parent = astronauts[selectedAstronaut].transform;
    }

    public void ChangeCamera()  //It's called when Change Camera button is pressed
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

    public void OnPointerEnter() {
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().SetTexture("point", true);
    }

    public void OnPointerExit()
    {
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().UnsetTextureButton();
    }
}
