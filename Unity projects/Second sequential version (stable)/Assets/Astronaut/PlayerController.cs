﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public CubeSphere attractor;
    public float walkSpeed = 3;
    public float runSpeed = 6;
    private float gravity = 9.8f;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float maxVelocityCteY = 5f;
    float velocityCteY;
    bool running;
    bool move;

    private Vector3 gravityDirection;
    private Vector3 gravityDirectionRotated;
    private Vector3 cross;
    private Vector3 pointDirection;

    private float latestTargetDirection = 0.0f;
    private int id;

    //STUCK
    private float secondsCounter = 0f;
    private float secondsToCount = 0.5f;
    private float latestX = 0f;
    private float latestY = 0f;
    private float latestZ = 0f;
    //END_STUCK

    //PSO
    public float trajectory;
    private float actualScore;
    public float personalBestScore;
    public Vector3 personalBestPosition;
    public float globalBestScore;
    public Vector3 globalBestPosition;

    public Vector3 directionToGlobal;
    public Vector3 directionToPersonal;
    public Vector3 destination;
    public Vector3 projectedDestination;

    private float Wcurrent;
    private float c1;
    private float c2;
    //END_PSO

    //LOGS
    FileWriter actualScoreLogs;
    FileWriter personalBestScoreLogs;
    //END LOGS

    float distToGround;
    bool setted = false;

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

    public void Initialize(int id)
    {
        this.id = id;
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;

        distToGround = collider.bounds.extents.y;
        personalBestScore = 0f;
        personalBestPosition = new Vector3(0, 0, 0);    //No tener en cuenta si personalBestScore es 0f
        velocityCteY = maxVelocityCteY;
        this.move = true;

        actualScore = attractor.gridSize / 2f;  //Minimum score

        //FileWriter
        actualScoreLogs = new FileWriter("Assets/Logs/Astronaut_" + this.id + "ActualScore.txt");
        personalBestScoreLogs = new FileWriter("Assets/Logs/Astronaut_" + this.id + "PersonalBestScore.txt");
    }

    public void SetInPlace(float x, float z, float angle)
    {
        float radius = (float)attractor.gridSize / 2f + CubeSphere.heightMultiplier;
        transform.Translate(new Vector3(x, radius, z));
        transform.RotateAround(transform.position, transform.up, angle);
        trajectory = angle;
        //STUCK
        latestX = transform.position.x;
        latestZ = transform.position.z;
        setted = true;
    }

    private bool TransformHasNotChanged()
    {
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float currentZ = transform.position.z;
        float distanceTravelled = Mathf.Sqrt(Mathf.Pow(currentX-latestX, 2) + Mathf.Pow(currentY - latestY, 2) + Mathf.Pow(currentZ-latestZ, 2));
        latestX = currentX;
        latestY = currentY;
        latestZ = currentZ;
        return distanceTravelled <= 0.1f;
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

    public void UpdatePersonalScore()
    {
        if(isGrounded())
        {
            actualScore = Vector3.Distance(attractor.transform.position, transform.position);
            if (actualScore > personalBestScore)
            {
                personalBestScore = actualScore;
                personalBestPosition = transform.position;
            }
        }
    }

    public void UpdateGlobalScore(float globalBestScore, Vector3 globalBestPosition)
    {
        this.globalBestScore = globalBestScore;
        this.globalBestPosition = globalBestPosition;

        //Logs
        actualScoreLogs.Write(actualScore);
        personalBestScoreLogs.Write(personalBestScore);
    }

    public void UpdateTrajectory(float Wcurrent, float c1, float c2)
    {
        //Update weights
        this.Wcurrent = Wcurrent;
        this.c1 = c1;
        this.c2 = c2;

        print(Wcurrent);

        //Update direction vectors
        float r1 = Random.Range(1f, 1.5f);
        float r2 = Random.Range(1f, 1.5f);
        directionToGlobal = (globalBestPosition - transform.position).normalized;
        directionToPersonal = (personalBestPosition - transform.position).normalized;
        destination = Wcurrent * transform.forward + r1 * c1 * directionToPersonal + r2 * c2 * directionToGlobal;
        projectedDestination = Vector3.ProjectOnPlane(destination, transform.up);

        //Update trajectory
        float angle = Vector3.Angle(transform.forward, projectedDestination);
        if (isGrounded())
        {
            //- => Izquierda
            float rightAngle = Vector3.Angle(transform.right, projectedDestination);
            float leftAngle = Vector3.Angle(-transform.right, projectedDestination);
            if (rightAngle > leftAngle)
            {
                if (transform.position.y >= 0)
                {
                    trajectory -= angle;
                }
                else
                {
                    trajectory += angle;
                }
            }
            else
            {
                if (transform.position.y >= 0)
                {
                    trajectory += angle;
                }
                else
                {
                    trajectory -= angle;
                }
            }
        }
    }

    public void Move()      //If move equals false, it will stop
    {
        //CHECK IF IT IS STUCK AND ITS ALSO MOVING
        if(ItIsStuck() && move) {
            trajectory += 180;  //Media vuelta
        }

        float targetSpeed = (projectedDestination.magnitude > walkSpeed) ? runSpeed : walkSpeed;
        running = (targetSpeed==runSpeed);

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        gravityDirection = (transform.position - attractor.transform.position).normalized;  //Vector3 ..

        //PERFORM ROTATIONS
        PerformGravityRotation();
        if(move) { PerformControllerRotation(); }

        //UPDATE HORIZONTAL TRANSLATION
        if(move && isGrounded()) { transform.position += transform.forward * currentSpeed * Time.deltaTime; }
        //UPDATE VERTICAL TRANSLATION
        rigidbody.AddForce(-gravityDirection * velocityCteY);

        float targetDirection = trajectory;
        latestTargetDirection = targetDirection;

        if (move && isGrounded())
        {
            float animationSpeedPercent = running ? 1 : 0.5f;
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("speedPercent", 0f, speedSmoothTime, Time.deltaTime);
        }

        if (isGrounded())
        {
            velocityCteY = 0;
        }
        else
        {
            velocityCteY = maxVelocityCteY;
        }
    }

    public void Stop()
    {
        this.move = false;
        animator.SetFloat("speedPercent", 0f, speedSmoothTime, Time.deltaTime);

        gravityDirection = (transform.position - attractor.transform.position).normalized;
        //PERFORM ROTATIONS
        PerformGravityRotation();
        rigidbody.AddForce(-gravityDirection * velocityCteY);

        if (isGrounded())
        {
            velocityCteY = 0;
        }
        else
        {
            velocityCteY = maxVelocityCteY;
        }

        //Logs
        this.actualScoreLogs.End();
        this.personalBestScoreLogs.End();
    }

    bool isGrounded()
    {
        return Physics.Raycast(collider.bounds.center, -gravityDirection, distToGround + distToGround / 4);
    }

    private void OnDrawGizmos()
    {
        
        /*Gizmos.color = Color.black;
        if(setted)
        {
            Gizmos.DrawSphere(personalBestPosition, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(globalBestPosition, 0.7f);
        }
        
        /*Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, directionToGlobal * c2);
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, directionToPersonal * c1);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * Wcurrent);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, destination);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, projectedDestination);
        /*Gizmos.color = Color.green;
        Gizmos.DrawRay(new Vector3(0, 0, 0), pointDirection * 100);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(new Vector3(0, 0, 0), cross*100);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(new Vector3(0, 0, 0), gravityDirectionRotated * 100);*/
    }
}
