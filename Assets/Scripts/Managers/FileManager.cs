using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public string fileName = "merky";
    public string fileExtension = ".txt";
    public bool saveWithTimeStamp = false;//true to save with date/timestamp in filename, even when not in demo build

    private string getFileName(bool useTimeStamp=false)
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
    /// <summary>
    /// Saves the memories, game states, and scene cache to a save file
    /// </summary>
    public void saveToFile()
    {
        string filename = getFileName(saveWithTimeStamp);
        //Save game states and memories
        Managers.Rewind.saveToFile(filename);
        Managers.Object.saveToFile(filename);
        //Save settings
        Managers.Settings.saveSettings();
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
    }
    /// <summary>
    /// Loads the game from the save file
    /// It assumes the file already exists
    /// </summary>
    public void loadFromFile()
    {
        try
        {
            string filename = getFileName(false);
            //Load game states and memories
            Managers.Rewind.loadFromFile(filename);
            Managers.Object.loadFromFile(filename);
            //Load settings
            Managers.Settings.loadSettings();
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
        }
        catch (System.Exception)
        {
            if (ES3.FileExists("merky.txt"))
            {
                ES3.DeleteFile("merky.txt");
            }
            Managers.Game.resetGame(false);
        }
    }
    #endregion
}
