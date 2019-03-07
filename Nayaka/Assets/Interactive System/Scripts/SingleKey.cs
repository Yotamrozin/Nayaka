//The single key has .... 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleKey : MonoBehaviour
{
    public KeyCode thisKeyCode;


    //INHERITED FROM BODY

    //excitement (0<x<1) or inhibition (-1<x<0).
    public float Excitement;
    public float DecayTime;


    //the desired stimulation
    //will be converted to duration/number of clicks the key wishes to be pressed 
    //after which it goes from excitement to inhibition
    [Range(0f, 1f)]
    public float DesiredStimulation;
    public float DesiredTime;
    public int DesiredClicks;
    public float DesiredRest;

    //how many keys to each direction does the pressing of the key affect
    public float areaOfEffect;

    //--------------

    //BACK PROPAGATION
    public float force = 0;
    public float forceIncrement = 0.1f;

    private float OriginalExcitement;
    private bool isPressed; //used for duration of press
    private float duration, durationFactor = 0.01f;
    private int numberOfClicks;

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
    public float ExcitementDecayTime;
    public float ClicksDecayTime;

    // Update is called once per frame
    void Update()
    {
        DecayExcitementAndNumberOfClicksToOriginalValue();

        CheckIfKeyIsPressedSendForceAndAddClickCount();

        KeyHoldingExciteOrInhibitAndCountDuration();

        SendForce();

        CheckIfKeyUpAndReset();

    }

    public void Reset()
    {
        Excitement = OriginalExcitement;
        numberOfClicks = 0;
    }

    public void SendForce()
    {
        transform.position += new Vector3(0, Excitement * force, 0);
        print(name + Excitement);
    }

    //DECAY (smoothed)
    public void DecayExcitementAndNumberOfClicksToOriginalValue()
    {
        if (Excitement != OriginalExcitement)
        {
            Excitement = SmoothStep(Excitement, OriginalExcitement, DecayTime);
        }
        if (numberOfClicks != 0)
        {
            numberOfClicks = (int)SmoothStep(numberOfClicks, 0, DecayTime);
        }
    }


    //ON KEY DOWN
    private void CheckIfKeyIsPressedSendForceAndAddClickCount()
    {
        if (Input.GetKeyDown(thisKeyCode))
        {
            numberOfClicks += 1;
            isPressed = true;
            duration = 0;
            force += forceIncrement;
        }
    }


    //KEY HOLD
    //calculate duration and increase or decrease excitement 
    //according to desired time
    private void KeyHoldingExciteOrInhibitAndCountDuration()
    {

        if (isPressed)
        {
            duration += Time.deltaTime;

            if (duration < DesiredTime || numberOfClicks < DesiredClicks)
            {

                if (Excitement < 1)
                    Excitement = Excitement + (duration * durationFactor);
            }
            if (duration > DesiredTime || numberOfClicks > DesiredClicks)
            {

                if (Excitement > -1)
                    Excitement = Excitement - (duration * durationFactor);
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
            durationFactor = 0;
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
}
