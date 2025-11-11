using UnityEngine;

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
    // TODO: Maybe add some settings for additional props generation like:
    // trees, tables, etc. - for more accurate props list, look into the real game

    public HouseLineSettings HouseLineSettings;
}
