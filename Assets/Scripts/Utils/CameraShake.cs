using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float _duration, _magnitude;
    private bool _canShake = true;
    public void ShakeCamera(Weapon weapon)
    {
        if(_canShake)
            StartCoroutine(Shake(weapon.ShakeDuration, weapon.ShakeMagnitude));
    }
    public void ShakeCamera()
    {
        //if (_canShake)
            StartCoroutine(Shake(_duration, _magnitude));
    }
    public void Init(Weapon weapon)
    {
        _duration = weapon.ShakeDuration;
        _magnitude = weapon.ShakeMagnitude;
    }
    private IEnumerator Shake(float duration, float magnitude)
    {
        _canShake = false;
        var originalPos = transform.localPosition;
        float elapsed = 0;
        while (elapsed < duration)
        {
            var x = Random.Range(-1f, 1f) * magnitude;
            var y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canShake= true;
        transform.localPosition = originalPos;
    }
}
