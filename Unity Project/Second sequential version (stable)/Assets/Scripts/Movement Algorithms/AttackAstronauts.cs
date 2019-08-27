using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAstronauts {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private int maxNumberOfSimultaneousAliensGoAway;   //Número máximo de aliens huyendo al mismo tiempo
    private int maxNumberOfSimultaneousAliensHelp;   //Número máximo de aliens que van a ayudar al alien que huye
    private List<int> alienStates;  //0 -> atacando(idle).  1 -> huyendo.  2 -> supporteando al que huye
    private Dictionary<int, int> alienAstronautTarget = new Dictionary<int, int>();   //Only for states 2
    [HideInInspector]
    public Dictionary<int, int> alienAttacksAstronaut = new Dictionary<int, int>();
    private Dictionary<int, bool> hittedAstronaut = new Dictionary<int, bool>();

    private float minTimeBetweenShoots = 1f;  //4.5f
    private float timeBetweenShoots = 0f;

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
            alienAttacksAstronaut.Add(controller.id, -1);   //Indicando que no ataca a nadie
        }
        maxNumberOfSimultaneousAliensGoAway = 8;    //2
        maxNumberOfSimultaneousAliensHelp = 3;

        alienStates = new List<int>();
        for(int i=0; i<alienControllers.Capacity; i++)  //Set states of aliens to idle
        {
            alienStates.Add(0);
        }

        foreach(AlienController alien in alienControllers)
        {
            hittedAstronaut.Add(alien.id, false);
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

    private void SetAlienState(int alienId, int state, /*Vector3 astronautPosition*/PlayerController astronautToEscapeFrom) //directionOfEscape is for state 2
    {
        alienStates[alienId] = state;
        if(state == 2)
        {
            alienAstronautTarget[alienId] = astronautToEscapeFrom.id;
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

    private void KeepDistance(AlienController controller, PlayerController astronaut) //Intentar mantener 17f unidades de distancia
    {
        float astronautSpeed = astronaut.GetSpeed();
        //Update speed to match objective's speed
        controller.SetSpeed(astronautSpeed + 1f, false);
        //

        Vector3 targetAstronautPosition = astronaut.transform.position;
        Vector3 closestAlienPosition = GetClosestAlienPosition(controller);

        //Check successRate
        int result = HittedAstronaut(controller);

        if (result == 1 || result == 2)
        {
            if (result == 1)
            {
                controller.shootSuccesses.Enqueue(1);
            }
            if (result == 2)
            {
                controller.shootSuccesses.Enqueue(0);
            }
            controller.shootSuccesses.Dequeue();
        }
        //End check successRate
        float successRate = GetSuccessRate(controller);
        float valueSuccess = 10f * (-successRate + 1);   //Cuanto menos success rate, más valor tiene (seguir camino contradictorio)
        if (successRate >= 0.9f) valueSuccess = 0f;
        if (successRate < 0.2f)
        {
            valueSuccess *= 5;
        }

        float distanceToAlien = Vector3.Distance(closestAlienPosition, controller.transform.position);
        float distanceBetweenAliens = 5f; //7f
        float valueAlien = -distanceToAlien + distanceBetweenAliens;
        if (distanceToAlien > distanceBetweenAliens) valueAlien = 0f;

        float distanceToAstronaut = Vector3.Distance(targetAstronautPosition, controller.transform.position);

        Vector3 perpendicularDirection = -Vector3.Cross(controller.transform.up, targetAstronautPosition - controller.transform.position).normalized;

        Vector3 directionAlien = -3 * (valueAlien * closestAlienPosition);
        Vector3 directionAstronaut = 3 * targetAstronautPosition;
        //Cuando más lejos del alien, menos valor tendrá directionPerpendicular
        float perpendicularMultiplier = 21f / distanceToAstronaut;
        Vector3 directionPerpendicular = perpendicularMultiplier * (valueSuccess * perpendicularDirection);   //7 * 

        Vector3 destination = directionAstronaut + directionAlien + directionPerpendicular;   //DECOMMENT THIS
        //Vector3 destination = directionAstronaut;

        if (distanceToAstronaut >= 7.5f)
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

    /*private void KeepDistance(AlienController controller, PlayerController astronaut) //Intentar mantener 17f unidades de distancia
    {
        float astronautSpeed = astronaut.GetSpeed();
        //Update speed to match objective's speed
        controller.SetSpeed(astronautSpeed + 0.5f, false);
        //

        Vector3 astronautPosition = astronaut.transform.position;

        Vector3 direction = (astronautPosition - controller.transform.position).normalized;

        controller.UpdateTrajectoryDirection(direction);

        float distance = Vector3.Distance(controller.transform.position, astronautPosition);

        if (distance < 7.5f)    //Porque si se aleja más de 20 no puede disparar
        {
            Vector3 direction2 = (controller.transform.position - astronautPosition).normalized;
            controller.UpdateTrajectoryDirection(direction2);
            controller.SetMove(true);
            controller.Move();
        }
        else
        {
            controller.SetMove(true);
            controller.Move();
        }
    }*/

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

    private void ThrowBall(AlienController controller, List<Vector3> direction, Vector3 predictionOfMovement)
    {
        if (controller.isDead()) return;    //Si está muerto, no puede disparar
        Vector3 from = direction[0];    //Posición del alien
        Vector3 to = direction[1];      //Posición del astronauta

        float distanceToObjective = Vector3.Magnitude(from - to);
        if (distanceToObjective > controller.GetMaxDistanceToShoot()) return;   //Disparar solo si están a menos de 20 unidades de distancia

        Vector3 directionToAstronaut = (from - to).normalized + predictionOfMovement.normalized * 0.2f;

        bool nowThrowing = controller.ball.GetComponent<ThrowBall>().NowThrowing();

        if(!nowThrowing)
        {
            if(controller.ball.GetComponent<ThrowBall>().hasHitAstronaut())
            {
                hittedAstronaut[controller.id] = true;
            }

            if(hittedAstronaut[controller.id])
            {
                timeBetweenShoots += Time.deltaTime;
                if(timeBetweenShoots >= minTimeBetweenShoots)
                {
                    timeBetweenShoots = 0f;
                    hittedAstronaut[controller.id] = false;

                    Vector3 positionFromShoot = controller.transform.position;
                    float distance = Vector3.Distance(positionFromShoot, Vector3.zero);
                    float targetDistace = distance + 0.6f;
                    Vector3 newPositionFromShoot = positionFromShoot * (targetDistace / distance);
                    controller.ball.GetComponent<ThrowBall>().Throw(newPositionFromShoot, directionToAstronaut);
                }
            }
            else
            {
                Vector3 positionFromShoot = controller.transform.position;
                float distance = Vector3.Distance(positionFromShoot, Vector3.zero);
                float targetDistace = distance + 0.6f;
                Vector3 newPositionFromShoot = positionFromShoot * (targetDistace / distance);
                controller.ball.GetComponent<ThrowBall>().Throw(newPositionFromShoot, directionToAstronaut);
            }
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
        float distanceBetweenAliens = 5f; //7f
        float valueAlien = -distanceToAlien + distanceBetweenAliens;
        if (distanceToAlien > distanceBetweenAliens) valueAlien = 0f;

        float distanceToAstronaut = Vector3.Distance(targetAstronautPosition, controller.transform.position);

        Vector3 perpendicularDirection = -Vector3.Cross(controller.transform.up, targetAstronautPosition - controller.transform.position).normalized;

        Vector3 directionAlien = -3 * (valueAlien * closestAlienPosition);
        Vector3 directionAstronaut = 3 * targetAstronautPosition;
        //Cuando más lejos del alien, menos valor tendrá directionPerpendicular
        float perpendicularMultiplier = 21f / distanceToAstronaut;
        Vector3 directionPerpendicular = perpendicularMultiplier * (valueSuccess * perpendicularDirection);   //7 * 

        Vector3 destination = directionAstronaut + directionAlien + directionPerpendicular;   //DECOMMENT THIS
        //Vector3 destination = directionAstronaut;

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
        //state = 0;

        switch(state)
        {
            case 0: //Atacando (idle)
                    //Hacer función que intente mantener la distancia con los astronautas y dispare
                List<Vector3> directionOfTargetAstronaut = new List<Vector3>();
                int targetAstronautId = 0;
                if (alienAttacksAstronaut[controller.id] != -1)
                {
                    targetAstronautId = alienAttacksAstronaut[controller.id];
                    if(astronautControllers[targetAstronautId].isDead())
                    {
                        alienAttacksAstronaut[controller.id] = -1;
                        DoSomething(controller);
                    }
                }
                else
                {
                    int randomAstronautId = controller.GetDirectionOfRandomAstronaut(astronautControllers);
                    int closestAstronautId = controller.GetRealDirectionOfClosestAstronaut(astronautControllers);

                    float probabilityOfChoosingClosest = 0.7f;
                    float probability = Random.Range(0f, 1f);
                    targetAstronautId = (probability <= probabilityOfChoosingClosest) ? closestAstronautId : randomAstronautId;
                    alienAttacksAstronaut[controller.id] = targetAstronautId;
                }

                directionOfTargetAstronaut.Add(astronautControllers[targetAstronautId].transform.position);
                directionOfTargetAstronaut.Add(controller.transform.position);

                Vector3 predictionOfMovement = Vector3.zero;
                if (astronautControllers[targetAstronautId].move)
                {
                    predictionOfMovement = astronautControllers[targetAstronautId].trajectoryDirection.normalized * 2;
                }

                ThrowBall(controller, directionOfTargetAstronaut, predictionOfMovement);
                UpdateAlienTrajectory(controller, directionOfTargetAstronaut);
                break;
            case 1: //Huyendo
                List<Vector3> directionOfEscape = new List<Vector3>();
                PlayerController astronautToEscapeFrom = controller.GetAstronautToEscapeFrom();
                alienAttacksAstronaut[controller.id] = astronautToEscapeFrom.id;
                directionOfEscape.Add(astronautToEscapeFrom.transform.position);
                directionOfEscape.Add(controller.transform.position);

                predictionOfMovement = Vector3.zero;
                if (astronautToEscapeFrom.move)
                {
                    predictionOfMovement = astronautToEscapeFrom.trajectoryDirection.normalized * 2;
                }

                ThrowBall(controller, directionOfEscape, predictionOfMovement);
                MoveAway(controller, directionOfEscape);
                break;
            case 2: //Supporteando al que huye
                int idFrom = alienAstronautTarget[controller.id];
                if(astronautControllers[idFrom].isDead())
                {
                    SetAlienState(controller.id, 0, null);
                    DoSomething(controller);
                }

                alienAttacksAstronaut[controller.id] = idFrom;

                PlayerController from = astronautControllers[idFrom];//astronautControllers[idFrom].transform.position;
                Vector3 to = controller.transform.position;
                List<Vector3> directionToAstronaut = new List<Vector3>();

                predictionOfMovement = Vector3.zero;
                if(from.move)
                {
                    predictionOfMovement = from.trajectoryDirection.normalized * 2;
                }

                directionToAstronaut.Add(from.transform.position);
                directionToAstronaut.Add(to);

                ThrowBall(controller, directionToAstronaut, predictionOfMovement);
                KeepDistance(controller, from); //Mantener la distancia contra el astronauta que hace bullying
                controller.SetSpeed(0f, true);  //Reset Speed
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
        /*
        foreach (AlienController controller in alienControllers)
        {
            DrawStateIcon(controller);
        }

        AlienController runningAwayAlien = null;
        foreach(AlienController controller in alienControllers)
        {
            if(!controller.isDead())
            {
                if (controller.CheckDistanceWithAstronauts(astronautControllers))    //True if this alien is too close to astronauts with swords
                {
                    if (GetNumberOfSimultaneousAliensGoAway() <= maxNumberOfSimultaneousAliensGoAway - 1)
                    {
                        SetAlienState(controller.id, 1, null);    //Set this alien state to running away
                        runningAwayAlien = controller;
                    }
                }
                else
                {
                    SetAlienState(controller.id, 0, null);
                }
            }
        }

        if (runningAwayAlien != null)
        {
            int numberOfSimultaneousAliensHelp = 0;
            foreach (AlienController controller in alienControllers)     //Warn other aliens about state 1 aliens, and override their state
            {
                if (runningAwayAlien.id != controller.id && (numberOfSimultaneousAliensHelp <= maxNumberOfSimultaneousAliensHelp - 1) && !controller.isDead())
                {
                    ++numberOfSimultaneousAliensHelp;
                    PlayerController astronautToEscapeFrom = runningAwayAlien.GetAstronautToEscapeFrom();
                    if (!astronautToEscapeFrom.isDead())
                    {
                        SetAlienState(controller.id, 2, astronautToEscapeFrom);
                    }
                    else
                    {
                        SetAlienState(controller.id, 0, null);
                    }
                }
            }
        }
        else
        {
            foreach(AlienController controller in alienControllers)
            {
                SetAlienState(controller.id, 0, null);
            }
        }

        foreach (AlienController controller in alienControllers)
        {
            DoSomething(controller);
        }*/

        
        foreach(AlienController controller in alienControllers)
        {
            //Draw State Icon
            DrawStateIcon(controller);
            //End Draw State Icon
            
            if (controller.CheckDistanceWithAstronauts(astronautControllers))    //True if this alien is too close to astronauts with swords
            {
                if(GetNumberOfSimultaneousAliensGoAway() <= maxNumberOfSimultaneousAliensGoAway - 1)
                {
                    SetAlienState(controller.id, 1, null);    //Set this alien state to running away
                    //Warn  other astronauts that don't have state 1
                    int numberOfSimultaneousAliensHelp = 0;
                    foreach (AlienController controller2 in alienControllers)
                    {
                        if(GetAlienState(controller2) != 1 && numberOfSimultaneousAliensHelp <= maxNumberOfSimultaneousAliensHelp - 1)
                        {
                            ++numberOfSimultaneousAliensHelp;
                            //Vector3 astronautPosition = controller.GetDirectionOfEscape()[0];
                            PlayerController astronautToEscapeFrom = controller.GetAstronautToEscapeFrom();
                            if(!astronautToEscapeFrom.isDead())
                            {
                                SetAlienState(controller2.id, 2, astronautToEscapeFrom);
                            }
                            else
                            {
                                SetAlienState(controller2.id, 0, null);
                            }
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
                        SetAlienState(controller.id, 0, null);
                    }
                }
                else
                {
                    SetAlienState(controller.id, 0, null);
                }
            }

            //Check states and act
            DoSomething(controller);
        }
    }
}
