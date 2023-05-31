using UnityEngine;
using UniversalMobileController;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    //[SerializeField] private TouchField _touchField;
    [SerializeField] private SpecialTouchPad _touchField;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    public float _sensitivity = 2;
    public float _smoothing = 1.5f;
    Vector2 velocity;
    Vector2 frameVelocity;
    private bool _isMobile;
    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }
    void Start()
    {
        // Lock the mouse cursor to the game screen.
        _isMobile = Application.isMobilePlatform;
        if (!_isMobile)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        _sensitivity = _itemsDatabase.Sensetivity;
    }

    void Update()
    {
        if (PauseButton.Paused) return;
        // Get smooth velocity.
        var mouseX = _isMobile ? _touchField.GetHorizontalValue() : Input.GetAxisRaw("Mouse X");
        var mouseY = _isMobile ? _touchField.GetVerticalValue() : Input.GetAxisRaw("Mouse Y");
        //var mouseX = !Application.isMobilePlatform ?
        //    Input.GetAxisRaw("Mouse X") : _touchField.TouchAxis.x;
        //var mouseY = !Application.isMobilePlatform ?
        //    Input.GetAxisRaw("Mouse Y") : _touchField.TouchAxis.y;
        Vector2 mouseDelta = new Vector2(mouseX, mouseY);
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * _sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / _smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);
        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
