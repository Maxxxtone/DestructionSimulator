using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private PauseButton _pauseButton;
    [SerializeField] private TextMeshProUGUI _gemsText;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private MobileInputPanel _mobileInputPanel;
    private int _gems;
    private bool _isMobile;
    public void InitPanel(int currentGemsCount)
    {
        _isMobile = Application.isMobilePlatform;
        print("GEMS ON PAUSE PANEL " + currentGemsCount);
        _gems = currentGemsCount;
        if(_isMobile)
            _mobileInputPanel.gameObject.SetActive(false);
        _gemsText.text = _gems.ToString();
    }
    public void Continue()
    {
        _pauseButton.Continue();
        Time.timeScale = 1.0f;
        if (!_isMobile)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _mobileInputPanel.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }
    //on restart and quit
    public void SaveMoneyBeforeQuit()
    {
        Time.timeScale = 1;
        _itemsDatabase.Money += _gems;
        _saveManager.SaveMoney();
    }
}
