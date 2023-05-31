using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour
{
    [SerializeField] private Button _previousButton, _nextButton;
    private int _currentChoice;
    private void Start()
    {
        Change(0);
    }
    public void Change(int change)
    {
        _currentChoice += change;
        Select(_currentChoice);
    }
    private void Select(int index)
    {
        _previousButton.interactable = (index != 0);
        _nextButton.interactable = (index != transform.childCount-1);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i==index);
        }
    }
}
