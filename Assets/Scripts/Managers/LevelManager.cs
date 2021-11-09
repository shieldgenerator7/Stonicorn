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
            Managers.Scene.getSceneLoader(levelInfo.sceneId).loadLevelIfUnLoaded();
        }
    }
    public int CurrentLevelId
    {
        get => levelInfo.levelId;
        set
        {
            int index = Mathf.Clamp(value, 0, levelInfoList.Count - 1);
            LevelInfo = levelInfoList[index];
            onLevelChanged?.Invoke(levelInfo);
        }
    }
    public delegate void OnLevelChanged(LevelInfo levelInfo);
    public event OnLevelChanged onLevelChanged;

    public List<LevelInfo> levelInfoList;

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
        onLevelFinished?.Invoke();
    }
    public delegate void OnLevelFinished();
    public event OnLevelFinished onLevelFinished;
}
