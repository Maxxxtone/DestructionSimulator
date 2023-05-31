using UnityEngine;

[CreateAssetMenu(fileName = "New Level Item", menuName = "ShopData/New Level Item")]
public class LevelItem : ScriptableObject
{
    public bool Opened;
    public string Key;
    [SerializeField] private int _sceneIdForOpen = 1;
    [SerializeField] private int _cost = 100;
    public int Cost => _cost;
    public int SceneIdForOpen => _sceneIdForOpen;
}
