using UnityEngine;

namespace VoxelDestruction
{
    public class SimpleSphere
    {
        public Vector3 centerPoint;
        public float radius;

        public SimpleSphere(Vector3 center, float _radius)
        {
            centerPoint = center;
            radius = _radius;
        }

        public bool IsInsideSphere(Vector3 point)
        {
            return Mathf.Pow(point.x - centerPoint.x, 2) +
                   Mathf.Pow(point.y - centerPoint.y, 2) +
                   Mathf.Pow(point.z - centerPoint.z, 2) <= 
                   Mathf.Pow(radius, 2);
        }
    }   
}
