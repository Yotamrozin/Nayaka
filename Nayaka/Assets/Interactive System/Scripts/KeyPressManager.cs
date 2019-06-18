using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressManager : MonoBehaviour {

    //BODY CHARACTERISTICS
    
    /// <summary>
    /// the gameobject to be moved (eg. Firefly gameobject)
    /// </summary>
    public GameObject Body;

    /// <summary>
    /// The rigidbody component of Body
    /// only used for bodies that are moved with physics
    /// </summary>
    private Rigidbody2D BodyRB;

    /// <summary>
    /// Target for following
    /// </summary>
    [SerializeField]
    private GameObject Target;
    
        
    /// <summary>
    /// how many times would a key like to be pressed
    /// </summary>
    [SerializeField]
    private int DesiredClicks = 1;
    /// <summary>
    /// How long does it take for excitement/inhibition, scale, and color to 
    /// go back to original values
    /// </summary>
    [SerializeField]
    private float DecayTime = 10;

    /// <summary>
    /// Desired time for a key to be held
    /// </summary>
    [SerializeField]
    private float DesiredHoldingDuration = 0.3f;

    /// <summary>
    /// Desired time before a key should be pressed again
    /// </summary>
    [SerializeField]
    private float DesiredRestBetweenClicks = 1;

    /// <summary>
    /// The size of the sphere in which any other keys (neighbors)
    /// would be excited or inhibited by the current key
    /// </summary>
    [SerializeField]
    private float AreaOfEffect = 2;

    /// <summary>
    /// The measure in which this key will affect its neighbors
    /// </summary>
    [SerializeField]
    private float NeighborEffectFactor =  0.5f;

    /// <summary>
    /// By how much should excitement rise or fall when a key is clicked
    /// </summary>
    [SerializeField]
    private float ClickForceIncrement = 0.4f;

    /// <summary>
    /// By how much should excitement rise or fall when key is held
    /// </summary>
    [SerializeField]
    private float HoldForceIncrement = 0.0001f;

    [SerializeField]
    private bool VisualizeKeyboard;

    ///-------------------

    /// <summary>
    /// An array of all the keys currently pressed
    /// </summary>
    //Key List
    public GameObject[] KeysPressed;

    /// <summary>
    /// A list of the last keys pressed
    /// </summary>
    public List<KeyPressData> Keypresses = new List<KeyPressData>();

    /// <summary>
    /// The time afterwhich the list of keys resets
    /// </summary>
    [SerializeField]
    private float TimeBetweenGestures = 5;

    /// <summary>
    /// used to make sure we don't reset the force more than one
    /// if no other keys have been pressed
    /// </summary>
    private bool DidResetForce;

    /// <summary>
    /// used to make sure we don't reset 
    /// the list of key presses more than one
    /// if no other keys have been pressed
    /// </summary>
    private bool DidResetList;

    /// <summary>
    /// stores the last time a key was pressed
    /// in order to calculate rest factor and 
    /// reset the list of key presses, once enough
    /// time has passed (List Memory).
    /// </summary>
    private float TimeOfLastKeyPress;

    /// <summary>
    /// stores the time between current press and previous one
    /// used to calculate rest and to end gesture
    /// time has passed (List Memory)
    /// </summary>
    private float TimePassedSinceLastPress;

    /// <summary>
    /// gives manual control to amplify or reduce 
    /// the Raw Force by a constant factor
    /// </summary>
    [SerializeField]
    private float ForceManualAdjustmentFactor;

    /// <summary>
    /// gives manual control to amplify or reduce 
    /// the Raw Force by a constant factor
    /// </summary>
    [SerializeField]
    private float AccumulativeForceManualAdjustmentFactor;

    /// <summary>
    /// the normalized force after multiplying the force from
    /// a single key with the manual adjustment factor.
    /// Used for changing target position
    /// </summary>
    public float NormalizedSinglekeyForce;

    /// <summary>
    /// the accumulative force used for speed
    /// </summary>
    private float AccumulativeGestureForce;

    /// <summary>
    /// a counter of how many keys are currently pressed
    /// Used to perform operations while some key is being pressed
    /// mostly, sending force
    /// </summary>
    public int NumberofKeysCurrentlyPressed;

    /// <summary>
    /// the direction of force
    /// </summary>
    public Vector2 Direction;

    /// <summary>
    /// cue for when the movement is done
    /// </summary>
    private bool isMoving;
    
    /// <summary>
    /// multiplies the force to amplify or inhibit the movement
    /// according to the desired rest
    /// </summary>
    private float RestFromClickFactor;

    /// <summary>
    /// the distance between the last two keys pressed
    /// </summary>
    private float KeyDistance;

    /// <summary>
    /// the distance between the last two keys pressed
    /// normalized to make the fartherst keys (12.17~) = 0
    /// and the closest keys (0.7~) = 1
    /// </summary>
    private float KeyDistanceFactor;

    [SerializeField]
    private float MaximumDistance = 6f;

    //-----------
    // Lerp Variables
    /// <summary>
    /// Stores the time lerp started for calculating percentage done
    /// </summary>
    private float TimeLerpStarted;

    /// <summary>
    /// Stores the time past since lerp started for calculating percentage done
    /// </summary>
    private float timeSinceLerpStarted;

    /// <summary>
    /// percentage of the lerp already done
    /// </summary>
    private float percentageComplete;

    /// <summary>
    /// Time it takes to Lerp - 
    /// Practically the speed of the Lerp
    /// </summary>
    private float TimeofLerp;

    /// <summary>
    /// Time of Lerp Changes the speed of the lerp towards the Target
    /// This is the maximum range for Randomizing it
    /// </summary>
    public float MaxTimeOfLerpRange = 36f;

    /// <summary>
    /// Time of Lerp Changes the speed of the lerp towards the Target
    /// This is the minimum range for Randomizing it
    /// </summary>
    public float MinTimeOfLerpRange = 34f;

    /// <summary>
    /// flag bool to know when lerping is underway
    /// </summary>
    private bool isLerping;

    // Use this for initialization
    void Start ()
    {
        BodyRB = Body.GetComponent<Rigidbody2D>();
        // inherit all keys with the characteristics of the current body
        EndowChildren();
        isMoving = false;
        if (ForceManualAdjustmentFactor == 0)
        {
            Debug.Log("Hey! Force Manual Adjustment Factor is 0");
        }
        

    }
	
	// Update is called once per frame
	void Update ()
    {
        //operations that happen as long as there is an active dance (the player is moving the firefly)
        //Check if reached destination
        if (isMoving)
        { 
            //if reached destination
            if (Vector2.Distance(Body.transform.position, Target.transform.position) < 0.1f)
            {
                isMoving = false;
                AccumulativeGestureForce = 0;
                Body.GetComponent<RandomFly>().ResetRandomFlight();
                Body.GetComponent<RandomFly>().ShouldRandomlyFly = true;
                Body.GetComponent<RandomPhysicsFly>().enabled = false;

            }
            else
            {
                //continue moving towards target
                Body.GetComponent<RandomFly>().ShouldRandomlyFly = false;
                SmoothMoveTowardsTarget();
            }
        }
        CheckGestureEndAndResetList();
    }

    /// <summary>
    /// Called from single keys and works only after the second key in the gesture is pressed.
    /// Perform the following functions:
    /// 1. Normalize Force (recieved from single key) 
    /// 2. Calculate direction (using normalized force and last two key presses)
    /// 3. Move target according to direction and force
    /// </summary>
    /// <param name="force"></param>
    public void ApplyForce(float force, string singlekeyname)
    {
        
        if (Keypresses.Count > 1)
        {
            //repeting the same key twice doesn't add force
        bool shoudApplyForce = CheckRepetition();   
            if (shoudApplyForce)
            {
                Body.GetComponent<RandomFly>().ShouldRandomlyFly = false;
                //Body.GetComponent<RandomFly>().enabled = false;
                CalculateNomalizedDirectionAndForce(force, singlekeyname);
                MoveTarget();
            }
        }
    }

    bool CheckRepetition()
    {
        bool shouldapplyforce = false;
        //check that it's not a repetition
        if (Keypresses[Keypresses.Count-1].Key != Keypresses[Keypresses.Count - 2].Key)
        {
            shouldapplyforce = true;
            //GameObject[] neighbors = Keypresses[Keypresses.Count - 1].Key.GetComponent<SingleKey>().NeighboringKeys;
            //foreach (GameObject neighbor in neighbors)
            //{
            //    if (!shouldapplyforce)
            //    {
            //        if (neighbor != Keypresses[Keypresses.Count - 1].Key)
            //        {
            //            shouldapplyforce = true;
            //            print("key pressed is a neighbor");
            //        }
            //    }
            //}
        }
        else
        {
            print("repeting key");
        }

        return shouldapplyforce;
    }
    /// <summary>
    /// as a key is pressed, check if it was pressed
    /// after the timespan of a gesture
    /// if so, reset the list (start a new gesture)
    /// </summary>
    void CheckGestureEndAndResetList()
    {
         if(!DidResetList)
        { 
            float timeSinceLastKeypress = Time.time - TimeOfLastKeyPress;
            //reset list if there has been more time
            //since the last key has been pressed than the list memory
            //and you haven't yet resetted the list(DidResetList)
            if (timeSinceLastKeypress > TimeBetweenGestures)
            {
                Keypresses.Clear();
                // to make sure you reset only once
                DidResetList = true;
                print("gesture reset");
            }
        }
    }


    /// <summary>
    /// Calculates Normalized Force and direction (according to previous press)
    /// </summary>
    void CalculateNomalizedDirectionAndForce(float force, string nameFromSingleKey)
    {
        print(force);

        //calculate rest factor and multiply force
        CalculateRestFactor();
        float restedforce = force*RestFromClickFactor;
        
        // calculated force by multiplying Raw Force with a manually adjustable factor for fine tuning
        NormalizedSinglekeyForce = restedforce * ForceManualAdjustmentFactor;
        AccumulativeGestureForce += restedforce * ForceManualAdjustmentFactor;
        
        // calculates direction of the last two key presses
        int KeypressCount = Keypresses.Count;
        Direction = Keypresses[KeypressCount - 1].position - Keypresses[KeypressCount - 2].position;

        //measuring distance to factor 
        SetKeyDistanceAndKeyDistanceFactor(Keypresses[KeypressCount - 1].position, Keypresses[KeypressCount - 2].position);
        
        //Direction  = NormalizeDirection(Direction);
        print(Direction);

    }

    /// <summary>
    /// Normailzes direction to represent absolute directions
    /// i.e (1,0), (-1. 1), (0, -1) etc....
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    Vector2 NormalizeDirection(Vector2 direction)
    {
        if (direction.x < 0)
        {
            direction.x = -1;
            print("direction is left");
        }
        else if (direction.x > 0)
        {
            direction.x = 1;
            print("direction is right");
        }
        if (direction.y < 0)
        {
            direction.y = -1;
            print("direction is down");
        }
        else if (direction.y > 0)
        {
            direction.y = 1;
            print("direction is up");
        }
        return direction;
    }

    /// <summary>
    /// the distance between the last two keys pressed
    /// normalized to make the fartherst keys (12.17~) = 0
    /// and the closest keys (0.7~) = 1
    /// </summary>
    /// <param name="Key1"></param>
    /// <param name="Key2"></param>
    void SetKeyDistanceAndKeyDistanceFactor(Vector2 Key1, Vector2 Key2)
    {
        KeyDistance = Vector2.Distance(Key1, Key2);
        //very distant keys should no be very effective
        float ClampedDistnace = Mathf.Clamp(KeyDistance, 0, MaximumDistance);
        float NormalizedDistance = 1f - Mathf.InverseLerp(0.8f, MaximumDistance, ClampedDistnace);
        KeyDistanceFactor = Mathf.SmoothStep(0, 1, NormalizedDistance);
    }
    /// <summary>
    /// inherit all the single keys with all the characteristics of the body
    /// </summary>
    void EndowChildren()
    {
       foreach(Transform child in transform)
        {
            child.GetComponent<SingleKey>().DesiredNumberOfClicks = DesiredClicks;
            child.GetComponent<SingleKey>().DecayTime = DecayTime;
            child.GetComponent<SingleKey>().DesiredHoldingDuration = DesiredHoldingDuration;
            child.GetComponent<SingleKey>().DesiredRest = DesiredRestBetweenClicks;
            child.GetComponent<SingleKey>().AreaOfEffect = AreaOfEffect;
            child.GetComponent<SingleKey>().NeighborEffectFactor = NeighborEffectFactor;
            child.GetComponent<SingleKey>().ExcitementClickIncrement = ClickForceIncrement;
            child.GetComponent<SingleKey>().ExcitementKeyHoldIncrement = HoldForceIncrement;
            child.GetComponent<SingleKey>().VisualizeKeyboard = VisualizeKeyboard;
            if (!VisualizeKeyboard)
            {
                child.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }


    /// <summary>
    /// moves target for Body (e.g. Firefly) to move to
    /// based on the direction of the key press and the distance, based on the force
    /// </summary>
    /// <param name="body"></param>
    /// <param name="target"></param>
    /// <param name="speed"></param>
    void MoveTarget()
    {
        print("Key Distance:" + KeyDistance);
        print("Key Distance Factor: " + KeyDistanceFactor);
        print("Rest from click factor: " + RestFromClickFactor);
        print("Normalized Force (with rest): " + NormalizedSinglekeyForce);
        print("Direction: " + Direction);
    
        Target.transform.position = Body.transform.position + ((Vector3)Direction * NormalizedSinglekeyForce * KeyDistanceFactor);
        isMoving = true;
        //turn on random physics flight
        Body.GetComponent<RandomPhysicsFly>().enabled = true;
    }

    /// <summary>
    /// Smoothly moves the Body of the Devaru towards the Target
    /// </summary>
    void SmoothMoveTowardsTarget()
    {
        Vector3 desiredposition = Target.transform.position;
        Vector3 smoothposition = Vector3.Lerp(Body.transform.position, desiredposition,
        Body.GetComponent<Devaru>().speed * NormalizedSinglekeyForce * Time.deltaTime);
        Body.transform.position = smoothposition;
        
    }


    /// <summary>
    /// Move Body With Physics Force
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    void PhysicsMove(Vector2 direction, float force)
    {
        BodyRB.AddForce(direction * force, ForceMode2D.Impulse);
        print("added force in direction " + direction + " " + force);
    }
    public void CalculateRestFactor()
    {
        //calculate difference between this press and previous one before
        // overriding / becoming the LastKeyPressed
        TimePassedSinceLastPress = Time.time - TimeOfLastKeyPress;

        // overriding / becoming the LastKeyPressed
        TimeOfLastKeyPress = Time.time;
        DidResetList = false;

        print("the time past since last press is " + TimePassedSinceLastPress);

            if (TimePassedSinceLastPress < DesiredRestBetweenClicks)
            {
                
                //InverseLerp calculates where 
                //TimePassedSinceLastPress is between 0 rest and Desired Rest
                //It is multiplied by 2 and subtracted 1 in order to make the range between -1 and 1
                //this wil be useful to be inhibiting as well as exciting
                RestFromClickFactor = Mathf.InverseLerp(0, DesiredRestBetweenClicks, TimePassedSinceLastPress);
                print("rest factor calculated: " + RestFromClickFactor);
            }
            else
            {
                RestFromClickFactor = 1;
                print("time passed is bigger than desired, rest is "+RestFromClickFactor);
        }
    }
}
