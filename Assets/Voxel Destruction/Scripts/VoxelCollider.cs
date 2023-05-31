using UnityEngine;

namespace VoxelDestruction
{
    public class VoxelCollider : MonoBehaviour
    {
        public float collisionScale = 1;
        public bool onlyDetectRb;

        private void OnCollisionEnter(Collision collision)
        {
            if (!onlyDetectRb)
            {
                if (collision.transform.GetComponentInParent<VoxelObject>())
                {
                    collision.transform.GetComponentInParent<VoxelObject>().OnVoxelCollision(collision, collisionScale);
                }
                else if (collision.transform.GetComponentInChildren<VoxelObject>())
                {
                    collision.transform.GetComponentInChildren<VoxelObject>().OnVoxelCollision(collision, collisionScale);
                }   
            }
            else if (collision.rigidbody)
            {
                if (collision.transform.parent.GetComponent<VoxelObject>())
                {
                    collision.transform.parent.GetComponent<VoxelObject>().OnVoxelCollision(collision, collisionScale);
                }
            }
        }
    }   
}
