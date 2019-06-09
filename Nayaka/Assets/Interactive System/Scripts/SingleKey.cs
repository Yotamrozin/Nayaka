//The single key has .... 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleKey : MonoBehaviour
{

    //--------------------
    //Keyboard
    /// <summary>
    /// Enter Keycode this Gameobject is representing
    /// </summary>
    public KeyCode thisKeyCode;

    /// <summary>
    /// General  Key Press Manager to handle the single keys
    /// </summary>
    public KeyPressManager m_KeyPressManager;



    //--------------------
    //INHERITED FROM BODY

    /// <summary>
    /// excitement (0<x<1) or inhibition (-1<x<0).
    /// </summary>
    public float Excitement;

    /// <summary>
    /// Time for the key to reset its excitement or inhibition
    /// </summary>
    public float DecayTime;

    /// <summary>
    ///  the desired stimulation
    /// </summary>
    [Range(0f, 2f)]
    public float DesiredHoldingDuration;

    /// <summary>
    /// Number of Clicks this key expects
    /// </summary>
    public int DesiredNumberOfClicks;

    /// <summary>
    /// Time this key desires to rest between clicks
    /// </summary>
    public float DesiredRest;

    /// <summary>
    /// How much rest this key received from -1 to 1
    /// Serves as a factor for excitement or inhibition
    /// </summary>
    private float RestFromClickFactor;




    //---------------------------------
    //Handling Neighbours

    /// <summary>
    ///     //how many keys to each direction does the pressing of the key affect
    /// </summary>
    public float AreaOfEffect;

    /// <summary>
    /// How much excitement or inhibition is the key going to transfer to neighbours
    /// </summary>
    public float NeighborEffectFactor;

    /// <summary>
    /// Stores all neighboring keys
    /// </summary>
    public GameObject[] NeighboringKeys;



    //----------------------------

    //Excitement measurements
    /// <summary>
    /// How much does a click effect excitement
    /// </summary>
    public float ExcitementClickIncrement = 0.1f;

    /// <summary>
    /// how much does a frame of holding effect excitement
    /// </summary>
    public float ExcitementKeyHoldIncrement = 0.001f;



    //----------------------
    //Orignal values saved for resetting and decay

    /// <summary>
    /// Original Excitement saved for decay and resetting
    /// </summary>
    private float OriginalExcitement;

    /// <summary>
    /// Original Scale x saved for decay and resetting
    /// </summary>
    private float OriginalScalex;

    /// <summary>
    /// Original Scale y saved for decay and resetting
    /// </summary>
    private float OriginalScaley;



    //---------------------------
    //Local operational private variables

    /// <summary>
    /// used for duration of key  holding
    /// </summary>
    private bool isPressed;

    /// <summary>
    /// Duration of key holding
    /// </summary>
    private float DurationOfHolding;

    /// <summary>
    /// Counting the number of consecutive clicks 
    /// </summary>
    private int numberOfClicks;

    /// <summary>
    /// Last Time this key was pressed
    /// </summary>
    private float LastPress;

    /// <summary>
    /// Used for Lerping the decay over time
    /// </summary>
    private float TimeLerpStarted;



//--------------------------------------------------------------------------------------
    // Use this for initialization
    void Start()
    {
        isPressed = false;
        LastPress = 0;
        OriginalScalex = transform.localScale.x;
        OriginalScaley = transform.localScale.y;

        //save the original excitement for resetting and decay
        OriginalExcitement = Excitement;

        DetectNeighbors();
    }


    // Update is called once per frame
    void Update()
    {
        //Handles Click
        KeyDownHandling();
        CheckIfKeyUpAndReset();
        // #TEMP CANCELED KEY HOLDING
        //if (isPressed)
        //{

        //    //handle keyholding by checking if holdtime is within keyhold desired duration
        //    KeyHolding();
        //    //handles the effect on neighboring keys
        //    ExciteOrInhibitNeighbors();
        //    //handles resetting when key is up
            CheckIfKeyUpAndReset();

        //}

        //whenever the key is not pressed 
        //check whether excitement, number of clicks, and scale are all reset
        //and if not, apply decay
        if (!isPressed)
        {
            DecayExcitementToOriginalValue();
            DecayNumberOfClicksToOriginalValue();
            DecayRestScalingToOriginalValue();
        }
        //converts excitement to a change of the key color
        //used for debugging
        ExcitementToColor();
    }


    //---------------------------------------------------------
    // MAIN KEY PRESS EVENTS
    // 3 functions handling 3 main events: Key down, Key holding, and Key Up

    /// <summary>
    /// ON KEY DOWN - calculate rest, add +1 to number of pressed keys
    /// adds click excitement and sends it to the RawForce of the Key Press manager
    /// +1 for the number of clicks this key was pressed, sets isPressed to true
    /// and adds this key press to the list
    /// </summary>
    private void KeyDownHandling()
    {
        if (Input.GetKeyDown(thisKeyCode))
        {
            isPressed = true;
            // +1 to number of keys pressed
            m_KeyPressManager.NumberofKeysCurrentlyPressed += 1;

            //#TEMP CANCELED REST
            CalculateRestFactor();

            // visualizes the amount
            // of rest by changing
            // the scale of the key gameobject
            RestFactorToScale();

            LastPress = Time.time;

            //@TEMP CANCEL REST FACTOR
            //changes the excitement of the key by the click increment 
            //and according to the rest factor (if it rested enough from being clicked
            //Excitement += ExcitementClickIncrement;
            //*RestFromClickFactor;

            //sends excitement*restfactor as force to the KeyPress Manager's ApplyForce Function 
            SendKeypressDataToList();
            m_KeyPressManager.ApplyForce(ExcitementClickIncrement, name);

            //adds +1 to number of clicks to later check with the desired number of clicks
            numberOfClicks += 1;

            //@TEMP CANCEL HOLDING
            //used to perform operations as long as the key is held
            //isPressed = true;

            DurationOfHolding = 0;

        }
    }



    /// <summary>
    /// KEY HOLD
    /// calculate duration and excite or inhibit 
    /// according to desired time
    /// Sends force to the body
    /// </summary>
    private void KeyHolding()
    {
        if (isPressed)
        {
            DurationOfHolding = Time.time - LastPress;
            Excitement = 0;
            // holding happens when it is longer than 0.1 sec
            // it is exciting if it is within the desired duration
            // and only if it happens on a well rested key
            if (0.1f < DurationOfHolding && DurationOfHolding < DesiredHoldingDuration && RestFromClickFactor > 0)
            {
                if (Excitement < 1)
                {
                    m_KeyPressManager.ApplyForce(ExcitementKeyHoldIncrement, name);
                }
                else
                {
                    Debug.Log("reached maximum");
                    Excitement = 1;
                }
            }
            // handles an inhibiting holding of the key
            // holding is inhibiting if it went on for too long or happens on an unrested key
            // the measure of inhibition of an unrested key is affected by the RestFactor
            // sends negative force to body.
            if (DurationOfHolding > DesiredHoldingDuration || RestFromClickFactor < 0)
            {
                // reduce excitement by the keyhold increment (effected by the rest factor)
                if (Excitement > -1)
                {
                        m_KeyPressManager.ApplyForce(ExcitementKeyHoldIncrement, name);
                }
                // normalize excitement to above -1
                else
                {
                    Debug.Log("reached minimum");
                    Excitement = -1;
                }
            }
        }
    }



    /// <summary>
    /// handles what happens when Key is Up
    /// </summary>
    private void CheckIfKeyUpAndReset()
    {
        if (Input.GetKeyUp(thisKeyCode))
        {
            //key is no longer pressed
            
            isPressed = false;
            //reduce 1 from the number of pressed keys
            m_KeyPressManager.NumberofKeysCurrentlyPressed -= 1;
            //resets lerp time (is this needed?)
            TimeLerpStarted = Time.time;
            //changes holding status of key on the list
            HeldStatusToFalseInKeypressesList();
            //@DEBUG
            //print(thisKeyCode + "is up, the number of pressed keys is" + m_KeyPressManager.NumberofKeysCurrentlyPressed);
        }
    }



    //---------------------------------
    //DECAY FUNCTIONS
    /// <summary>
    /// Decay excitement to original value
    /// </summary>
    public void DecayExcitementToOriginalValue()
    {
        if (Excitement != OriginalExcitement)
        {
            Excitement = SmoothStep(Excitement, OriginalExcitement, DecayTime);
        }
        //Excitement goes crazy when reaching very low values
        //therefore we let it drop suddenly when it is close to zero
        if (Mathf.Abs(Excitement - OriginalExcitement)<0.01)
        {
            Excitement = OriginalExcitement;
        }
    }
    /// <summary>
    /// Decay Number Of Clicks To Original Value
    /// </summary>
    public void DecayNumberOfClicksToOriginalValue()
    {
        if (numberOfClicks != 0)
        {
            numberOfClicks = (int)SmoothStep(numberOfClicks, 0, DecayTime);
        }
    }
    /// <summary>
    /// Decay Rest Scaling To Original Value
    /// </summary>
    public void DecayRestScalingToOriginalValue()
    {
        if (OriginalScalex != transform.localScale.x)
        {
            Vector3 newscale = transform.localScale;
            newscale.x = SmoothStep(transform.localScale.x, OriginalScalex, DesiredRest);
            transform.localScale = newscale;
        }
        if (OriginalScalex != transform.localScale.y)
        {
            Vector3 newscale = transform.localScale;
            newscale.y = SmoothStep(transform.localScale.y, OriginalScaley, DesiredRest);
            transform.localScale = newscale;
        }
    }

   
 
    /// <summary>
    /// calculates how much rest the key got from -1 to 1
    /// this will be used to affect excitement and inhibition 
    /// </summary>
    public void CalculateRestFactor()
    {
        float TimePassedSinceLastPress = Time.time - LastPress;
        if(numberOfClicks < DesiredNumberOfClicks)
        { 
            if (TimePassedSinceLastPress < DesiredRest)
            {
                //InverseLerp calculates where 
                //TimePassedSinceLastPress is between 0 rest and Desired Rest
                //It is multiplied by 2 and subtracted 1 in order to make the range between -1 and 1
                //this wil be useful to be inhibiting as well as exciting
                RestFromClickFactor = (Mathf.InverseLerp(0, DesiredRest, TimePassedSinceLastPress)*2)-1;
            }
            else
            {
                RestFromClickFactor = 1;
            }
        }
        else
        {
            RestFromClickFactor = 0;
        }
    }   
    
    
    
    //----------------
    //DEBUGGING SIMULATIONS

    /// <summary>
    /// Make the excitement of a key change its color from white to black
    /// </summary>
    public void ExcitementToColor()
    {
        Color color = GetComponent<MeshRenderer>().material.color;
        color.g = (Excitement + 1) / 2;
        color.r = (Excitement + 1) / 2;
        color.b = (Excitement + 1) / 2;
        GetComponent<MeshRenderer>().material.color = color;
    }


    /// <summary>
    /// Visualizes Rest by changing the key gameobject's scale - for debugging
    /// </summary>
    public void RestFactorToScale()
    {
//        print("scaling");
        float NormalizedRestFactor = (RestFromClickFactor + 1);
        transform.localScale = new Vector3(OriginalScalex* NormalizedRestFactor, OriginalScalex * NormalizedRestFactor, OriginalScalex * NormalizedRestFactor);
    }



    //-----------------------------
    //Functions handling the storing of the key press in the Keypress Data List
    /// <summary>
    /// Sends position, excitement, and pressed status to the Keypress Data List
    /// </summary>
    private void SendKeypressDataToList()
    {
        m_KeyPressManager.Keypresses.Add( new KeyPressData(this, transform.position, Excitement, isPressed, gameObject));
    }


    /// <summary>
    /// Changes the isHeld status of the key in the Keypress Data List to False when holding is finished
    /// </summary>
    private void HeldStatusToFalseInKeypressesList()
    { 
        
        int length = m_KeyPressManager.Keypresses.Count;
        if (length > 0)
            m_KeyPressManager.Keypresses[length - 1].isHeld = false;
    }



    //Functions handling Neighbors
    /// <summary>
    /// detects the gameobjects of neighbors using Physics.OverlapSphere
    /// </summary>
    private void DetectNeighbors()
    {
        //detects Neigbors, but skips itself

        if (AreaOfEffect > 0)
        {
            Collider[] neighborColliders = Physics.OverlapSphere(transform.position, AreaOfEffect);
            NeighboringKeys = new GameObject[neighborColliders.Length - 1];
            int i = 0;
            bool foundItself = false;
            // print(neighborColliders.Length);
            while (i < neighborColliders.Length - 1)
            {
                if (neighborColliders[i].gameObject == this.gameObject)
                {
                    foundItself = true;
                }
                if (foundItself)
                {
                    NeighboringKeys[i] = neighborColliders[i + 1].gameObject;
                    //                    print(NeighboringKeys[i].name);
                    i++;
                }
                else
                {
                    NeighboringKeys[i] = neighborColliders[i].gameObject;
                    //   print(NeighboringKeys[i].name);
                    i++;
                }
            }
            //            print(NeighboringKeys.Length);
        }
    }


    
    private void ExciteOrInhibitNeighbors()
    {
        if(NeighboringKeys != null)
        { 
            foreach (GameObject neighbor in NeighboringKeys)
            {
                SingleKey neighborKey = neighbor.GetComponent<SingleKey>();
                neighborKey.Excitement += (Excitement-OriginalExcitement)*NeighborEffectFactor;

                //normalize neighbor excitement between -1 and 1
                if (neighborKey.Excitement > 1)
                    neighborKey.Excitement = 1;
                if (neighborKey.Excitement < -1)
                    neighborKey.Excitement = -1;

                
            }
        }
        else
            Debug.Log("no neighbors?");
    }


    //-------------
    // smooth lerp for decay
    /// <summary>
    /// Smooth Step in time (for decay)
    /// </summary>
    /// <param name="LerpStartValue"></param>
    /// <param name="LerpEndValue"></param>
    /// <param name="LerpTime"></param>
    /// <returns></returns>
    public float SmoothStep(float LerpStartValue, float LerpEndValue, float LerpTime)
    {
        float TimeSinceStarted = Time.time - TimeLerpStarted;
        float PercentageComplete = TimeSinceStarted / LerpTime;
        var result = Mathf.SmoothStep(LerpStartValue, LerpEndValue, PercentageComplete);
        return result;
    }
}

