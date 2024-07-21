using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileManager : Manager
{
    public string fileName = "merky";
    public string fileExtension = ".txt";
    public bool saveWithTimeStamp = false;//true to save with date/timestamp in filename, even when not in demo build

    private string getFileName(bool useTimeStamp = false)
    {
        string filename = this.fileName;
        //If saving with time stamp,
        if (useTimeStamp)
        {
            //Add the time stamp to the filename
            System.DateTime now = System.DateTime.Now;
            filename += "-" + now.Ticks;
        }
        //Add an extension to the filename
        filename += this.fileExtension;
        return filename;
    }

    #region File Management
    public delegate void OnFileAccess(string filename);

    /// <summary>
    /// Saves the memories, game states, and scene cache to a save file
    /// </summary>
    public void saveToFile()
    {
        string filename = getFileName(saveWithTimeStamp);
        //Save file settings
        List<SettingObject> settings = 
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISetting>()
            .Where(setting => setting.Scope == SettingScope.SAVE_FILE)
            .ToList()
            .ConvertAll(setting => setting.Setting)
            .Where(so => so)
            .ToList();
        ES3.Save<List<SettingObject>>("settings", settings, filename);

        //Save Game Data
        ES3.Save<GameData>("data", data, filename);

        //Delegate
        onFileSave?.Invoke(filename);
    }
    public event OnFileAccess onFileSave;
    /// <summary>
    /// Loads the game from the save file
    /// It assumes the file already exists
    /// </summary>
    public void loadFromFile()
    {
        string filename = getFileName(false);
        try
        {
            //Load file settings
            List<SettingObject> settings = ES3.Load<List<SettingObject>>("settings", filename);
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ISetting>()
                .Where(setting => setting.Scope == SettingScope.SAVE_FILE)
                .ToList()
                .ForEach(setting =>
                {
                    string id = setting.ID;
                    SettingObject setObj = settings.Find(setObj => setObj.id == id);
                    if (setObj)
                    {
                            setting.Setting = setObj;
                    }
                });

            //Load Game Data
            GameData data = ES3.Load<GameData>("data", filename);
            this.data.memories = data.memories;
            this.data.knownObjects = data.knownObjects;
            this.data.gameStates = data.gameStates;

            //Delegate
            onFileLoad?.Invoke(filename);
        }
        catch (System.Exception e)
        {
            if (ES3.FileExists(filename))
            {
                ES3.DeleteFile(filename);
            }
            Debug.LogError("Error loading file: " + filename + "; error: " + e);
            Managers.Game.resetGame(false);
        }
    }
    public event OnFileAccess onFileLoad;
    #endregion
}
