using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
    private List<PlayerController> astronautControllers;
    private List<AlienController> alienControllers;

    //Astronauts
    public float globalBestScore_astronaut;
    public Vector3 globalBestPosition_astronaut;

    //Aliens
    public float globalBestScore_alien;
    public Vector3 globalBestPosition_alien;

    private float Wmin = 1;     //1
    private float Wmax;  //2000
    private float Wcurrent;
    private float c1 = 1;    //1.5
    private float c2 = 1.5f;       //2

    private float caida;

    private int gridSize = DataBetweenScenes.getSize();

    //Astronauts variables
    private int iteration_astronaut = 1;
    private int maxIterations_astronaut = 1000;   //3000
    private int maxIterWithWmin_astronaut = 5;  //400

    private bool stopExploring_astronaut = false;
    private bool signalSent_astronaut = false;

    //Aliens variables
    private int iteration_alien = 1;
    private int maxIterations_alien = 1000;   //3000
    private int maxIterWithWmin_alien = 5;  //400

    private bool stopExploring_alien = false;
    private bool signalSent_alien = false;

    //LOGS
    FileWriter globalBestScoreLogs;
    FileWriter testLog;
    //END LOGS

    public PSO(List<PlayerController> astronautControllers)
    {
        this.astronautControllers = astronautControllers;
        
        globalBestScore_astronaut = gridSize / 2f;  //Minimum best score
        globalBestPosition_astronaut = Vector3.zero;

        //FileWriter
        globalBestScoreLogs = new FileWriter("Assets/Logs/GlobalAstronautBestScore.txt");
        testLog = new FileWriter("Assets/Logs/AstronautTestLog.txt");
    }

    public PSO(List<AlienController> alienControllers)
    {
        this.alienControllers = alienControllers;

        globalBestScore_alien = gridSize / 2f;  //Minimum best score
        globalBestPosition_alien = Vector3.zero;

        //FileWriter
        globalBestScoreLogs = new FileWriter("Assets/Logs/GlobalAlienBestScore.txt");
        testLog = new FileWriter("Assets/Logs/AlienTestLog.txt");
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
        float minInertia = 20;
        Wmax = minInertia + (maxInertia - minInertia) * inertia;

        this.Wcurrent = Wmax;
        this.caida = (Wmax - Wmin) / maxIterations_astronaut;
    }

    public void SetInertiaAlien(float inertia)
    {
        float maxInertia = 500;
        switch (gridSize)
        {
            case 100: maxInertia = 500; maxIterations_alien = 1500; break;
            case 200: maxInertia = 1000; maxIterations_alien = 1750; break;
            case 400: maxInertia = 4000; maxIterations_alien = 2000; break;
        }
        float minInertia = 20;
        Wmax = minInertia + (maxInertia - minInertia) * inertia;

        this.Wcurrent = Wmax;
        this.caida = (Wmax - Wmin) / maxIterations_alien;
    }

    private void UpdateGlobalScore()    //ACOPLADO A ASTRONAUT
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
        globalBestScoreLogs.Write(globalBestScore_astronaut);
    }

    private void UpdateTrajectory(float Wcurrent, float c1, float c2, bool goToGlobalMax = false)
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.UpdateTrajectory(Wcurrent, c1, c2, goToGlobalMax);
        }
    }

    private void UpdateWeights()
    {
        if(Wcurrent > Wmin)
        {
            Wcurrent -= this.caida;
            if(Wcurrent < Wmin)
            {
                Wcurrent = Wmin;
            }
        }
        //TestLog
        testLog.Write(Wcurrent);
    }

    private void Stop()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.Stop();
        }
        //Logs
        globalBestScoreLogs.End();
        testLog.End();
    }

    private void GoToGlobalMax()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.SetBestPosition();
            controller.Move();
            controller.UpdatePersonalScore();
        }
        UpdateTrajectory(Wcurrent, c1, c2, true);
    }

    private void CheckGlobalMax()
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

    //Main loop for astroanuts
    public bool UpdateAstronauts()
    {
        if(iteration_astronaut == (maxIterations_astronaut + maxIterWithWmin_astronaut) || stopExploring_astronaut)
        {
            GoToGlobalMax();
            CheckGlobalMax();   //Will stop player is has reached mountain
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
            UpdateGlobalScore();
            UpdateTrajectory(Wcurrent, c1, c2);
            UpdateWeights();
            ++iteration_astronaut;
        }
        return false;
    }

    //Main loop for aliens
    public bool UpdateAliens()
    {
        /*
        if (iteration == (maxIterations + maxIterWithWmin) || stopExploring)
        {
            GoToGlobalMax();
            CheckGlobalMax();   //Will stop player is has reached mountain
            if (AllReachedHighestMountain())
            {
                if (!signalSent)
                {
                    SendSignalReachedHighestMountain();
                    signalSent = true;
                }
            }
        }
        else
        {
            foreach (PlayerController controller in astronautControllers)
            {
                controller.Move();
                controller.UpdatePersonalScore();
            }
            UpdateGlobalScore();
            UpdateTrajectory(Wcurrent, c1, c2);
            UpdateWeights();
            ++iteration;
        }*/
        return false;
    }
}
