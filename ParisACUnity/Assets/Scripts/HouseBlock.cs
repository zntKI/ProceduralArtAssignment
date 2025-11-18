using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

public class HouseBlock : MonoBehaviour
{
    private Mesh _polyMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private List<Vector3> _houseLinesConnections = new List<Vector3>();
    private List<HouseLine> _houseLines = new List<HouseLine>();


    #region SettingsRegion

    [HideInInspector] public HouseBlockSettings BlockSettings;
    [HideInInspector] public HouseBlockSettingsData BlockSettingsDataCopy;

    [HideInInspector] public HouseLineSettings HouseLineSettings;
    [HideInInspector] public HouseLineSettingsData HouseLineSettingsDataCopy;

    [HideInInspector] public HouseSettings HouseSettings;
    [HideInInspector] public HouseSettingsData HouseSettingsDataCopy;

    #endregion


    private void OnEnable()
    {
        if (transform.parent.TryGetComponent<VoronoiCell>(out var vc))
        {
            Init(vc.CellSettings);
        }
        else
            Debug.LogError($"OnEnable: game object expects to have a VoronoiCell type of parent but doesn't!?");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellSettings">Cell settings to inherit from</param>
    public void Init(VoronoiCellSettings cellSettings)
    {
        _polyMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _polyMesh;

        #region SettingsInit

        if (!cellSettings.HouseBlockSettings)
        {
            BlockSettings = AssetDatabase.LoadAssetAtPath<HouseBlockSettings>(
                Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseBlockSettingsFolder,
                    Config.ScriptableObjects_HouseBlockSettingsDefault));
            Debug.LogWarning(
                "No appropriate HouseBlockSettings present in VoronoiCellSettings - Initialized with default HouseBlockSettings");
        }
        else
            BlockSettings = cellSettings.HouseBlockSettings;

        BlockSettingsDataCopy = new HouseBlockSettingsData(BlockSettings.SettingsData);

        if (!BlockSettings.HouseLineSettings)
        {
            HouseLineSettings = AssetDatabase.LoadAssetAtPath<HouseLineSettings>(
                Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseLineSettingsFolder,
                    Config.ScriptableObjects_HouseLineSettingsDefault));
            Debug.LogWarning(
                "No appropriate HouseLineSettings present in HouseBlockSettings - Initialized with default HouseLineSettings");
        }
        else
            HouseLineSettings = BlockSettings.HouseLineSettings;

        HouseLineSettingsDataCopy = new HouseLineSettingsData(HouseLineSettings.SettingsData);

        if (!BlockSettings.HouseSettings)
        {
            HouseSettings = AssetDatabase.LoadAssetAtPath<HouseSettings>(
                Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseSettingsFolder,
                    Config.ScriptableObjects_HouseSettingsDefault));
            Debug.LogWarning(
                "No appropriate HouseSettings present in HouseBlockSettings - Initialized with default HouseSettings");
        }
        else
            HouseSettings = BlockSettings.HouseSettings;

        HouseSettingsDataCopy = new HouseSettingsData(HouseSettings.SettingsData);

        #endregion

        // TODO: Make it take the half of the average between the min and max height properties
        float averageHeight = (HouseSettingsDataCopy.MinHeight + HouseSettingsDataCopy.MaxHeight) * 0.5f;
        transform.position += new Vector3(0.0f, averageHeight * 0.5f, 0.0f);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = BlockSettings.SettingsData.BlockMaterial;
    }

    #region HouseBlockGenerationCode

    /// <summary>
    /// Sets up a smaller polygon area derived from the original voronoi cell polygon,
    /// taking into consideration the distance from the cell center to each line
    /// and using that as the inwards displacement 
    /// </summary>
    /// <param name="polyVertices">The vertices of the voronoi cell in world space</param>
    public void SetupHouseBlockVertices(List<Vector3> polyVertices)
    {
        List<Vector3> newVertices = new List<Vector3>();

        // Offset of new lines - used for creation of house lines later on
        List<float> offsets = new List<float>();

        float distanceMultiplier = 0.2f;

        for (int i = 0; i < polyVertices.Count; i++)
        {
            Vector3 v1 = polyVertices[i];
            Vector3 v2 = polyVertices[(i + 1) % polyVertices.Count];
            Vector3 v3 = polyVertices[(i + 2) % polyVertices.Count];


            // Simple representation of what is happening:
            //
            // new block mesh    cell mesh
            //        |             |
            //        |             |
            //            /            / < - v3
            //           / - offset - /
            //  --------/            /
            //   newVert^           /
            //  -------------------/ < - v2
            //  ^
            //  |
            // v1

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


            newVertices.Add(newVertex);
            offsets.Add(offsetAmount2);
        }

        SetupHouseLineConnections(new Tuple<List<Vector3>, List<float>>(newVertices, offsets));
    }

    private Vector3 CalculatePerpVectorLineToSeed(Vector3 v1, Vector3 v2)
    {
        Vector3 delta = (v1 - v2).normalized;
        Vector3 toSeed = transform.parent.position - v1;
        Vector3 projected = delta * Vector3.Dot(toSeed, delta);
        Vector3 distance = toSeed - projected;

        return distance;
    }

    /// <summary>
    /// </summary>
    /// <param name="param"><br/>
    /// List of Vector3 - house line connections<br/>
    /// List of float - offsets for each new line
    /// </param>
    public void SetupHouseLineConnections(Tuple<List<Vector3>, List<float>> param)
    {
        _polyMesh.Clear();

        var houseLineConnections = param.Item1;

        // Adjust the height of the house block mesh vertices according to the set height of the houses
        float averageHeight = (HouseSettingsDataCopy.MinHeight + HouseSettingsDataCopy.MaxHeight) * 0.5f;
        houseLineConnections = houseLineConnections
            .Select(v => v += new Vector3(0, averageHeight * 0.5f, 0)).ToList();

        // Transform from world to local space
        _polyMesh.vertices = houseLineConnections
            .Select(v => v = transform.InverseTransformPoint(v)).ToArray();

        var meshTriangles = new List<int>();
        for (int i = 0; i < houseLineConnections.Count - 2; i++)
        {
            meshTriangles.Add(0);
            meshTriangles.Add(i + 2);
            meshTriangles.Add(i + 1);
        }

        _polyMesh.triangles = meshTriangles.ToArray();


        // House Line logic:

        // If house lines number changed - recreate house line game objects
        if (houseLineConnections.Count != _houseLinesConnections.Count)
            CreateHouseLines(houseLineConnections.Count);

        _houseLinesConnections = houseLineConnections;

        // Called always to recalculate mesh position/scale
        SetupHouseLines(param.Item2);
    }

    /// <summary>
    /// Iterates through all house lines and updates their properties
    /// </summary>
    /// <param name="offsets">Collection storing the inwards displacement for each house line</param>
    private void SetupHouseLines(List<float> offsets)
    {
        // Indexing is VERY STRICT and is directly related to how the house block mesh generation is set up
        for (int i = 0; i < _houseLinesConnections.Count; i++)
        {
            var v1NeighbouringConnection = _houseLinesConnections[((i - 1) + _houseLinesConnections.Count) % _houseLinesConnections.Count];
            var v1 = _houseLinesConnections[i];

            var v2 = _houseLinesConnections[(i + 1) % _houseLinesConnections.Count];
            var v2NeighbouringConnection = _houseLinesConnections[(i + 2) % _houseLinesConnections.Count];

            _houseLines[i].Setup(new Tuple<Vector3, Vector3>(v1, v2),
                new Tuple<Vector3, Vector3>(v1NeighbouringConnection, v2NeighbouringConnection));
        }
    }

    /// <summary>
    /// Called when the house block has been moved and in the process, the amount of house lines changed.<br/>
    /// <br/>
    /// Destroys any previous house lines and creates <see cref="houseLinesCount"/> number of new ones.
    /// </summary>
    /// <param name="houseLinesCount"></param>
    private void CreateHouseLines(int houseLinesCount)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Undo.DestroyObjectImmediate(child.gameObject);
        }

        _houseLines.Clear();

        for (int i = 0; i < houseLinesCount; i++)
        {
            HouseLine houseLine = new GameObject($"House Line {transform.childCount + 1}",
                typeof(HouseLine), typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<HouseLine>();

            houseLine.transform.SetParent(transform);

            Undo.RegisterCreatedObjectUndo(houseLine.gameObject, "Created House Line");

            houseLine.Init(BlockSettings);

            _houseLines.Add(houseLine);
        }

        EditorUtility.SetDirty(this);
    }

    #endregion


    #region EditorCode

    #region ScriptableObjectSwappingLogic

    public void UpdateSettings(string settingsName)
    {
        if (settingsName == nameof(BlockSettings))
            UpdateBlockSettings();
        else if (settingsName == nameof(HouseLineSettings))
            UpdateHouseLineSettings();
        else if (settingsName == nameof(HouseSettings))
            UpdateHouseSettings();
    }

    private void UpdateBlockSettings()
    {
        BlockSettingsDataCopy = new HouseBlockSettingsData(BlockSettings.SettingsData);

        UpdateBlockMaterial();
    }

    private void UpdateHouseLineSettings()
    {
        HouseLineSettingsDataCopy = new HouseLineSettingsData(HouseLineSettings.SettingsData);

        _houseLines.ForEach(hl => hl.UpdateHouseLineSettings(HouseLineSettings));
    }

    private void UpdateHouseSettings()
    {
        HouseSettingsDataCopy = new HouseSettingsData(HouseSettings.SettingsData);

        _houseLines.ForEach(hl => hl.UpdateHouseSettings(HouseSettings));
    }

    #endregion


    #region SettingsFieldChangeLogic

    public void UpdateSettingsCopyByField(Type settingsType, FieldInfo fieldInfo)
    {
        if (settingsType == typeof(HouseBlockSettingsData))
            UpdateBlockSettingsCopyByField(fieldInfo);
        else if (settingsType == typeof(HouseLineSettingsData))
            UpdateHouseLineSettingsCopyByField(fieldInfo);
        else if (settingsType == typeof(HouseSettingsData))
            UpdateHouseSettingsCopyByField(fieldInfo);
    }

    private void UpdateBlockSettingsCopyByField(FieldInfo fieldInfo)
    {
        if (fieldInfo.Name == nameof(BlockSettingsDataCopy.BlockMaterial))
            UpdateBlockMaterial();
    }

    private void UpdateBlockMaterial()
    {
#if UNITY_EDITOR
        Undo.RecordObject(_meshRenderer, "Updated Block Settings");
        _meshRenderer.material = BlockSettingsDataCopy.BlockMaterial;
        EditorUtility.SetDirty(_meshRenderer);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    private void UpdateHouseLineSettingsCopyByField(FieldInfo fieldInfo)
    {
        _houseLines.ForEach(hl => hl.UpdateHouseLineSettingsCopy(HouseLineSettingsDataCopy, fieldInfo));
    }

    private void UpdateHouseSettingsCopyByField(FieldInfo fieldInfo)
    {
        _houseLines.ForEach(hl => hl.UpdateHouseSettingsCopy(HouseSettingsDataCopy, fieldInfo));
    }

    public void ApplySettingsCopyToOriginal(Type settingsType)
    {
        if (settingsType == typeof(HouseBlockSettingsData))
            ApplyBlockSettingsCopyToOriginal();
        else if (settingsType == typeof(HouseLineSettingsData))
            ApplyHouseLineSettingsCopyToOriginal();
        else if (settingsType == typeof(HouseSettingsData))
            ApplyHouseSettingsCopyToOriginal();
    }

    private void ApplyBlockSettingsCopyToOriginal()
    {
#if UNITY_EDITOR
        Undo.RecordObject(BlockSettings, "Applied Block Settings To Original");
        BlockSettings.SettingsData = new HouseBlockSettingsData(BlockSettingsDataCopy);
        EditorUtility.SetDirty(BlockSettings);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    private void ApplyHouseLineSettingsCopyToOriginal()
    {
#if UNITY_EDITOR
        Undo.RecordObject(HouseLineSettings, "Applied House Line Settings To Original");
        HouseLineSettings.SettingsData = new HouseLineSettingsData(HouseLineSettingsDataCopy);
        EditorUtility.SetDirty(HouseLineSettings);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    private void ApplyHouseSettingsCopyToOriginal()
    {
        Debug.Log("In");
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
        if (settingsType == typeof(HouseBlockSettingsData))
            SaveBlockSettingsAsNewSO();
        else if (settingsType == typeof(HouseLineSettingsData))
            SaveHouseLineSettingsAsNewSO();
        else if (settingsType == typeof(HouseSettingsData))
            SaveHouseSettingsAsNewSO();
    }

    private void SaveBlockSettingsAsNewSO()
    {
        var newSettings = ScriptableObject.CreateInstance<HouseBlockSettings>();
        newSettings.SettingsData = new HouseBlockSettingsData(BlockSettingsDataCopy);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save New House Block Settings - new name goes AFTER '_'",
            "HouseBlockSettings_New",
            "asset",
            "Choose a location for the new settings file",
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseBlockSettingsFolder));

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(newSettings);
        }
    }

    private void SaveHouseLineSettingsAsNewSO()
    {
        var newSettings = ScriptableObject.CreateInstance<HouseLineSettings>();
        newSettings.SettingsData = new HouseLineSettingsData(HouseLineSettingsDataCopy);

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

    #endregion
}