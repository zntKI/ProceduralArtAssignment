using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VoronoiGeneration3D : MonoBehaviour
{
    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Called when new virtual seed is added to the collection<br/>
    /// Integrates the point as a new seed in the Voronoi diagram
    /// </summary>
    public void AddVoronoiCell(Vector3 point)
    {
        CreateCell(point);

        CalculateVoronoiDiagram();
    }

    public void CalculateVoronoiDiagram()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            Debug.LogWarning("Plane requires a MeshFilter with a mesh.");
            return;
        }

        var half = mesh.bounds.extents;
        var basePolyTemp = new List<Vector3>
        {
            new(-half.x, 0f, -half.z),
            new( half.x, 0f, -half.z),
            new( half.x, 0f,  half.z),
            new(-half.x, 0f,  half.z)
        };
        basePolyTemp = basePolyTemp.Select(v => transform.TransformPoint(v)).ToList();

        List<Vector2> basePoly = basePolyTemp.Select(v => new Vector2(v.x, v.z)).ToList();

        var cellsGenerated = new Dictionary<VoronoiCell, List<Vector2>>();
        var seeds2D = new Dictionary<VoronoiCell, Vector2>();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<VoronoiCell>(out var voronoiCell))
            {
                cellsGenerated[voronoiCell] = new List<Vector2>();
                var wp = child.position;
                seeds2D[voronoiCell] = new Vector2(wp.x, wp.z);
            }
        }
        
        foreach (var seed2D in seeds2D.Keys)
        {
            var cellPoly = new List<Vector2>(basePoly);
            
            foreach (var otherSeed2D in seeds2D.Keys.Where(s => s != seed2D))
            {
                var seed2DPos = seeds2D[seed2D];
                var otherSeed2DPos = seeds2D[otherSeed2D];
                
                // Create a bisector
                Vector2 midpoint = (seed2DPos + otherSeed2DPos) * 0.5f;
                Vector2 normal = (otherSeed2DPos - seed2DPos).normalized;
                float c = Vector2.Dot(normal, midpoint);
                
                float sideSeed = Vector2.Dot(normal, seed2DPos) - c;
                float seedSideSign = sideSeed >= 0 ? 1f : -1f;
                
                var outPoly = new List<Vector2>();

                for (int i = 0; i < cellPoly.Count; i++)
                {
                    var p1 = cellPoly[i];
                    var p2 = cellPoly[(i + 1) % cellPoly.Count];
                    
                    // Check if points are on the correct side
                    bool isP1CorrectSide = seedSideSign * (Vector2.Dot(normal, p1) - c) >= -Mathf.Epsilon;
                    bool isP2CorrectSide = seedSideSign * (Vector2.Dot(normal, p2) - c) >= -Mathf.Epsilon;

                    if (isP1CorrectSide && isP2CorrectSide)
                    {
                        outPoly.Add(p2);
                    }
                    else if (isP1CorrectSide)
                    {
                        // Intersect bisector with line
                        //
                        // ----------------------------
                        //
                        //     c - (n . p1)
                        // t = ------------
                        //     n . (p2 - p1)
                        var line = p2 - p1;
                        var numerator = c - Vector2.Dot(normal, p1);
                        var denominator = Vector2.Dot(normal, line);
                        var t = numerator / denominator;
                        
                        // Compute intersection
                        //
                        // I = p1 + t * (p2 - p1)
                        var intersectionPoint = p1 + t * line;
                        
                        outPoly.Add(intersectionPoint);
                    }
                    else if (isP2CorrectSide)
                    {
                        // Intersect bisector with line
                        //
                        // ----------------------------
                        //
                        //     c - (n . p1)
                        // t = ------------
                        //     n . (p2 - p1)
                        var line = p2 - p1;
                        var numerator = c - Vector2.Dot(normal, p1);
                        var denominator = Vector2.Dot(normal, line);
                        var t = numerator / denominator;
                        
                        // Compute intersection
                        //
                        // I = p1 + t * (p2 - p1)
                        var intersectionPoint = p1 + t * line;
                        
                        outPoly.Add(intersectionPoint);
                        outPoly.Add(p2);
                    }
                    
                }

                cellPoly = outPoly;
            }
            
            cellsGenerated[seed2D] = cellPoly;
        }

        foreach (var cell in cellsGenerated.Keys)
        {
            List<Vector3> polyVertices3D = new List<Vector3>();
            foreach (var polyVert in cellsGenerated[cell])
            {
                polyVertices3D.Add(new Vector3(polyVert.x, cell.transform.position.y, polyVert.y));
            }
            cell.GenerateMesh(polyVertices3D);
        }
    }

    private void OnDrawGizmosSelected()
    {
    }

    private void CreateCell(Vector3 point)
    {
        var cell = new GameObject($"Voronoi Cell {transform.childCount+1}",
            typeof(VoronoiCell), typeof(MeshFilter), typeof(MeshRenderer)).GetComponent<VoronoiCell>();
        
        cell.transform.position = point + transform.up * 0.01f;
        cell.transform.SetParent(transform);

        Undo.RegisterCreatedObjectUndo(cell.gameObject, "Created Voronoi Cell");
        
        cell.Init();
        
        EditorUtility.SetDirty(this);
    }
}