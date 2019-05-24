using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBall : MonoBehaviour {

    public AstronautManager astronautManager;

    private GameObject[] astronauts;
    private bool throwing;
    private Vector3 direction;
    private float velocityCte = 3f;//3
    private float duration = 10f;    //Duration of maximum throw time in seconds
    private float timeThrowing;
    private float maxTimeBetweenShoots = 3f;
    private float timeBetweenShoots;

    private bool hit;   //Dira si ha golpeado a un astronauta para dar feedback al alien

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
        hit = false;
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
        this.hit = false;

        foreach(GameObject astronaut in astronauts)
        {
            if(astronaut.GetComponent<Collider>() == collider && !astronaut.GetComponent<PlayerController>().isDead())
            {
                int astronautId = astronaut.GetComponent<PlayerController>().id;
                astronaut.GetComponent<PlayerController>().Hit();

                this.hit = true;
            }
        }
    }

    public bool hasHitAstronaut()
    {
        return hit;
    }

    public bool isColliding()
    {
        RaycastHit hit;
        Ray ray = new Ray(collider.bounds.center, direction);
        if(Physics.Raycast(ray, out hit, 0.5f)) //0.4f
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
