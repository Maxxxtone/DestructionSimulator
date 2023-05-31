using UnityEngine;
using System.Linq;

public class WeaponChooser : MonoBehaviour
{
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private Weapon[] _weapons;
    public Weapon GetSelectedWeapon()
    {
        var selectedWeaponItem = _itemsDatabase.WeaponsItems.Where(w => w.Selected).FirstOrDefault();
        var selectedWeapon = _weapons.Where(w=>w.WeaponItem.Key == selectedWeaponItem.Key).FirstOrDefault();
        selectedWeapon.gameObject.SetActive(true);
        return selectedWeapon;
    }
}
