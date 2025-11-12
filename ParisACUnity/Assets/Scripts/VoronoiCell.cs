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
    public string VoronoiCellSettingsName => nameof(CellSettings);

    [HideInInspector] public VoronoiCellSettingsData CellSettingsDataCopy;
    public string VoronoiCellSettingsDataCopyName => nameof(CellSettingsDataCopy);

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
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_VoronoiCellSettingsDefault));
        CellSettingsDataCopy = new VoronoiCellSettingsData(CellSettings.SettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = CellSettings.SettingsData.CellMaterial;

        CreateHouseBlock();
    }

    private void CreateHouseBlock()
    {
        _houseBlock = new GameObject($"House Block",
            typeof(HouseBlock), typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<HouseBlock>();
        _houseBlock.transform.position = transform.position + transform.up * 0.1f;
        _houseBlock.Init();

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(_houseBlock, "Created House Block");
        Undo.SetTransformParent(_houseBlock.transform, transform, "Set House Block Parent");
        EditorUtility.SetDirty(this);
#else
        houseBlock.transform.SetParent(this.transform);
#endif
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

        SetupHouseBlockVertices(polyVertices);
    }

    private void SetupHouseBlockVertices(List<Vector3> polyVertices)
    {
        List<Vector3> newVertices = new List<Vector3>();

        float distanceMultiplier = 0.2f;

        for (int i = 0; i < polyVertices.Count; i++)
        {
            Vector3 v1 = polyVertices[i];
            Vector3 v2 = polyVertices[(i + 1) % polyVertices.Count];
            Vector3 v3 = polyVertices[(i + 2) % polyVertices.Count];

            // Get the perpendicular of the line which also goes through the seed
            Vector3 distance1 = CalculatePerpVectorLineToSeed(v1, v2);
            // Start constructing the infinite line that v1 and v2 lie on in its 'normal' form:
            // ------------------
            // Get the normal
            Vector3 nLine1 = distance1.normalized;
            // Get 'c' from the line equation n . p = c (where p is a given point on the line)
            float cLine1 = Vector3.Dot(nLine1, v1);
            // Calculate offset amount based on how far was the original edge from the cell's seed
            float offsetAmount1 = distance1.magnitude * distanceMultiplier;
            // Get the new 'c' for the offset line
            float cNewLine1 = cLine1 + offsetAmount1;

            // Repeat the same process for the second line
            Vector3 distance2 = CalculatePerpVectorLineToSeed(v2, v3);
            Vector3 nLine2 = distance2.normalized;
            float cLine2 = Vector3.Dot(nLine2, v2);
            float offsetAmount2 = distance2.magnitude * distanceMultiplier;
            float cNewLine2 = cLine2 + offsetAmount2;

            
            // Find the intersection by expanding the following system:
            //
            // | n1 . P = c1New
            // | n2 . P = c2New
            //
            // where P is the intersection point as it satisfies both equations
            
            float denominator = nLine1.x * nLine2.z - nLine1.z * nLine2.x;
            if (Mathf.Approximately(denominator, Mathf.Epsilon)) // Parallel - skip
                continue;

            Vector3 newVertex = new Vector3
            (
                (cNewLine1 * nLine2.z - cNewLine2 * nLine1.z) / denominator,
                v2.y,
                (nLine1.x * cNewLine2 - nLine2.x * cNewLine1) / denominator
            );
            
            
            // Add to the new collection of vertices
            newVertices.Add(newVertex);
        }

        _houseBlock.SetupHouseLineConnections(newVertices);
    }

    private Vector3 CalculatePerpVectorLineToSeed(Vector3 v1, Vector3 v2)
    {
        Vector3 delta = (v1 - v2).normalized;
        Vector3 toSeed = transform.position - v1;
        Vector3 projected = delta * Vector3.Dot(toSeed, delta);
        Vector3 distance = toSeed - projected;

        return distance;
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