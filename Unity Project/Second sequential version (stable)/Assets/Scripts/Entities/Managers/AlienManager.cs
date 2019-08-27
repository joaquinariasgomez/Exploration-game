using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienManager : MonoBehaviour {

    public GameObject[] aliens;
    public GameObject[] astronauts;

    private List<AlienController> alienControllers = new List<AlienController>();
    private List<PlayerController> astronautControllers = new List<PlayerController>();
    private int numAliens;
    [HideInInspector]
    public bool startPSO = false;
    private float inertia = 0.33f;

    private Vector3 setPosition = Vector3.zero;

    private bool initialized = false;
    private Vector3 targetCoordinates;

    PSO pso;
    [HideInInspector]
    public AttackAstronauts attackAstronauts;

    // Use this for initialization
    void Start () {
        numAliens = aliens.Length;
        foreach (GameObject alien in aliens)
        {
            alienControllers.Add(alien.GetComponent<AlienController>());
        }
        foreach (GameObject astronaut in astronauts)
        {
            astronautControllers.Add(astronaut.GetComponent<PlayerController>());
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

        //Ensure we have a min and max speed entities
        float maxSpeedAlien = DataBetweenScenes.getMaxSpeedAlien();
        float minSpeedAlien = 1.75f;
        int alienIdMin = Random.Range(0, 7); alienControllers[alienIdMin].SetSpeed(minSpeedAlien);
        int alienIdMax = Random.Range(0, 7); alienControllers[alienIdMax].SetSpeed(maxSpeedAlien);

        SetAliensInPlace();
        pso = new PSO(alienControllers);
        attackAstronauts = new AttackAstronauts(astronautControllers, alienControllers);
        //Start looking for astronauts
        startPSO = true;
        pso.SetInertiaAlien(inertia);
    }

    void SetAliensInPlace()
    {
        Vector3 averagePosition = Vector3.zero;
        foreach(PlayerController astronaut in astronautControllers)
        {
            Vector3 position = astronaut.transform.position;
            averagePosition += position;
        }
        averagePosition = averagePosition / 8f;

        Vector3 newPosition = (averagePosition - Vector3.zero).normalized;
        int degrees = 35;
        switch(DataBetweenScenes.getSize())
        {
            case 100: degrees = 55; break;
            case 200: degrees = 40; break;
            case 400: degrees = 25; break;
        }
        newPosition = Quaternion.Euler(degrees, 0, 0) * newPosition * (DataBetweenScenes.getSize() / 2f + CubeSphere.heightMultiplier);

        //Set Astronauts in place forming a circle, for example
        float radius = 2.5f;

        /*alienControllers[0].SetInPlace(-radius, 0f, -90f);
        alienControllers[1].SetInPlace(-radius * 3f / 4f, radius * 3f / 4f, -45f);
        alienControllers[2].SetInPlace(0f, radius, 0f);
        alienControllers[3].SetInPlace(radius * 3f / 4f, radius * 3f / 4f, 45f);
        alienControllers[4].SetInPlace(radius, 0f, 90f);
        alienControllers[5].SetInPlace(radius * 3f / 4f, -radius * 3f / 4f, 135f);
        alienControllers[6].SetInPlace(0f, -radius, 180f);
        alienControllers[7].SetInPlace(-radius * 3f / 4f, -radius * 3f / 4f, -135f);*/

        foreach(AlienController alien in alienControllers)
        {
            alien.SetInPlace(newPosition, Random.Range(0f, 360f));
        }

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
        else
        {
            //AttackAstronauts
            attackAstronauts.UpdateAliens();
        }
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.red;
        Gizmos.DrawSphere(setPosition, 5f);*/
    }
}
