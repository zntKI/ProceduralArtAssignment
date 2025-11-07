using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class VoronoiCell : MonoBehaviour
{
    [SerializeField] private float debugSeedCubeSizeModifier = 0.3f;

    private Mesh _polyMesh;
    private MeshFilter _meshFilter;

    public void Init()
    {
        _polyMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _polyMesh;
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

        // Color[] colors = new Color[_polyMesh.vertices.Length];
        
        Color randomColor = new Color(Random.value, Random.value, Random.value, 0.5f);
        // for (int i = 0; i < _polyMesh.vertices.Length; i++)
        // {
        //     colors[i] = randomColor;
        // }
        //
        // _polyMesh.colors = colors;

        GetComponent<MeshRenderer>().material.color = randomColor;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.4f);
    
        var debugCubeSize = new Vector3(
            transform.parent.localScale.x * debugSeedCubeSizeModifier,
            .5f,
            transform.parent.localScale.z * debugSeedCubeSizeModifier
        );
    
        Gizmos.DrawCube(transform.position, debugCubeSize);
    }
}