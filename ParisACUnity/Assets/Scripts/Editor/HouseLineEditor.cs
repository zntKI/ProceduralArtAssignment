using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(HouseLine))]
public class HouseLineEditor : ShapeEditor
{
    private HouseLine _houseLine;
    
    private void OnEnable()
    {
        //Tools.hidden = true;
        if (_houseLine)
            return;

        _houseLine = (HouseLine)target;
    }

    private void OnDisable()
    {
        //Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();

        DrawSerializedObjectField("House Line Scriptable Object Template", nameof(_houseLine.LineSettings),
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseLineSettingsFolder,
                Config.ScriptableObjects_HouseLineSettingsDefault));
        DrawSettingsFields("Current Object Properties:", nameof(_houseLine.LineSettingsDataCopy),
            typeof(HouseLineSettingsData));
        
        DrawSerializedObjectField("House Scriptable Object Template", nameof(_houseLine.HouseSettings),
            Config.CombinePaths(Config.ScriptableObjects_Path, Config.ScriptableObjects_HouseSettingsFolder,
                Config.ScriptableObjects_HouseSettingsDefault));
        DrawSettingsFields("House Properties:", nameof(_houseLine.HouseSettingsDataCopy),
            typeof(HouseSettingsData));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSerializedObjectField(string headerText, string scriptableObjectName, string defaultObjectPath)
    {
        DrawHeader(headerText);
        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Settings", GUILayout.ExpandWidth(true), GUILayout.MinWidth(80));

            EditorGUI.BeginChangeCheck();
            {
                SerializedProperty scriptableObjectSettings = serializedObject.FindProperty(scriptableObjectName);
                EditorGUILayout.PropertyField(scriptableObjectSettings, GUIContent.none,
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(200));   
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                _houseLine.UpdateSettings(scriptableObjectName);
            }

            if (GUILayout.Button("Change from Folder", GUILayout.ExpandWidth(true), GUILayout.MinWidth(150)))
            {
                Object currentSettingsObj = AssetDatabase.LoadAssetAtPath<Object>(defaultObjectPath);
                EditorGUIUtility.PingObject(currentSettingsObj);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSettingsFields(string headerText, string settingsName, Type settingsType)
    {
        DrawHeader(headerText);
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("If you edit the properties below,\nclick on Save to create a new Scriptable Object with the new properties", MessageType.Info);
        EditorGUILayout.HelpBox("Switching the Scriptable Object Template above would erase all current changes to these properties!", MessageType.Warning);
        
        GUILayout.Space(15);
        
        // Get the field of the SettingsData within the HouseBlock class
        SerializedProperty settingsDataProp = serializedObject.FindProperty(settingsName);
        
        // Get all the fields within the SettingsData class in general through Reflection
        FieldInfo[] fields = settingsType.GetFields();
        foreach (var field in fields)
        {
            // FindPropertyRelative goes one level deeper into the SettingsData field
            // to retrieve its properties
            SerializedProperty property = settingsDataProp.FindPropertyRelative(field.Name);
            if (property != null)
            {
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.PropertyField(property);
                
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    _houseLine.UpdateSettingsCopyByField(settingsType, field);
                }
            }
        }
        
        GUILayout.Space(15);
        
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Apply to Original")) _houseLine.ApplySettingsCopyToOriginal(settingsType);
            if (GUILayout.Button("Save as New")) _houseLine.SaveSettingsAsNewSO(settingsType);
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawHeader(string title)
    {
        var style = new GUIStyle(EditorStyles.boldLabel);
        
        GUILayout.Space(20);
        EditorGUILayout.LabelField(title, style);
    }
}
