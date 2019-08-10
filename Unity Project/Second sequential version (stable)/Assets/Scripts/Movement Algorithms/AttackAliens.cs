using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAliens {

    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private GameObject gameOverMenu;
    private GameObject missionSuccessMenu;
    private AlienManager alienManager;

    private float distanceToDefender = 5.5f;
    private float separationBetweenDefensors = 1f;
    private Dictionary<int, int> defenderAstronaut;    //Key defends Value
    private Dictionary<int, int> astronautAttacksAlien;    //Key attacks Value
    private Dictionary<AlienController, int> defensorBall;   //Defensor goes for ball
    private float minDistanceToAttackAlien = 2f;
    private List<float> timeSinceAttack = new List<float>();
    private float minTimeToAttack = 0.6f;

    public AttackAliens(List<PlayerController> astronautControllers, List<AlienController> alienControllers, GameObject gameOverMenu, GameObject missionSuccessMenu, AlienManager alienManager)
    {
        this.astronautControllers = astronautControllers;
        this.alienControllers = alienControllers;
        this.gameOverMenu = gameOverMenu;
        this.missionSuccessMenu = missionSuccessMenu;
        this.alienManager = alienManager;

        defenderAstronaut = new Dictionary<int, int>();
        astronautAttacksAlien = new Dictionary<int, int>();
        defensorBall = new Dictionary<AlienController, int>();

        for(int i = 0; i<8; i++)
        {
            timeSinceAttack.Add(0f);
        }

        foreach (PlayerController defender in astronautControllers)
        {
            defenderAstronaut.Add(defender.id, -1); //Indicando que no defiende a nadie
        }

        foreach (PlayerController controller in astronautControllers)
        {
            astronautAttacksAlien.Add(controller.id, -1); //Indicando que no ataca a nadie
        }

        foreach (AlienController alien in alienControllers)
        {
            defensorBall.Add(alien, -1);
        }
    }

    private bool AlienNotFound(int alienId)
    {
        foreach(PlayerController controller in astronautControllers)
        {
            if(astronautAttacksAlien[controller.id] == alienId)
            {
                return false;
            }
        }
        return true;
    }

    private int GetClosestAlienId(PlayerController controller)
    {
        int alienToAttackId = -1;

        foreach (AlienController alien in alienControllers)
        {
            float minDistance = 100000f;

            if (!alien.isDead())
            {
                float distance = Vector3.Distance(alien.transform.position, controller.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    alienToAttackId = alien.id;
                }
            }
        }

        return alienToAttackId;
    }

    private AlienController GetAlienToAttack(PlayerController controller)
    {
        //This will select the closest alien to the astronaut
        float minDistance = 100000f;
        int alienToAttackId = -1;

        //Check if assigned Alien is dead, then deassign that alien
        if(astronautAttacksAlien[controller.id] != -1)
        {
            AlienController alienTest = GetAlienById(astronautAttacksAlien[controller.id]);
            if(alienTest.isDead())
            {
                astronautAttacksAlien[controller.id] = -1;
            }
        }

        foreach (AlienController alien in alienControllers)
        {
            if(!alien.isDead())
            {
                if(AlienNotFound(alien.id))    //Ningún astronauta está atacando a este alien
                {
                    float distance = Vector3.Distance(alien.transform.position, controller.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        alienToAttackId = alien.id;
                    }
                }
            }
        }

        if(astronautAttacksAlien[controller.id] != -1)
        {
            alienToAttackId = astronautAttacksAlien[controller.id];
        }
        else
        {
            if(alienToAttackId != -1)
            {
                astronautAttacksAlien[controller.id] = alienToAttackId;
            }
            else
            {
                //Encontrar el alien más cercano para asignarselo a este astronauta
                alienToAttackId = GetClosestAlienId(controller);
                if(alienToAttackId != -1)
                {
                    astronautAttacksAlien[controller.id] = alienToAttackId;
                }
            }
        }

        return GetAlienById(alienToAttackId);
    }

    private AlienController GetAlienById(int id)
    {
        AlienController alienController = null;

        foreach(AlienController alien in alienControllers)
        {
            if(alien.id == id)
            {
                alienController = alien;
            }
        }
        return alienController;
    }

    private void Attack(PlayerController controller, AlienController alien)
    {
        if (alien == null) return;

        Vector3 attackerToAlien = alien.transform.position - controller.transform.position;
        float distanceToAlien = Vector3.Distance(controller.transform.position, alien.transform.position);
        if(distanceToAlien <= minDistanceToAttackAlien)
        {
            //Attack
            timeSinceAttack[controller.id] += Time.deltaTime;
            if(timeSinceAttack[controller.id] > minTimeToAttack)
            {
                timeSinceAttack[controller.id] = 0f;
                alien.Hit();
            }
        }
        else
        {
            //Move to alien
            Vector3 direction = controller.transform.position + attackerToAlien;
            controller.UpdateTrajectoryDirection(direction);
            controller.SetMove(true);
            controller.Move();
        }   
    }

    private void Defend(PlayerController controller, PlayerController defender)   //Controller will defend defender
    {
        //Search for closest ball to defender which is not registered by controller
        float minimumDistanceToBall = 1000f;
        Vector3 positionOfAlien = Vector3.zero;
        AlienController closestBall = null;

        foreach(KeyValuePair<AlienController, int> element in defensorBall)
        {
            if(element.Value == controller.id)
            {
                positionOfAlien = element.Key.transform.position;
            }
        }

        foreach (AlienController alien in alienControllers) //Itera sobre las bolas
        {
            if(defensorBall[alien] == -1 && alien.ball.activeInHierarchy)
            {
                float distanceToBall = Vector3.Distance(defender.transform.position, alien.ball.transform.position);
                if (distanceToBall < minimumDistanceToBall)
                {
                    closestBall = alien;
                    minimumDistanceToBall = distanceToBall;
                    positionOfAlien = alien.transform.position;
                }
            }
        }

        if(closestBall != null)
        {
            defensorBall[closestBall] = controller.id;
        }

        Vector3 defenderToAlien = positionOfAlien - defender.transform.position;
        float multiplier = distanceToDefender / defenderToAlien.magnitude;

        Vector3 direction = defender.transform.position + defenderToAlien * multiplier;

        float minimumDistanceToDefender = 1000f;
        Vector3 defenderAway = Vector3.zero;
        foreach (PlayerController player in astronautControllers)
        {
            if (player.GetWeapon() == "shield" && player.id != controller.id)
            {
                float distance = Vector3.Distance(player.transform.position, controller.transform.position);
                if (distance < minimumDistanceToDefender)
                {
                    minimumDistanceToDefender = distance;
                    defenderAway = (controller.transform.position - player.transform.position).normalized * 2f;
                }
            }
        }

        if (minimumDistanceToDefender < separationBetweenDefensors)
        {
            direction += defenderAway;
        }

        controller.UpdateTrajectoryDirection(direction);
        controller.SetMove(true);
        controller.Move();
    }

    /*private void Defend(PlayerController controller, PlayerController defender)   //Controller will defend defender
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
    }*/

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

    private bool AllAlienDead()
    {
        bool allDead = true;

        foreach (AlienController controller in alienControllers)
        {
            if (!controller.isDead())
            {
                allDead = false;
            }
        }
        return allDead;
    }

    private int GetNumberOfPlayersDefending(int defensorId)
    {
        int counter = 0;
        int defended = defenderAstronaut[defensorId];

        foreach(PlayerController controller in astronautControllers)
        {
            if(controller.GetWeapon() == "shield")
            {
                if(defenderAstronaut[controller.id] == defended)
                {
                    counter++;
                }
            }
        }
        return counter;
    }

    private void CheckForAstronautsToDefend(PlayerController controller)    //Itera sobre los astronautas y dirá cuales estan solicitando ayuda
    {
        PlayerController playerToDefend = null;
        foreach (PlayerController player in astronautControllers)
        {
            if (player.GetDefendThis())
            {
                playerToDefend = player;
            }
        }
        if (playerToDefend == null) return;
        //Si todos los defensores estan defendiendo a playerToDefend, settearlo a false
        bool condition = true;
        foreach (PlayerController player in astronautControllers)
        {
            if (player.GetWeapon() == "shield")
            {
                if (defenderAstronaut[player.id] != playerToDefend.id)
                {
                    condition = false;
                }
            }
        }
        if (condition)
        {
            playerToDefend.SetDefendThis(false);
            return;
        }
        if (defenderAstronaut[controller.id] == playerToDefend.id) return;

        defenderAstronaut[controller.id] = playerToDefend.id;
        playerToDefend.SetDefendThis(false);
    }

    private PlayerController UpdateDefender(PlayerController controller, bool condition)
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
            if(condition) CheckForAstronautsToDefend(controller);   //Will update defenderAstronaut[controller.id] if needed
            int defenderId = defenderAstronaut[controller.id];
            foreach(PlayerController player in astronautControllers)
            {
                if(player.id == defenderId)
                {
                    if(player.isDead())
                    {
                        defenderAstronaut[controller.id] = -1;
                        return UpdateDefender(controller, false);
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

    public int GetBestAstronautToLeave()
    {
        int maxNumberOfPlayersDefending = 0;
        int selectedId = 0;
        foreach (PlayerController controller in astronautControllers)
        {
            if (controller.GetWeapon() == "shield")
            {
                int numberOfPlayersDefending = GetNumberOfPlayersDefending(controller.id);
                if (numberOfPlayersDefending > maxNumberOfPlayersDefending)
                {
                    maxNumberOfPlayersDefending = numberOfPlayersDefending;
                    selectedId = controller.id;
                }
            }
        }
        return selectedId;
    }

    private void GameOver()
    {
        PauseMenu.GamePaused = true;
        foreach (PlayerController controller in astronautControllers)
        {
            controller.Stop();
        }
        foreach (AlienController controller in alienControllers)
        {
            controller.Stop();
        }
        this.gameOverMenu.SetActive(true);
        Time.timeScale = 0f;    //Freeze game
    }

    private void MissionSuccess()
    {
        PauseMenu.GamePaused = true;
        foreach (PlayerController controller in astronautControllers)
        {
            controller.Stop();
        }
        foreach (AlienController controller in alienControllers)
        {
            controller.Stop();
        }
        this.missionSuccessMenu.SetActive(true);
        Time.timeScale = 0f;    //Freeze game
        
    }

    private bool AliensStartedAttack()
    {
        return !alienManager.startPSO;
    }

    public void UpdateAstronauts()
    {
        if (!AliensStartedAttack())
        {
            foreach(PlayerController controller in astronautControllers)
            {
                controller.Stop();
            }
            return;
        }

        //CHECK GAME OVER OR PASSED
        if (AllPlayerDead())
        {
            GameOver();
        }
        if (AllAlienDead())
        {
            MissionSuccess(); //Should be Game passed or smthing
        }

        //int bestIdToLeave = GetBestAstronautToLeave();
        foreach (PlayerController controller in astronautControllers)
        {
            if (controller.GetWeapon() == "shield")
            {
                //DEFENDERS
                PlayerController defender = UpdateDefender(controller, true);
                Defend(controller, defender);
            }
            else
            {
                //ATTACKERS
                controller.PerformJustGravity();

                AlienController alien = GetAlienToAttack(controller);
                Attack(controller, alien);
            }
        }
    }
}
