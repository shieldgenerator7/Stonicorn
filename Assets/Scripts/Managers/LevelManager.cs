using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : Manager
{
    private LevelInfo levelInfo;
    public LevelInfo LevelInfo
    {
        get => levelInfo;
        private set
        {
            levelInfo = value;
        }
    }
    public int CurrentLevelId
    {
        get => levelInfo.levelId;
        set
        {
            int index = Mathf.Clamp(value, 0, levelInfoList.Count - 1);
            LevelInfo = levelInfoList[index];
            levelFinished = false;
            onLevelChanged?.Invoke(levelInfo);
        }
    }
    public delegate void OnLevelChanged(LevelInfo levelInfo);
    public event OnLevelChanged onLevelChanged;

    public List<LevelInfo> levelInfoList;

    private bool levelFinished = false;

    public void registerLevelGoalDelegates()
    {
        FindObjectsOfType<LevelGoal>().ToList().ForEach(lg =>
        {
            lg.onGoalReached -= levelGoalReached;
            lg.onGoalReached += levelGoalReached;
        });
    }

    private void levelGoalReached()
    {
        levelFinished = true;
        onLevelFinished?.Invoke();
    }
    public delegate void OnLevelFinished();
    public event OnLevelFinished onLevelFinished;

    public void checkLevelIncrement()
    {
        if (levelFinished)
        {
            CurrentLevelId++;
        }
    }
}
