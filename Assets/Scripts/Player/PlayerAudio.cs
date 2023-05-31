using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private Gun _gun;
    [SerializeField] private AudioSource _attackAudioSource;
    private void OnEnable()
    {
        _gun.Attacking += PlayAttackAudio;
    }
    private void PlayAttackAudio(Weapon weapon) => weapon.PlayAttackAudio(_attackAudioSource);
}
