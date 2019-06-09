using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPhysicsFly : MonoBehaviour
{

    [Range(0, 4)]
    [SerializeField]
    private float MaximumFluctuation;

    [Range(0, 0.1f)]
    [SerializeField]
    private float MinimumFluctuation;

    [Range(0, 5)]
    [SerializeField]
    private float MaximumSpeed;

    [Range(0, 5)]
    [SerializeField]
    private float MinimumSpeed;

    private Vector2 NewFlightDirection;


    private float NewSpeed;
    private float TimeOfLastDirectionChange;
    private float TimeForChange;

    private Rigidbody2D body;



    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        TimeOfLastDirectionChange = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeDirection();
        ChangeSpeed();
        body.AddForce(NewFlightDirection * NewSpeed, ForceMode2D.Force);
        // print("Added force in " + NewFlightDirection + "speed: " + NewSpeed);

    }
    void LimitDirection()
    {

    }
    void ChangeDirection()
    {
        TimeForChange = Random.Range(MinimumFluctuation, MaximumFluctuation);
        if (Time.time - TimeOfLastDirectionChange > TimeForChange)
        {
            NewFlightDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            //            print("Direction Changed to" + NewFlightDirection);
            TimeOfLastDirectionChange = Time.time;
        }

    }
    void ChangeSpeed()
    {
        NewSpeed = Random.Range(MinimumSpeed, MaximumSpeed);
    }
}

