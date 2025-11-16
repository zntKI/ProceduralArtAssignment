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
    // TODO: Make min and max height separate properties
    [Range(5.0f, 10.0f)]
    public float MinHeight;
    [Range(5.0f, 10.0f)]
    public float MaxHeight;
    
    public HouseSettingsData(HouseSettingsData other)
    {
        Copy(other);
    }

    public void Copy(HouseSettingsData other)
    {
        MinHeight = other.MinHeight;
        MaxHeight = other.MaxHeight;
    }
}