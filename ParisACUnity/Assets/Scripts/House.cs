using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class House : Shape
{
    private Mesh _cubeMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [HideInInspector] public HouseSettings Settings;
    [HideInInspector] public HouseSettingsData SettingsDataCopy;

    private bool isInit = false;

    [HideInInspector] public bool hasFirstBaseGenerated = false;
    [HideInInspector] public bool hasFirstRoofGenerated = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lineSettings">Line settings to inherit from</param>
    public void Init(HouseSettings houseSettings, HouseSettingsData houseSettingsDataCopy)
    {
        _cubeMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.sharedMesh = _cubeMesh;
        
        Settings = houseSettings;
        SettingsDataCopy = new HouseSettingsData(houseSettingsDataCopy);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = Settings.SettingsData.HouseMaterial;
    }

    public void ReInit(HouseSettings houseSettings, HouseSettingsData houseSettingsDataCopy)
    {
        Init(houseSettings, houseSettingsDataCopy);
    }

    public void Init(int houseWidth = -1)
    {
        SettingsDataCopy.Height = RandomInt(SettingsDataCopy.MinHeight, SettingsDataCopy.MaxHeight + 1);
        if (houseWidth == -1)
            SettingsDataCopy.Width = RandomInt(SettingsDataCopy.MinWidth, SettingsDataCopy.MaxWidth + 1);
        else
            SettingsDataCopy.Width = houseWidth;
        SettingsDataCopy.Depth = RandomInt(SettingsDataCopy.MinDepth, SettingsDataCopy.MaxDepth + 1);

        isInit = true;
    }

    protected override void Execute()
    {
        hasFirstBaseGenerated = false;
        hasFirstRoofGenerated = false;
        
        if (!isInit)
            Init();
        
        Base basee = CreateSymbol<Base>("Base", 
            default(Vector3), default(Quaternion), default(Vector3), transform,
            this.gameObject);
        basee.Init(SettingsDataCopy.Height - 1);
        basee.Generate();
    }

    #region EditorCode

    public void UpdateSettings(string settingsName)
    {
        if (settingsName == nameof(Settings))
            UpdateHouseSettings();
    }

    private void UpdateHouseSettings()
    {
        SettingsDataCopy = new HouseSettingsData(Settings.SettingsData);

#if UNITY_EDITOR
        Undo.RecordObject(_meshRenderer, "Updated Line Settings");
        _meshRenderer.material = SettingsDataCopy.HouseMaterial;
        EditorUtility.SetDirty(_meshRenderer);
#else
        Debug.LogError("Updating of Editor Tooling settings should not be happening in Play Mode!");
#endif
    }

    #endregion
}