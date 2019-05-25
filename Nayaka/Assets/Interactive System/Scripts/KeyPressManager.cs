using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressManager : MonoBehaviour {
    //Organism preferences
    [SerializeField]
    private int DesiredClicks = 1;
    [SerializeField]
    private float DecayTime = 10;
    [SerializeField]
    private float DesiredHoldingDuration = 0.3f;
    [SerializeField]
    private float DesiredRestBetweenClicks = 1;
    [SerializeField]
    private float AreaOfEffect = 2;
    [SerializeField]
    private float NeighborEffectFactor =  0.5f;
    [SerializeField]
    private float ClickForceIncrement = 0.4f;
    [SerializeField]
    private float HoldForceIncrement = 0.0001f;



    //Key List
    public GameObject[] KeysPressed;
    public List<KeyPressData> Keypresses = new List<KeyPressData>();
    [SerializeField]
    private float ListMemoryTime = 5;

    //reseting
    private bool didReset;
    private float TimeOfLastKeyPress;

    //force translations
    public float RawForce;
    public float NormalizedForce;
    [SerializeField]
    private float NormalizingForceFactor;

    public bool SomeKeyisPressed;
    public int NumberofKeysCurrentlyPressed;

    public Vector2 direction;

    // Use this for initialization
    void Start () {
        NumberofKeysCurrentlyPressed = 0;
        TimeOfLastKeyPress = 0;
        didReset = true;

    }
	
	// Update is called once per frame
	void Update () {
        
       if (NumberofKeysCurrentlyPressed > 0)
        {
            TimeOfLastKeyPress = Time.time;
            NormalizedForce += RawForce * NormalizingForceFactor;

            print(Keypresses.Count + "keypresses in list");
            int KeypressCount = Keypresses.Count;
            direction = Keypresses[KeypressCount-1].position - Keypresses[KeypressCount-2].position;
            print("applied force is" + NormalizedForce);
            didReset = false;
        }
       else
        {
            //reset
            if (!didReset)
            {
                RawForce = 0;
                NormalizedForce = 0;
              
                didReset = true;
                float timeSinceLastKeypress = Time.time - TimeOfLastKeyPress;
                if (timeSinceLastKeypress > ListMemoryTime)
                    Keypresses = new List<KeyPressData>();  
                TimeOfLastKeyPress = Time.time;
            }
            
        }

	}
    void EndowChildren()
    {
       foreach(Transform child in transform)
        {
            child.GetComponent<SingleKey>().DesiredClicks = DesiredClicks;
            child.GetComponent<SingleKey>().DecayTime = DecayTime;
            child.GetComponent<SingleKey>().DesiredHoldingDuration = DesiredHoldingDuration;
            child.GetComponent<SingleKey>().DesiredRest = DesiredRestBetweenClicks;
            child.GetComponent<SingleKey>().areaOfEffect = AreaOfEffect;
            child.GetComponent<SingleKey>().NeighborEffectFactor = NeighborEffectFactor;
            child.GetComponent<SingleKey>().ExcitementClickIncrement = ClickForceIncrement;
            child.GetComponent<SingleKey>().ExcitementKeyHoldIncrement = HoldForceIncrement;
        }
    }
}
