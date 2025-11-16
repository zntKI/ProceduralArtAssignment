using System;
using UnityEngine;

/// <summary>
/// Contains house line properties' settings.<br/><br/>
///
/// It may contain additional house settings like:<br/>
/// horizontal house displacement.<br/><br/>
///
/// Contains a subset of properties for house lines as <see cref="HouseSettings"/>
/// </summary>
[CreateAssetMenu(fileName = "HouseLineSettings", menuName = "Scriptable Objects/HouseLineSettings")]
public class HouseLineSettings : ScriptableObject
{
    public HouseLineSettingsData SettingsData;

    public HouseSettingsData HouseSettingsData;
}

[Serializable]
public class HouseLineSettingsData
{
    public Material LineMaterial;
    public string LineMaterialName => nameof(LineMaterial);
    
    // TODO: Maybe add horizontal house displacement
    
    public HouseLineSettingsData(HouseLineSettingsData other)
    {
        Copy(other);
    }

    public void Copy(HouseLineSettingsData other)
    {
        LineMaterial = other.LineMaterial;
    }
}