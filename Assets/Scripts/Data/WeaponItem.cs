using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName ="New Weapon Item", menuName = "ShopData/New Weapon Item")]
public class WeaponItem : ScriptableObject
{
    public bool Selected, Opened;
    public string Key;
    [SerializeField] private int _cost = 100;
    [SerializeField] private Sprite _sprite;
    public int Cost => _cost;
    public Sprite Icon => _sprite;
}
