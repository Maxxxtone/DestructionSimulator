using System.Collections.Generic;
using UnityEngine;
using UniversalMobileController;

public class FirstPersonMovement : MonoBehaviour
{
    //[SerializeField] private FixedJoystick _joystick;
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    private Rigidbody _rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();
    [SerializeField] private FloatingJoyStick _joystick;
    private bool isMobile;
    void Awake()
    {
        // Get the rigidbody on this.
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        isMobile = Application.isMobilePlatform;
        //if (_joystick == null)
        //    _joystick = FindObjectOfType<FixedJoystick>();
    }
    void FixedUpdate()
    {
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }
        var inputX = isMobile ? _joystick.GetHorizontalValue() : Input.GetAxis("Horizontal");
        var inputY = isMobile ? _joystick.GetVerticalValue() : Input.GetAxis("Vertical");
        // Get targetVelocity from input.
        //var inputX = Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(_joystick.Horizontal)
        //    ? Input.GetAxis("Horizontal") : _joystick.Horizontal;
        //var inputY = Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Abs(_joystick.Vertical)
        //    ? Input.GetAxis("Vertical") : _joystick.Vertical;
        Vector2 targetVelocity =new Vector2( inputX * targetMovingSpeed, inputY * targetMovingSpeed);
        // Apply movement.
        _rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, _rigidbody.velocity.y, targetVelocity.y);
    }
}