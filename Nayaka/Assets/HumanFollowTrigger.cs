using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFollowTrigger : MonoBehaviour {

    [SerializeField]
    private HumanFollow HumanFollowScript;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Firefly")
        {
            print("collision");
            HumanFollowScript.enabled = true;
        }
    }
}
