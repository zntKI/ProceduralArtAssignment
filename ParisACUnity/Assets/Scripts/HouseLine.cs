using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class HouseLine : Shape
{
    private Mesh _cubeMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [HideInInspector] public HouseLineSettings LineSettings;
    [HideInInspector] public HouseLineSettingsData LineSettingsDataCopy;

    [HideInInspector] public HouseSettings HouseSettings;
    [HideInInspector] public HouseSettingsData HouseSettingsDataCopy;

    private Tuple<Vector3, Vector3> _meshLine;
    private Tuple<Vector3, Vector3> _neighbouringTwoLinePoints;

    private List<House> _houses = new List<House>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockSettings">Block settings to inherit from</param>
    public void Init(HouseBlockSettings blockSettings)
    {
        _cubeMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _cubeMesh;

        if (!blockSettings.HouseLineSettings)
        {
            LineSettings = AssetDatabase.LoadAssetAtPath<HouseLineSettings>(
                Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseLineSettingsFolder,
                    Config.ScriptableObjects_HouseLineSettingsDefault));
            Debug.LogWarning(
                "No appropriate HouseLineSettings present in HouseBlockSettings - Initialized with default HouseLineSettings");
        }
        else
            LineSettings = blockSettings.HouseLineSettings;

        LineSettingsDataCopy = new HouseLineSettingsData(LineSettings.SettingsData);

        if (!blockSettings.HouseSettings)
        {
            HouseSettings = AssetDatabase.LoadAssetAtPath<HouseSettings>(
                Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseSettingsFolder,
                    Config.ScriptableObjects_HouseSettingsDefault));
            Debug.LogWarning(
                "No appropriate HouseSettings present in HouseBlockSettings - Initialized with default HouseSettings");
        }
        else
            HouseSettings = blockSettings.HouseSettings;

        HouseSettingsDataCopy = new HouseSettingsData(HouseSettings.SettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = LineSettings.SettingsData.LineMaterial;
    }

    public void ReInit(HouseBlockSettings blockSettings)
    {
        Init(blockSettings);
        
        _houses.ForEach(h => h.ReInit(HouseSettings, HouseSettingsDataCopy));
    }

    /// <summary>
    /// Generates a rectangular parallelepiped mesh which is: <br/>
    /// - displaced inwards by <see cref="offset"/>/2 <br/>
    /// - high as much as HouseSettingsDataCopy says <br/>
    /// - initially wide as much as <see cref="offset"/> is <br/>
    /// - deep as much as the length of <see cref="line"/> 
    /// </summary>
    /// <param name="line">The vertices (part of the house block mesh polygon) that mark the start and end of this line (in world space)</param>
    /// <param name="neighbouringTwoLinePoints">The neighbour points to line which are used to calculate small house line displacement</param>
    public void Setup(Tuple<Vector3, Vector3> line, Tuple<Vector3, Vector3> neighbouringTwoLinePoints)
    {
        var houseLineV1 = line.Item1;
        var houseLineV2 = line.Item2;

        Vector3 edgeDir1 = (houseLineV2 - houseLineV1).normalized;
        Vector3 cross1 = Vector3.Cross(edgeDir1, transform.up);
        Vector3 cross2 = Vector3.Cross(transform.up, edgeDir1);

        var midPoint = (houseLineV1 + houseLineV2) * 0.5f;
        Vector3 widthDirVector = (Vector3.Dot(cross1, transform.parent.position - midPoint)) > 0 ? cross1 : cross2;
        Vector3 localForwardVector = widthDirVector * -1.0f;

        int meshHeight = HouseSettingsDataCopy.MaxHeight;
        float meshHeightHalf = meshHeight * 0.5f;
        int meshDepth = HouseSettingsDataCopy.MaxDepth;
        float meshDepthHalf = meshDepth * 0.5f;

        #region MyRegion
        
        _neighbouringTwoLinePoints =
            new Tuple<Vector3, Vector3>(neighbouringTwoLinePoints.Item1, neighbouringTwoLinePoints.Item2);
        var neighbouringPointV1 = _neighbouringTwoLinePoints.Item1;
        var neighbouringPointV2 = _neighbouringTwoLinePoints.Item2;

        float angleV1 = Vector3.Angle(edgeDir1, neighbouringPointV1 - houseLineV1);
        float angleV1Half = angleV1 * 0.5f;
        float displacementAmountV1 = meshDepth / Mathf.Tan(Mathf.Deg2Rad * angleV1Half);

        Vector3 v1FaceWidthDisplacement = edgeDir1 * displacementAmountV1;


        Vector3 edgeDir2 = (houseLineV1 - houseLineV2).normalized;

        float angleV2 = Vector3.Angle(edgeDir2, neighbouringPointV2 - houseLineV2);
        float angleV2Half = angleV2 * 0.5f;
        float displacementAmountV2 = meshDepth / Mathf.Tan(Mathf.Deg2Rad * angleV2Half);

        Vector3 v2FaceWidthDisplacement = edgeDir2 * displacementAmountV2;

        #endregion
        
        houseLineV1 = houseLineV1 + (-transform.up * meshHeightHalf) + (widthDirVector * meshDepthHalf) + v1FaceWidthDisplacement;
        houseLineV2 = houseLineV2 + (-transform.up * meshHeightHalf) + (widthDirVector * meshDepthHalf) + v2FaceWidthDisplacement;
        _meshLine = new Tuple<Vector3, Vector3>(houseLineV1, houseLineV2);

        // Position set to the center of the bottom for ease of procedural house placement
        transform.position = (houseLineV1 + houseLineV2) * 0.5f;
        transform.LookAt(transform.position + localForwardVector);
        
        List<Vector3> meshVertices = new List<Vector3>()
        {
            // houseLineV1 face
            houseLineV1 + (-widthDirVector * meshDepthHalf), // v1
            houseLineV1 + (widthDirVector * meshDepthHalf), // v2
            houseLineV1 + (widthDirVector * meshDepthHalf) + (transform.up * meshHeight), // v3
            houseLineV1 + (-widthDirVector * meshDepthHalf) + (transform.up * meshHeight), // v4

            // houseLineV2 face
            houseLineV2 + (-widthDirVector * meshDepthHalf), // v5
            houseLineV2 + (widthDirVector * meshDepthHalf), // v6
            houseLineV2 + (widthDirVector * meshDepthHalf) + (transform.up * meshHeight), // v7
            houseLineV2 + (-widthDirVector * meshDepthHalf) + (transform.up * meshHeight), // v8
        };

        //  meshVertices = meshVertices.Select(v => v += -widthDirVector * meshDepth * 0.5f).ToList();

        _cubeMesh.vertices = meshVertices.Select(v => v = transform.InverseTransformPoint(v)).ToArray();

        _cubeMesh.triangles = new int[]
        {
            // Front face
            0, 1, 2,
            0, 2, 3,

            // Back face
            4, 6, 5,
            4, 7, 6,

            // Left face
            0, 4, 1,
            1, 4, 5,

            // Right face
            3, 2, 7,
            2, 6, 7,

            // Top face
            0, 3, 4,
            3, 7, 4,

            // Bottom face
            1, 5, 2,
            2, 5, 6
        };
    }

    #region ShapeGrammarCode

    protected override void Execute()
    {
        _meshRenderer.enabled = false;
        
        var houseLineV1 = _meshLine.Item1;
        var houseLineV2 = _meshLine.Item2;
        var houseLine = houseLineV2 - houseLineV1;
        var houseLineDir = houseLine.normalized;

        float remainingLength = houseLine.magnitude;
        int houseWidth = RandomInt(HouseSettingsDataCopy.MinWidth, HouseSettingsDataCopy.MaxWidth + 1);
        int filledLength = 0;

        while (remainingLength >= houseWidth)
        {
            Vector3 housePosOffsetAlongLineDir = houseLineDir * (filledLength + (houseWidth * 0.5f));
            Vector3 newPoint = houseLineV1 + housePosOffsetAlongLineDir;
            Vector3 houseLocalPos = newPoint - transform.position;

            houseLocalPos = transform.InverseTransformDirection(houseLocalPos);

            House house = CreateSymbol<House>($"House {transform.childCount + 1}", houseLocalPos,
                default(Quaternion), default(Vector3), transform, null,
                typeof(MeshFilter), typeof(MeshRenderer));
            house.Init(HouseSettings, HouseSettingsDataCopy);
            house.Init(houseWidth);

            _houses.Add(house);

            house.Generate();

            remainingLength -= houseWidth;
            filledLength += houseWidth;

            houseWidth = RandomInt(LineSettings.HouseSettings.SettingsData.MinWidth,
                LineSettings.HouseSettings.SettingsData.MaxWidth + 1);
        }
    }

    #endregion

    #region EditorCode

    #region ScriptableObjectSwappingLogic

    public void UpdateSettings(string settingsName)
    {
        if (settingsName == nameof(LineSettings))
            UpdateLineSettings();
        else if (settingsName == nameof(HouseSettings))
            UpdateHouseSettings();
    }

    private void UpdateLineSettings()
    {
        LineSettingsDataCopy = new HouseLineSettingsData(LineSettings.SettingsData);

        UpdateLineMaterial();
        
        _houses.ForEach(h => h.ReInit(HouseSettings, HouseSettingsDataCopy));
    }

    private void UpdateHouseSettings()
    {
        HouseSettingsDataCopy = new HouseSettingsData(HouseSettings.SettingsData);

        Setup(_meshLine, _neighbouringTwoLinePoints);

        // TODO: Forward down to houses if they exist
    }

    #endregion


    #region SettingsFieldChangeLogic

    public void UpdateSettingsCopyByField(Type settingsType, FieldInfo fieldInfo)
    {
        if (settingsType == typeof(HouseLineSettingsData))
            UpdateLineSettingsCopyByField(fieldInfo);
        else if (settingsType == typeof(HouseSettingsData))
            UpdateHouseSettingsCopyByField(fieldInfo);
    }

    private void UpdateLineSettingsCopyByField(FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(LineSettingsDataCopy.LineMaterial))
            UpdateLineMaterial();
    }

    private void UpdateLineMaterial()
    {
#if UNITY_EDITOR
        Undo.RecordObject(_meshRenderer, "Updated Line Settings");
        _meshRenderer.material = LineSettingsDataCopy.LineMaterial;
        EditorUtility.SetDirty(_meshRenderer);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    private void UpdateHouseSettingsCopyByField(FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(HouseSettingsDataCopy.MinHeight) ||
            fieldInfo.Name == nameof(HouseSettingsDataCopy.MaxHeight))
        {
            Setup(_meshLine, _neighbouringTwoLinePoints);
        }

        // TODO: Forward down to houses if they exist
    }

    public void ApplySettingsCopyToOriginal(Type settingsType)
    {
        if (settingsType == typeof(HouseLineSettingsData))
            ApplyLineSettingsCopyToOriginal();
        else if (settingsType == typeof(HouseSettingsData))
            ApplyHouseSettingsCopyToOriginal();
    }

    private void ApplyLineSettingsCopyToOriginal()
    {
#if UNITY_EDITOR
        Undo.RecordObject(LineSettings, "Applied House Line Settings To Original");
        LineSettings.SettingsData = new HouseLineSettingsData(LineSettingsDataCopy);
        EditorUtility.SetDirty(LineSettings);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    private void ApplyHouseSettingsCopyToOriginal()
    {
#if UNITY_EDITOR
        Undo.RecordObject(HouseSettings, "Applied House Settings To Original");
        HouseSettings.SettingsData = new HouseSettingsData(HouseSettingsDataCopy);
        EditorUtility.SetDirty(HouseSettings);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    public void SaveSettingsAsNewSO(Type settingsType)
    {
        if (settingsType == typeof(HouseLineSettingsData))
            SaveLineSettingsAsNewSO();
        else if (settingsType == typeof(HouseSettingsData))
            SaveHouseSettingsAsNewSO();
    }

    private void SaveLineSettingsAsNewSO()
    {
        var newSettings = ScriptableObject.CreateInstance<HouseLineSettings>();
        newSettings.SettingsData = new HouseLineSettingsData(LineSettingsDataCopy);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save New House Line Settings - new name goes AFTER '_'",
            "HouseLineSettings_New",
            "asset",
            "Choose a location for the new settings file",
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseLineSettingsFolder));

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(newSettings);
        }
    }

    private void SaveHouseSettingsAsNewSO()
    {
        var newSettings = ScriptableObject.CreateInstance<HouseSettings>();
        newSettings.SettingsData = new HouseSettingsData(HouseSettingsDataCopy);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save New House Settings - new name goes AFTER '_'",
            "HouseSettings_New",
            "asset",
            "Choose a location for the new settings file",
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseSettingsFolder));

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(newSettings);
        }
    }

    #endregion

    public void UpdateHouseLineSettings(HouseLineSettings overrideHouseLineSettings)
    {
        LineSettings = overrideHouseLineSettings;

        LineSettingsDataCopy = new HouseLineSettingsData(LineSettings.SettingsData);
    }

    public void UpdateHouseSettings(HouseSettings overrideHouseSettings)
    {
        HouseSettings = overrideHouseSettings;

        HouseSettingsDataCopy = new HouseSettingsData(HouseSettings.SettingsData);

        Setup(_meshLine, _neighbouringTwoLinePoints);

        // TODO: Forward down to houses if they exist
    }

    public void UpdateHouseLineSettingsCopy(HouseLineSettingsData overrideSettingsData, FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(LineSettingsDataCopy.LineMaterial))
            UpdateLineMaterial();
    }

    public void UpdateHouseSettingsCopy(HouseSettingsData overrideSettingsData, FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(HouseSettingsDataCopy.MinHeight))
            HouseSettingsDataCopy.MinHeight = overrideSettingsData.MinHeight;
        else if (fieldInfo.Name == nameof(HouseSettingsDataCopy.MaxHeight))
            HouseSettingsDataCopy.MaxHeight = overrideSettingsData.MaxHeight;
        else if (fieldInfo.Name == nameof(HouseSettingsDataCopy.MaxDepth))
            HouseSettingsDataCopy.MaxDepth = overrideSettingsData.MaxDepth;

        if (fieldInfo.Name == nameof(HouseSettingsDataCopy.MinHeight) ||
            fieldInfo.Name == nameof(HouseSettingsDataCopy.MaxHeight))
        {
            Setup(_meshLine, _neighbouringTwoLinePoints);
        }

        // TODO: Forward down to houses if they exist
    }

    #endregion
}