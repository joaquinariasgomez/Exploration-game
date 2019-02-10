using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSO
{
    private List<PlayerController> astronautControllers;
    public float globalBestScore;
    public Vector3 globalBestPosition;

    private float Wmin;
    private float Wmax;
    private float Wcurrent;
    private float magicNumber;
    private float c1;
    private float c2;

    private float caida = 0.15f;
    private int iteration = 1;

    //LOGS
    FileWriter globalBestScoreLogs;
    FileWriter testLog;
    //END LOGS

    public PSO(List<PlayerController> astronautControllers, float Wmin, float Wmax, float c1, float c2)
    {
        this.astronautControllers = astronautControllers;
        this.Wmin = Wmin;
        this.Wmax = Wmax;
        this.Wcurrent = Wmax;
        this.c1 = c1;
        this.c2 = c2;
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
        magicNumber = Wcurrent;//Mathf.Log(iteration, 24);      //Después de 2870 iteraciones llega a casi 6
        //TestLog
        testLog.Write(this.caida);
        float caida_variable = this.caida - 0.005f;
        if(caida_variable>=0f)
        {
            this.caida = caida_variable;
        }
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
        if(Wcurrent <= Wmin)
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
