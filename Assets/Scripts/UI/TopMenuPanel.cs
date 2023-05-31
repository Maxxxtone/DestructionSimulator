using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopMenuPanel : MonoBehaviour
{
    [SerializeField] private int _money;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private ItemsDatabase _itemsDatabase;
    [SerializeField] private Button _adButton;
    [SerializeField] private RewardAd _rewardAd;
    [SerializeField] private SaveManager _saveManager;
    private void OnEnable()
    {
        _saveManager.DataLoaded += ShowMoney;
        _saveManager.LoadData();
        _rewardAd.RewardGet += OnGetReward;
        _itemsDatabase.UpdateLeftMoney += ShowMoney;
    }
    private void OnDisable()
    {
        _saveManager.DataLoaded -= ShowMoney;
        _rewardAd.RewardGet -= OnGetReward;
        _itemsDatabase.UpdateLeftMoney -= ShowMoney;
    }
    public void OnGetReward()
    {
        print("GET REWARD ON MAIN MENU");
        _adButton.interactable = false;
        _itemsDatabase.Money += 500;
        _moneyText.text = _itemsDatabase.Money.ToString();
        _rewardAd.RewardGet -= OnGetReward;
        _saveManager.SaveMoney();
    }
    private void ShowMoney()
    {
        _moneyText.text = _itemsDatabase.Money.ToString();
    }
    private void ShowMoney(int moneyLeft)
    {
        _moneyText.text = moneyLeft.ToString();
    }
}
