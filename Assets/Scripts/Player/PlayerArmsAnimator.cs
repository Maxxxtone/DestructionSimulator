using UnityEngine;

public class PlayerArmsAnimator : MonoBehaviour
{
    private Animator _animator;
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    public void PlayMeleeAttackAnimation()
    {
        _animator.SetTrigger("MeleeAttack");
    }
}
