using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsPanel : MonoBehaviour
{
    private bool _initialized;
    [SerializeField] private WeaponButton _weaponButton;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private SaveManager _saveManager;
    private List<WeaponButton> _weaponsButtons;   
    public ItemsDatabase ItemsDatabase => _itemsDatabase;
    private void OnEnable()
    {
        if(!_initialized)
        {
            _weaponsButtons= new List<WeaponButton>();
            _itemsDatabase.SetWeaponsKeys();
            foreach (var item in _itemsDatabase.WeaponsItems)
            {
                var button = Instantiate(_weaponButton,transform);
                button.InitButton(item, this);
                _weaponsButtons.Add(button);
            }
            _initialized = true;
        }
    }
    public void ChangeWeapon(WeaponItem weaponItem)
    {
        foreach (var button in _weaponsButtons)
        {
            if(button.Weapon.Key != weaponItem.Key)
                button.ChangeSelectedState();
        }
    }
    public void SaveWeaponsData()
    {
        _saveManager.SaveWeaponsData();
        _saveManager.SaveMoney();
    }
}
