using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressManager : MonoBehaviour {

    //BODY CHARACTERISTICS
    
    /// <summary>
    /// the gameobject to be moved (eg. Firefly gameobject)
    /// </summary>
    [SerializeField]
    private GameObject Body;

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
    /// in order to reset the list of key presses once enough
    /// time has passed (List Memory)
    /// </summary>
    private float TimeOfLastKeyPress;

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
    private bool FinishedMoving;
    
    // Use this for initialization
    void Start ()
    {
        BodyRB = Body.GetComponent<Rigidbody2D>();
        // inherit all keys with the characteristics of the current body
        EndowChildren();
        FinishedMoving = true;
        if (ForceManualAdjustmentFactor == 0)
        {
            Debug.Log("Hey! Force Manual Adjustment Factor is 0");
        }

    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        // set the time of the last key press
        // so the list can be reset if no key was pressed for the duration of List Memory Time
        if (Input.anyKeyDown)
        {
            DidResetList = false;
            TimeOfLastKeyPress = Time.time;
        }
        if (FinishedMoving)
        {
            //reset accumulative force
            AccumulativeGestureForce = 0;   
        }
        else
        {
            //continue moving towards target
            SmoothMoveTowardsTarget();
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
                CalculateDirectionForceNormalization(force, singlekeyname);
                print(NormalizedSinglekeyForce + "force to the direction of" + Direction);
                MoveTarget();
            }
    }
    /// <summary>
    /// as a key is pressed, check if it was pressed
    /// after the timespan of a gesture
    /// if so, reset the list (start a new gesture)
    /// </summary>
    void CheckGestureEndAndResetList()
    {
            float timeSinceLastKeypress = Time.time - TimeOfLastKeyPress;
            //reset list if there has been more time
            //since the last key has been pressed than the list memory
            //and you haven't yet resetted the list(DidResetList)
            if (timeSinceLastKeypress > TimeBetweenGestures && !DidResetList)
            {
                Keypresses.Clear();
                // to make sure you reset only once
                DidResetList = true;
            }
    }


    /// <summary>
    /// Calculates Normalized Force and direction (according to previous press)
    /// </summary>
    void CalculateDirectionForceNormalization(float force, string nameFromSingleKey)
    {
        print(force);   
        // calculated force by multiplying Raw Force with a manually adjustable factor for fine tuning
        NormalizedSinglekeyForce = force * ForceManualAdjustmentFactor;
        AccumulativeGestureForce += force * ForceManualAdjustmentFactor;
        // calculates direction of the last two key presses
        int KeypressCount = Keypresses.Count;
        Direction = Keypresses[KeypressCount - 1].position - Keypresses[KeypressCount - 2].position;
        Direction  = NormalizeDirection(Direction);
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
        }
    }


    /// <summary>
    /// moves target for Body (e.g. Firefly) to move to
    /// based on the direction of the press and the distance, based on the force
    /// </summary>
    /// <param name="body"></param>
    /// <param name="target"></param>
    /// <param name="speed"></param>
    void MoveTarget()
    {
        Target.transform.position = Body.transform.position + ((Vector3)Direction * NormalizedSinglekeyForce);
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
}
