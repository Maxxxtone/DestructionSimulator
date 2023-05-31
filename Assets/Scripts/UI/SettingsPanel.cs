using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider _sensetivitySlider;
    [SerializeField] private Sprite _soundOnSprite, _soundOffSprite;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private Image _soundImage, _mouseImage;
    private bool _soundState = true;
    private float _sensetivity;
    private bool _isMobile;
    private void OnEnable()
    {
        _isMobile = Application.isMobilePlatform;
        _soundState = _itemsDatabase.SoundOn;
        _soundImage.sprite = _soundState ? _soundOnSprite : _soundOffSprite;
        if (!_isMobile)
        {
            _sensetivitySlider.value = _itemsDatabase.Sensetivity;
        }
        else
        {
            _mouseImage.gameObject.SetActive(false);
            _sensetivitySlider.gameObject.SetActive(false);
        }
    }
    public void ApplySettings()
    {
        var sens = _isMobile ? 2f : _sensetivitySlider.value;
        _itemsDatabase.SoundOn = _soundState;
        _saveManager.SaveSettings(sens, _soundState);
    }
    public void ChangeSoundState()
    {
        _soundState = !_soundState;
        _soundImage.sprite = _soundState ? _soundOnSprite : _soundOffSprite;
        AudioListener.volume = _soundState ? 1 : 0;
    }
}
