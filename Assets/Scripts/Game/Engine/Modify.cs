using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Modify
{
    public bool _show = false;
    public string modify_ID;
    public ValueType modify_Type = ValueType.Int;
    public string modify_Des;
    // Value

    public int modify_IntValue;
    public float modify_FloatValue;
    public bool modify_BoolValue;
    public string modify_StringValue;
}