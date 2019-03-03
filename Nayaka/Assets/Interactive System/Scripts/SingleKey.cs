//The single key has .... 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleKey : MonoBehaviour {
    private KeyCode thisKeyCode;

   
    
    //INHERITED FROM BODY

    //excitement (0<x<1) or inhibition (-1<x<0).
    public float Excitement;
    private float OriginalExcitement;
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
    private bool isPressed; //used for duration of press
    private float duration, durationFactor = 0;
    private int numberOfClicks;
    public float force;
	// Use this for initialization
	void Start () {
        //save the original excitement for resetting and decay
        OriginalExcitement = Excitement;
        //convert the name of the GameObject to Keycode
        thisKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), name);
    }

    //--------------------

    //Lerp Over Time
    private float TimeLerpStarted;
    public float ExcitementDecayTime;
    public float ClicksDecayTime;

    // Update is called once per frame
    void Update () {
        //DECAY (smoothed)
        if (Excitement != OriginalExcitement)
        { 
            Excitement = Decay(Excitement, OriginalExcitement, ExcitementDecayTime);
        }
        if (numberOfClicks != 0)
        {
            numberOfClicks = (int)Decay(numberOfClicks, 0, ClicksDecayTime);
        }
            
        //ON KEY DOWN
        if (Input.GetKeyDown(thisKeyCode))
        {
            numberOfClicks += 1;
            isPressed = true;
            duration = 0;
        }

        //KEY HOLD
        //calculate duration and increase or decrease excitement 
        //according to desired time
        if (isPressed)
        {
            duration += Time.deltaTime;

            if (duration < DesiredTime || numberOfClicks < DesiredClicks)
            {
                durationFactor = duration * 0.01f;
                if(Excitement < 1)
                Excitement = Excitement + durationFactor;
            }
            if (duration > DesiredTime || numberOfClicks > DesiredClicks)
            {
                durationFactor = duration * 0.01f;
                if (Excitement > -1)
                Excitement = Excitement - durationFactor;
            }
            force = Excitement;
            print(name + force);

            transform.position += new Vector3(0, force*0.1f, 0);

        }
        if (Input.GetKeyUp(thisKeyCode))
        {
            isPressed = false;
            TimeLerpStarted = Time.time;
            durationFactor = 0;
        }

    }
    void OnGUI()
    {
        ////Targeting the right key.
        //Event CurrentEvent = Event.current;
        //if (CurrentEvent.isKey && !isPressed)
        //{
        //    Debug.Log("Detected key code: " + CurrentEvent.keyCode);
        //    if (name == CurrentEvent.keyCode.ToString())
        //    {
        //        isPressed = true;
        //    }
        //}
    }
    public void Reset()
    {
        Excitement = OriginalExcitement;
        numberOfClicks = 0;
    }
    public void SendForce()
    {

    }
    public float Decay(float LerpStartValue, float LerpEndValue, float LerpTime)
    {
        float TimeSinceStarted = Time.time - TimeLerpStarted;
        float PercentageComplete = TimeSinceStarted / LerpTime;
        var result = Mathf.SmoothStep(LerpStartValue, LerpEndValue, PercentageComplete);
        return result;
    }
}
