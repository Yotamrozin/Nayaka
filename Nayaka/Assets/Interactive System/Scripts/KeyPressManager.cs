using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressManager : MonoBehaviour {
    
    //BODY CHARACTERISTICS
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
    private float ListMemoryTime = 5;

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
    /// the Raw Force accumulated from single keys 
    /// sending their impact to the keypress manager
    /// </summary>
    public float RawForce;

    /// <summary>
    /// gives manual control to amplify or reduce 
    /// the Raw Force by a constant factor
    /// </summary>
    [SerializeField]
    private float ForceManualAdjustmentFactor;

    /// <summary>
    /// the normalized force after multiplying the raw force with the manual adjustment factor
    /// </summary>
    public float NormalizedForce;

    /// <summary>
    /// a counter of how many keys are currently pressed
    /// Used to perform operations while some key is being pressed
    /// mostly, sending force
    /// </summary>
    public int NumberofKeysCurrentlyPressed;

    /// <summary>
    /// the direction of force
    /// </summary>
    public Vector2 direction;

    // Use this for initialization
    void Start ()
    {
        // inherit all keys with the characteristics of the current body
        EndowChildren();

        // no need to reset force and list at the beginning of the game
        DidResetForce = true;
        DidResetList = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // set the time of the last key press
        // so the list can be reset if no key was pressed for the duration of List Memory Time
        if (Input.anyKeyDown)
        {
            TimeOfLastKeyPress = Time.time;
        }

        // Sends force as long as some key is pressed
        if (NumberofKeysCurrentlyPressed > 0)
        {
            // calculated force by multiplying Raw Force with a manually adjustable factor for fine tuning
            NormalizedForce = RawForce * ForceManualAdjustmentFactor;
            // calculates direction of the last two key presses
            int KeypressCount = Keypresses.Count;
            direction = Keypresses[KeypressCount-1].position - Keypresses[KeypressCount-2].position;
            //as long as keys a pressed, do not reset
            DidResetForce = false;
            DidResetList = false;
            //@DEBUG
            print("applied force is" + NormalizedForce);
        }
        // when no key is pressed, reset force
        else
        {
            //reset force only once, as long as there hasn't been any new presses
            if (!DidResetForce)
            {
                RawForce = 0;
                NormalizedForce = 0;
                // to make sure you reset only once
                DidResetForce = true;
            }

            //reset key press list, as long as there hasn't been any new presses
            if (!DidResetList)
            {
                float timeSinceLastKeypress = Time.time - TimeOfLastKeyPress;
                //reset list if there has been more time
                //since the last key has been pressed than the list memory
                if (timeSinceLastKeypress > ListMemoryTime)
                    { 
                        Keypresses.Clear();
                        // to make sure you reset only once
                        DidResetList = true;
                    }
            }
        }
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
}
