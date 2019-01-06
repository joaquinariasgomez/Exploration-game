using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityCamera : MonoBehaviour {

    public CubeSphere attractor;
    Rigidbody rigidbody;    //TODO: Quitar Rigidbody

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;
    }

    void Update()
    {
        attractor.AttractCamera(transform);
    }
}
