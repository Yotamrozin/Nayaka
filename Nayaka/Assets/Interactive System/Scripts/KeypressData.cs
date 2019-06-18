using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPressData
{
    public SingleKey SingleKey;
    public Vector3 position;
    public float excitement;
    public bool isHeld;
    public GameObject Key;
    public int Row;

    public KeyPressData(SingleKey newSingleKey, Vector3 newposition, float newexcitement, bool newIsHeld, GameObject newKey, int newRow)
    {
        SingleKey = newSingleKey;
        position = newposition;
        excitement = newexcitement;
        isHeld = newIsHeld;
        Key = newKey;
        Row = newRow;
    }

}
