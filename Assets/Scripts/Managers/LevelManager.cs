using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : Manager
{
    private LevelGoal goal;
    public LevelGoal Goal
    {
        get => goal;
        set
        {
            goal = value;
        }
    }
    public int currentLevelId
    {
        get => goal.levelId;
        set
        {
            
            //LevelGoal lg = levelInfoList.FirstOrDefault(info => info.levelId == value);
        }
    }

    public List<LevelInfo> levelInfoList;
}
