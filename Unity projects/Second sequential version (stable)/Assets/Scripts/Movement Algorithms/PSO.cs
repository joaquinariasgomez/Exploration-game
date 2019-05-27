using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;
    private GameObject progressBar;
    private GameObject StopExploringButton;

    //Astronauts
    public float globalBestScore_astronaut;
    public Vector3 globalBestPosition_astronaut;

    //Aliens
    public float globalBestScore_alien;
    public Vector3 globalBestPosition_alien;

    //Astronauts
    private float Wmin_astronaut = 1;     //1
    private float Wmax_astronaut;  //2000
    private float Wcurrent_astronaut;
    private float c1_astronaut = 1;    //1.5
    private float c2_astronaut = 1.5f;       //2

    private float caida_astronaut;

    //Tiempo que van a estar parados antes de ir a la máxima montaña
    private float maxTimeStopped = 0.25f;
    private float timeStopped = 0f;

    //Aliens
    private float Wmin_alien = 1;     //1
    private float Wmax_alien;  //2000
    private float Wcurrent_alien;
    private float c1_alien = 1;    //1.5
    private float c2_alien = 1.5f;       //2

    private float caida_alien;

    private int gridSize = DataBetweenScenes.getSize();

    //Astronauts variables
    private int iteration_astronaut = 1;
    private int maxIterations_astronaut = 1000;   //3000
    private int maxIterWithWmin_astronaut = 50;  //400

    private bool stopExploring_astronaut = false;
    private bool signalSent_astronaut = false;

    //Aliens variables
    private int iteration_alien = 1;
    private int maxIterations_alien = 1000;   //3000
    private int maxIterWithWmin_alien = 5;  //400

    private Vector3 positionCloseToAstronauts = Vector3.zero;

    private bool stopExploring_alien = false;
    private bool signalSent_alien = false;
    //private float timeUpdatingAlien = 0f;

    //LOGS
    FileWriter globalBestScoreLogs_astronaut;
    FileWriter testLog_astronaut;

    FileWriter globalBestScoreLogs_alien;
    FileWriter testLog_alien;
    //END LOGS

    public PSO(List<PlayerController> astronautControllers, GameObject progressBar, GameObject StopExploringButton)
    {
        this.astronautControllers = astronautControllers;
        this.progressBar = progressBar;
        this.StopExploringButton = StopExploringButton;
        
        globalBestScore_astronaut = gridSize / 2f;  //Minimum best score
        globalBestPosition_astronaut = Vector3.zero;

        //FileWriter
        globalBestScoreLogs_astronaut = new FileWriter("Assets/Logs/GlobalAstronautBestScore.txt");
        testLog_astronaut = new FileWriter("Assets/Logs/AstronautTestLog.txt");
    }

    public PSO(List<AlienController> alienControllers)
    {
        this.alienControllers = alienControllers;

        globalBestScore_alien = gridSize * 4;  //Minimum best score
        globalBestPosition_alien = Vector3.zero;

        //FileWriter
        globalBestScoreLogs_alien = new FileWriter("Assets/Logs/GlobalAlienBestScore.txt");
        testLog_alien = new FileWriter("Assets/Logs/AlienTestLog.txt");
    }

    public void SetInertiaAstronaut(float inertia)
    {
        float maxInertia = 500;
        switch (gridSize)
        {
            case 100: maxInertia = 500; maxIterations_astronaut = 1500; break;
            case 200: maxInertia = 1000; maxIterations_astronaut = 1750;  break;
            case 400: maxInertia = 4000; maxIterations_astronaut = 2000; break;
        }
        float minInertia = 4;
        Wmax_astronaut = minInertia + (maxInertia - minInertia) * inertia;

        this.Wcurrent_astronaut = Wmax_astronaut;
        this.caida_astronaut = (Wmax_astronaut - Wmin_astronaut) / maxIterations_astronaut;
    }

    public void SetInertiaAlien(float inertia)
    {
        float maxInertia = 500;
        switch (gridSize)
        {
            case 100: maxInertia = 50; maxIterations_alien = 1500; break;
            case 200: maxInertia = 10; maxIterations_alien = 1750; break;
            case 400: maxInertia = 40; maxIterations_alien = 2000; break;
        }
        float minInertia = 2;
        Wmax_alien = minInertia + (maxInertia - minInertia) * inertia;

        this.Wcurrent_alien = Wmax_alien;
        this.caida_alien = (Wmax_alien - Wmin_alien) / maxIterations_alien;
    }

    private void UpdateGlobalScoreAstronaut()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            if (controller.personalBestScore > globalBestScore_astronaut)
            {
                globalBestScore_astronaut = controller.personalBestScore;
                globalBestPosition_astronaut = controller.personalBestPosition;
            }
            controller.UpdateGlobalScore(globalBestScore_astronaut, globalBestPosition_astronaut);
        }
        //Logs
        globalBestScoreLogs_astronaut.Write(globalBestScore_astronaut);
    }

    private void UpdateGlobalScoreAlien()
    {
        foreach (AlienController controller in alienControllers)
        {
            if (controller.personalBestScore < globalBestScore_alien)
            {
                globalBestScore_alien = controller.personalBestScore;
                globalBestPosition_alien = controller.personalBestPosition;
            }
            controller.UpdateGlobalScore(globalBestScore_alien, globalBestPosition_alien);
        }
        //Logs
        //globalBestScoreLogs_alien.Write(globalBestScore_astronaut);
    }

    private void UpdateTrajectoryAstronaut(float Wcurrent, float c1, float c2, bool goToGlobalMax = false)
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.UpdateTrajectory(Wcurrent, c1, c2, goToGlobalMax);
        }
    }

    private void UpdateTrajectoryAlien(float Wcurrent, float c1, float c2, bool goToGlobalMax = false)
    {
        foreach (AlienController controller in alienControllers)
        {
            controller.UpdateTrajectory(Wcurrent, c1, c2, goToGlobalMax);
        }
    }

    private void UpdateWeightsAstronaut()
    {
        if(Wcurrent_astronaut > Wmin_astronaut)
        {
            Wcurrent_astronaut -= this.caida_astronaut;
            if(Wcurrent_astronaut < Wmin_astronaut)
            {
                Wcurrent_astronaut = Wmin_astronaut;
            }
        }
        //TestLog
        testLog_astronaut.Write(Wcurrent_astronaut);
    }

    private void UpdateWeightsAlien()
    {
        if (Wcurrent_alien > Wmin_alien)
        {
            Wcurrent_alien -= this.caida_alien;
            if (Wcurrent_alien < Wmin_alien)
            {
                Wcurrent_alien = Wmin_alien;
            }
        }
        //TestLog
        //testLog_alien.Write(Wcurrent);
    }


    private void Stop()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.Stop();
        }
        //Logs
        globalBestScoreLogs_astronaut.End();
        testLog_astronaut.End();
    }

    private void GoToGlobalMaxAstronaut()
    {
        if(timeStopped < maxTimeStopped)
        {
            foreach (PlayerController controller in astronautControllers)
            {
                controller.SetBestPosition();
                controller.Stop();
            }
            timeStopped += Time.deltaTime;
            return;
        }
        foreach (PlayerController controller in astronautControllers)
        {
            controller.SetBestPosition();
            controller.Move();
            controller.UpdatePersonalScore();
        }
        UpdateTrajectoryAstronaut(Wcurrent_astronaut, c1_astronaut, c2_astronaut, true);
    }

    private void CheckGlobalMaxAstronaut()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            if(controller.HasReachedHighestMountain())
            {
                controller.Stop();
            }
            else
            {
                controller.SetMove(true);
            }
        }
    }

    /*private bool IsCloseEnoughToAstronauts()
    {
        bool result = true;

        foreach(AlienController controller in alienControllers)
        {
            if(!controller.IsCloseEnoughToAstronauts())
            {
                result = false;
                controller.SetMove(true);
            }
            else
            {
                controller.Stop();
            }
        }
        return result;
    }*/

    private Vector3 OneIsCloseEnoughToAstronauts()  //Devuelve la posicion a la que tendrán que acercarse los demás astronuatas
    {
        foreach (AlienController controller in alienControllers)
        {
            if (controller.IsCloseEnoughToAstronauts())
            {
                controller.Stop();
                return controller.transform.position;
            }
        }
        return Vector3.zero;
    }

    private bool AllCloseEnoughToPosition(Vector3 position)
    {
        bool condition = true;

        foreach(AlienController controller in alienControllers)
        {
            if(Vector3.Distance(controller.transform.position, position) > 4f)
            {
                controller.UpdateTrajectoryDirection(position - controller.transform.position);
                controller.SetMove(true);
                controller.Move();
                condition = false;
            }
            else
            {
                //Esta cerca a position
                controller.Stop();
            }
        }
        return condition;
    }

    private bool AllReachedHighestMountain()
    {
        bool condition = true;

        foreach(PlayerController controller in astronautControllers)
        {
            if(!controller.HasReachedHighestMountain())
            {
                condition = false;
            }
        }
        return condition;
    }

    private void SendSignalReachedHighestMountain()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.SetReachedHighestMountain();
        }
    }

    public Vector3 GetTargetCoordinates()
    {
        Vector3 target = Vector3.zero;

        foreach (PlayerController controller in astronautControllers)
        {
            target += controller.GetCoordinates();
        }
        target = target / 8f;
        return target;
    }

    private bool AllWeaponsAssigned()
    {
        bool value = true;
        foreach (PlayerController controller in astronautControllers)
        {
            if (!controller.weaponAssigned()) value = false;
        }
        return value;
    }

    public void StopExploringAstronaut()
    {
        stopExploring_astronaut = true;
    }

    public void StopExploringAlien()
    {
        stopExploring_alien = true;
    }

    private void UpdateProgressBar()
    {
        float progressPercentage = (float)iteration_astronaut / (float)(maxIterations_astronaut + maxIterWithWmin_astronaut);
        progressBar.GetComponent<ProgressBar>().SetProgress(progressPercentage);
    }

    //Main loop for astroanuts
    public bool UpdateAstronauts()
    {
        UpdateProgressBar();
        if(iteration_astronaut == (maxIterations_astronaut + maxIterWithWmin_astronaut) || stopExploring_astronaut)
        {
            StopExploringButton.SetActive(false);   //Delete stopExploringButton
            progressBar.SetActive(false);    //Delete progress bar
            GoToGlobalMaxAstronaut();
            CheckGlobalMaxAstronaut();   //Will stop player is has reached mountain
            if(AllReachedHighestMountain())
            {
                if(!signalSent_astronaut)
                {
                    SendSignalReachedHighestMountain();
                    signalSent_astronaut = true;
                }   
            }
            //Check if all astronauts have been assigned with weapon
            bool weaponsAssigned = AllWeaponsAssigned();
            if(weaponsAssigned)
            {
                return true;
            }
        }
        else
        {
            foreach (PlayerController controller in astronautControllers)
            {
                controller.Move();
                controller.UpdatePersonalScore();
            }
            UpdateGlobalScoreAstronaut();
            UpdateTrajectoryAstronaut(Wcurrent_astronaut, c1_astronaut, c2_astronaut);
            UpdateWeightsAstronaut();
            ++iteration_astronaut;
        }
        return false;
    }

    //Main loop for aliens
    public bool UpdateAliens()
    {
        if(positionCloseToAstronauts == Vector3.zero)
        {
            positionCloseToAstronauts = OneIsCloseEnoughToAstronauts();
        }
        
        if (positionCloseToAstronauts != Vector3.zero)
        {
            if(AllCloseEnoughToPosition(positionCloseToAstronauts))
            {
                return true;
            }
        }
        else
        {
            foreach (AlienController controller in alienControllers)
            {
                controller.Move();
                controller.UpdatePersonalScore();
            }
            UpdateGlobalScoreAlien();
            UpdateTrajectoryAlien(Wcurrent_alien, c1_alien, c2_alien);
            UpdateWeightsAlien();
            ++iteration_alien;
        }
        return false;
    }
}
