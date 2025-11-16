using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HouseBlock))]
public class HouseBlockEditor : Editor
{
    private void OnEnable()
    {
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }
}
