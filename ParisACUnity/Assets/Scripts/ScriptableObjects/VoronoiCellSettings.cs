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

    [Tooltip("Easier access to the appropriate SO for the house block according to the VoronoiCellSettings one")]
    public HouseBlockSettings HouseBlockSettings;
}

[Serializable]
public class VoronoiCellSettingsData
{
    public Material CellMaterial;
    public string CellMaterialName => nameof(CellMaterial);
    
    public float DebugSeedCubeSizeModifier;
    public Color DebugDrawColor;

    public VoronoiCellSettingsData(VoronoiCellSettingsData other)
    {
        Copy(other);
    }

    public void Copy(VoronoiCellSettingsData other)
    {
        CellMaterial = other.CellMaterial;
        DebugSeedCubeSizeModifier = other.DebugSeedCubeSizeModifier;
        DebugDrawColor = other.DebugDrawColor;
    }
}