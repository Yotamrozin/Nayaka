//The single key has .... 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleKey : MonoBehaviour
{
    public KeyCode thisKeyCode;


    //INHERITED FROM BODY

    //excitement (0<x<1) or inhibition (-1<x<0).
    public float BaseForce;
    public float Excitement;
    public float DecayTime;


    //the desired stimulation
    //will be converted to duration/number of clicks the key wishes to be pressed 
    //after which it goes from excitement to inhibition
    //should i make key holding, number of clicks, and resting time codependent?
    [Range(0f, 1f)]
    public float DesiredStimulation;
    public float DesiredTime;
    public int DesiredClicks;
    public float DesiredRest;
    private float RestViolationFactor;

    //how many keys to each direction does the pressing of the key affect
    public float areaOfEffect;

    //--------------

    //BACK PROPAGATION
    public float force = 0;
    public float ExcitementClickIncrement = 0.1f;
    public float KeyHoldExcitementFactor = 0.1f;

    private float OriginalExcitement;
    private bool isPressed; //used for duration of press
    public float duration;
    private int numberOfClicks;
    private float LastPress;

    // Use this for initialization
    void Start()
    {
        //save the original excitement for resetting and decay
        OriginalExcitement = Excitement;

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
        CheckIfKeyIsPressedSendForceAndAddClickCount();
        if (isPressed)
        {
            KeyHolding();

            SendForce();

            CheckIfKeyUpAndReset();
        }
        if (!isPressed)
        {
            DecayExcitementToOriginalValue();
            DecayNumberOfClicksToOriginalValue();
        }

    }


    
    //ON KEY DOWN
    private void CheckIfKeyIsPressedSendForceAndAddClickCount()
    {
        if (Input.GetKeyDown(thisKeyCode))
        {
            print(thisKeyCode + "clicked");
            //check if the key rested enough
            if (Time.time - LastPress < DesiredRest)
            { 
                RestViolationFactor = Mathf.Lerp(1, 0, Time.time - LastPress);
                print(RestViolationFactor + "Rest Violation Factor");
                Excitement -= ExcitementClickIncrement*RestViolationFactor;
            }
            else
            {
                Excitement += ExcitementClickIncrement;
            } 
            numberOfClicks += 1;
            isPressed = true;
            LastPress = Time.time;
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
            print(thisKeyCode + "is held");
            if (duration < DesiredTime)
            {
                
                if (Excitement < 1)
                {
                    Excitement += KeyHoldExcitementFactor;
                    print("excitement up from holding " + Excitement);
                }
                else
                {
                    Debug.Log("reached maximum");
                }
            }
            if (duration > DesiredTime)
            {
                if (Excitement > -1)
                {
                    Excitement -= KeyHoldExcitementFactor;
                    print("excitement down from holding " + duration);
                }
                else
                {
                    Debug.Log("reached minimum");
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
            TimeLerpStarted = Time.time;
        }
    }
    //DECAY (smoothed)
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

    //Smooth Step in time
    public float SmoothStep(float LerpStartValue, float LerpEndValue, float LerpTime)
    {
        float TimeSinceStarted = Time.time - TimeLerpStarted;
        float PercentageComplete = TimeSinceStarted / LerpTime;
        var result = Mathf.SmoothStep(LerpStartValue, LerpEndValue, PercentageComplete);
        return result;
    }

    public void Reset()
    {
        Excitement = OriginalExcitement;
        numberOfClicks = 0;
    }

    public void SendForce()
    {
        //print(Excitement + " " + thisKeyCode);
        //       transform.position += new Vector3(0, Excitement * force, 0);
        //        print(name + Excitement);
    }
}
