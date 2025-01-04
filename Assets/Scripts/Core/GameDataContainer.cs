using UnityEngine;

[CreateAssetMenu(menuName ="GameDataContainer", fileName ="GameDataContainer")]
public class GameDataContainer : ScriptableObject
{
    [SerializeField]
    private GameData gameData;

    public GameData GameData
    {
        get => (GameData)gameData.Clone();
        set => gameData = value;
    }

#if UNITY_EDITOR
    public GameData _gameData
    {
        get => gameData;
        set => gameData = value;
    }
#endif
}
