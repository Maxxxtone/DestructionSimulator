using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelDestruction;

//[RequireComponent(typeof(WeaponsInventory))]
public class Gun : MonoBehaviour
{
    public System.Action StartReloading;
    public System.Action<Weapon> Attacking;
    [SerializeField] private Camera _fpsCamera;
    [SerializeField] private WeaponChooser _weaponChooser;
    //[SerializeField] private CameraShake _cameraShake;
    [SerializeField] private Weapon _currentWeapon;
    [SerializeField] private PlayerArmsAnimator _armsAnimator;
    [SerializeField] private FirstPersonMovement _movement;
    [SerializeField] private Recoil _recoil;
    [SerializeField] private Sway _sway;
    [SerializeField] private MeleeAttackPoint _meleeAttackPoint;
    //[SerializeField] private Crosshair _crosshair;
    [SerializeField] private LayerMask _crosshairEnemyLayer;
    private int _bulletsShot;
    private bool _shooting, _readyToShoot, _reloading, _aiming;
    //
    [SerializeField] private EffectsPool _muzzleFlashesPool, _bulletTrailsPool;//, _bulletHolesPool;
    [SerializeField] private LayerMask _whatIsEnemy;
    private RaycastHit rayHit;
    //private WeaponsInventory _inventory;
    private float _spread;
    private bool _isMobile;
    //public bool Aiming { get => _aiming; }
    private void Start()
    {
        _isMobile = Application.isMobilePlatform;
        //_inventory = GetComponent<WeaponsInventory>();
        ChangeGunPreset(_weaponChooser.GetSelectedWeapon());
        _currentWeapon.BulletsLeft = _currentWeapon.MagazineSize;
        //_armsAnimator.ChangeWeapon(_currentWeapon);
        _readyToShoot = true;
        //update weapons stats
        _spread = _currentWeapon.Spread;
        //_inventory.Equip += _armsAnimator.PlayEquipAnimation;
        //_inventory.Equip += StartChangeWeapon;
        //_inventory.WeaponSelected += _armsAnimator.ChangeWeapon;
        //_inventory.WeaponSelected += ChangeGunPreset;
        //_cameraShake.Init(_currentWeapon);
        //Attacking += _cameraShake.ShakeCamera;
    }
    private void Update()
    {
        CrosshairCheck();
        if(!_isMobile)
            ShootingInput();
    }
    private void CrosshairCheck()
    {
        //RaycastHit hit;
        //if (Physics.Raycast(_fpsCamera.transform.position, _fpsCamera.transform.forward, out hit, _currentWeapon.Range, _crosshairEnemyLayer))
        //    _crosshair.TargetOnEnemy(true);
        //else
        //    _crosshair.TargetOnEnemy(false);
    }
    public void MobileShooting()
    {
        _shooting = true;
        if (!_movement.IsRunning && _readyToShoot && _shooting && !_reloading && _currentWeapon.BulletsLeft > 0)
        {
            if (_currentWeapon.IsMelee)
                MeleeAttack();
            else
            {
                _bulletsShot = _currentWeapon.BulletsPerTap;
                Shoot();
            }
            Attacking?.Invoke(_currentWeapon);
        }
    }
    public void StopMobileShooting()
    {
        _shooting = false;
    }
    public void ShootingInput()
    {
        if (!_currentWeapon.IsMelee && _readyToShoot && !_shooting && !_reloading && Input.GetMouseButtonDown(1))
            Aim();

        if (_currentWeapon.AllowButtonHold) _shooting = Input.GetKey(KeyCode.Mouse0);
        else _shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if(!_shooting && !_reloading)
            SelectWeaponInput();

        if (Input.GetKeyDown(KeyCode.R) && _currentWeapon.BulletsLeft < _currentWeapon.MagazineSize 
            && !_reloading && !_currentWeapon.IsMelee && !_movement.IsRunning
            && _currentWeapon.TotalBullets > 0) Reload();

        if (!_movement.IsRunning && _readyToShoot && _shooting && !_reloading && _currentWeapon.BulletsLeft > 0 && Time.timeScale > 0)
        {
            if (_currentWeapon.IsMelee)
                MeleeAttack();
            else if (_currentWeapon.GunType == GunType.Throwable)
                ThrowWeapon();
            else
            {
                _bulletsShot = _currentWeapon.BulletsPerTap;
                Shoot();
            }
            Attacking?.Invoke(_currentWeapon);
        }
    }
    private void ThrowWeapon()
    {
        _currentWeapon.GetComponent<Rigidbody>().AddForce(_fpsCamera.transform.forward * _currentWeapon.Range, ForceMode.VelocityChange);
    }
    private void SelectWeaponInput()
    {
        //Переключение оружия с помощью мыши
        //if(Input.GetAxis("Mouse ScrollWheel") > 0f)
        //    _inventory.SelectWeapon(true);
        //else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        //    _inventory.SelectWeapon(false);
        ////Переключение оружия с помощью клавиш
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //    _inventory.SelectWeapon(0);
        //else if(Input.GetKeyDown(KeyCode.Alpha2))
        //    _inventory.SelectWeapon(1);
        //else if(Input.GetKeyDown(KeyCode.Alpha3))
        //    _inventory.SelectWeapon(2);
    }
    private void ChangeGunPreset(Weapon weapon)
    {
        //TODO: Сделать задержку перед сменой оружия
        _currentWeapon = weapon;
        _spread = _currentWeapon.Spread;
        _recoil.UpdateRecoilPreset(weapon);
        _readyToShoot = true;
    }
    //private void StartChangeWeapon() => _readyToShoot = false;
    private void Aim()
    {
        _aiming = !_aiming;
        _spread = _aiming ? _currentWeapon.AimSpread : _currentWeapon.Spread;
        _sway.ChangeAimState(_aiming);
        //_armsAnimator.PlayAimAnimation(_aiming);
    }
    private void Shoot()
    {
        _readyToShoot = false;
        //_armsAnimator.SetShotState();
        float xSpread = Random.Range(-_spread, _spread);
        float ySpread = Random.Range(-_spread, _spread);
        Vector3 direction = _fpsCamera.transform.forward + new Vector3(xSpread, ySpread, 0);
        bool targetIsEnemy = false;
        bool createBulletHole = false;
        if (Physics.Raycast(_fpsCamera.transform.position, direction, out rayHit, _currentWeapon.Range, _whatIsEnemy))
        {
           if(rayHit.transform.root.TryGetComponent(out VoxelObject voxelCollider))
            {
                voxelCollider.AddDestruction(_currentWeapon.Damage, rayHit.point, rayHit.normal);
            }
            //var trail = _bulletTrailsPool.GetEffect(_currentWeapon.AttackPoint.position, Quaternion.identity);
            //StartCoroutine(SpawnTrail(trail, rayHit.point, rayHit, createBulletHole, targetIsEnemy));
        }
        _recoil.RecoilFire();
        //_currentWeapon.BulletsLeft--;
        _bulletsShot--;
        var flash = _muzzleFlashesPool.GetEffect(_currentWeapon.AttackPoint.position, Quaternion.identity);
        flash.transform.parent = _currentWeapon.AttackPoint;
        Invoke("ResetShot", _currentWeapon.TimeBetweenShooting);
        if (_bulletsShot > 0 && _currentWeapon.BulletsLeft > 0)
            Invoke("Shoot", _currentWeapon.TimeBetweenShots);
        UpdateInventoryInfo();
    }
    private void UpdateInventoryInfo()
    {
        //_inventory.UpdateAmmoText(_currentWeapon.BulletsLeft / _currentWeapon.BulletsPerTap,
        //       _currentWeapon.TotalBullets/_currentWeapon.BulletsPerTap);
    }
    private void MeleeAttack()
    {
        if (_readyToShoot)
        {
            _armsAnimator.PlayMeleeAttackAnimation();
            _readyToShoot = false;
            StartCoroutine(MeleeAttackDelay());
        }
    }
    private IEnumerator MeleeAttackDelay()
    {
        yield return new WaitForSeconds(_currentWeapon.TimeBetweenShooting / 2);
        _meleeAttackPoint.CreateAttackSphere(_fpsCamera, _currentWeapon.Damage);
        yield return new WaitForSeconds(_currentWeapon.TimeBetweenShooting/2);
        _readyToShoot = true;
    }
    private void ResetShot()
    {
        _readyToShoot = true;
    }
    private void Reload()
    {
        //StartReloading?.Invoke();
        //_reloading = true;
        //_armsAnimator.PlayReloadingAnimation();
        //Invoke(nameof(ReloadFinished), _currentWeapon.ReloadTime);
    }
    private void ReloadFinished()
    {
        //_currentWeapon.BulletsLeft = _currentWeapon.MagazineSize%_currentWeapon.TotalBullets;
        //_currentWeapon.TotalBullets -= _currentWeapon.BulletsLeft;_
        _currentWeapon.Reload();
        _reloading = false;
        UpdateInventoryInfo();
    }
    private IEnumerator SpawnTrail(GameObject trailObject, Vector3 point, RaycastHit hit, bool createBulletHole, bool isEnemy)
    {
        float time = 0;
        Vector3 startPosition = _currentWeapon.AttackPoint.position;
        var trail = trailObject.GetComponent<TrailRenderer>();
        while(time < .1)
        {
            startPosition = Vector3.Lerp(startPosition, point, time);
            trail.transform.position = startPosition;
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        trail.transform.position = point;
        //if (createBulletHole && !isEnemy)
        //    _bulletHolesPool.GetEffect(hit.point, Quaternion.LookRotation(hit.normal));
        StartCoroutine(DisableTrail(trail));
    }
    private IEnumerator DisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(trail.time);
        trail.gameObject.SetActive(false);
    }
}
