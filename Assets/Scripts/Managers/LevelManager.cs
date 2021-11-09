using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : Manager
{
    public LevelInfo LevelInfo => levelInfoList[levelId];

    private int levelId;
    public int CurrentLevelId
    {
        get => levelId;
        set
        {
            levelId = Mathf.Clamp(value, 0, levelInfoList.Count - 1);
            levelFinished = false;
            onLevelChanged?.Invoke(LevelInfo);
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
