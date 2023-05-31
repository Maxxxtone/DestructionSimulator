using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Color = UnityEngine.Color;
using Unity.Burst;

namespace VoxelDestruction
{
    [BurstCompile]
    public struct VoxelRenderJobSafe : IJob
    {
        public NativeList<Vector3> vertices;
        [WriteOnly]
        public NativeList<int> triangles;
        [WriteOnly]
        public NativeList<Vector2> uvs;
        [WriteOnly]
        public NativeList<Color> colors;

        [ReadOnly]
        public NativeArray<TempBlock> blocks;
        public int3 modelSize;

        private TempBlock GetBlock(int3 pos)
        {
            return blocks[pos.x + modelSize.x * (pos.y + modelSize.y * pos.z)];
        }
        
        public void Execute()
        {
            TempBlock emptyBlock = new TempBlock();
            
            for (int d = 0; d < 3; d++)
            {
                int u = (d + 1) % 3;
                int v = (d + 2) % 3;
                var x = new int3();
                var q = new int3();
                
                NativeArray<TempBlock> mask = new NativeArray<TempBlock>(modelSize[u] * modelSize[v], Allocator.Temp);

                q[d] = 1;
                
                for (x[d] = -1; x[d] < modelSize[d];)
                {
                    var n = 0;
                    for (x[v] = 0; x[v] < modelSize[v]; ++x[v])
                    {
                        for (x[u] = 0; x[u] < modelSize[u]; ++x[u])
                        {
                            var blockCurrent = (x[d] >= 0) ? GetBlock(x) : emptyBlock;
                            var blockCompare = (x[d] < modelSize[d] - 1) ? GetBlock(x + q) : emptyBlock;

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

                    for (int j = 0; j < modelSize[v]; j++)
                    {
                        for (int i = 0; i < modelSize[u];)
                        {
                            if (mask[n].active == 1 || mask[n].normal != 0)
                            {
                                int w = 1;
                                int h = 1;
                                for (; i + w < modelSize[u] && mask[n + w].Equals(mask[n]); w++) { }
                                
                                var done = false;
                                for (; j + h < modelSize[v]; h++)
                                {
                                    for (int k = 0; k < w; ++k)
                                    {
                                        if (mask[n + k + h * modelSize[u]].active == 1 || !mask[n + k + h * modelSize[u]].Equals(mask[n]))
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
                                        mask[n + k + l * modelSize[u]] = emptyBlock;
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
            triangles.Add(vertices.Length);
            triangles.Add(vertices.Length + 2 - mask.normal);
            triangles.Add(vertices.Length + 2 + mask.normal);
            triangles.Add(vertices.Length + 3);
            triangles.Add(vertices.Length + 1 + mask.normal);
            triangles.Add(vertices.Length + 1 - mask.normal);

            vertices.Add(bottomLeft);
            vertices.Add(bottomRight);
            vertices.Add(topLeft);
            vertices.Add(topRight);
            
            colors.Add(mask.color);
            colors.Add(mask.color);
            colors.Add(mask.color);
            colors.Add(mask.color);
            
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }
    }
}