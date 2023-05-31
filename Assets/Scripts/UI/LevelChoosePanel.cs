using UnityEngine;

public class LevelChoosePanel : MonoBehaviour
{
    [SerializeField] private SaveManager _saveManager;
    public void SaveLevelsState()
    {
        _saveManager.SaveLevelsData();
        print("SAVE LEVELS DATA");
        _saveManager.SaveMoney();
    }
}
