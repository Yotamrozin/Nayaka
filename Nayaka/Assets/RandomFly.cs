using UnityEngine;
using System.Collections;

public class RandomFly : MonoBehaviour
{
    

    /// <summary>
    /// speed of lookat 2d rotation
    /// @DISABLED
    /// </summary>
    public float rotateSpeed = 5.0f;

    /// <summary>
    /// Maximum distance for the target to be set
    /// </summary>
    [SerializeField]
    private float DistanceRange = 1;

    /// <summary>
    /// the firefly follows this Target,
    /// which has a randomized position.
    /// </summary>
    [SerializeField]
    private GameObject Target;

    /// <summary>
    /// Randomized Direction for the next target
    /// </summary>
    private Vector2 Direction;

    /// <summary>
    /// Distance, after randomization
    /// </summary>
    private float Distance;

    //-----------
    // Lerp Variables
    /// <summary>
    /// Stores the time lerp started for calculating percentage done
    /// </summary>
    private float TimeLerpStarted;
    
    /// <summary>
    /// Stores the time past since lerp started for calculating percentage done
    /// </summary>
    private float timeSinceLerpStarted;

    /// <summary>
    /// percentage of the lerp already done
    /// </summary>
    private float percentageComplete;

    /// <summary>
    /// Time it takes to Lerp - 
    /// Practically the speed of the Lerp
    /// </summary>
    private float TimeofLerp;

    /// <summary>
    /// Time of Lerp Changes the speed of the lerp towards the Target
    /// This is the maximum range for Randomizing it
    /// </summary>
    public float MaxTimeOfLerpRange = 36f;

    /// <summary>
    /// Time of Lerp Changes the speed of the lerp towards the Target
    /// This is the minimum range for Randomizing it
    /// </summary>
    public float MinTimeOfLerpRange = 34f;

    /// <summary>
    /// flag bool to know when lerping is underway
    /// </summary>
    private bool isLerping;

    /// <summary>
    /// allows the KeypressManager to disable this script
    /// whenever the player is successfully moving the firefly
    /// </summary>
    public bool ShouldRandomlyFly = true;

    void Start()
    {
        ShouldRandomlyFly = true;
    }

    /// <summary>
    /// changes target position to random distance and random direction
    /// </summary>
    void RandomizeTargetPosition()
    {
        float distancex = Random.Range(-1.0f, 1.0f);
        float distancey = Random.Range(-1.0f, 1.0f);
        Direction = new Vector2(distancex, distancey);
        print("direction is: " + Direction);
        Distance = Random.Range(0.03f, DistanceRange);
        Target.transform.position = transform.position + ((Vector3)Direction * Distance);
    }

    void Update()
    {
        if (ShouldRandomlyFly)
        { 
        if(Target != null)
        {
            //change target when arriving
            if (Vector2.Distance(transform.position, Target.transform.position) <= 0.1f)
            {
                isLerping = false;
                RandomizeTargetPosition();
            }
            
            else
            {
                if (isLerping)
                {
                    timeSinceLerpStarted = Time.time - TimeLerpStarted;
                    percentageComplete = timeSinceLerpStarted / TimeofLerp;
                    Vector3 smoothposition = Vector3.Slerp(transform.position, Target.transform.position, percentageComplete);
                    transform.position = smoothposition;
                }
                else
                { 
                    TimeLerpStarted = Time.time;
                    TimeofLerp = Random.Range(MaxTimeOfLerpRange, MinTimeOfLerpRange);
                    isLerping = true;
                }

            }
            //randomize speed
            
            
        }
        else
        {
            Debug.LogError("Target Game Object Not Set!");
        }
            //LookAt2D(newPosition);
        }
    }

    void LookAt2D(Vector3 lookAtPosition)
    {
        float distanceX = lookAtPosition.x - transform.position.x;
        float distanceY = lookAtPosition.y - transform.position.y;
        float angle = Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;

        Quaternion endRotation = Quaternion.AngleAxis(angle, Vector3.back);
        transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * rotateSpeed);
    }
    public void ResetRandomFlight()
    {
        Distance = 0;
        TimeLerpStarted = Time.time;
        TimeofLerp = Random.Range(MaxTimeOfLerpRange, MinTimeOfLerpRange);
        RandomizeTargetPosition();
        isLerping = true;
    }
}
