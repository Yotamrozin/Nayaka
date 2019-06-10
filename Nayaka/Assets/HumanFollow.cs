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
    /// the character starts slowing down.
    /// when getting close to the tree,
    /// This distance is changed by entering the trigger
    /// (HumanReachedTheTree.cs)
    /// </summary>
    public float DistanceToStartSlowingDown;

    /// <summary>
    /// Target Gameobject to follow (beings as the firefly 
    /// and later becomed the trigger near the trees
    /// </summary>
    public GameObject Target;

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
    /// Animator of the Human.
    /// Used to get the idle to walk animation flag
    /// and start a delay before starting to move
    /// in order to let the animation to finish
    /// </summary>
    private Animator HumanAnimator;

    /// <summary>
    /// The animation clip of the human getting ready
    /// too accept the firefly in. The clip's length is used to
    /// wait until animation is finished and the human is ready for
    /// the firefly to enter.
    /// </summary>
    [SerializeField]
    private AnimationClip GettingReadyForFireflyAnimationClip;

    /// <summary>
    /// Triggers animation for human 
    /// becoming ready to take the firefly
    /// </summary>
    public bool GettingReadyForFireFly;

    /// <summary>
    /// Triggers changes target 
    /// 
    /// </summary>
    public bool GoToTree;

    /// <summary>
    /// when 'getting ready for firefly' animation 
    /// is finished, the human becomes ready for firefly to come in
    /// </summary>
    private bool ReadyForFireFly;




    private void Awake()
    {
        m_Character = GetComponent<PlatformerCharacter2D>();
        HumanAnimator = GetComponent<Animator>();
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

        //If HumanBeginWalk is Playing, the player should not move yet
        //plus, if we reached the tree (and getting ready for firefly)
        //there will be no more walking
        if (!HumanAnimator.GetBool("HumanBeginWalkIsPlaying"))
        {
            Vector2 TargetGroundPosition = new Vector2(Target.transform.position.x, transform.position.y);
            Distance = Vector2.Distance(transform.position, TargetGroundPosition);
            CalculateAcceleration();
            IsTargetLeftOrRight();
            m_Character.Move(HorizontalInput * DirectionLeftOrRight, crouch, m_Jump);
        }
        if (GoToTree && Distance < DistanceToStartSlowingDown && !GettingReadyForFireFly)
        {
            GettingReadyForFireFly = true;
            print("distance is:" + Distance);
            HumanAnimator.SetTrigger("GetReadyForFirefly");
            StartCoroutine(DelayForAnimation(GettingReadyForFireflyAnimationClip, ReadyForFireFly));
        }
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
        if (Target.transform.position.x > transform.position.x)
        {
            DirectionLeftOrRight = 1;
        }
        else
            DirectionLeftOrRight = -1;
    }
    IEnumerator DelayForAnimation(AnimationClip AnimClip, bool FlagToChange)
    {
        yield return new WaitForSeconds(AnimClip.length);
        //@DEBUG
        FlagToChange = !FlagToChange;
        print("it's " + FlagToChange + "! we're ready for firefly!");

    }
}

