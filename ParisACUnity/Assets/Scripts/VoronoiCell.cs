using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Serialization;

public class VoronoiCell : MonoBehaviour
{
    private Mesh _polyMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [HideInInspector] public VoronoiCellSettings CellSettings;
    public string VoronoiCellSettingsName => nameof(CellSettings);

    [HideInInspector] public VoronoiCellSettingsData CellSettingsDataCopy;
    public string VoronoiCellSettingsDataCopyName => nameof(CellSettingsDataCopy);

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
        CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = CellSettings.SettingsData.CellMaterial;
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
        UpdateCellSettingsDataCopy();

        UpdateCellMaterial();
    }

    private void UpdateCellSettingsDataCopy()
    {
        CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);
    }

    public void UpdateCellSettingsCopyByField(FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == CellSettingsDataCopy.CellMaterialName)
            UpdateCellMaterial();
    }

    private void UpdateCellMaterial()
    {
#if UNITY_EDITOR
        Undo.RecordObject(_meshRenderer, "Updated Cell Settings");
        _meshRenderer.material = CellSettingsDataCopy.CellMaterial;
        EditorUtility.SetDirty(_meshRenderer);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    public void ApplyCellSettingsCopyToOriginal()
    {
#if UNITY_EDITOR
        Undo.RecordObject(CellSettings, "Applied Cell Settings To Original");
        CellSettings.SettingsData = new VoronoiCellSettingsData(CellSettingsDataCopy);
        EditorUtility.SetDirty(CellSettings);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    public void SaveCellSettingsAsNewSO()
    {
        var newSettings = ScriptableObject.CreateInstance<VoronoiCellSettings>();
        newSettings.SettingsData = new VoronoiCellSettingsData(CellSettingsDataCopy);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save New Voronoi Cell Settings",
            "VoronoiCellSettings_New",
            "asset",
            "Choose a location for the new settings file",
            Config.ScriptableObjects_Path);

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(newSettings);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = CellSettings.SettingsData.DebugDrawColor;

        Gizmos.DrawSphere(transform.position,
            transform.parent.localScale.x * CellSettingsDataCopy.DebugSeedCubeSizeModifier);
    }
}