using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
    private List<PlayerController> astronautControllers;
    public float globalBestScore;
    public Vector3 globalBestPosition;

    private float Wmin = 1;     //1
    private float Wmax;  //2000
    private float Wcurrent;
    private float c1 = 1;    //1.5
    private float c2 = 1.5f;       //2

    private float caida;

    private int gridSize = DataBetweenScenes.getSize();

    private int iteration = 1;
    private int maxIterations = 1000;   //3000
    private int maxIterWithWmin = 5;  //400

    private bool stopExploring = false;

    //LOGS
    FileWriter globalBestScoreLogs;
    FileWriter testLog;
    //END LOGS

    public PSO(List<PlayerController> astronautControllers)
    {
        this.astronautControllers = astronautControllers;
        
        globalBestScore = gridSize / 2f;  //Minimum best score
        globalBestPosition = Vector3.zero;

        //FileWriter
        globalBestScoreLogs = new FileWriter("Assets/Logs/GlobalBestScore.txt");
        testLog = new FileWriter("Assets/Logs/testLog.txt");
    }

    public void SetInertia(float inertia)
    {
        float maxInertia = 500;
        switch (gridSize)
        {
            case 100: maxInertia = 500; maxIterations = 1500; break;
            case 200: maxInertia = 1000; maxIterations = 1750;  break;
            case 400: maxInertia = 4000; maxIterations = 2000; break;
        }
        float minInertia = 20;
        Wmax = minInertia + (maxInertia - minInertia) * inertia;

        this.Wcurrent = Wmax;
        this.caida = (Wmax - Wmin) / maxIterations;
    }

    private void UpdateGlobalScore()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            if (controller.personalBestScore > globalBestScore)
            {
                globalBestScore = controller.personalBestScore;
                globalBestPosition = controller.personalBestPosition;
            }
            controller.UpdateGlobalScore(globalBestScore, globalBestPosition);
        }
        //Logs
        globalBestScoreLogs.Write(globalBestScore);
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

    public void StopExploring()
    {
        stopExploring = true;
    }

    //Main loop
    public void UpdateAstronauts()
    {
        if(iteration == (maxIterations + maxIterWithWmin) || stopExploring)
        {
            GoToGlobalMax();
            CheckGlobalMax();   //Will stop player is has reached mountain
            if(AllReachedHighestMountain())
            {
                SendSignalReachedHighestMountain();
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
        }
    }
}
