using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiCell))]
public class VoronoiCellEditor : Editor
{
    private VoronoiCell _cell;

    private VoronoiGeneration3D _diagram;

    private Vector3 _lastPosition;

    private void OnEnable()
    {
        _cell = (VoronoiCell)target;
        if (_cell)
            _diagram = _cell.GetComponentInParent<VoronoiGeneration3D>();

        _lastPosition = _cell ? _cell.transform.position : Vector3.zero;
    }

    private void OnSceneGUI()
    {
        //DrawAndMoveCell();
    }

    // TODO: Make it calculate only when mouse is released after moving for efficiency
    private void DrawAndMoveCell()
    {
        Vector3 currentPosition = _cell.transform.position;

        if ((currentPosition - _lastPosition).sqrMagnitude > Mathf.Epsilon)
        {
            if (!_diagram)
                _diagram = _cell.GetComponentInParent<VoronoiGeneration3D>();
            
            _diagram.CalculateVoronoiDiagram();

            _lastPosition = currentPosition;
        }
    }
}