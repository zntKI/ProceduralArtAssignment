using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

    [HideInInspector] public VoronoiCellSettingsData CellSettingsDataCopy;

    private HouseBlock _houseBlock;
    
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
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_VoronoiCellSettingsFolder,
                Config.ScriptableObjects_VoronoiCellSettingsDefault));
        CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = CellSettings.SettingsData.CellMaterial;

        CreateHouseBlock();
    }

    private void CreateHouseBlock()
    {
        _houseBlock = new GameObject($"House Block",
            typeof(HouseBlock), typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<HouseBlock>();
        
        _houseBlock.transform.position = transform.position;
        _houseBlock.transform.SetParent(transform);
        
        Undo.RegisterCreatedObjectUndo(_houseBlock.gameObject, "Created House Block");
        
        _houseBlock.Init(CellSettings);
        
        EditorUtility.SetDirty(this);
    }

    public void GenerateMesh(List<Vector3> polyVertices)
    {
        _polyMesh.Clear();

        _polyMesh.vertices = polyVertices.Select(v => v = transform.InverseTransformPoint(v)).ToArray();

        var meshTriangles = new List<int>();
        for (int i = 0; i < polyVertices.Count - 2; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i + 2);
            meshTriangles.Add(i + 1);
        }

        _polyMesh.triangles = meshTriangles.ToArray();

        _houseBlock.SetupHouseBlockVertices(polyVertices);
    }

    /// <summary>
    /// Called when whole scriptable object is swapped in the inspector<br/><br/>
    ///
    /// Resets the temporary DataCopy to the new scriptable object's data
    /// </summary>
    public void UpdateCellSettings()
    {
        CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);

        UpdateCellMaterial();
    }

    /// <summary>
    /// Called when the scriptable object asset gets changed itself<br/><br/>
    ///
    /// Overrides the temporary DataCopy with the new scriptable object's data
    /// </summary>
    private void UpdateCellSettingsDataCopy()
    {
        // Get all the fields within VoronoiCellSettingsData class in general through Reflection
        FieldInfo[] fields = typeof(VoronoiCellSettingsData).GetFields();
        foreach (var field in fields)
        {
            var dataCopyValue = field.GetValue(CellSettingsDataCopy);
            var dataOriginalValue = field.GetValue(CellSettings.SettingsData);
            
            if (dataCopyValue != dataOriginalValue)
            {
                field.SetValue(CellSettingsDataCopy, dataOriginalValue);
                
                UpdateCellSettingsCopyByField(field);
            }
        }
    }

    public void UpdateCellSettingsCopyByField(FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(CellSettingsDataCopy.CellMaterial))
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
            "Save New Voronoi Cell Settings - new name goes AFTER '_'",
            "VoronoiCellSettings_New",
            "asset",
            "Choose a location for the new settings file",
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_VoronoiCellSettingsFolder));

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

        Vector3 drawSpherePos = transform.GetChild(0).position;
        Gizmos.DrawSphere(drawSpherePos,
            transform.parent.localScale.x * CellSettingsDataCopy.DebugSeedCubeSizeModifier);
    }
}