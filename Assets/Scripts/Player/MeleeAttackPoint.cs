using UnityEngine;
using VoxelDestruction;

public class MeleeAttackPoint : MonoBehaviour
{
    [SerializeField] private float _attackRadius = 3.2f;
    [SerializeField] private LayerMask _damageLayer;
    public void CreateAttackSphere(Camera fpsCamera, int damage)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _attackRadius, _damageLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.root.TryGetComponent(out VoxelObject obj))
            {
                obj.transform.root.GetComponent<VoxelObject>().AddDestruction(damage,
                    fpsCamera.transform.position + fpsCamera.transform.forward * _attackRadius,
                    -fpsCamera.transform.forward);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}
