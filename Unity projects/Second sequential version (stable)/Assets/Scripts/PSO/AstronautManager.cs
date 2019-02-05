using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautManager : MonoBehaviour {

    public GameObject astronaut;
    PlayerController astronautController;

    // Use this for initialization
    void Start () {
        astronautController=astronaut.GetComponent<PlayerController>();
        astronautController.Initialize();
    }
	
	// Update is called once per frame
	void Update () {
        bool running = Input.GetKey(KeyCode.LeftShift);     //Shift izquierdo para correr

        astronautController.Move(0f, false, running);
    }
}
