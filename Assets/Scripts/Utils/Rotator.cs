using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed;
    private void Update()
    {
        transform.Rotate(Vector3.up * _rotateSpeed);
    }
}
