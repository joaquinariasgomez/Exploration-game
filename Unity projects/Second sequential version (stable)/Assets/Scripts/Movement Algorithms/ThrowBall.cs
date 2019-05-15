using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBall : MonoBehaviour {

    private bool throwing;
    private Vector3 direction;
    private float velocityCte = 4f;
    private float duration = 2f;    //Duration of maximum throw time in seconds
    private float timeThrowing;
    private float maxTimeBetweenShoots = 3f;
    private float timeBetweenShoots;

    Rigidbody rigidbody;
    Collider collider;

	void Start () {
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

    public void Throw(Vector3 direction)
    {
        throwing = true;
        this.direction = direction;

        rigidbody.velocity = direction * velocityCte;

        transform.Translate(Vector3.zero);
        gameObject.SetActive(true);
    }

    public bool NowThrowing()
    {
        return throwing;
    }
}
