using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAliens {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private GameObject gameOverMenu;

    private float distanceToDefender = 5f;
    private float separationBetweenDefensors = 1f;
    private Dictionary<int, int> defenderAstronaut;    //Key defends Value

    public AttackAliens(List<PlayerController> astronautControllers, List<AlienController> alienControllers, GameObject gameOverMenu)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
        this.gameOverMenu = gameOverMenu;

        defenderAstronaut = new Dictionary<int, int>();
        foreach (PlayerController defender in astronautControllers)
        {
            defenderAstronaut.Add(defender.id, -1); //Indicando que no defiende a nadie
        }
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

    private bool AllPlayerDead()
    {
        bool allDead = true;

        foreach(PlayerController controller in astronautControllers)
        {
            if(controller.GetWeapon() == "sword" && !controller.isDead())
            {
                allDead = false;
            }
        }
        return allDead;
    }

    private PlayerController UpdateDefender(PlayerController controller)
    {
        //Comprobar que controller tenga defender. Si no es así, asignarle uno aleatorio
        if(defenderAstronaut[controller.id] == -1)
        {
            if(AllPlayerDead())
            {
                return astronautControllers[0];
            }
            int randomId = Random.Range(0, 8);
            PlayerController randomAstronaut = astronautControllers[randomId];
            while(randomAstronaut.GetWeapon() != "sword" || randomAstronaut.isDead())
            {
                randomId = Random.Range(0, 8);
                randomAstronaut = astronautControllers[randomId];
            }

            defenderAstronaut[controller.id] = randomAstronaut.id;
            return randomAstronaut;
        }
        else
        {
            int defenderId = defenderAstronaut[controller.id];
            foreach(PlayerController player in astronautControllers)
            {
                if(player.id == defenderId)
                {
                    if(player.isDead())
                    {
                        defenderAstronaut[controller.id] = -1;
                        return UpdateDefender(controller);
                    }
                    else
                    {
                        return player;
                    }
                }
            }
            return null;    //Never should happen
        }
    }

    private void GameOver()
    {
        this.gameOverMenu.SetActive(true);
        Time.timeScale = 0f;    //Freeze game
        PauseMenu.GamePaused = true;
    }

    public void UpdateAstronauts()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            //MoveRandomly(controller);
            if(controller.GetWeapon() == "shield")
            {
                //DEFENDERS
                PlayerController defender = UpdateDefender(controller);
                if (AllPlayerDead()) {
                    controller.Stop();
                    GameOver();
                }
                else
                {
                    Defend(controller, defender);
                }
            }
            else
            {
                //ATTACKERS
                controller.Stop();
            }
        }
    }
}
