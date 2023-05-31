using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DestructionUI : MonoBehaviour
{
    [SerializeField] private Slider _progressFill;
    [SerializeField] private TextMeshProUGUI _progressText;
    public void UpdateProgress(int target, int progress)
    {
        //добавить dotween
        _progressText.text = Mathf.Floor((float)progress/target).ToString() + "%";
        _progressFill.value = Mathf.Floor((float)progress / target)/100;
    }
}
