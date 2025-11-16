using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
    public string HouseBlockSettingsName => nameof(BlockSettings);


    [HideInInspector] public HouseBlockSettingsData BlockSettingsDataCopy;
    public string BlockSettingsDataCopyName => nameof(BlockSettingsDataCopy);


    [HideInInspector] public HouseLineSettingsData HouseLineSettingsDataCopy;
    public string HouseLineSettingsDataCopyName => nameof(HouseLineSettingsDataCopy);


    [HideInInspector] public HouseSettingsData HouseSettingsDataCopy;
    public string HouseSettingsDataCopyName => nameof(HouseSettingsDataCopy);

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

        BlockSettings = cellSettings.HouseBlockSettings;
        BlockSettingsDataCopy = new HouseBlockSettingsData(BlockSettings.SettingsData);
        HouseLineSettingsDataCopy = new HouseLineSettingsData(BlockSettings.HouseLineSettingsData);
        HouseSettingsDataCopy = new HouseSettingsData(BlockSettings.HouseSettingsData);

        // TODO: Make it take the half of the average between the min and max height properties
        float averageHeight = (HouseSettingsDataCopy.MinHeight + HouseSettingsDataCopy.MaxHeight) * 0.5f;
        transform.position += new Vector3(0.0f, averageHeight * 0.5f, 0.0f);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = BlockSettings.SettingsData.BlockMaterial;
    }

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
            var v1 = _houseLinesConnections[i];
            var v2 = _houseLinesConnections[(i + 1) % _houseLinesConnections.Count];

            _houseLines[i].Setup(new Tuple<Vector3, Vector3>(v1, v2), offsets[i]);
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

    private void OnDrawGizmos()
    {
        // TODO: Maybe draw the house lines connections?
    }
}