using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautManager : MonoBehaviour {

    public GameObject[] astronauts;
    private List<PlayerController> astronautControllers = new List<PlayerController>();
    private int numAstronauts;

    PSO pso;

    // Use this for initialization
    void Start () {
        numAstronauts = astronauts.Length;
        foreach (GameObject astronaut in astronauts)
        {
            astronautControllers.Add(astronaut.GetComponent<PlayerController>());
        }
        //Initialize
        int counter = 0;
        foreach(PlayerController controller in astronautControllers)
        {
            controller.Initialize(counter);
            counter++;
        }

        SetAstronautsInPlace();
        pso = new PSO(astronautControllers);
    }

    void SetAstronautsInPlace()
    {
        //Set Astronauts in place forming a circle, for example
        float radius = 5f;

        astronautControllers[0].SetInPlace(-radius, 0f, -90f);
        astronautControllers[1].SetInPlace(-radius * 3f / 4f, radius * 3f / 4f, -45f);
        astronautControllers[2].SetInPlace(0f, radius, 0f);
        astronautControllers[3].SetInPlace(radius * 3f / 4f, radius * 3f / 4f, 45f);
        astronautControllers[4].SetInPlace(radius, 0f, 90f);
        astronautControllers[5].SetInPlace(radius * 3f / 4f, -radius * 3f / 4f, 135f);
        astronautControllers[6].SetInPlace(0f, -radius, 180f);
        astronautControllers[7].SetInPlace(-radius * 3f / 4f, -radius * 3f / 4f, -135f);
    }
	
	// Update is called once per frame
	void Update () {
        pso.UpdateAstronauts();
    }
}
