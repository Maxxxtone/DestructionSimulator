using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelChooseButton : MonoBehaviour
{
    [SerializeField] private LevelItem _levelItem;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private Button _unblockButton, _playButton;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private SceneSwitcher _sceneSwitcher;
    [SerializeField] private GameObject _levelPresentation;
    private LevelChoosePanel _levelChoosePanel;
    //добавить ссылку на модель и скрывать ее при нажатии плей
    private void OnEnable()
    {
        if(_levelChoosePanel == null)
            _levelChoosePanel = transform.parent.GetComponent<LevelChoosePanel>();
        _costText.text = _levelItem.Cost.ToString();
        if (_levelItem.Opened)
        {
            _unblockButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(true);
        }
        else
        {
            _unblockButton.gameObject.SetActive(true);
            _playButton.gameObject.SetActive(false);
        }
    }
    public void Play()
    {
        _levelPresentation.SetActive(false);
        _sceneSwitcher.LoadSceneWithLoadingScreen(_levelItem.SceneIdForOpen);
    }
    public void BuyLevel()
    {
        if(_itemsDatabase.Money >= _levelItem.Cost)
        {
            _itemsDatabase.Money -= _levelItem.Cost;
            _itemsDatabase.UpdateLeftMoney?.Invoke(_itemsDatabase.Money);
            _levelItem.Opened = true;
            _levelChoosePanel.SaveLevelsState();
            _unblockButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(true); 
        }
    }
}
