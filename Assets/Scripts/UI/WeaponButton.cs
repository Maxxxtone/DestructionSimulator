using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    private WeaponItem _weapon;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _buyBanner;
    [SerializeField] private Image _selectIcon;
    [SerializeField] private Image _weaponImage;
    private WeaponsPanel _weaponPanel;
    public WeaponItem Weapon => _weapon;
    public void InitButton(WeaponItem weapon, WeaponsPanel panel)
    {
        _weapon = weapon;
        _weaponImage.sprite = weapon.Icon;
        _weaponPanel = panel;
        if (weapon.Opened && weapon.Selected)
        {
            _buyBanner.gameObject.SetActive(false);
            _selectIcon.gameObject.SetActive(true);
        }
        else if (weapon.Opened)
        {
            _buyBanner.gameObject.SetActive(false);
            _selectIcon.gameObject.SetActive(false);
        }
        else
        {
            _costText.text = weapon.Cost.ToString();
            _selectIcon.gameObject.SetActive(false);
        }
    }
    public void ChangeSelectedState()
    {
        if (_weapon.Selected)
        {
            _selectIcon.gameObject.SetActive(false);
            _weapon.Selected = false;
        }
    }
    public void SelectWeapon()
    {
        if (_weapon.Selected) return;
        if (_weapon.Opened)
        {
            _weaponPanel.ChangeWeapon(_weapon);
            _weapon.Selected = true;
            _selectIcon.gameObject.SetActive(true);
        }
        else
        {
            if(_weaponPanel.ItemsDatabase.Money >= _weapon.Cost)
            {
                _buyBanner.gameObject.SetActive(false);
                _selectIcon.gameObject.SetActive(false);
                _weapon.Opened = true;
                _weaponPanel.ItemsDatabase.Money -= _weapon.Cost;
                _weaponPanel.ItemsDatabase.UpdateLeftMoney?.Invoke(_weaponPanel.ItemsDatabase.Money);
            }
        }
        _weaponPanel.SaveWeaponsData();
    }
}
