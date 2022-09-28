using System;

[Serializable]
public class Quote : DialogueComponent
{

    public string characterName = "";
    public string text = "";
    /// <summary>
    /// The image filename without the folder path and without the file extension
    /// </summary>
    public string imageName
    {
        get
        {
            //set image name
            string[] split = imageFileName.Split(new char[] { '\\', '/' });
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
    public string imageFileName = "";

    /// <summary>
    /// The voice line filename without the folder path and without the file extension
    /// </summary>
    public string voiceLineName
    {
        get
        {
            //set image name
            string[] split = voiceLineFileName.Split(new char[] { '\\', '/' });
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
    public string voiceLineFileName = "";

    public int Index => path.quotes.IndexOf(this);

    public Quote(string charName = "", string txt = "", string imageFileName = "")
    {
        this.characterName = charName;
        this.text = txt;
        this.imageFileName = imageFileName;
    }
}
