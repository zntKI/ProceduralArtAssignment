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
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            _diagram.AddVoronoiCell(hit.point);
        }
    }
}
