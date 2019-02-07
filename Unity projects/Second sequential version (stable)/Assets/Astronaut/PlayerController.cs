using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public CubeSphere attractor;
    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float gravity = -9.8f;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    //private float trajectory;

    private Vector3 gravityDirection;
    private Vector3 gravityDirectionRotated;
    private Vector3 cross;
    private Vector3 pointDirection;

    private float latestTargetDirection = 0.0f;

    //STUCK
    private float secondsCounter = 0;
    private float secondsToCount = 0.5f;
    private float latestX = 0f;
    private float latestZ = 0f;
    //END_STUCK

    //PSO
    private float trajectory;

    //END_PSO

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
                Vector3 forward = new Vector3(0, -1, 0).normalized;
                Vector3 upwards = gravityDirection.normalized;
                transform.rotation = Quaternion.LookRotation(forward, upwards);
            }
            else
            {   //y>0
                cross = -Vector3.Cross(gravityDirection, pointDirection).normalized;
                if(cross==Vector3.zero)
                {
                    cross = new Vector3(-1, 0, 0);
                }

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

    void PerformControllerRotation()
    {
        //Averiguar el angulo a girar para ir recto
        Vector3 referenceDirection = (new Vector3(0, 0, 1) - attractor.transform.position).normalized;
        Vector3 situatorDirection = new Vector3(transform.position.x, attractor.transform.position.y, transform.position.z).normalized;
        float forwardAngle = Vector3.Angle(referenceDirection, situatorDirection);
        if (situatorDirection == Vector3.zero)
        {
            forwardAngle = 0f;
        }

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if (y >= 0)
        {
            if (x >= 0)
            {
                transform.RotateAround(transform.position, transform.up, latestTargetDirection - forwardAngle);
            }
            else
            {
                transform.RotateAround(transform.position, transform.up, latestTargetDirection + forwardAngle);
            }
        }
        else
        {
            transform.RotateAround(transform.position, transform.up, 180);
            referenceDirection = (new Vector3(0, 0, -1) - attractor.transform.position).normalized;
            forwardAngle = Vector3.Angle(referenceDirection, situatorDirection);
            if (x >= 0)
            {
                transform.RotateAround(transform.position, transform.up, 360 - latestTargetDirection - forwardAngle); //-
            }
            else
            {
                transform.RotateAround(transform.position, transform.up, 360 - latestTargetDirection + forwardAngle); //+
            }
        }
    }

    public void Initialize()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;

        distToGround = collider.bounds.extents.y;
    }

    public void SetInPlace(float x, float z, float angle)
    {
        float radius = (float)attractor.gridSize / 2f + 15f;
        transform.Translate(new Vector3(x, radius, z));
        transform.RotateAround(transform.position, transform.up, angle);
        trajectory = angle;
        //STUCK
        latestX = transform.position.x;
        latestZ = transform.position.z;
    }

    private bool TransformHasNotChanged()
    {
        float currentX = transform.position.x;
        float currentZ = transform.position.z;
        float distanceTravelled = Mathf.Sqrt(Mathf.Pow(currentX-latestX, 2) + Mathf.Pow(currentZ-latestZ, 2));
        latestX = currentX;
        latestZ = currentZ;
        if(distanceTravelled <= 0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ItIsStuck()
    {
        bool stuck = false;
        secondsCounter += Time.deltaTime;
        if (secondsCounter > secondsToCount)
        {
            secondsCounter = 0;
            //DO THINGS EVERY secondsToCount SECONDS
            if(TransformHasNotChanged())
            {
                stuck = true;
            }
        }
        return stuck;
    }

    public void Move(bool running, bool move)      //If move equals false, it will stop
    {
        //CHECK IF IT IS STUCK
        if(ItIsStuck() && move) {
            trajectory += 180;  //Media vuelta
        }

        float targetSpeed = running ? runSpeed : walkSpeed;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        gravityDirection = (transform.position - attractor.transform.position).normalized;  //Vector3 ..

        //PERFORM ROTATIONS
        PerformGravityRotation();
        if(move) { PerformControllerRotation(); }

        //UPDATE HORIZONTAL TRANSLATION
        if(move) { transform.position += transform.forward * currentSpeed * Time.deltaTime; }
        //UPDATE VERTICAL TRANSLATION
        velocityY += Time.deltaTime * gravity;
        rigidbody.AddForce(gravityDirection * velocityY);

        if (move)
        {
            float targetDirection = trajectory;
            latestTargetDirection = targetDirection;
            float animationSpeedPercent = running ? 1 : 0.5f;
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("speedPercent", 0f, speedSmoothTime, Time.deltaTime);
        }

        if (isGrounded())
        {
            velocityY = 0;
        }
    }

	/*void Start () {
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;

        distToGround = collider.bounds.extents.y;
    }*/

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    /*void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;

        bool running = Input.GetKey(KeyCode.LeftShift);     //Shift izquierdo para correr
        float targetSpeed = (running ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        velocityY += Time.deltaTime * gravity;

        gravityDirection = (transform.position - attractor.transform.position).normalized;  //Vector3 ..

        PerformGravityRotation();
        PerformControllerRotation();

        //UPDATE TRASLATION
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
        //Vector3 velocity = transform.forward * currentSpeed + gravityDirection * velocityY;

        rigidbody.AddForce(gravityDirection * velocityY);

        if (inputDir != Vector2.zero)
        {
            float targetDirection = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
            latestTargetDirection = targetDirection;
        }

        float animationSpeedPercent = (running ? 1 : 0.5f) * inputDir.magnitude;
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

        if (isGrounded())
        {
            velocityY = 0;
        }*/

        //Character standing on the ground
        /*if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }*/

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirection*100);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(new Vector3(0, 0, 0), pointDirection * 100);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(new Vector3(0, 0, 0), cross*100);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirectionRotated * 100);
    }*/
}
