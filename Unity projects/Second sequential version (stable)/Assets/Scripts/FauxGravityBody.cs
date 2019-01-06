using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityBody : MonoBehaviour {

    public CubeSphere attractor;
    //Rigidbody rigidbody;

	void Start () {
        /*rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;*/

	}
	
	void Update () {
        //attractor.Attract(transform);
	}
}
