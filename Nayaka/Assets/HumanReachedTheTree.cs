using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanReachedTheTree : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// The trigger is where the human stops
    /// and gets ready to invite the firefly into itself
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if triggered by human or not
        print("something's here");
        if (collision.GetComponent<HumanFollow>() != null)
        {

            collision.GetComponent<HumanFollow>().Target = gameObject;
            collision.GetComponent<HumanFollow>().GoToTree = true;
            collision.GetComponent<HumanFollow>().DistanceToStartSlowingDown = 0.5f;


        }


    }
}
