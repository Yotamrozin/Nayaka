using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// PlayerScript requires the GameObject to have a Rigidbody component
[RequireComponent(typeof(Rigidbody2D))]

public class NayakaRelationship : MonoBehaviour {

    /// <summary>
    /// the Key Press Manager that collects the force to be applied on the body
    /// </summary>
    public KeyPressManager KeyManager;

    /// <summary>
    /// the rigidbody component of this body
    /// </summary>
    private Rigidbody2D body;


    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }



    // Update is called once per frame
    void Update()
    {
        // as long as some key is pressed
        // get the force from the key manager and move
        // after move is performed, reset the LeyManager's Raw Force
        // and Normalized Force, so we know the force has been applied
        // and it doesn't accumulate.
        if (KeyManager.NumberofKeysCurrentlyPressed > 0)
        {
            //Move the body using physics force
            Move(KeyManager.direction, KeyManager.NormalizedForce);
            //reset key press manager force and normalized force
            KeyManager.RawForce = 0;
            KeyManager.NormalizedForce = 0;
        }
    }



    /// <summary>
    /// Handles the actual movement for Physics based Bodies
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    void Move(Vector2 direction, float force)
    {
        body.AddForce(direction * force, ForceMode2D.Impulse);
        print("added force in direction " + direction + " " + force);
    }
    
}
