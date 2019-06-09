using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets._2D;

    [RequireComponent(typeof(PlatformerCharacter2D))]

    //BASED ON Platformer2DUserControl.cs
public class HumanFollow : MonoBehaviour
{
    /// <summary>
    /// Toggle Following on and off
    /// will be changed by Firefly position
    /// and then by entering trigger in front of the tree
    /// </summary>
//    public bool shoudFollow;

    /// <summary>
    /// Time it takes to accelerate to 
    /// and input of 1 (intead of the getaxis(horizontal)
    /// that is
    /// </summary>
    [SerializeField]
    private float DurationOfAcceleration;

    /// <summary>
    /// Distance from Firefly at which
    /// the character starts slowing down
    /// </summary>
    [SerializeField]
    private float DistanceToStartSlowingDown;

    /// <summary>
    /// Firefly Gameobject to follow
    /// </summary>
    [SerializeField]
    private GameObject FireFly;

    [SerializeField]
    private AnimationClip IdletoWalkAnimation;

    /// <summary>
    /// the input for walking
    /// between -1 and 1
    /// </summary>
    private float HorizontalInput;

    /// <summary>
    /// crouching is distabled but needed for
    /// communicate with PlatformerCharacter2D
    /// </summary>
    private bool crouch = false;

    /// <summary>
    /// the Character Controller Script
    /// </summary>
    private PlatformerCharacter2D m_Character;

    /// <summary>
    /// Jump - Not used here, but 
    /// left to communicate with the PlatformerCharacter2D
    /// </summary>
    private bool m_Jump = false;

    /// <summary>
    /// is the firefly to the left or right
    /// of the human.
    /// Either -1 (left) or 1 (right).
    /// </summary>
    private int DirectionLeftOrRight;

    /// <summary>
    /// movement Start Time helps calculate Acceleration
    /// </summary>
    private float movementStartTime;

    /// <summary>
    /// Distance from Firefly to know when to stop
    /// </summary>
    private float Distance;

    /// <summary>
    /// detects change from horizontalinput of 0.1 to anything above that
    /// meaning a change from idle to walking
    /// we need to wait with moving until Begin Walking Animation is over
    /// </summary>
    private bool AnimationDelay;

    private float LastFrameHorizontalInput;


    private void Awake()
    {
        m_Character = GetComponent<PlatformerCharacter2D>();
    }

    private void OnEnable()
    {
        movementStartTime = Time.time;
    }

    private void Update()
    {
    }


    private void FixedUpdate()
    {
        // Read the inputs.
        Vector2 FireflyGroundPosition = new Vector2(FireFly.transform.position.x, transform.position.y);
        Distance = Vector2.Distance(transform.position, FireflyGroundPosition);

        //AnimationDelay is used as a flag to know if delay
        //is finished. StartCoroutine
        //if (HorizontalInput < 0.1 && HorizontalInput < LastFrameHorizontalInput)
        //{
        //    AnimationDelay = true;
        //    StartCoroutine("DelayAnimation");
        //}
        //if (!AnimationDelay)
        //{
        LastFrameHorizontalInput = HorizontalInput;
        CalculateAcceleration();
        IsTargetLeftOrRight();
        m_Character.Move(HorizontalInput * DirectionLeftOrRight, crouch, m_Jump);
        //}
    }
    void CalculateAcceleration()
    {
        //Accelerating
        if (Distance > DistanceToStartSlowingDown)
        {
            float t = (Time.time - movementStartTime) / DurationOfAcceleration;
            HorizontalInput = Mathf.SmoothStep(0, 1, t);
            // Pass all parameters to the character control script.
            
        }

        //Slowing Down
        if (Distance < DistanceToStartSlowingDown && HorizontalInput != 0)
        {
            HorizontalInput = Mathf.SmoothStep(1, 0, Time.deltaTime);
            float t = (Time.time - movementStartTime) / DurationOfAcceleration;
            HorizontalInput = Mathf.SmoothStep(1, 0, t);
        }

        //Halt
        if (HorizontalInput == 0)
        {
            movementStartTime = Time.time;
        }
    }
    void IsTargetLeftOrRight()
    {
        if (FireFly.transform.position.x > transform.position.x)
        {
            DirectionLeftOrRight = 1;
        }
        else
            DirectionLeftOrRight = -1;
    }
    IEnumerator DelayForAnimation()
    {
        yield return new WaitForSeconds(IdletoWalkAnimation.length);
        print("idle to walka animation's length is:" + IdletoWalkAnimation.length);
        AnimationDelay = false;

    }
}

