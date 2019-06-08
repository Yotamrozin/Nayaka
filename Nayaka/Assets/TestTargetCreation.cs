using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTargetCreation : MonoBehaviour {

    public Vector2 direction;
    public float force;
    public GameObject target;
    public float speed;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            
            TargetMove();   
        }
        if(transform.position != target.transform.position)
        {
            SmoothMoveTowardsTarget();
        }
    }

    void TargetMove()
    {
        target.transform.position = transform.position + ((Vector3)direction * force);
    }
    void SmoothMoveTowardsTarget()
    {
        Vector3 desiredposition = target.transform.position;
        Vector3 smoothposition = Vector3.Lerp(transform.position, desiredposition, speed*force*Time.deltaTime);
        transform.position = smoothposition;
    }
}
