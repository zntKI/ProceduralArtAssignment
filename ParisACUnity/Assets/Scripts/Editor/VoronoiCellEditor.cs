using System;
using System.Reflection;
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

        DrawSerializedObjectField();
        DrawSettingsFields();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSerializedObjectField()
    {
        DrawHeader("Scriptable Object Template");
        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Settings", GUILayout.ExpandWidth(true), GUILayout.MinWidth(80));

            EditorGUI.BeginChangeCheck();
            {
                SerializedProperty scriptableObjectSettings = serializedObject.FindProperty(_cell.VoronoiCellSettingsName);
                EditorGUILayout.PropertyField(scriptableObjectSettings, GUIContent.none,
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(200));   
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                _cell.UpdateCellSettings();
            }

            if (GUILayout.Button("Change from Folder", GUILayout.ExpandWidth(true), GUILayout.MinWidth(150)))
            {
                Object currentSettingsObj = AssetDatabase.LoadAssetAtPath<Object>(
                    Config.CombinePaths(Config.ScriptableObjects_Path,
                        Config.ScriptableObjects_VoronoiCellSettingsDefault));
                EditorGUIUtility.PingObject(currentSettingsObj);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSettingsFields()
    {
        DrawHeader("Current Object Properties:");
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("If you edit the properties below,\nclick on Save to create a new Scriptable Object with the new properties", MessageType.Info);
        EditorGUILayout.HelpBox("Switching the Scriptable Object Template above would erase all current changes to these properties!", MessageType.Warning);
        
        GUILayout.Space(15);
        
        // Get the field of VoronoiCellSettingsData within the VoronoiCell class
        SerializedProperty settingsDataProp = serializedObject.FindProperty(_cell.VoronoiCellSettingsDataCopyName);
        
        // Get all the fields within VoronoiCellSettingsData class in general through Reflection
        FieldInfo[] fields = typeof(VoronoiCellSettingsData).GetFields();
        foreach (var field in fields)
        {
            // FindPropertyRelative goes one level deeper into the VoronoiCellSettingsData field
            // to retrieve its properties
            SerializedProperty property = settingsDataProp.FindPropertyRelative(field.Name);
            if (property != null)
            {
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.PropertyField(property);
                
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    _cell.UpdateCellSettingsCopyByField(field);
                }
            }
        }
        
        GUILayout.Space(15);
        
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Apply to Original")) _cell.ApplyCellSettingsCopyToOriginal();
            if (GUILayout.Button("Save as New")) _cell.SaveCellSettingsAsNewSO();
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawHeader(string title)
    {
        var style = new GUIStyle(EditorStyles.boldLabel);
        
        GUILayout.Space(20);
        EditorGUILayout.LabelField(title, style);
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