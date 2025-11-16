using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HouseLine : MonoBehaviour
{
    private Mesh _cubeMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [HideInInspector] public HouseLineSettings LineSettings;
    public string HouseLineSettingsName => nameof(LineSettings);

    [HideInInspector] public HouseLineSettingsData LineSettingsDataCopy;
    public string LineSettingsDataCopyName => nameof(LineSettingsDataCopy);

    [HideInInspector] public HouseSettingsData HouseSettingsDataCopy;
    public string HouseSettingsDataCopyName => nameof(HouseSettingsDataCopy);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellSettings">Cell settings to inherit from</param>
    public void Init(HouseBlockSettings blockSettings)
    {
        _cubeMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _cubeMesh;

        LineSettings = blockSettings.HouseLineSettings;
        LineSettingsDataCopy = new HouseLineSettingsData(LineSettings.SettingsData);
        HouseSettingsDataCopy = new HouseSettingsData(LineSettings.HouseSettingsData);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = LineSettings.SettingsData.LineMaterial;
    }

    /// <summary>
    /// Generates a rectangular parallelepiped mesh which is: <br/>
    /// - displaced inwards by <see cref="offset"/>/2 <br/>
    /// - high as much as HouseSettingsDataCopy says <br/>
    /// - initially wide as much as <see cref="offset"/> is <br/>
    /// - deep as much as the length of <see cref="line"/> 
    /// </summary>
    /// <param name="line">The vertices (part of the house block mesh polygon) that mark the start and end of this line (in world space)</param>
    /// <param name="offset">The inward displacement previously calculated from within the house block</param>
    public void Setup(Tuple<Vector3, Vector3> line, float offset)
    {
        var houseLineV1 = line.Item1;
        var houseLineV2 = line.Item2;

        var midPoint = (houseLineV1 + houseLineV2) * 0.5f;
        transform.position = midPoint;

        var center = transform.position;

        float meshHeight = (HouseSettingsDataCopy.MinHeight + HouseSettingsDataCopy.MaxHeight) * 0.5f;
        float meshHeightHalf = meshHeight * 0.5f;
        float meshWidth = offset;

        Vector3 edge = (houseLineV2 - houseLineV1).normalized;
        Vector3 cross1 = Vector3.Cross(edge, transform.up);
        Vector3 cross2 = Vector3.Cross(transform.up, edge);

        Vector3 widthDirVector = (Vector3.Dot(cross1, transform.parent.position - center)) > 0 ? cross1 : cross2;

        List<Vector3> meshVertices = new List<Vector3>()
        {
            // houseLineV1 face
            houseLineV1 + (transform.up * meshHeightHalf), // v1
            houseLineV1 + (-transform.up * meshHeightHalf), // v2
            houseLineV1 + (-transform.up * meshHeightHalf) + widthDirVector * meshWidth, // v3
            houseLineV1 + (transform.up * meshHeightHalf) + widthDirVector * meshWidth, // v4

            // houseLineV2 face
            houseLineV2 + (transform.up * meshHeightHalf), // v5
            houseLineV2 + (-transform.up * meshHeightHalf), // v6
            houseLineV2 + (-transform.up * meshHeightHalf) + widthDirVector * meshWidth, // v7
            houseLineV2 + (transform.up * meshHeightHalf) + widthDirVector * meshWidth, // v8
        };

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
}