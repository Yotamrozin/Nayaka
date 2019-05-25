using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// PlayerScript requires the GameObject to have a Rigidbody component
[RequireComponent(typeof(Rigidbody2D))]

public class NayakaRelationship : MonoBehaviour {
    public KeyPressManager KeyManager;
    private Rigidbody2D body;


    

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //movement by keys
        if (KeyManager.NumberofKeysCurrentlyPressed > 0)
        {
            Move(KeyManager.direction, KeyManager.NormalizedForce);
            KeyManager.RawForce = 0;
            KeyManager.NormalizedForce = 0;
            print("moving" + KeyManager.direction * KeyManager.NormalizedForce);
        }
    }
    void Move(Vector2 Direction, float Force)
    {
        body.AddForce(Direction * Force, ForceMode2D.Impulse);
        
        print("added force in direction " + Direction + " " + Force);

    }
    
}
