using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAstronauts {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private int maxNumberOfSimultaneousAliensGoAway;   //Número máximo de aliens huyendo al mismo tiempo
    private List<int> alienStates;  //0 -> atacando(idle).  1 -> huyendo.  2 -> supporteando al que huye

    public AttackAstronauts(List<PlayerController> astronautControllers, List<AlienController> alienControllers)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
        maxNumberOfSimultaneousAliensGoAway = 2;

        alienStates = new List<int>();
        for(int i=0; i<alienControllers.Capacity; i++)  //Set states of aliens to idle
        {
            alienStates.Add(0);
        }
    }

    private int GetNumberOfSimultaneousAliensGoAway()
    {
        int number = 0;

        foreach(int element in alienStates)
        {
            if(element == 1)
            {
                number++;
            }
        }
        return number;
    }

    private void SetAlienState(int alienId, int state)
    {
        alienStates[alienId] = state;
    }

    private int GetAlienState(AlienController controller)
    {
        int alienId = controller.id;
        return alienStates[alienId];
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
        Vector3 from = direction[0];    //Posición del alien
        Vector3 to = direction[1];      //Posición del astronauta

        Vector3 directionToAstronaut = (to - from).normalized;  //(from - to).normalized PARA IR POR EL ASTRONAUTA

        controller.UpdateTrajectoryDirection(directionToAstronaut);
        controller.SetMove(true);
        controller.Move();
    }

    private void ThrowBall(AlienController controller, List<Vector3> direction)
    {
        Vector3 from = direction[0];    //Posición del alien
        Vector3 to = direction[1];      //Posición del astronauta

        Vector3 directionToAstronaut = (from - to).normalized;

        GameObject ball = controller.gameObject.transform.GetChild(3).gameObject;
        bool nowThrowing = ball.GetComponent<ThrowBall>().NowThrowing();
        if(!nowThrowing)
        {
            if(controller.id==1)
            {
                ball.GetComponent<ThrowBall>().Throw(directionToAstronaut);
                Debug.Log("Throwing again!");
            }
        }
    }

    private void DoSomething(AlienController controller)
    {
        int state = GetAlienState(controller);
        switch(state)
        {
            case 0:
                controller.Stop();
                List<Vector3> directionOfClosestAstronaut = controller.GetDirectionOfClosestAstronaut();
                ThrowBall(controller, directionOfClosestAstronaut);
                break;
            case 1:
                List<Vector3> directionOfEscape = controller.GetDirectionOfEscape();
                MoveAway(controller, directionOfEscape);
                break;
        }
    }

    public void UpdateAliens()
    {
        foreach(AlienController controller in alienControllers)
        {
            //Draw State Icon

            //End Draw State Icon

            if (controller.CheckDistanceWithAstronauts(astronautControllers))    //True if this alien is too close
            {
                if(GetNumberOfSimultaneousAliensGoAway() <= maxNumberOfSimultaneousAliensGoAway - 1)
                {
                    SetAlienState(controller.id, 1);    //Set this alien state to running away
                }
            }
            else
            {
                SetAlienState(controller.id, 0);
            }

            //Check states and act
            DoSomething(controller);

        }
    }
}
