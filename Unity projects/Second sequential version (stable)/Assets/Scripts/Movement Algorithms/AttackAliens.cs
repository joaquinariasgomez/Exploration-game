using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAliens {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;

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

    public void UpdateAstronauts()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            MoveRandomly(controller);
        }
    }
}
