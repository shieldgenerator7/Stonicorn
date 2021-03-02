using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileManager : MonoBehaviour
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
        List<SettingObject> settings = new List<SettingObject>();
        foreach (ISetting setting in FindObjectsOfType<MonoBehaviour>().OfType<ISetting>())
        {
            if (setting.Scope == SettingScope.SAVE_FILE)
            {
                settings.Add(setting.Setting);
            }
        }
        ES3.Save<List<SettingObject>>("settings", settings, filename);
        //Delegate
        onFileSave?.Invoke(filename);
    }
    public OnFileAccess onFileSave;
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
            foreach (ISetting setting in FindObjectsOfType<MonoBehaviour>().OfType<ISetting>())
            {
                if (setting.Scope == SettingScope.SAVE_FILE)
                {
                    string id = setting.ID;
                    foreach (SettingObject setObj in settings)
                    {
                        if (id == setObj.id)
                        {
                            setting.Setting = setObj;
                            break;
                        }
                    }
                }
            }
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
    public OnFileAccess onFileLoad;
    #endregion
}
