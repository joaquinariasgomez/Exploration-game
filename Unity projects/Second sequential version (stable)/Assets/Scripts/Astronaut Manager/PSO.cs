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
    private float c1;
    private float c2;

    private float rate = 0.5f;

    public PSO(List<PlayerController> astronautControllers, float Wmin, float Wmax, float c1, float c2)
    {
        this.astronautControllers = astronautControllers;
        this.Wmin = Wmin;
        this.Wmax = Wmax;
        this.Wcurrent = Wmax;
        this.c1 = c1;
        this.c2 = c2;
        globalBestScore = 0f;
        globalBestPosition = Vector3.zero;
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
            Wcurrent -= this.rate;
            if(Wcurrent <= 0)
            {
                Wcurrent = Wmin;
            }
        }
    }

    public void UpdateAstronauts()
    {
        foreach (PlayerController controller in astronautControllers)
        {
            controller.Move(false, true);
            controller.UpdatePersonalScore();
        }
        UpdateGlobalScore();
        UpdateTrajectory(Wcurrent, c1, c2);
        UpdateWeights();
    }
}
