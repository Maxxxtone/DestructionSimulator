using UnityEngine;
using VoxelDestruction;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GunType _gunType;
    [SerializeField] private AmmoType _ammoType;
    [SerializeField] private int _damage, _magazineSize, _bulletsPerTap;
    [SerializeField] private float _timeBetweenShooting, _spread, _aimSpread, _range, _reloadTime, _timeBetweenShots;
    [SerializeField] private bool _allowButtonHold;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _recoilX, _recoilY, _recoilZ;
    [SerializeField] private Sprite _weaponIcon;
    [SerializeField] private AudioClip _attackAudio;
    [SerializeField] private int _totalBullets;
    [SerializeField] private WeaponItem _weaponItem;
    [Header("Shake Settings")]
    [SerializeField] private float _shakeDuration, _shakeMagnitude;
    public WeaponItem WeaponItem => _weaponItem;
    public int BulletsLeft { get; set; }
    public GunType GunType { get => _gunType; }
    public AmmoType AmmoType { get => _ammoType; }
    public int Damage { get=>_damage; }
    public int MagazineSize { get=>_magazineSize; }
    public int BulletsPerTap { get=>_bulletsPerTap; }
    public float TimeBetweenShooting { get => _timeBetweenShooting; }
    public float Spread { get => _spread; }
    public float Range { get => _range; }
    public float ReloadTime { get => _reloadTime; }
    public float TimeBetweenShots { get => _timeBetweenShots; }
    public bool AllowButtonHold { get => _allowButtonHold; }
    public int TotalBullets { get => _totalBullets; set
        {
            _totalBullets = Mathf.Clamp(value, 0, 1000);
        } 
    }
    public Transform AttackPoint { get => _attackPoint; }
    public bool IsMelee { get => _gunType == GunType.Melee; }
    public Sprite WeaponIcon { get => _weaponIcon; }
    //recoil
    public float RecoilX { get => _recoilX; }
    public float RecoilY { get => _recoilY; }
    public float RecoilZ { get => _recoilZ; }
    public float AimSpread { get => _aimSpread; }
    public float ShakeDuration { get => _shakeDuration; }
    public float ShakeMagnitude { get => _shakeMagnitude; }
    public void PlayAttackAudio(AudioSource source)
    {
        source.PlayOneShot(_attackAudio);
    }
    public void Reload()
    {
        if (MagazineSize <= TotalBullets)
        {
            TotalBullets -= MagazineSize - BulletsLeft;
            BulletsLeft = MagazineSize;
        }
        else
        {
            var bulletsToAdd = MagazineSize - BulletsLeft;
            if (TotalBullets <= bulletsToAdd)
            {
                BulletsLeft += TotalBullets;
                TotalBullets = 0;
            }
            else
            {
                TotalBullets -= bulletsToAdd;
                BulletsLeft += bulletsToAdd;
            }
        }
    }
}
public enum GunType { Pistol, Rifle, Melee, Throwable}
public enum AmmoType { NoAmmo, Pistol, Rifle, Shotgun}