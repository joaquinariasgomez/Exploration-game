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

    float distToGround;

    Animator animator;
    //CharacterController controller;
    CapsuleCollider collider;
    Rigidbody rigidbody;

    public float rotX
    {
        get { return transform.rotation.eulerAngles.x; }
        set
        {
            Vector3 v = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(value, v.y, v.z);
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

        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        Vector3 velocity = transform.forward * currentSpeed + gravityDirection * velocityY;

        //rigidbody.AddForce(gravityDirection * velocityY);

        //controller.Move(velocity * Time.deltaTime);

        Vector3 currentRotation = transform.rotation.eulerAngles;
        //Quaternion gravityRotation = Quaternion.FromToRotation(transform.up, gravityDirection);
        float gravityRotation = Vector3.Angle(Vector3.up, gravityDirection);
        //print(Quaternion.AngleAxis(gravityRotation, new Vector3(1, 0, 0)).eulerAngles);
        //print(Quaternion.LookRotation(gravityDirection, Vector3.down).eulerAngles);

        Vector3 rotation = transform.rotation.eulerAngles;
        float targetDirection = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
        rotation.y = -90;//Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref turnSmoothVelocity, turnSmoothTime);
        rotation.z = 0;

        if(transform.position.y >= 0)
        {
            //gravityDirectionRotated = Quaternion.Euler(90, 0, 0) * gravityDirection;
            Vector3 point = new Vector3(transform.position.x, attractor.transform.position.y, transform.position.z);
            pointDirection = (point - attractor.transform.position).normalized;
            cross = Vector3.Cross(gravityDirection, pointDirection).normalized;
            gravityDirectionRotated = Quaternion.Euler(cross * 90) * gravityDirection;
            transform.rotation = Quaternion.LookRotation(gravityDirectionRotated);
        }
        else
        {
            //gravityDirectionRotated = Quaternion.Euler(90, 0, 0) * gravityDirection;
            //transform.rotation = Quaternion.LookRotation(gravityDirectionRotated, Vector3.down);
        }

        print("X: " + transform.position.x + " Y: " + transform.position.y + " Z: " + transform.position.z);
        
        



        //print(Vector3.Angle(gravityDirection, transform.up));
        /*if(transform.position.z >= 0f && transform.position.y >= 0f)
        {
            ++ocurrencias;
            //print("RESTANDO POR PRIMERA VEZ A "+currentRotation.x+" "+Vector3.Angle(gravityDirection, transform.up)+" QUEDA "+ (currentRotation.x - Vector3.Angle(gravityDirection, transform.up)));
            currentRotation.x = currentRotation.x + Mathf.Round(Vector3.Angle(gravityDirection, transform.up) * 100f) / 100f;
            transform.rotation = Quaternion.Euler(currentRotation);
            print("ANGULO RESTANTE " + Vector3.Angle(gravityDirection, transform.up));
        }
        if (transform.position.z < 0f && transform.position.y >= 0f)
        {
            ++ocurrencias;
            print("CASO 2");
            currentRotation.x = currentRotation.x + Vector3.Angle(gravityDirection, transform.up);
        }
        if (transform.position.z >= 0f && transform.position.y < 0f)
        {
            ++ocurrencias;
            //if(Vector3.Angle(gravityDirection, transform.up)!=0f)
            //{
                currentRotation.x = currentRotation.x + 1f;//+ Mathf.Floor(Vector3.Angle(gravityDirection, transform.up));
                transform.rotation = Quaternion.Euler(currentRotation);
            //}
            //print("ACABO DE SUMAR " + Mathf.Floor(Vector3.Angle(gravityDirection, transform.up)));
            //currentRotation.x = currentRotation.x - Vector3.Angle(gravityDirection, transform.up);
        }
        if (transform.position.z < 0f && transform.position.y < 0f)
        {
            ++ocurrencias;
            print("CASO 4");
            currentRotation.x = currentRotation.x - Vector3.Angle(gravityDirection, transform.up);
        }*/
        //print("con "+ocurrencias+" ocurrencias");

        //Quaternion.LookRotation(gravityDirection).eulerAngles.x;
        //currentRotation.y = 0f;//Quaternion.LookRotation(gravityDirection).eulerAngles.y;// currentRotation.y + gravityRotation.eulerAngles.y;
        //currentRotation.z = 0f;//gravityRotation.z + gravityRotation.eulerAngles.z;
        //transform.rotation = Quaternion.Euler(currentRotation);

        //Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityDirection) * transform.rotation;
        //transform.rotation = targetRotation; //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1000000 * Time.deltaTime);

        if (inputDir != Vector2.zero)
        {
            //float targetDirection = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
                        //transform.rotation = Quaternion.AngleAxis(Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref turnSmoothVelocity, turnSmoothTime), new Vector3(0, 1, 0));
            //Vector3 rotationVector = transform.rotation.eulerAngles;
            //rotationVector.y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref turnSmoothVelocity, turnSmoothTime);
            //transform.rotation = Quaternion.Euler(rotationVector);
            //transform.eulerAngles = new Vector3(0, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetDirection, ref turnSmoothVelocity, turnSmoothTime), 0);
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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirection*100);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(new Vector3(0, 0, 0), cross*100);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirectionRotated * 100);
    }
}
