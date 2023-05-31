using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider _fillSlider;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _progressText;
    public Slider FillSlider { get => _fillSlider; }
    public void SetBackground(Sprite bg)
    {
        _background.sprite = bg;
    }
    public void SetProgress(float progress)
    {
        _fillSlider.value = progress;
        _progressText.text = $"{100 * System.Math.Round(progress,1)} %";
    }
}
