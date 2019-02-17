using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickManager : MonoBehaviour {

    public GameObject myCamera;
    public AstronautManager astronautManager;
    private GameObject[] astronauts;

    private float minimumDistanceToClickAstronaut = 70;
    private float minimumDepthToClickAstronaut = 80;

    private void Update () {    //Traer coordenadas de los astronautas a 2D

        astronauts = astronautManager.astronauts;
        int closestAstronaut = 0;
        int astronautCounter = 0;
        float closestDepth = 0;
        float minimumDistance = float.MaxValue;

        Vector2 mousePos = Input.mousePosition;
        foreach(GameObject astronaut in astronauts)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(astronaut.transform.position);
            float distance = Vector2.Distance(mousePos, screenPos);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                closestAstronaut = astronautCounter;
                closestDepth = Camera.main.WorldToScreenPoint(astronaut.transform.position).z;
            }
            ++astronautCounter;
        }
        if(minimumDistance <= minimumDistanceToClickAstronaut && closestDepth <= minimumDepthToClickAstronaut)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectAstronaut(closestAstronaut);
            }
            else
            {
                Point();
            }
        }
    }

    private void Point()
    {
        gameObject.GetComponent<MouseSkinManager>().SetTexture("point");
    }

    private void SelectAstronaut(int astronautId)
    {
        switch (astronautId)
        {
            case 0:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut0(); break;
            case 1:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut1(); break;
            case 2:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut2(); break;
            case 3:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut3(); break;
            case 4:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut4(); break;
            case 5:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut5(); break;
            case 6:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut6(); break;
            case 7:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut7(); break;
        }
    }
}
