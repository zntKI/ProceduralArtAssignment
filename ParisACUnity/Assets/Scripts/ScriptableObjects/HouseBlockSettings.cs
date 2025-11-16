using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains house block properties' settings.<br/><br/>
///
/// It may contain settings about additional props contained<br/>
/// within the walls of the house block.<br/><br/>
///
/// Contains a subset of properties for house lines as <see cref="HouseLineSettings"/>
/// </summary>
[CreateAssetMenu(fileName = "HouseBlockSettings", menuName = "Scriptable Objects/HouseBlockSettings")]
public class HouseBlockSettings : ScriptableObject
{
    public HouseBlockSettingsData SettingsData;

    public HouseLineSettingsData HouseLineSettingsData;
    public HouseSettingsData HouseSettingsData;
    
    [Tooltip("Easier access to the appropriate SO for the house line according to the HouseBlockSettings one")]
    public HouseLineSettings HouseLineSettings;
}

[Serializable]
public class HouseBlockSettingsData
{
    public Material BlockMaterial;
    public string BlockMaterialName => nameof(BlockMaterial);

    // TODO: Maybe add some settings for additional props generation like:
    // trees, tables, etc. - for more accurate props list, look into the real game
    
    public HouseBlockSettingsData(HouseBlockSettingsData other)
    {
        Copy(other);
    }

    public void Copy(HouseBlockSettingsData other)
    {
        BlockMaterial = other.BlockMaterial;
    }
}