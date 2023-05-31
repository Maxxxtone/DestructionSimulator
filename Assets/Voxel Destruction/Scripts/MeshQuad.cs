using Unity.Collections;
using Unity.Mathematics;

namespace VoxelDestruction
{
    [BurstCompatible]
    public struct MeshQuad
    {
        public float3 bottomLeft;
        public float3 topLeft;
        public float3 topRight;
        public float3 bottomRight;
        public TempBlock mask;

        public MeshQuad (float3 bl, float3 tl, float3 tr, float3 br, TempBlock tb)
        {
            bottomLeft = bl;
            topLeft = tl;
            topRight = tr;
            bottomRight = br;
            mask = tb;
        }
    }   
}
