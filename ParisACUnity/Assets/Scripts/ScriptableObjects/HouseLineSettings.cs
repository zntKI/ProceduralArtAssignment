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
    // TODO: Maybe add horizontal house displacement
    
    public HouseSettings HouseSettings;
}
