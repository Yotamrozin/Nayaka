//The single key has .... 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleKey : MonoBehaviour
{
    
    public KeyCode thisKeyCode;
    public KeyPressManager m_KeyPressManager;

    //INHERITED FROM BODY

    //excitement (0<x<1) or inhibition (-1<x<0).
    public float BaseForce;
    public float Excitement;
    public float DecayTime;


    //the desired stimulation
    //will be converted to duration/number of clicks the key wishes to be pressed 
    //after which it goes from excitement to inhibition
    //should i make key holding, number of clicks, and resting time codependent?
    [Range(0f, 2f)]
 //   public float DesiredStimulation;
    public float DesiredHoldingDuration;
    public int DesiredClicks;
    public float DesiredRest;
    private float RestFromClickFactor;

    //how many keys to each direction does the pressing of the key affect
    public float areaOfEffect;
    public float NeighborEffectFactor;

    //--------------

    //BACK PROPAGATION
    public float force = 0;
    public float ExcitementClickIncrement = 0.1f;
    public float ExcitementKeyHoldIncrement = 0.1f;

    private float OriginalExcitement;
    private bool isPressed; //used for duration of press
    private float duration;
    private int numberOfClicks;
    private float LastPress;
    private Color MaterialColor;
    private float OriginalScalex;
    private float OriginalScaley;
    private GameObject[] NeighboringKeys;

    // Use this for initialization
    void Start()
    {

        isPressed = false;
        LastPress = 0;
        OriginalScalex = transform.localScale.x;
        OriginalScaley = transform.localScale.y;
        MaterialColor = GetComponent<MeshRenderer>().material.color;
        //save the original excitement for resetting and decay
        OriginalExcitement = Excitement;
        MaterialColor = new Color(Excitement, Excitement, Excitement);
        DetectNeighbors();
        //convert the name of the GameObject to Keycode
        // thisKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), name);
    }

    //--------------------

    //Lerp Over Time
    private float TimeLerpStarted;
   // public float ExcitementDecayTime;
   // public float ClicksDecayTime;

    // Update is called once per frame
    void Update()
    {
        //Click + Key Hold (calculates excitement) -> Rest
        KeyDownHandling();
        if (isPressed)
        {
            KeyHolding();

            ExciteOrInhibitNeighbors();

            CheckIfKeyUpAndReset();

        }
        if (!isPressed)
        {
            DecayExcitementToOriginalValue();
            DecayNumberOfClicksToOriginalValue();
            DecayRestScalingToOriginalValue();
        }
        
        ExcitementToColor();
    }


    
    //ON KEY DOWN
    private void KeyDownHandling()
    {
        if (Input.GetKeyDown(thisKeyCode))
        {
            m_KeyPressManager.NumberofKeysCurrentlyPressed += 1;

            CalculateRestFactor();

            LastPress = Time.time;

            Excitement += ExcitementClickIncrement*RestFromClickFactor;

            m_KeyPressManager.RawForce += ExcitementClickIncrement;

            numberOfClicks += 1;

            isPressed = true;

            SendKeypressDataToList();
        


            duration = 0;

        }
    }


    //KEY HOLD
    //calculate duration and increase or inhibition 
    //according to desired time
    private void KeyHolding()
    {
        if (isPressed)
        {
            duration = Time.time - LastPress;
            Excitement = 0;
//            print(thisKeyCode + "is held");
            // holding happens when it is longer than 0.1 sec
            // it is exciting if it is within the desired duration
            // and only if it happens on a well rested key
            if (0.1f < duration && duration < DesiredHoldingDuration && RestFromClickFactor > 0)
            {
                if (Excitement < 1)
                {
                    Excitement += (ExcitementKeyHoldIncrement*RestFromClickFactor);
                    m_KeyPressManager.RawForce = ExcitementKeyHoldIncrement;
//                  print("excitement up from holding " + Excitement);
                }
                else
                {
                    Debug.Log("reached maximum");
                    Excitement = 1;
                }
            }
            // holding is inhibiting if it went on for too long or happens on an unrested key
            // the measure of inhibition of an unrested key is affected by the RestFactor
            if (duration > DesiredHoldingDuration || RestFromClickFactor < 0)
            {

                if (Excitement > -1)
                {
                    if (RestFromClickFactor > 0)
                        Excitement -= ExcitementKeyHoldIncrement;
                    else
                        Excitement += ExcitementKeyHoldIncrement * RestFromClickFactor;
             
 //                   print("excitement down from holding " + duration);
                }
                else
                {
                    Debug.Log("reached minimum");
                    Excitement = -1;
                }
            }
        }
    }

    //when Key is Up
    private void CheckIfKeyUpAndReset()
    {
        if (Input.GetKeyUp(thisKeyCode))
        {
            isPressed = false;
            m_KeyPressManager.NumberofKeysCurrentlyPressed -= 1;
            print(m_KeyPressManager.NumberofKeysCurrentlyPressed);
            TimeLerpStarted = Time.time;
            HeldStatusToFalseInKeypressesList();
        }
    }

    //DECAY (smoothed)---------
    public void DecayExcitementToOriginalValue()
    {
        if (Excitement != OriginalExcitement)
        {
            Excitement = SmoothStep(Excitement, OriginalExcitement, DecayTime);
        }
    }
    public void DecayNumberOfClicksToOriginalValue()
    {
        if (numberOfClicks != 0)
        {
            numberOfClicks = (int)SmoothStep(numberOfClicks, 0, DecayTime);
        }
    }

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

    //----------------

    public void ExcitementToColor()
    {
        Color color = GetComponent<MeshRenderer>().material.color;
        color.g = (Excitement + 1) / 2;
        color.r = (Excitement + 1) / 2;
        color.b = (Excitement + 1) / 2;
        GetComponent<MeshRenderer>().material.color = color;
    }
    public void CalculateRestFactor()
    {
        float TimePassedSinceLastPress = Time.time - LastPress;
        if(numberOfClicks < DesiredClicks)
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
        RestFactorToScale();
//        print(RestFactor + " Rest Factor");
    }
    public void RestFactorToScale()
    {
//        print("scaling");
        float NormalizedRestFactor = (RestFromClickFactor + 1);
        transform.localScale = new Vector3(OriginalScalex* NormalizedRestFactor, OriginalScalex * NormalizedRestFactor, OriginalScalex * NormalizedRestFactor);
    }
   private void DetectNeighbors()
    {
        //detects Neigbors, but skips itself
        
        if (areaOfEffect > 0)
        {
            Collider[] neighborColliders = Physics.OverlapSphere(transform.position, areaOfEffect);
            NeighboringKeys = new GameObject[neighborColliders.Length-1];
            int i = 0; 
            bool foundItself = false;
            // print(neighborColliders.Length);
            while (i < neighborColliders.Length-1)
            {
                if (neighborColliders[i].gameObject == this.gameObject)
                {
                    foundItself = true;
                }
                if (foundItself)
                {
                    NeighboringKeys[i] = neighborColliders[i+1].gameObject;
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
     
    private void SendKeypressDataToList()
    {
        m_KeyPressManager.Keypresses.Add( new KeyPressData(this, transform.position, Excitement, isPressed));
//        print(KeyPressManager.Keypresses[KeyPressManager.Keypresses.Count-1].SingleKey.gameObject + " with excitement:" + KeyPressManager.Keypresses[KeyPressManager.Keypresses.Count - 1].excitement);
    }
    private void HeldStatusToFalseInKeypressesList()
    { 
        int length = m_KeyPressManager.Keypresses.Count;
        m_KeyPressManager.Keypresses[length - 1].isHeld = false;
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


    //Smooth Step in time (for decay)
    public float SmoothStep(float LerpStartValue, float LerpEndValue, float LerpTime)
    {
        float TimeSinceStarted = Time.time - TimeLerpStarted;
        float PercentageComplete = TimeSinceStarted / LerpTime;
        var result = Mathf.SmoothStep(LerpStartValue, LerpEndValue, PercentageComplete);
        return result;
    }
}

