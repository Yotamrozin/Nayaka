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

    /// <summary>
    /// When firefly goes into humanbird, it triggers the humanbird animation (Firefly In)
    /// But only after waiting for the firefly  animation to finish. This is this animation clip.
    /// FireflyComing bool is changed by FireflyIntoHumanbird.cs on FireflyIntoHumanTrigger
    /// </summary>
    public AnimationClip FireflyComingInAnimationClip;

    /// <summary>
    /// When firefly goes into humanbird, it triggers the humanbird animation (Firefly In)
    /// But only after waiting for the firefly  animation to finish. This is this animation clip.
    /// FireflyComing bool is changed by FireflyIntoHumanbird.cs on FireflyIntoHumanTrigger
    /// </summary>
    public AnimationClip ReactionToFireflyAnimationClip;

    /// <summary>
    /// when the transformation into bird is complete,
    /// it is ready to recieve the firefly and the trigger is activated.
    /// </summary>
    [SerializeField]
    private GameObject FireflyIntoHumanTrigger;

    /// <summary>
    /// starts the delay, waiting for the firefly to dive into the bird.
    /// After the animation is over, we can play the reaction animation.
    /// </summary>
    public bool reactToFirefly;


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

        // When human reaches its position next to tree
        // it starts transforming into the bird, waiting for the animation to be over
        // and sets ReadyForFireFly to true
        if (GoToTree && Distance < DistanceToStartSlowingDown && !GettingReadyForFireFly)
        {
            GettingReadyForFireFly = true;
            print("distance is:" + Distance);
            HumanAnimator.SetTrigger("GetReadyForFirefly");
            StartCoroutine(DelayForGettingReadyAnimation(GettingReadyForFireflyAnimationClip));
        }
        
        // when the humanbird is ready for the firefly, the FireflyIntoHumanTrigger is activated through
        // the Cocked bool.
        if (ReadyForFireFly)
        {
            FireflyIntoHumanTrigger.GetComponent<FireflyIntoHumanbird>().Cocked = true;
            ReadyForFireFly = false;
        }

        // When the FireflyIntoHumanTrigger is triggered by the firefly,
        // we wait for the firefly animation to end
        // and play the reaction animation.
        // When reaction animation is over, 
        // Reconfigure Key Press Manager().
        if (reactToFirefly)
        {
            StartCoroutine(WaitForFireflyAnimation_PlayReaction_ChangeKeyPressManager(FireflyComingInAnimationClip, ReactionToFireflyAnimationClip));
            reactToFirefly = false;
        }
        
        
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

    /// <summary>
    /// Used to determine if target (firefly) is on 
    /// the right or left of the human
    /// </summary>
    void IsTargetLeftOrRight()
    {
        if (Target.transform.position.x > transform.position.x)
        {
            DirectionLeftOrRight = 1;
        }
        else
            DirectionLeftOrRight = -1;
    }

    /// <summary>
    /// changes ReadyForFirefly when the transformation from human to bird
    /// has finished and it is ready for the firefly.
    /// </summary>
    /// <param name="AnimClip"></param>
    /// <returns></returns>
    public IEnumerator DelayForGettingReadyAnimation(AnimationClip AnimClip)
    {
        yield return new WaitForSeconds(AnimClip.length);
        //@DEBUG
        print("it's we're ready for firefly!");
        ReadyForFireFly = true;
    }

    /// <summary>
    /// changes FireflyIsIn when the firefly finishes its plunge into the bird
    /// and then changes FireflyIsIn to True.
    /// </summary>
    /// <param name="AnimClip"></param>
    /// <returns></returns>
    IEnumerator WaitForFireflyAnimation_PlayReaction_ChangeKeyPressManager(AnimationClip AnimClip01, AnimationClip AnimClip02)
    {
        print("wainting for a tenth of " + AnimClip01.length);
        yield return new WaitForSeconds(AnimClip01.length/10);
        //@DEBUG
        HumanAnimator.SetTrigger("FireflyIn");
        yield return new WaitForSeconds(AnimClip02.length);
        ReconfigureKeyPressManager();
    }

    /// <summary>
    /// loads the Humanbird as the new Body and reconfigures how it operates.
    /// </summary>
    public void ReconfigureKeyPressManager()
    {

    }
}

