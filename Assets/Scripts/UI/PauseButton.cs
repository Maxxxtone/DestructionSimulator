using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public static bool Paused;
    [SerializeField] private Sprite _mobileIcon, _desktopIcon;
    [SerializeField] private DestructionGoal _destructionGoal;
    private Image _image;
    private Button _button;
    private void Start()
    {
        Paused = false;
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _image.sprite = Application.isMobilePlatform?_mobileIcon:_desktopIcon;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P) && !Paused)
        {
            Pause();
        }   
    }
    public void Pause()
    {
        _button.enabled = false;
        _destructionGoal.SetPauseState();
        Paused = true;
    }
    public void Continue()
    {
        _button.enabled = true;
        Paused = false;
    }
}
