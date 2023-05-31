using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VoxelDestruction
{
    [BurstCompile]
    public struct VoxelRenderJobParallel : IJobFor
    {
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<MeshQuad> quads;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<int> quadIndex;
        
        [ReadOnly]
        public NativeArray<TempBlock> blocks;
        public int3 chunkSize;

        private TempBlock GetBlock(int3 pos)
        {
            return blocks[pos.x + chunkSize.x * (pos.y + chunkSize.y * pos.z)];
        }
        
        public void Execute(int index)
        {
            //Index: 0 to chunkSize.x + chunkSize.y + chunksSize.z + 3

            TempBlock emptyBlock = new TempBlock();
            int d;
            var x = new int3();

            if (index < chunkSize.x + 1)
            {
                d = 0;
                x[0] = index - 1;
            }
            else if (index < chunkSize.y + chunkSize.x + 2)
            {
                d = 1;
                x[1] = index - chunkSize.x - 2;
            }
            else
            {
                d = 2;
                x[2] = index - chunkSize.x - chunkSize.y - 3;
            }

            int u = (d + 1) % 3;
            int v = (d + 2) % 3;
            var q = new int3();
            
            NativeArray<TempBlock> mask = new NativeArray<TempBlock>(chunkSize[u] * chunkSize[v], Allocator.Temp);
            
            q[d] = 1;
            
            var n = 0;
            for (x[v] = 0; x[v] < chunkSize[v]; ++x[v])
            {
                for (x[u] = 0; x[u] < chunkSize[u]; ++x[u])
                {
                    var blockCurrent = (x[d] >= 0) ? GetBlock(x) : emptyBlock;
                    var blockCompare = (x[d] < chunkSize[d] - 1) ? GetBlock(x + q) : emptyBlock;

                    if (blockCurrent.active == blockCompare.active)
                    {
                        mask[n++] = emptyBlock;
                    }
                    else if (blockCurrent.active != 1)
                    {
                        blockCurrent.normal = 1;
                        blockCurrent.color = blockCompare.color;
                        mask[n++] = blockCurrent;
                    }
                    else
                    {
                        blockCompare.normal = -1;
                        blockCompare.color = blockCurrent.color;
                        mask[n++] = blockCompare;
                    }
                }
            }

            x[d]++;
            
            n = 0;

            for (int j = 0; j < chunkSize[v]; j++)
            {
                for (int i = 0; i < chunkSize[u];)
                {
                    if (mask[n].active == 1 || mask[n].normal != 0)
                    {
                        int w = 1;
                        int h = 1;
                        for (; i + w < chunkSize[u] && mask[n + w].Equals(mask[n]); w++) { }

                        var done = false;
                        for (; j + h < chunkSize[v]; h++)
                        {
                            for (int k = 0; k < w; ++k)
                            {
                                if (mask[n + k + h * chunkSize[u]].active == 1 || !mask[n + k + h * chunkSize[u]].Equals(mask[n]))
                                {
                                    done = true;
                                    break;
                                }
                            }
                            if (done)
                                break;
                        }

                        x[u] = i;
                        x[v] = j;
                        
                        var du = new int3();
                        du[u] = w;

                        var dv = new int3();
                        dv[v] = h;

                        var blockMinVertex = new float3(-0.5f, -0.5f, -0.5f);

                        AddToMesh(
                            blockMinVertex + x,
                            blockMinVertex + x + du,
                            blockMinVertex + x + du + dv,
                            blockMinVertex + x + dv,
                            mask[n]
                        );

                         for (int l = 0; l < h; ++l)
                            for (int k = 0; k < w; ++k)
                            {
                                mask[n + k + l * chunkSize[u]] = emptyBlock;
                            }

                        i += w;
                        n += w;
                    }
                    else
                    {
                        i++;
                        n++;
                    }
                }
            }
        }
        
        private void AddToMesh(
                float3 bottomLeft,
                float3 topLeft,
                float3 topRight,
                float3 bottomRight,
                TempBlock mask
        )
        {
            quads[quadIndex[0]++] = new MeshQuad(bottomLeft, topLeft, topRight, bottomRight, mask);
        }
    }   
}