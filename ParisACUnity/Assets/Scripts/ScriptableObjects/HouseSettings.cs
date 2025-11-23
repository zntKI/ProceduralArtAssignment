using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Contains house properties' settings.<br/><br/>
///
/// It may contain additional house settings like:<br/>
/// horizontal house displacement.<br/><br/>
///
/// It may contain a subset of properties for houses like Body, Roof, etc...
/// </summary>
[CreateAssetMenu(fileName = "HouseSettings", menuName = "Scriptable Objects/HouseSettings")]
public class HouseSettings : ScriptableObject
{
    public HouseSettingsData SettingsData;
}

[Serializable]
public class HouseSettingsData
{
    public Material HouseMaterial;
    
    [Range(2.0f, 8.0f)]
    public int MinHeight;
    [Range(2.0f, 8.0f)]
    public int MaxHeight;
    public int Height { get; set; } // Randomly selected height

    [Range(1, 10)]
    public int MinWidth;
    [Range(1, 10)]
    public int MaxWidth;
    public int Width { get; set; } // Randomly selected width
    
    [Range(2, 5)]
    public int MinDepth;
    [Range(2, 5)]
    public int MaxDepth;
    public int Depth { get; set; } // Randomly selected depth

    public GameObject wallType;
    public GameObject doorType;
    public GameObject windowType;
    public GameObject roofType1;
    public GameObject roofType2;
    public GameObject roofType3;
    public List<GameObject> getRoofTypes()
    {
        List<GameObject> list = new List<GameObject>();
        if (roofType1)
            list.Add(roofType1);
        if (roofType3)
            list.Add(roofType2);
        if (roofType3)
            list.Add(roofType3);

        return list;
    }
    public GameObject roofTop;

    public GameObject roofGenType;
    
    // TODO: Min and max depth house displacement
    
    public HouseSettingsData(HouseSettingsData other)
    {
        Copy(other);
    }

    public void Copy(HouseSettingsData other)
    {
        FieldInfo[] fields = this.GetType().GetFields();
        foreach (var field in fields)
        {
            field.SetValue(this, field.GetValue(other));
        }
    }
}