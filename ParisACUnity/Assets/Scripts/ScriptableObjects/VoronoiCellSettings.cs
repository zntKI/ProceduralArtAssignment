using System;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Contains voronoi cell properties' settings.<br/><br/>
///
/// Contains a subset of properties for house blocks as <see cref="HouseBlockSettings"/>
/// </summary>
[CreateAssetMenu(fileName = "VoronoiCellSettings", menuName = "Scriptable Objects/VoronoiCellSettings")]
public class VoronoiCellSettings : ScriptableObject
{
    public VoronoiCellSettingsData SettingsData;

    //public HouseBlockSettings HouseBlockSettings;
}

[Serializable]
public class VoronoiCellSettingsData
{
    public Material CellMaterial;
    public float DebugSeedCubeSizeModifier;

    public VoronoiCellSettingsData(VoronoiCellSettingsData other)
    {
        Copy(other);
    }

    public void Copy(VoronoiCellSettingsData other)
    {
        CellMaterial = other.CellMaterial;
        DebugSeedCubeSizeModifier = other.DebugSeedCubeSizeModifier;
    }
}