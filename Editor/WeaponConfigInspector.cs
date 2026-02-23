// WeaponConfigInspector.cs - Đặt trong thư mục Editor

using PullGame;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponConfig))]
public class WeaponConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        
        if (GUILayout.Button("Open in Weapon Editor", GUILayout.Height(40)))
        {
            var window = EditorWindow.GetWindow<WeaponConfigEditorWindow>("Weapon Editor");
            
            // Set the selected config using reflection
            var field = typeof(WeaponConfigEditorWindow).GetField("selectedConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(window, target);
            
            window.Show();
        }
    }
}