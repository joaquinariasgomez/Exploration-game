using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
    private List<PlayerController> astronautControllers;
    public float globalBestScore;
    public Vector3 globalBestPosition;

    private float Wmin = 1;
    private float Wmax = 200;
    private float Wcurrent;
    private float c1 = 1.5f;
    private float c2 = 2;

    private float caida;

    private int iteration = 1;
    private int maxIterations = 3000;
    private int maxIterWithWmin = 400;

    //LOGS
    FileWriter globalBestScoreLogs;
    FileWriter testLog;
    //END LOGS

    public PSO(List<PlayerController> astronautControllers)
    {
        this.astronautControllers = astronautControllers;
        this.Wcurrent = Wmax;
        this.caida = (Wmax - Wmin) / maxIterations;

        globalBestScore = astronautControllers[0].attractor.gridSize / 2f;  //Minimum best score
        globalBestPosition = Vector3.zero;

        //FileWriter
        globalBestScoreLogs = new FileWriter("Assets/Logs/GlobalBestScore.txt");
        testLog = new FileWriter("Assets/Logs/testLog.txt");
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

    private void UpdateTrajectory(float Wcurrent, float c1, float c2)
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.UpdateTrajectory(Wcurrent, c1, c2);
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

    //Main loop
    public void UpdateAstronauts()
    {
        if(iteration == (maxIterations + maxIterWithWmin))
        {
            Stop();
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
