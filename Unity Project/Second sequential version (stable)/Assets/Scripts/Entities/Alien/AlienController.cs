using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienController : MonoBehaviour {

    public CubeSphere attractor;
    public Texture2D Angry;
    public Texture2D Sad;
    public GameObject ball;
    public GameObject HealthBar;

    private int imageToDraw = 0;    //0 -> no image; 1 -> angry; 2 -> sad

    private Vector3 upComponent = Vector3.zero;

    private int gridSize;

    private float maxSpeed = DataBetweenScenes.getMaxSpeedAlien();//2.5f; //5
    private float minSpeed = 0;
    private float correspondentSpeed;

    private float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    private float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float maxVelocityCteY = 6f;    //10f;
    float velocityCteY;
    bool running;
    bool move;
    private float targetDistanceToAstronauts = 35f;
    private float maxDistanceToShoot = DataBetweenScenes.getMaxDistanceToShoot();//20f;
    private float maximumDistanceToAstronaut = 5f; //20f
    private Vector3 directionOfEscape = Vector3.zero;
    private float enoughCloseToHightestMountain = 6f;   //This will let Astronaut find a stable position within this distance

    private Vector3 gravityDirection;
    private Vector3 gravityDirectionRotated;
    private Vector3 cross;
    private Vector3 pointDirection;

    public Queue<int> shootSuccesses = new Queue<int>();   //1 -> acierto; 0 -> fallo

    private float latestTargetDirection = 0.0f;
    [HideInInspector]
    public int id;

    //STUCK
    private float secondsCounter = 0f;
    private float secondsToCount = 0.1f;    //0.5
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

    public Vector3 foundBestPosition;
    private float foundBestScore;

    public Vector3 directionToGlobal;
    public Vector3 directionToPersonal;
    public Vector3 directionToForward;
    public Vector3 destination;
    public Vector3 projectedDestination;

    private Vector3 targetCoordinates = Vector3.zero;

    private float Wcurrent;
    private float c1;
    private float c2;
    //END_PSO

    //STEPS
    int stepId = 0; //0 -> Right    1 -> Left
    private float timeBetweenStepsCounter = 0f;
    private float timeBetweenStepsToCount;   //THIS WILL DEPEND on astronaut's speed
    //END_STEPS

    //STATUS
    private bool dead = false;
    private float life;
    private float speed;
    //END_STATUS

    //TEST
    //Vector3 fromDirectionOfEscape = Vector3.zero;
    PlayerController fromDirectionOfEscape = null;
    Vector3 toDirectionOfEscape = Vector3.zero;

    Vector3 fromDirectionOfClosestAstronaut = Vector3.zero;
    Vector3 toDirectionOfClosestAstronaut = Vector3.zero;
    //END_TEST

    float distToGround;
    bool setted = false;
    private bool settedBestPosition = false;

    Animator animator;
    CapsuleCollider collider;
    Rigidbody rigidbody;

    private void Awake()
    {
        gridSize = DataBetweenScenes.getSize();
        HealthBar.SetActive(false);
    }

    public void SetImageToDraw(int num)
    {
        this.imageToDraw = num;
    }

    public float GetMaxDistanceToShoot()
    {
        return maxDistanceToShoot;
    }

    public bool isDead()
    {
        return dead;
    }

    private void Update()
    {
        if (dead)
        {
            animator.SetFloat("speedPercent", 1f, speedSmoothTime, Time.deltaTime);
        }
    }

    public void Hit()
    {
        float value = 10.5f;
        this.DecreaseLifeBy(value);
        this.HealthBar.GetComponent<HealthBar>().UpdateHealth(this.life);
    }

    private void DecreaseLifeBy(float value)
    {
        if (life > 0f)
        {
            life -= value;
            if (life < 0f) life = 0f;
        }
        if (life == 0f)
        {
            this.dead = true;
        }
    }

    private void OnGUI()
    {
        if(PauseMenu.GamePaused || dead)
        {
            return;
        }
        switch(imageToDraw)
        {
            case 0: return;
            case 1:
                Vector2 alienPos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                GUI.DrawTexture(new Rect(alienPos.x - 18, Screen.height - alienPos.y - 60, 36, 40), Angry);
                break;
            case 2:
                Vector2 alienPos2 = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                GUI.DrawTexture(new Rect(alienPos2.x - 18, Screen.height - alienPos2.y - 52, 36, 32), Sad);
                break;
        }
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetBestPosition()
    {
        if (settedBestPosition) { return; }
        settedBestPosition = true;

        foundBestPosition = globalBestPosition;
        foundBestScore = globalBestScore;
    }

    public bool CheckDistanceWithAstronauts(List<PlayerController> astronautControllers)
    {
        bool condition = false;
        float bestDistance = 1000f;

        foreach (PlayerController controller in astronautControllers)
        {
            if(!controller.isDead())
            {
                if (Vector3.Distance(controller.transform.position, transform.position) < maximumDistanceToAstronaut)
                {
                    condition = true;
                    toDirectionOfEscape = transform.position;
                    fromDirectionOfEscape = controller;// = controller.transform.position;
                }
                if (Vector3.Distance(controller.transform.position, transform.position) < bestDistance)
                {
                    bestDistance = Vector3.Distance(controller.transform.position, transform.position);
                    toDirectionOfClosestAstronaut = transform.position;
                    fromDirectionOfClosestAstronaut = controller.transform.position;
                }
            }
        }
        return condition;
    }

    public /*List<Vector3>*/PlayerController /*GetDirectionOfEscape*/GetAstronautToEscapeFrom()
    {
        /*List<Vector3> result = new List<Vector3>();
        result.Add(fromDirectionOfEscape);
        result.Add(toDirectionOfEscape);*/

        return fromDirectionOfEscape;
    }

    public List<Vector3> GetDirectionOfClosestAstronaut()
    {
        List<Vector3> result = new List<Vector3>();
        result.Add(fromDirectionOfClosestAstronaut);
        result.Add(toDirectionOfClosestAstronaut);
        return result;
    }

    public List<Vector3> GetRealDirectionOfClosestAstronaut(List<PlayerController> astronautControllers)
    {
        float bestDistance = 1000f;
        List<Vector3> result = new List<Vector3>();
        bool allDead = true;

        foreach (PlayerController controller in astronautControllers)
        {
            if (!controller.isDead())
            {
                allDead = false;
                if (Vector3.Distance(controller.transform.position, transform.position) < bestDistance)
                {
                    bestDistance = Vector3.Distance(controller.transform.position, transform.position);
                    result.Add(controller.transform.position);
                    result.Add(transform.position);
                }
            }
        }
        if (allDead)
        {
            result.Add(Vector3.zero);
            result.Add(transform.position);
            return result;
        }
        else
        {
            return result;
        }
    }

    public List<Vector3> GetDirectionOfRandomAstronaut(List<PlayerController> astronautControllers)
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 toDirectionOfRandomAstronaut = transform.position;
        Vector3 fromDirectionOfRandomAstronaut = Vector3.zero;

        bool oneIsChosen = false;

        foreach(PlayerController controller in astronautControllers)
        {
            if(!controller.isDead())
            {
                float chooseThis = Random.Range(0f, 1f);
                if(chooseThis <= 0.3f)
                {
                    oneIsChosen = true;
                    fromDirectionOfRandomAstronaut = controller.transform.position;
                }
            }
        }

        if(!oneIsChosen)
        {
            foreach (PlayerController controller in astronautControllers)
            {
                if (!controller.isDead())
                {
                    fromDirectionOfRandomAstronaut = controller.transform.position;
                }
            }
        }

        result.Add(fromDirectionOfRandomAstronaut);
        result.Add(toDirectionOfRandomAstronaut);
        return result;
    }

    public bool IsCloseEnoughToAstronauts()
    {
        float actualScore = Vector3.Distance(targetCoordinates, transform.position);
        return actualScore <= targetDistanceToAstronauts;
    }

    private void PerformGravityRotation()
    {
        Vector3 point = new Vector3(transform.position.x, attractor.transform.position.y, transform.position.z);
        pointDirection = (point - attractor.transform.position).normalized;

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if (y >= 0)
        {
            if (y == 0)
            {
                Vector3 forward = new Vector3(0, -1, 0).normalized;
                Vector3 upwards = gravityDirection.normalized;
                transform.rotation = Quaternion.LookRotation(forward, upwards);
            }
            else
            {   //y>0
                cross = -Vector3.Cross(gravityDirection, pointDirection).normalized;
                if (cross == Vector3.zero)
                {
                    cross = new Vector3(-1, 0, 0);
                }

                gravityDirectionRotated = Vector3.Cross(gravityDirection, cross);
                transform.rotation = Quaternion.LookRotation(gravityDirectionRotated);
            }
        }
        else
        {
            if (x == 0 && z == 0)
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
        //float forwardAngle = Vector3.Angle(referenceDirection, situatorDirection);

        Vector3 situatorDirectionProjected = Vector3.ProjectOnPlane(situatorDirection, new Vector3(0, 1, 0));
        float forwardAngle = Vector3.Angle(referenceDirection, situatorDirectionProjected);

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if (y >= 0)
        {
            if (x >= 0)
            {
                transform.RotateAround(transform.position, transform.up, latestTargetDirection - forwardAngle); //-
            }
            else
            {
                transform.RotateAround(transform.position, transform.up, latestTargetDirection + forwardAngle); //+
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
        gameObject.SetActive(true);

        this.id = id;
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.useGravity = false;

        distToGround = collider.bounds.extents.y;
        personalBestScore = gridSize * 4;  //Maximum score
        personalBestPosition = new Vector3(0, 0, 0);    //No tener en cuenta si personalBestScore es 0f
        velocityCteY = maxVelocityCteY;
        this.move = true;

        actualScore = gridSize / 2f;  //Minimum score

        speed = Random.Range(2, maxSpeed);
        correspondentSpeed = speed;
        life = 100;

        this.HealthBar.GetComponent<HealthBar>().Initialize(life);

        float slowTime = 0.5f;
        float fastTime = 0.1f;
        float difTime = slowTime - fastTime;
        float difSpeed = maxSpeed - 3;

        float timeIncrease = ((speed - 3) / difSpeed) * difTime;

        timeBetweenStepsToCount = slowTime - timeIncrease;
    }

    /*public void SetInPlace(float x, float z, float angle, bool condition = true)
    {
        float radius = (float)gridSize / 2f + CubeSphere.heightMultiplier;
        if(condition)
        {
            transform.Translate(new Vector3(x, radius, z)); //radius
        }
        else
        {
            transform.Translate(new Vector3(x, -radius, z)); //radius
        }
        transform.RotateAround(transform.position, transform.up, angle);
        trajectory = angle;
        //STUCK
        latestX = transform.position.x;
        latestZ = transform.position.z;
        setted = true;
    }*/

    public void SetInPlace(Vector3 position, float angle)
    {
        transform.Translate(position); //radius
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
        float distanceTravelled = Mathf.Sqrt(Mathf.Pow(currentX - latestX, 2) + Mathf.Pow(currentY - latestY, 2) + Mathf.Pow(currentZ - latestZ, 2));
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
            if (TransformHasNotChanged())
            {
                stuck = true;
            }
        }
        return stuck;
    }

    public void SetTargetCoordinates(Vector3 targetCoordinates)
    {
        this.targetCoordinates = targetCoordinates;
    }

    public void UpdatePersonalScore()
    {
        if (isGrounded())
        {
            actualScore = Vector3.Distance(targetCoordinates, transform.position);
            if (actualScore < personalBestScore)
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
    }

    public void UpdateTrajectory(float Wcurrent, float c1, float c2, bool goToGlobalMax = false)
    {
        //Update weights
        this.Wcurrent = Wcurrent;
        this.c1 = c1;
        this.c2 = c2;

        //Update direction vectors
        float r1 = 1; //Random.Range(1f, 1.5f);
        float r2 = 1; //Random.Range(1f, 1.5f);
        directionToGlobal = r2 * c2 * (globalBestPosition - transform.position).normalized;
        directionToPersonal = r1 * c1 * (personalBestPosition - transform.position).normalized;
        directionToForward = Wcurrent * transform.forward;
        destination = directionToForward + directionToPersonal + directionToGlobal;
        //WHEN CLOSE TO HIGHEST MOUNTAIN THINGS
        if (goToGlobalMax)
        {
            float distanceToHighestMountain = Vector3.Distance(transform.position, foundBestPosition);
            if (distanceToHighestMountain <= enoughCloseToHightestMountain)
            {
                if (isGrounded())
                {
                    actualScore = Vector3.Distance(attractor.transform.position, transform.position);
                    if (actualScore > foundBestScore)
                    {
                        foundBestScore = actualScore;
                        foundBestPosition = transform.position;
                    }
                }
            }

            directionToGlobal = (foundBestPosition - transform.position).normalized;
            destination = directionToGlobal;
        }
        //END WHEN CLOSE TO HIGHEST MOUNTAIN THINGS
        projectedDestination = Vector3.ProjectOnPlane(destination, transform.up);

        //Update trajectory
        float angle = Vector3.Angle(transform.forward, projectedDestination);

        //- => Izquierda
        if (trajectory >= 360) trajectory -= 360;
        if (trajectory <= -360) trajectory += 360;

        float rightAngle = Vector3.Angle(transform.right, projectedDestination);
        float leftAngle = Vector3.Angle(-transform.right, projectedDestination);

        if (rightAngle >= leftAngle)
        {
            if (transform.position.y >= 0)
            {
                trajectory -= angle;    //-
            }
            else
            {
                trajectory += angle;    //+
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
        latestTargetDirection = trajectory;
    }

    public void UpdateTrajectoryDirection(Vector3 goToDirection)
    {
        destination = goToDirection;

        projectedDestination = Vector3.ProjectOnPlane(destination, transform.up);

        //Update trajectory
        float angle = Vector3.Angle(transform.forward, projectedDestination);

        //- => Izquierda
        if (trajectory >= 360) trajectory -= 360;
        if (trajectory <= -360) trajectory += 360;

        float rightAngle = Vector3.Angle(transform.right, projectedDestination);
        float leftAngle = Vector3.Angle(-transform.right, projectedDestination);

        if (rightAngle >= leftAngle)
        {
            if (transform.position.y >= 0)
            {
                trajectory -= angle;    //-
            }
            else
            {
                trajectory += angle;    //+
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
        latestTargetDirection = trajectory;
    }

    private void ManageStepSound()
    {
        if (move && isGrounded())
        {
            //Continue steping
            if (!FindObjectOfType<AudioManager>().isPlayingStep(id, stepId))
            {
                timeBetweenStepsCounter += Time.deltaTime;
                if (timeBetweenStepsCounter > timeBetweenStepsToCount)
                {
                    timeBetweenStepsCounter = 0;
                    //DO THINGS EVERY timeBetweenStepsToCount SECONDS
                    if (stepId == 0)
                    {
                        stepId = 1;
                    }
                    else
                    {
                        stepId = 0;
                    }
                    FindObjectOfType<AudioManager>().PlayStep(id, stepId);
                }
            }
        }
        else
        {
            //Stop steps
            FindObjectOfType<AudioManager>().StopStep(id, 0);
            FindObjectOfType<AudioManager>().StopStep(id, 1);
        }
    }

    private void UpdateHealthPosition()
    {
        if(!this.HealthBar.activeInHierarchy)
        {
            HealthBar.SetActive(true);
        }
        Vector2 alienPos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        alienPos.y += 100f;
        this.HealthBar.transform.position = alienPos;
    }

    public void Move()
    {
        UpdateHealthPosition();
        if(dead)
        {
            move = false;
        }
        ManageStepSound();

        //CHECK IF IT IS STUCK AND ITS ALSO MOVING
        if (ItIsStuck() && move)
        {
            upComponent += transform.up;
        }
        else
        {
            upComponent = Vector3.zero;
        }

        currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref speedSmoothVelocity, speedSmoothTime);

        gravityDirection = (transform.position - attractor.transform.position).normalized;  //Vector3 ..

        //PERFORM ROTATIONS
        PerformGravityRotation();

        if (move) { PerformControllerRotation(); }

        //UPDATE HORIZONTAL TRANSLATION
        if (move && isGrounded())
        {
            transform.position += (transform.forward * currentSpeed + upComponent * currentSpeed) * Time.deltaTime;
        }
        //UPDATE VERTICAL TRANSLATION
        //rigidbody.AddForce(-gravityDirection * velocityCteY);
        rigidbody.velocity = -gravityDirection * velocityCteY;

        float targetDirection = trajectory;
        latestTargetDirection = targetDirection;

        if (move && isGrounded())
        {
            float animationSpeedPercent = (10f / maxSpeed) * currentSpeed / 10f;              //running ? 1 : 0.5f;
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("speedPercent", 0f, speedSmoothTime, Time.deltaTime);
        }
    }

    public void SetMove(bool condition)
    {
        this.move = condition;
    }

    public void Stop()
    {
        this.move = false;
        animator.SetFloat("speedPercent", 0f, speedSmoothTime, Time.deltaTime);

        gravityDirection = (transform.position - attractor.transform.position).normalized;
        //PERFORM ROTATIONS
        PerformGravityRotation();
        //UPDATE VERTICAL TRANSLATION
        //rigidbody.AddForce(-gravityDirection * velocityCteY);
        rigidbody.velocity = -gravityDirection * velocityCteY;
    }

    bool isGrounded()
    {
        return Physics.Raycast(collider.bounds.center, -gravityDirection, distToGround + distToGround / 4);
    }
}
