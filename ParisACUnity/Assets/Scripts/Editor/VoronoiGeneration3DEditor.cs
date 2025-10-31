using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiGeneration3D))]
public class VoronoiGeneration3DEditor : Editor
{
    private VoronoiGeneration3D _diagram;

    private void OnEnable()
    {
        _diagram = (VoronoiGeneration3D)target;
    }

    private void OnSceneGUI()
    {
        CheckForInput();
        ShowAndMovePoints();
    }

    private void CheckForInput()
    {
        Event eCurrent = Event.current;
        if (eCurrent.type == EventType.KeyDown && eCurrent.keyCode == KeyCode.Space)
        {
            AddPoint();
            eCurrent.Use();
        }
    }

    private void AddPoint()
    {
        Transform handleTransform = _diagram.transform;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Undo.RecordObject(_diagram, "Add voronoi seed");
            _diagram.voronoiSeeds.Add(hit.point);
            EditorUtility.SetDirty(_diagram);
        }
    }
    
    private void ShowAndMovePoints()
    {
        for (int i = 0; i < _diagram.voronoiSeeds.Count; i++)
        {
            Vector3 currentSeed = _diagram.voronoiSeeds[i];
            
            EditorGUI.BeginChangeCheck();
            currentSeed = Handles.PositionHandle(currentSeed, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_diagram, "Updated location");
                _diagram.voronoiSeeds[i] = currentSeed;
                EditorUtility.SetDirty(_diagram);
            }   
        }
    }
}
