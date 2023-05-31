using UnityEngine;

public class Recoil : MonoBehaviour
{
    //[SerializeField] private CinemachineImpulseSource _impulse;
    [SerializeField] private float _recoilX, _recoilY, _recoilZ;
    [SerializeField] private float _snappiness, _returnSpeed;
    private Vector3 _targetRotation, _currentRotation;
    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(_currentRotation);
    }
    public void RecoilFire()
    {
        //_impulse.GenerateImpulse();
        _targetRotation += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), Random.Range(-_recoilZ, _recoilZ));
    }
    public void UpdateRecoilPreset(Weapon weapon)
    {
        _recoilX = weapon.RecoilX;
        _recoilY = weapon.RecoilY;
        _recoilZ = weapon.RecoilZ;
    }
}
