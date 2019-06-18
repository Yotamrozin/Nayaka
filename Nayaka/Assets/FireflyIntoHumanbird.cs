using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyIntoHumanbird : MonoBehaviour {

    public GameObject Keyboard;
    public GameObject Human;
    public bool Cocked;
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Firefly")
        {
            if(Cocked)
            {
                collision.GetComponent<Animator>().SetTrigger("FireflyIntoHuman");
                Keyboard.GetComponent<KeyPressManager>().enabled = false;
                HumanFollow humanfollowscript = Human.GetComponent<HumanFollow>();
                humanfollowscript.reactToFirefly = true;
                Cocked = false;
            }
        }
    }
}
