using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBall : MonoBehaviour {

    public AstronautManager astronautManager;

    private GameObject[] astronauts;
    private bool throwing;
    private Vector3 direction;
    private float velocityCte = 7f;//3
    private float duration = 10f;    //Duration of maximum throw time in seconds
    private float timeThrowing;
    private float maxTimeBetweenShoots = 3f;
    private float timeBetweenShoots;

    Rigidbody rigidbody;
    Collider collider;

	void Start () {
        this.astronauts = astronautManager.GetAstronauts();

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;
        collider.enabled = false;

        timeThrowing = 0f;
        timeBetweenShoots = 0f;

        gameObject.SetActive(false);
        throwing = false;
        direction = Vector3.zero;
	}
	
	void Update () {
		if(throwing)
        {
            timeThrowing += Time.deltaTime;
            if(timeThrowing >= duration) {
                timeThrowing = 0f;
                throwing = false;
            }
            if(isColliding())
            {
                timeThrowing = 0f;
                throwing = false;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
        /*else
        {
            timeBetweenShoots += Time.deltaTime;
            if(timeBetweenShoots >= maxTimeBetweenShoots)
            {
                timeBetweenShoots = 0f;
            }
        }*/
	}

    private void CheckCollission(Collider collider)
    {
        foreach(GameObject astronaut in astronauts)
        {
            if(astronaut.GetComponent<Collider>() == collider)
            {
                int astronautId = astronaut.GetComponent<PlayerController>().id;
                astronaut.GetComponent<PlayerController>().Hit();
            }
        }
    }

    bool isColliding()
    {
        RaycastHit hit;
        Ray ray = new Ray(collider.bounds.center, direction);
        if(Physics.Raycast(ray, out hit, 0.4f))
        {
            if(hit.collider != null)
            {
                //comprobar qué colider es hit.collider, dado que es el collider que ha sido hiteado
                CheckCollission(hit.collider);
                return true;
            }
        }
        return false;
    }
    /*bool isColliding()
    {
        return Physics.Raycast(collider.bounds.center, direction, 2f);
    }*/

    public void Throw(Vector3 from, Vector3 direction)
    {
        throwing = true;
        this.direction = direction;

        rigidbody.velocity = direction * velocityCte;

        transform.position = from;
        gameObject.SetActive(true);
    }

    public bool NowThrowing()
    {
        return throwing;
    }
}
