using System;
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
    [Range(3.0f, 8.0f)]
    public float MinHeight;
    [Range(3.0f, 8.0f)]
    public float MaxHeight;

    [Range(2, 5)]
    public int Depth;
    
    public HouseSettingsData(HouseSettingsData other)
    {
        Copy(other);
    }

    public void Copy(HouseSettingsData other)
    {
        // Do with reflection instead for unified approach
        MinHeight = other.MinHeight;
        MaxHeight = other.MaxHeight;
        Depth = other.Depth;
    }
}