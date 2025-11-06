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
        var basePoly = new List<Vector2>
        {
            new(-half.x, -half.z),
            new( half.x, -half.z),
            new( half.x,  half.z),
            new(-half.x,  half.z)
        };

        var seeds = new List<Transform>();
        var seeds2D = new List<Vector2>();
        foreach (Transform child in transform)
        {
            seeds.Add(child);
            var lp = child.localPosition;
            seeds2D.Add(new Vector2(lp.x, lp.z));
        }

        var cellsGenerated = new List<List<Vector2>>();
        
        foreach (var seed2D in seeds2D)
        {
            var cellPoly = new List<Vector2>(basePoly);
            
            foreach (var otherSeed2D in seeds2D.Where(s => s != seed2D))
            {
                // Create a bisector
                Vector2 midpoint = (seed2D + otherSeed2D) * 0.5f;
                Vector2 normal = (otherSeed2D - seed2D).normalized;
                float c = Vector2.Dot(normal, midpoint);
                
                float sideSeed = Vector2.Dot(normal, seed2D) - c;
                float seedSideSign = Mathf.Sign(sideSeed);
                
                var outPoly = new List<Vector2>();

                for (int i = 0; i < cellPoly.Count; i++)
                {
                    var p1 = cellPoly[i];
                    var p2 = cellPoly[(i + 1) % cellPoly.Count];
                    
                    // Check if p1 is on the correct side
                    float p1SideSign = Mathf.Sign(Vector2.Dot(normal, p1) - c);
                    bool isP1CorrectSide = Mathf.Approximately(p1SideSign, seedSideSign);
                    
                    // Check if p2 is on the correct side
                    float p2SideSign = Mathf.Sign(Vector2.Dot(normal, p2) - c);
                    bool isP2CorrectSide = Mathf.Approximately(p2SideSign, seedSideSign);

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
            
            cellsGenerated.Add(cellPoly);
        }

        foreach (var cell in cellsGenerated)
        {
            Debug.Log("Cell " + cellsGenerated.IndexOf(cell));
            foreach (var vertex in cell)
            {
                Debug.Log($"\t{vertex}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
    }

    private void CreateCell(Vector3 point)
    {
        var cell = new GameObject($"Cell{transform.childCount+1}",
            typeof(VoronoiCell));
        cell.transform.position = point + transform.up;
        
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(cell, "Created Voronoi Cell");
        Undo.SetTransformParent(cell.transform, transform, "Set Voronoi Cell Parent");
        EditorUtility.SetDirty(this);
#else
        cell.transform.SetParent(this.transform);
#endif
    }
}