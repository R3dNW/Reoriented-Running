using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputManager))]
[CanEditMultipleObjects]
public class InputManagerEditor : Editor
{
    public class SerializableStorage : ScriptableObject
    {
        public InputManager.Settings.ButtonKey[] buttonKeys;
        public InputManager.Settings.NamedAxis[] namedAxes;
    }

    SerializableStorage storage;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InputManager targetIM = (InputManager)this.target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Defaults"))
        {
            targetIM.LoadSettings();
        }

        if (GUILayout.Button("Save Defaults"))
        {
            targetIM.SaveSettingsAsDefault();
        }

        EditorGUILayout.EndHorizontal();
    }
}
