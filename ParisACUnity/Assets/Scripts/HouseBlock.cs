using System;
using UnityEngine;
using System.Collections.Generic;

public class HouseBlock : MonoBehaviour
{
    // private Mesh _polyMesh;
    // private MeshFilter _meshFilter;
    // private MeshRenderer _meshRenderer;

    // [HideInInspector] public VoronoiCellSettings CellSettings;
    // public string VoronoiCellSettingsName => nameof(CellSettings);
    //
    // [HideInInspector] public VoronoiCellSettingsData CellSettingsDataCopy;
    // public string VoronoiCellSettingsDataCopyName => nameof(CellSettingsDataCopy);

    private List<Vector3> _houseLineConnections;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        // _polyMesh = new Mesh();
        // _meshFilter = GetComponent<MeshFilter>();
        // _meshFilter.sharedMesh = _polyMesh;
        //
        // CellSettings = AssetDatabase.LoadAssetAtPath<VoronoiCellSettings>(
        //     Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_VoronoiCellSettingsDefault));
        // CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);
        //
        // _meshRenderer = GetComponent<MeshRenderer>();
        // _meshRenderer.material = CellSettings.SettingsData.CellMaterial;
    }

    public void SetupHouseLineConnections(List<Vector3> houseLineConnections)
    {
        _houseLineConnections = new List<Vector3>(houseLineConnections);
    }

    private void OnDrawGizmos()
    {
        Color color = new Color(0, 0, 1, 0.4f);

        foreach (var houseLineConnection in _houseLineConnections)
        {
            Gizmos.DrawSphere(houseLineConnection, 0.1f);
        }
    }
}