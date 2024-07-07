using System;

[Serializable]
public class Quote : DialogueComponent
{
    public string characterName = "";
    public string text = "";
    /// <summary>
    /// The image filename without the folder path and without the file extension
    /// </summary>
    public string ImageName => getBaseFileName(imageFileName);
    public string imageFileName = "";

    /// <summary>
    /// The voice line filename without the folder path and without the file extension
    /// </summary>
    public string VoiceLineName => getBaseFileName(voiceLineFileName);
    public string voiceLineFileName = "";

    public int Index => path.quotes.IndexOf(this);

    public Quote(string charName = "", string txt = "", string imageFileName = "")
    {
        this.characterName = charName;
        this.text = txt;
        this.imageFileName = imageFileName;
    }

    public static string getBaseFileName(string fileName)
    {
        string[] split = fileName.Split(new char[] { '\\', '/' });
        string name = split[split.Length - 1];
        int lastDotIndex = name.LastIndexOf('.');
        if (lastDotIndex >= 0)
        {
            return name.Substring(0, lastDotIndex);
        }
        else
        {
            return name;
        }
    }
}
