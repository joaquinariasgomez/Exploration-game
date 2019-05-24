using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickManager : MonoBehaviour {

    public GameObject myCamera;
    public AstronautManager astronautManager;
    public Texture2D Pointed_astronaut;
    public GameObject attack_defend;

    private bool draw_point_astronaut = false;
    private int pointedAstronaut = 0;

    private GameObject[] astronauts;
    private GameObject[] aliens;

    private float minimumDistanceToClickAstronaut = 70;
    private float minimumDepthToClickAstronaut = 80;

    private void Update () {    //Traer coordenadas de los astronautas a 2D
        if(PauseMenu.GamePaused)
        {
            return;
        }
        astronauts = astronautManager.astronauts;
        aliens = astronautManager.aliens;
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
            if (Input.GetMouseButtonUp(0))
            {
                bool defend = attack_defend.GetComponent<Attack_Defend>().Clicked() == "clickedDefend";
                if(defend && astronauts[closestAstronaut].GetComponent<PlayerController>().GetWeapon() == "sword")
                {
                    SelectAstronaut(closestAstronaut, true);
                }
                else
                {
                    SelectAstronaut(closestAstronaut, false);
                }
            }
            else
            {
                if(attack_defend.GetComponent<Attack_Defend>().Clicked() == "clickedDefend" && astronauts[closestAstronaut].GetComponent<PlayerController>().GetWeapon() == "sword")
                {
                    PointDefend(closestAstronaut);
                }
                else
                {
                    Point(closestAstronaut);
                }
            }
        }
        else
        {
            Unpoint();
        }

        //Do same with aliens
        int closestAlien = 0;
        int alienCounter = 0;
        closestDepth = 0;
        minimumDistance = float.MaxValue;

        mousePos = Input.mousePosition;
        foreach (GameObject alien in aliens)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(alien.transform.position);
            float distance = Vector2.Distance(mousePos, screenPos);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                closestAlien = alienCounter;
                closestDepth = Camera.main.WorldToScreenPoint(alien.transform.position).z;
            }
            ++alienCounter;
        }
        if (minimumDistance <= minimumDistanceToClickAstronaut && closestDepth <= minimumDepthToClickAstronaut)
        {
            if (Input.GetMouseButtonUp(0))
            {
                //SelectAlien(closestAlien);
            }
            else
            {
                if (attack_defend.GetComponent<Attack_Defend>().Clicked() == "clickedAttack")
                {
                    PointAttack();
                }
            }
        }
    }

    private void OnGUI()
    {
        if(PauseMenu.GamePaused)
        {
            return;
        }
        if(draw_point_astronaut)
        {
            Vector2 astronautPos = Camera.main.WorldToScreenPoint(astronauts[pointedAstronaut].transform.position);
            GUI.DrawTexture(new Rect(astronautPos.x - 14, Screen.height - astronautPos.y - 36, 28, 17), Pointed_astronaut);
        }
    }

    private void PointDefend(int pointedAstronaut)
    {
        gameObject.GetComponent<MouseSkinManager>().Unpoint("astronaut");
        gameObject.GetComponent<MouseSkinManager>().Point("defend");

        draw_point_astronaut = true;
        this.pointedAstronaut = pointedAstronaut;
    }

    private void PointAttack()
    {
        gameObject.GetComponent<MouseSkinManager>().Point("attack");
    }

    private void Point(int pointedAstronaut)
    {
        gameObject.GetComponent<MouseSkinManager>().Unpoint("defend");
        gameObject.GetComponent<MouseSkinManager>().Unpoint("attack");
        gameObject.GetComponent<MouseSkinManager>().Point();

        draw_point_astronaut = true;
        this.pointedAstronaut = pointedAstronaut;
    }

    private void Unpoint()
    {
        gameObject.GetComponent<MouseSkinManager>().Unpoint();
        gameObject.GetComponent<MouseSkinManager>().Unpoint("defend");
        gameObject.GetComponent<MouseSkinManager>().Unpoint("attack");
        draw_point_astronaut = false;
    }

    private void SelectAstronaut(int astronautId, bool Defend = false)
    {
        switch (astronautId)
        {
            case 0:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut0(Defend); break;
            case 1:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut1(Defend); break;
            case 2:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut2(Defend); break;
            case 3:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut3(Defend); break;
            case 4:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut4(Defend); break;
            case 5:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut5(Defend); break;
            case 6:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut6(Defend); break;
            case 7:
                myCamera.GetComponent<AstronautSelector>().SelectAstronaut7(Defend); break;
        }
    }
}
