using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 15;

    private Vector3 moveDir;
    //private float xDisplacement;
    //private float yDisplacement;
    private Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        /*if(Input.GetMouseButton(0))
        {
            xDisplacement = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            yDisplacement = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        }*/
    }

    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(moveDir) * moveSpeed * Time.deltaTime);
        //rigidbody.MovePosition(new Vector3(rigidbody.position.x - xDisplacement, rigidbody.position.y, rigidbody.position.z - yDisplacement));
    }
}
