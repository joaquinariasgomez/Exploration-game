using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAliens {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private float distanceToDefender = 5f;
    private float separationBetweenDefensors = 1f;

    public AttackAliens(List<PlayerController> astronautControllers, List<AlienController> alienControllers)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
    }

    private void MoveRandomly(PlayerController controller)
    {
        Vector3 objective = alienControllers[0].transform.position;
        controller.UpdateTrajectoryDirection(objective);
        controller.SetMove(true);
        controller.Move();
    }

    private void Defend(PlayerController controller, PlayerController defender)   //Controller will defend defender
    {
        //Search for closest ball to defender
        float minimumDistanceToBall = 1000f;
        Vector3 positionOfAlien = Vector3.zero;

        foreach(AlienController alien in alienControllers)
        {
            float distanceToBall = Vector3.Distance(defender.transform.position, alien.ball.transform.position);
            if(distanceToBall < minimumDistanceToBall)
            {
                minimumDistanceToBall = distanceToBall;
                positionOfAlien = alien.transform.position;
            }
        }

        Vector3 defenderToAlien = positionOfAlien - defender.transform.position;
        float multiplier = distanceToDefender / defenderToAlien.magnitude;

        Vector3 direction = defender.transform.position + defenderToAlien * multiplier;

        float minimumDistanceToDefender = 1000f;
        Vector3 defenderAway = Vector3.zero;
        foreach(PlayerController player in astronautControllers)
        {
            if(player.GetWeapon() == "shield" && player.id != controller.id)
            {
                float distance = Vector3.Distance(player.transform.position, controller.transform.position);
                if(distance < minimumDistanceToDefender)
                {
                    minimumDistanceToDefender = distance;
                    defenderAway = (controller.transform.position - player.transform.position).normalized * 2f;
                }
            }
        }

        if(minimumDistanceToDefender < separationBetweenDefensors)
        {
            direction += defenderAway;
        }

        controller.UpdateTrajectoryDirection(direction);
        controller.SetMove(true);
        controller.Move();
    }

    public void UpdateAstronauts()
    {
        PlayerController defender = astronautControllers[0];
        foreach(PlayerController controller in astronautControllers)
        {
            if(controller.GetWeapon() == "sword")
            {
                defender = controller;
            }
        }
        foreach (PlayerController controller in astronautControllers)
        {
            //MoveRandomly(controller);
            if(controller.GetWeapon() == "shield")
            {
                Defend(controller, defender);
            }
            else
            {
                controller.Stop();
            }
        }
    }
}
