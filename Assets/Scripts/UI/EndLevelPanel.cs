using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndLevelPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _gemsText, _adGemsText;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private RewardAd _rewardAd;
    [SerializeField] private Button _rewardButton;
    [SerializeField] private InterstitialAd _interstitialAd;
    [SerializeField] private PauseButton _pauseButton;
    [SerializeField] private MobileInputPanel _mobileInputPanel;
    private int _adGems, _gems;
    private void OnDisable()
    {
        _rewardAd.RewardGet -= GetReward;
    }
    public void InitializePanel(int gems, int adGems)
    {
        print("INIT PANEL");
        _gems = gems;
        _adGems = adGems;
        _itemsDatabase.Money += gems;
        _saveManager.SaveMoney();
        _gemsText.text = $"+{gems}";
        _adGemsText.text = $"+{adGems}";
        _rewardAd.RewardGet += GetReward;
        _interstitialAd.Show();
        _mobileInputPanel.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(false);
    }
    public void GetReward()
    {
        _itemsDatabase.Money += _adGems;
        print("AD GEMS " + _adGems);
        _rewardButton.interactable = false;
        _rewardAd.RewardGet -= GetReward;
        _saveManager.SaveMoney();
        _gemsText.text = (_gems + _adGems).ToString();
    }
}
