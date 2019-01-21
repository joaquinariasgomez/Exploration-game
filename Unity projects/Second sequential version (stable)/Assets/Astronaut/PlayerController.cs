using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public CubeSphere attractor;
    public float walkSpeed = 2;
    public float runSpeed = 6;
    public float gravity = -9.8f;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    private Vector3 gravityDirection;
    private Vector3 gravityDirectionRotated;
    private Vector3 cross;
    private Vector3 pointDirection;

    //Test
    private float latestTargetDirection = 0.0f;

    float distToGround;

    Animator animator;
    //CharacterController controller;
    CapsuleCollider collider;
    Rigidbody rigidbody;

    private void PerformGravityRotation()
    {
        Vector3 point = new Vector3(transform.position.x, attractor.transform.position.y, transform.position.z);
        pointDirection = (point - attractor.transform.position).normalized;

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if(y >= 0)
        {
            if(y == 0)
            {
                if(z >= 0)
                {
                    cross = new Vector3(-1, 0, 0);
                    gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                    //Personalized Upwards to mock cross between gravityDirectionRotated and it.
                    Vector3 personalizedUpwards = new Vector3(0, 1, 1);
                    transform.rotation = Quaternion.LookRotation(gravityDirectionRotated.normalized, personalizedUpwards);
                }
                else
                {
                    cross = new Vector3(1, 0, 0);
                    gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                    //Personalized Upwards to mock cross between gravityDirectionRotated and it.
                    Vector3 personalizedUpwards = new Vector3(0, 1, -1);
                    transform.rotation = Quaternion.LookRotation(gravityDirectionRotated, personalizedUpwards);
                }
            }
            else
            {
                cross = -Vector3.Cross(gravityDirection, pointDirection).normalized;
                gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                transform.rotation = Quaternion.LookRotation(gravityDirectionRotated);
            }
        }
        else
        {
            if(x == 0 && z == 0)
            {
                cross = new Vector3(1, 0, 0);
                gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                transform.rotation = Quaternion.LookRotation(gravityDirectionRotated, Vector3.down);
            }
            else
            {
                cross = Vector3.Cross(gravityDirection, pointDirection).normalized;
                gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                transform.rotation = Quaternion.LookRotation(gravityDirectionRotated, Vector3.down);
            }
        }
    }

	void Start () {
        animator = GetComponent<Animator>();
        //controller = GetComponent<CharacterController>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;

        distToGround = collider.bounds.extents.y;
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;

        bool running = Input.GetKey(KeyCode.LeftShift);     //Shift izquierdo para correr
        float targetSpeed = (running ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        velocityY += Time.deltaTime * gravity;

        gravityDirection = (transform.position - attractor.transform.position).normalized;  //Vector3 ..

        //transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        //UPDATE TRASLATION
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
        Vector3 velocity = transform.forward * currentSpeed + gravityDirection * velocityY;

        rigidbody.AddForce(gravityDirection * velocityY);

        //controller.Move(velocity * Time.deltaTime);

        PerformGravityRotation();
        Vector3 rotationVector1 = transform.localRotation.eulerAngles;  //rotation
        //transform.localRotation = Quaternion.Euler(rotationVector1);    //rotation
        rotationVector1.y = latestTargetDirection;
        transform.RotateAround(transform.position, transform.up, latestTargetDirection);

        if (inputDir != Vector2.zero)
        {
            float targetDirection = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
            latestTargetDirection = targetDirection;
            print("Target direction " + targetDirection);
            //Vector3 rotationVector = transform.rotation.eulerAngles;
            //rotationVector.y = targetDirection;//Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref turnSmoothVelocity, turnSmoothTime);
            //transform.rotation = Quaternion.Euler(rotationVector);
        }

        float animationSpeedPercent = (running ? 1 : 0.5f) * inputDir.magnitude;
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

        if (isGrounded())
        {
            velocityY = 0;
        }

        //Character standing on the ground
        /*if (controller.isGrounded)
        {
            velocityY = 0;
        }*/
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirection*100);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(new Vector3(0, 0, 0), pointDirection * 100);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(new Vector3(0, 0, 0), cross*100);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirectionRotated * 100);*/
    }
}
