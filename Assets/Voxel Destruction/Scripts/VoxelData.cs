using UnityEngine;

namespace VoxelDestruction
{
    public struct VoxelData
    {
        public Vector3Int Size;
        public Voxel[] Blocks;

        public VoxelData(Vector3Int length, Voxel[] blocks)
        {
            Size = length;

            Blocks = blocks;
        }
    }   
}