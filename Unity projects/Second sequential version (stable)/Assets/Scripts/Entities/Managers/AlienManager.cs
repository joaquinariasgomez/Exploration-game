using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienManager : MonoBehaviour {

    public GameObject[] aliens;

    private List<AlienController> alienControllers = new List<AlienController>();
    private int numAliens;
    private bool startPSO = false;
    private float inertia = 0.33f;

    private bool initialized = false;
    private Vector3 targetCoordinates;

    PSO pso;

    // Use this for initialization
    void Start () {
        numAliens = aliens.Length;
        foreach (GameObject alien in aliens)
        {
            alienControllers.Add(alien.GetComponent<AlienController>());
        }
    }

    public void Initialize(Vector3 targetCoordinates)
    {
        this.targetCoordinates = targetCoordinates;
        this.initialized = true;
        //Initialize
        int counter = 0;
        foreach (AlienController controller in alienControllers)
        {
            controller.Initialize(counter);
            controller.SetTargetCoordinates(targetCoordinates);
            counter++;
        }

        SetAliensInPlace();
        pso = new PSO(alienControllers);
        //Start looking for astronauts
        startPSO = true;
        pso.SetInertiaAlien(inertia);
    }

    void SetAliensInPlace()
    {
        //Set Astronauts in place forming a circle, for example
        float radius = 2.5f;

        alienControllers[0].SetInPlace(-radius, 0f, -90f);
        alienControllers[1].SetInPlace(-radius * 3f / 4f, radius * 3f / 4f, -45f);
        alienControllers[2].SetInPlace(0f, radius, 0f);
        alienControllers[3].SetInPlace(radius * 3f / 4f, radius * 3f / 4f, 45f);
        alienControllers[4].SetInPlace(radius, 0f, 90f);
        alienControllers[5].SetInPlace(radius * 3f / 4f, -radius * 3f / 4f, 135f);
        alienControllers[6].SetInPlace(0f, -radius, 180f);
        alienControllers[7].SetInPlace(-radius * 3f / 4f, -radius * 3f / 4f, -135f);

        startPSO = true;
    }

    private void StopExploring()
    {
        pso.StopExploringAlien();
    }
	
	// Update is called once per frame
	void Update () {
        if(PauseMenu.GamePaused || !initialized)
        {
            return;
        }
        if(startPSO)
        {
            bool astronautsFound = pso.UpdateAliens();
            if(astronautsFound)
            {
                startPSO = false;
            }
        }
    }
}
