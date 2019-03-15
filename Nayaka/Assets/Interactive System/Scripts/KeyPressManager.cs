using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class KeyData
{
    public SingleKey SingleKey;
    public Vector3 position;
    public float excitement;

    public KeyData(SingleKey newSingleKey, Vector3 newposition, float newexcitement)
    {
        SingleKey = newSingleKey;
        position = newposition;
        excitement = newexcitement;
    }

}

public class KeyPressManager : MonoBehaviour {
    public GameObject[] KeysPressed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}
    void OnGUI()
    {
       
    }
}
