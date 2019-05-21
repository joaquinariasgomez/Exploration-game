using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAstronauts {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private int maxNumberOfSimultaneousAliensGoAway;   //Número máximo de aliens huyendo al mismo tiempo
    private List<int> alienStates;  //0 -> atacando(idle).  1 -> huyendo.  2 -> supporteando al que huye
    private Dictionary<int, Vector3> alienAstronautTarget = new Dictionary<int, Vector3>();   //Only for states 2

    private int numberOfShoots = 4;    //Numero de disparos que guardará en la cola

    public AttackAstronauts(List<PlayerController> astronautControllers, List<AlienController> alienControllers)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;

        foreach(AlienController controller in alienControllers)
        {
            for(int i=0; i<numberOfShoots; i++)
            {
                controller.shootSuccesses.Enqueue(1);
            }
        }
        maxNumberOfSimultaneousAliensGoAway = 5;    //2

        alienStates = new List<int>();
        for(int i=0; i<alienControllers.Capacity; i++)  //Set states of aliens to idle
        {
            alienStates.Add(0);
        }
    }

    private float GetSuccessRate(AlienController controller)
    {
        float numberOfSuccess = 0f;
        foreach(int element in controller.shootSuccesses)
        {
            if(element == 1)
            {
                numberOfSuccess++;
            }
        }
        return numberOfSuccess / (float)numberOfShoots;
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

    private int HittedAstronaut(AlienController controller)   //0 -> ignore; 1 -> hit astronaut; 2 -> hit ground
    {
        bool nowThrowing = controller.ball.GetComponent<ThrowBall>().NowThrowing();
        if (controller.ball.GetComponent<ThrowBall>().isColliding() && nowThrowing)
        {
            if (controller.ball.GetComponent<ThrowBall>().hasHitAstronaut())
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        else
        {
            return 0;
        }
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
            Vector3 newPositionFromShoot = positionFromShoot * (targetDistace / distance);
            controller.ball.GetComponent<ThrowBall>().Throw(newPositionFromShoot, directionToAstronaut);
        }
    }

    private Vector3 GetClosestAlienPosition(AlienController controller)
    {
        float minDistance = 400f;
        Vector3 position = Vector3.zero;

        foreach(AlienController alien in alienControllers)
        {
            if(Vector3.Distance(controller.transform.position, alien.transform.position) < minDistance && controller.id != alien.id)
            {
                minDistance = Vector3.Distance(controller.transform.position, alien.transform.position);
                position = alien.transform.position;
            }
        }
        return position;
    }

    private void UpdateAlienTrajectory(AlienController controller, List<Vector3> directionOfTargetAstronaut)
    {
        Vector3 targetAstronautPosition = directionOfTargetAstronaut[0];
        Vector3 closestAlienPosition = GetClosestAlienPosition(controller);

        //Check successRate
        int result = HittedAstronaut(controller);

        if (result == 1 || result == 2)
        {
            if(result == 1)
            {
                controller.shootSuccesses.Enqueue(1);
            }
            if(result == 2)
            {
                controller.shootSuccesses.Enqueue(0);
            }
            controller.shootSuccesses.Dequeue();
        }
        //End check successRate
        float successRate = GetSuccessRate(controller);
        float valueSuccess = 10f * (-successRate + 1);   //Cuanto menos success rate, más valor tiene (seguir camino contradictorio)
        if (successRate >= 0.9f) valueSuccess = 0f;
        if(successRate < 0.2f)
        {
            valueSuccess *= 5;
        }

        float distanceToAlien = Vector3.Distance(closestAlienPosition, controller.transform.position);
        float distanceBetweenAliens = 7f; //6.5f
        float valueAlien = -distanceToAlien + distanceBetweenAliens;
        if (distanceToAlien > distanceBetweenAliens) valueAlien = 0f;

        float distanceToAstronaut = Vector3.Distance(targetAstronautPosition, controller.transform.position);

        Vector3 perpendicularDirection = -Vector3.Cross(controller.transform.up, targetAstronautPosition - controller.transform.position).normalized;

        Vector3 directionAlien = -5 * (valueAlien * closestAlienPosition);
        Vector3 directionAstronaut = 3 * targetAstronautPosition;
        Vector3 directionPerpendicular = 7 * (valueSuccess * perpendicularDirection);

        Vector3 destination = directionAlien + directionAstronaut + directionPerpendicular;

        if (distanceToAstronaut >= 12f)
        {
            controller.UpdateTrajectoryDirection(destination);
            controller.SetMove(true);
            controller.Move();
        }
        else
        {
            controller.UpdateTrajectoryDirection(-directionAstronaut);
            controller.SetMove(true);
            controller.Move();
        }
    }

    private void DoSomething(AlienController controller)
    {
        int state = GetAlienState(controller);
        switch(state)
        {
            case 0:
                //Hacer función que intente mantener la distancia con los astronautas y dispare
                List<Vector3> directionOfRandomAstronaut = controller.GetDirectionOfRandomAstronaut(astronautControllers);
                List<Vector3> directionOfClosestAstronaut = controller.GetRealDirectionOfClosestAstronaut(astronautControllers);
                float probabilityOfHittingClosest = 0.35f;
                float probability = Random.Range(0f, 1f);
                List<Vector3> directionOfTargetAstronaut = (probability <= probabilityOfHittingClosest) ? directionOfClosestAstronaut : directionOfRandomAstronaut;
                //if (controller.id == 3)
                //{
                    ThrowBall(controller, directionOfTargetAstronaut);
                    UpdateAlienTrajectory(controller, directionOfTargetAstronaut);
                //}
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
