using UnityEngine;

public class MilestoneActivatorURL : MilestoneActivator
{//2017-03-14: copied from MilestoneActivatorAirport

    public string url;//the url to open when activated

    public override void activateEffect()
    {
        Application.OpenURL(url);
    }
    protected override void previouslyDiscovered()
    {
        //don't do anything
        //this way it won't activate the link when opening the game
        //and it will activate the link once each session
    }
}
