using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAstronauts {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;

    public AttackAstronauts(List<PlayerController> astronautControllers, List<AlienController> alienControllers)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
    }

    private void StopAll()
    {
        foreach (AlienController controller in alienControllers)
        {
            controller.Stop();
        }
    }

    private void MoveAway(AlienController controller, List<Vector3> direction) //Correr en la dirección indicada
    {
        Vector3 from = direction[0];
        Vector3 to = direction[1];

        controller.Move();

    }

    public void UpdateAliens()
    {
        foreach(AlienController controller in alienControllers)
        {
            if(controller.CheckDistanceWithAstronauts(astronautControllers))    //True if this alien is too close
            {
                List<Vector3> directionOfEscape = controller.GetDirectionOfEscape();
                MoveAway(controller, directionOfEscape);
            }
        }
    }
}
