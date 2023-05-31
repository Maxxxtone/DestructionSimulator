using UnityEngine;

[CreateAssetMenu(fileName = "New Items Database", menuName = "ShopData/New Database")]
public class ItemsDatabase : ScriptableObject
{
    public int Money;
    public float Sensetivity;
    public bool SoundOn = true;
    public System.Action<int> UpdateLeftMoney;
    [SerializeField] private WeaponItem[] _weaponsItems;
    [SerializeField] private LevelItem[] _levelsItems;
    public WeaponItem[] WeaponsItems => _weaponsItems;
    public LevelItem[] LevelsItems => _levelsItems;
    public void SetWeaponsKeys()
    {
        for (int i = 0; i < _weaponsItems.Length; i++)
        {
            _weaponsItems[i].Key = "weapon" + i;
        }
    }
    public void SetLevelsKeys()
    {
        for (int i = 0; i < _levelsItems.Length; i++)
        {
            _levelsItems[i].Key = "level" + i;
        }
    }
}
