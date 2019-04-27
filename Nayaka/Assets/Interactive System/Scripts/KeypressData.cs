using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressData
{
    public SingleKey SingleKey;
    public Vector3 position;
    public float excitement;
    public bool isHeld;

    public KeyPressData(SingleKey newSingleKey, Vector3 newposition, float newexcitement, bool newIsHeld)
    {
        SingleKey = newSingleKey;
        position = newposition;
        excitement = newexcitement;
        newIsHeld = isHeld;
    }

}
