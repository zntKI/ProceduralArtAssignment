using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

public class VoronoiCell : MonoBehaviour
{
    private Mesh _polyMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    [HideInInspector]
    public VoronoiCellSettings CellSettings;
    
    private VoronoiCellSettingsData _cellSettingsData;
    private VoronoiCellSettingsData _lastCellSettingsData;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        _polyMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _polyMesh;

        CellSettings = AssetDatabase.LoadAssetAtPath<VoronoiCellSettings>(
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_VoronoiCellSettingsDefault));
        
        _cellSettingsData = CellSettings.SettingsData;
        _lastCellSettingsData = new VoronoiCellSettingsData(_cellSettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = _cellSettingsData.CellMaterial;
    }
    
    public void GenerateMesh(List<Vector3> polyVertices)
    {   
        _polyMesh.Clear();

        _polyMesh.vertices = polyVertices.ToArray();

        var meshTriangles = new List<int>();
        for (int i = 0; i < polyVertices.Count - 2; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i + 2);
            meshTriangles.Add(i + 1);
        }

        _polyMesh.triangles = meshTriangles.ToArray();
    }

    public void UpdateCellSettings()
    {
        _cellSettingsData = CellSettings.SettingsData;
        _meshRenderer.material = _cellSettingsData.CellMaterial;
        _lastCellSettingsData.Copy(_cellSettingsData);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.4f);
    
        Gizmos.DrawSphere(transform.position, transform.parent.localScale.x * _cellSettingsData.DebugSeedCubeSizeModifier);
    }
}