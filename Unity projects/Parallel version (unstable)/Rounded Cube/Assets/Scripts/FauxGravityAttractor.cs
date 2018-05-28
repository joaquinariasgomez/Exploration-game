using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityAttractor : MonoBehaviour {

    public float gravity = -9.8f;

    private Rigidbody rigidbodyAttracted;
    public Transform bodyAttracted;
    private Vector3 oldBodyAttracted;

    private float secondsCounter = 0;
    private float secondsToCount = 2;

    void Start()
    {
        oldBodyAttracted = new Vector3(0,0,0);
    }

	public void Attract(Transform body)
    {
        bodyAttracted = body;

        Vector3 gravityUp = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        rigidbodyAttracted = body.GetComponent<Rigidbody>();

        rigidbodyAttracted.AddForce(gravityUp * gravity);

        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * body.rotation;
        body.rotation = Quaternion.Slerp(body.rotation, targetRotation, 50 * Time.deltaTime);
    }

    void Update()
    {
        secondsCounter += Time.deltaTime;
        if(secondsCounter > secondsToCount)
        {
            secondsCounter = 0;
            oldBodyAttracted = bodyAttracted.position;
        }
    }

    private void OnDrawGizmos()
    {
        if(bodyAttracted!=null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, bodyAttracted.position);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(oldBodyAttracted, 1f);
    }
}
