using System.Text;
using UnityEngine;

/// <summary>
/// Paths should end WITHOUT '/'
/// </summary>
public static class Config
{
    public const string ScriptableObjects_Path = "Assets/ScriptableObjects";
    
    public const string ScriptableObjects_VoronoiCellSettingsDefault = "VoronoiCellSettings_Default.asset";

    public static string CombinePaths(params string[] paths)
    {
        StringBuilder pathBuilder = new StringBuilder();
        foreach (var path in paths)
        {
            pathBuilder.Append(path);

            if (!path.EndsWith('/') && !path.EndsWith(".asset"))
                pathBuilder.Append('/');
        }

        return pathBuilder.ToString();
    }
}
