using UnityEngine;

public class DestructionGoal : MonoBehaviour
{
    [SerializeField] private int _targetDestructionPoints = 100;
    [SerializeField] private DestructionUI _destructionUI;
    [SerializeField] private EndLevelPanel _endLevelPanel;
    [SerializeField] private FirstPersonLook _firstPersonLook;
    [SerializeField] private FirstPersonMovement _movement;
    [SerializeField] private Gun _gunHolder;
    [SerializeField] private Player _player;
    [SerializeField] private Camera _endGameCamera;
    [SerializeField] private PausePanel _pausePanel;
    private int _currentDestructionGoal;
    private bool _reachTarget;
    private void Start()
    {
        _destructionUI.UpdateProgress(_targetDestructionPoints, _currentDestructionGoal);
    }
    public void ChangeProgress(int destructibles)
    {
        _currentDestructionGoal+=destructibles;
        _destructionUI.UpdateProgress(_targetDestructionPoints, _currentDestructionGoal);
        if(_currentDestructionGoal/100 >= _targetDestructionPoints && !_reachTarget)
        {
            CompleteLevel();
            _reachTarget = true;
        }
    }
    public void CompleteLevel()
    {
        _endLevelPanel.gameObject.SetActive(true);
        _endLevelPanel.InitializePanel(_currentDestructionGoal / 10000, _targetDestructionPoints / 15);
        DisableCharacter();
    }
    public void SetPauseState()
    {
        Time.timeScale = 0;
        _pausePanel.gameObject.SetActive(true);
        _pausePanel.InitPanel(_currentDestructionGoal / 10000);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    private void DisableCharacter()
    {
        _player.gameObject.SetActive(false);
        _endGameCamera.gameObject.SetActive(true);
        Cursor.lockState= CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
