using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(VoronoiCell))]
public class VoronoiCellEditor : Editor
{
    private VoronoiCell _cell;
    private VoronoiCellSettings _voronoiCellSettings;

    private VoronoiGeneration3D _diagram;

    private Vector3 _lastPosition;

    private void OnEnable()
    {
        _cell = (VoronoiCell)target;
        if (_cell)
        {
            _diagram = _cell.GetComponentInParent<VoronoiGeneration3D>();
            _voronoiCellSettings = _cell.CellSettings;
        }

        _lastPosition = _cell ? _cell.transform.position : Vector3.zero;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        bool settingsChanged = false;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Settings", GUILayout.ExpandWidth(true), GUILayout.MinWidth(80));

            SerializedProperty scriptableObjectSettings = serializedObject.FindProperty("CellSettings");
            EditorGUILayout.PropertyField(scriptableObjectSettings, GUIContent.none,
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(200));

            if (GUILayout.Button("Change from Folder", GUILayout.ExpandWidth(true), GUILayout.MinWidth(150)))
            {
                Object currentSettingsObj = AssetDatabase.LoadAssetAtPath<Object>(
                    Config.CombinePaths(Config.ScriptableObjects_Path,
                        Config.ScriptableObjects_VoronoiCellSettingsDefault));
                EditorGUIUtility.PingObject(currentSettingsObj);
            }
        }
        EditorGUILayout.EndHorizontal();

        settingsChanged = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        if (settingsChanged)
            _cell.UpdateCellSettings();
    }

    private void OnSceneGUI()
    {
        DrawAndMoveCell();
    }

    // TODO: Make it calculate only when mouse is released after moving for efficiency (if needed)
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