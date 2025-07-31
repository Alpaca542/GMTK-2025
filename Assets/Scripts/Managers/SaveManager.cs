using UnityEngine;

// SO the save manager has a SaveObject called "save"
// Edit the variable at any time and it will automatically be saved
// Or you can call Save() and Load() manually
// Add a save name like "save1", etc if you ever do multiple saves.
// If you want to save something automatically without calling externally
// then edit the UpdateProps() function

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveObject save; // The current save
    private SaveObject storedSave; // A copy of whats stored inside the playerprefs
    public bool autoSave = true;

    private void Awake()
    {
        instance = this;
        Load(); // Load on start coz why not chat
        InvokeRepeating("SaveIfChanged", 1, 1); // Save the game every one second
    }

    private void SaveIfChanged()
    {
        if (autoSave)
        {
            UpdateProps();
            if (!save.Equals(storedSave))
            {
                Save();
            }
        }
    }
    // Update Any properties of the save without calling externally
    private void UpdateProps()
    {
        // For example
        // save.level = Global.currentLevel;
    }
    public void Load() { Load("primarySave"); } // Default save
    public void Load(string saveName)
    {
        string json = PlayerPrefs.GetString(saveName, "");
        if (json == "")
        {
            return; // Liek, yeah
        }
        SaveObject _save = JsonUtility.FromJson<SaveObject>(json);
        save = _save;
    }

    public void Save(string saveName)
    {
        UpdateProps();
        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(saveName, json);
        PlayerPrefs.Save();
        storedSave = save;
    }
    public void Save() { Save("primarySave"); } // Default save

    // Save before quitting
    static bool WantsToQuit()
    {
        SaveManager.instance.Save();
        return true; // Returning false would cancel the exit and the game would be stuck
                     // Please dont do that
    }
}
