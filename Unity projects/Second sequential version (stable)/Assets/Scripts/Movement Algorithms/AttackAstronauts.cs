using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAstronauts {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private int maxNumberOfSimultaneousAliensGoAway;   //Número máximo de aliens huyendo al mismo tiempo
    private List<int> alienStates;  //0 -> atacando(idle).  1 -> huyendo.  2 -> supporteando al que huye
    private Dictionary<int, Vector3> alienAstronautTarget = new Dictionary<int, Vector3>();   //Only for states 2

    public AttackAstronauts(List<PlayerController> astronautControllers, List<AlienController> alienControllers)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
        maxNumberOfSimultaneousAliensGoAway = 5;    //2

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

    private void SetAlienState(int alienId, int state, Vector3 astronautPosition) //directionOfEscape is for state 2
    {
        alienStates[alienId] = state;
        if(state == 2)
        {
            alienAstronautTarget[alienId] = astronautPosition;
        }
    }

    private int GetAlienState(AlienController controller)
    {
        int alienId = controller.id;
        return alienStates[alienId];
    }

    private int GetNumberOfAlienStates(int desiredState)  //Returns number of "state" states
    {
        int counter = 0;

        foreach(int state in alienStates)
        {
            if(state == desiredState)
            {
                counter++;
            }
        }
        return counter;
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

        bool nowThrowing = controller.ball.GetComponent<ThrowBall>().NowThrowing();

        if(!nowThrowing)
        {
            Vector3 positionFromShoot = controller.transform.position;
            float distance = Vector3.Distance(positionFromShoot, Vector3.zero);
            float targetDistace = distance + 0.6f;
            Vector3 newPositionFromShoot = positionFromShoot * (targetDistace / distance);//Vector3.ClampMagnitude(positionFromShoot, targetDistace);
            controller.ball.GetComponent<ThrowBall>().Throw(newPositionFromShoot, directionToAstronaut);
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
            case 2:
                Vector3 from = alienAstronautTarget[controller.id];
                Vector3 to = controller.transform.position;
                List<Vector3> directionToAstronaut = new List<Vector3>();
                directionToAstronaut.Add(from);
                directionToAstronaut.Add(to);

                //KeepDistanceAndShoot(controller, directionToAstronaut);
                ThrowBall(controller, directionToAstronaut);
                //MoveAway(controller, directionToAstronaut);
                break;
        }
    }

    private void DrawStateIcon(AlienController controller)
    {
        int state = GetAlienState(controller);
        switch(state)
        {
            case 0: controller.SetImageToDraw(0); break;    //0
            case 1: controller.SetImageToDraw(2); break;    //2
            case 2: controller.SetImageToDraw(1); break;    //1
        }
    }

    public void UpdateAliens()
    {
        foreach(AlienController controller in alienControllers)
        {
            //Draw State Icon
            DrawStateIcon(controller);
            //End Draw State Icon

            if (controller.CheckDistanceWithAstronauts(astronautControllers))    //True if this alien is too close
            {
                if(GetNumberOfSimultaneousAliensGoAway() <= maxNumberOfSimultaneousAliensGoAway - 1)
                {
                    SetAlienState(controller.id, 1, Vector3.zero);    //Set this alien state to running away
                    //Warn  other astronauts that don't have state 1
                    foreach(AlienController controller2 in alienControllers)
                    {
                        if(GetAlienState(controller2) != 1)
                        {
                            Vector3 astronautPosition = controller.GetDirectionOfEscape()[0];
                            SetAlienState(controller2.id, 2, astronautPosition);
                        }
                    }
                }
            }
            else
            {
                if(GetAlienState(controller) == 2)
                {
                    if(GetNumberOfSimultaneousAliensGoAway() == 0)
                    {
                        SetAlienState(controller.id, 0, Vector3.zero);
                    }
                }
                else
                {
                    SetAlienState(controller.id, 0, Vector3.zero);
                }
            }

            //Check states and act
            DoSomething(controller);

        }
    }
}
