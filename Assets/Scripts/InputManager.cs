using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.IO;

public class InputManager : MonoBehaviour
{
    [System.Serializable]
    public struct Axis
    {
        public KeyCode positive;
        public KeyCode negative;

        public float GetValue()
        {
            return (Input.GetKeyDown(positive) ? 1 : 0) - (Input.GetKeyDown(negative) ? 1 : 0);
        }
    }

    [System.Serializable]
    public struct Settings
    {
        [System.Serializable]
        public struct ButtonKey
        {
            public string name;
            public KeyCode key;
        }

        [System.Serializable]
        public struct NamedAxis
        {
            public string name;
            public Axis axis;
        }

        public Dictionary<string, KeyCode> buttonMap;
        public Dictionary<string, Axis> axisMap;
    }

    protected static InputManager Instance { get; set; }

    private Settings settings;

    public Settings.ButtonKey[] buttonKeys;
    public Settings.NamedAxis[] namedAxes;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("Can only have one InputManager in the Scene, there are currently at least two.");
            return;
        }

        Instance = this;

        this.LoadSettings();
    }

    public void LoadSettings()
    {
        //this.settings = JsonConvert.DeserializeObject<Settings>(Resources.Load<TextAsset>("inputsettings_default.json").text);

        this.settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "inputsettings_default.json")));

#if UNITY_EDITOR
        List<Settings.ButtonKey> buttonKeysList = new List<Settings.ButtonKey>();

        foreach (string name in this.settings.buttonMap.Keys)
        {
            buttonKeysList.Add(new Settings.ButtonKey() { name = name, key = this.settings.buttonMap[name] });
        }

        List<Settings.NamedAxis> namedAxesList = new List<Settings.NamedAxis>();

        foreach (string key in this.settings.axisMap.Keys)
        {
            namedAxesList.Add(new Settings.NamedAxis() { name = key, axis = this.settings.axisMap[key] });
        }

        this.buttonKeys = buttonKeysList.ToArray();
        this.namedAxes = namedAxesList.ToArray();
#endif
    }

    public void SaveSettingsAsDefault()
    {
        this.settings = new Settings()
        {
            buttonMap = new Dictionary<string, KeyCode>(),
            axisMap = new Dictionary<string, Axis>()
        };

        foreach (Settings.ButtonKey buttonKey in this.buttonKeys)
        {
            this.settings.buttonMap.Add(buttonKey.name, buttonKey.key);
        }

        foreach (Settings.NamedAxis namedAxis in this.namedAxes)
        {
            this.settings.axisMap.Add(namedAxis.name, namedAxis.axis);
        }

        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "inputsettings_default.json"), JsonConvert.SerializeObject(this.settings));
    }

    protected bool GetButtonDownI(string buttonName)
    {
        if (this.settings.buttonMap.ContainsKey(buttonName) == false)
        {
            Debug.LogError("InputManager.GetButtonDown() - No button named \"" + buttonName + "\"");
            return false;
        }

        return Input.GetKeyDown(this.settings.buttonMap[buttonName]);
    }

    protected float GetAxisI(string axisName)
    {
        if (axisName == "Mouse X")
        {
            return Input.GetAxis("Mouse X");
        }

        if (axisName == "Mouse Y")
        {
            return Input.GetAxis("Mouse Y");
        }

        if (this.settings.axisMap.ContainsKey(axisName) == false)
        {
            Debug.LogError("InputManager.GetAxis() - No axis named \"" + axisName + "\"");
            return 0;
        }
        
        return this.settings.axisMap[axisName].GetValue();
    }

    protected string[] GetButtonNamesI()
    {
        return this.settings.buttonMap.Keys.ToArray();
    }

    protected string[] GetAxisNamesI()
    {
        return this.settings.axisMap.Keys.ToArray();
    }

    public static bool GetButtonDown(string buttonName)
    {
        return Instance.GetButtonDownI(buttonName);
    }

    public static float GetAxis(string axisName)
    {
        return Instance.GetAxisI(axisName);
    }

    public static string[] GetButtonNames()
    {
        return Instance.GetButtonNamesI();
    }

    public static string[] GetAxisNames()
    {
        return Instance.GetAxisNamesI();
    }
}