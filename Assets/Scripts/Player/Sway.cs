using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{
    [SerializeField] private float _defaultSwayClamp = .09f, _aimSwayClamp = .01f;
    [SerializeField] private float _defaultSmoothing = 3f, _aimSmoothing = 1f;
    private float _swayClamp, _smoothing;
    private Vector3 _origin;

    private void Start()
    {
        _origin = transform.localPosition;
        ChangeAimState(false);
    }
    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        input.x = Mathf.Clamp(input.x, -_swayClamp, _swayClamp);
        input.y = Mathf.Clamp(input.y, -_swayClamp, _swayClamp);

        Vector3 target = new Vector3(-input.x, -input.y, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, target + _origin, Time.deltaTime * _smoothing);
    }
    public void ChangeAimState(bool aim)
    {
        if (aim)
        {
            _swayClamp = _aimSwayClamp;
            _smoothing = _aimSmoothing;
        }
        else
        {
            _swayClamp = _defaultSwayClamp;
            _smoothing = _defaultSmoothing;
        }
    }
}
