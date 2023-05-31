using UnityEngine;

public class Killzone : MonoBehaviour
{
    [SerializeField] private DestructionGoal _destructionGoal;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            _destructionGoal.CompleteLevel();
        }
    }
}
