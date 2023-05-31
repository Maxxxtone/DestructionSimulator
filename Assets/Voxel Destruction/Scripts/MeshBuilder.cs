using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoxelDestruction
{
    public sealed class MeshBuilder : IDisposable
    {
        private VoxelRenderJob greedyJob;
        
        private void CullVoxels(VoxelData chunk, out TempBlock[] result)
        {
            var blocks = new TempBlock[chunk.Size.x * chunk.Size.y * chunk.Size.z];
            for (int i = 0; i < chunk.Blocks.Length; i++)
            {
                if (!chunk.Blocks[i].active) continue;
                blocks[i].active = 1;
                blocks[i].color = chunk.Blocks[i].color;
            }

            result = blocks;
        }

        private JobHandle handle;

        public void StartMeshDrawing(VoxelData chunk, int4 meshIndexes, Allocator allocator, bool parallel)
        {
            //MESH ARRAY LENGTH: X = VERT, Y = TRIS, Z = UV, W = COL
            
            greedyJob = new VoxelRenderJob()
            {
                quads = new NativeArray<MeshQuad>(Mathf.CeilToInt((float)meshIndexes.x / 4), allocator),
                quadIndex = new NativeArray<int>(1, allocator)
            };
            
            CullVoxels(chunk, out TempBlock[] blocks);

            greedyJob.chunkSize = new int3(chunk.Size.x, chunk.Size.y, chunk.Size.z);
            greedyJob.blocks = new NativeArray<TempBlock>(blocks, allocator);

            if (!parallel)
                handle = greedyJob.Schedule(3, default);
            else
                handle = greedyJob.ScheduleParallel(3, 1, default);
        }

        public Mesh GetVoxelObject (bool use32BitInt)
        {
            handle.Complete();
            greedyJob.blocks.Dispose();
            
            return BuildMesh(use32BitInt);
        }

        public bool IsCompleted()
        {
            return handle.IsCompleted;
        }

        private Mesh BuildMesh(bool use32BitInt)
        {
            try
            {
                MeshQuad[] quads = new MeshQuad[greedyJob.quadIndex.ToArray()[0]];

                Array.Copy(greedyJob.quads.ToArray(), 0, quads, 0, quads.Length);
                
                Vector3[] vert = new Vector3[quads.Length * 4];
                int[] tris = new int[quads.Length * 6];
                Color[] col = new Color[vert.Length];
                Vector2[] uv = new Vector2[vert.Length];

                int trisIndex = 0;
                int vertIndex = 0;
                for (int i = 0; i < quads.Length; i++)
                {
                    tris[trisIndex] = vertIndex;
                    tris[trisIndex + 1] = vertIndex + 2 - quads[i].mask.normal;
                    tris[trisIndex + 2] = vertIndex + 2 + quads[i].mask.normal;
                    tris[trisIndex + 3] = vertIndex + 3;
                    tris[trisIndex + 4] = vertIndex + 1 + quads[i].mask.normal;
                    tris[trisIndex + 5] = vertIndex + 1 - quads[i].mask.normal;

                    vert[vertIndex] = quads[i].bottomLeft;
                    vert[vertIndex + 1] = quads[i].bottomRight;
                    vert[vertIndex + 2] = quads[i].topLeft;
                    vert[vertIndex + 3] = quads[i].topRight;
            
                    col[vertIndex] = quads[i].mask.color;
                    col[vertIndex + 1] = quads[i].mask.color;
                    col[vertIndex + 2] = quads[i].mask.color;
                    col[vertIndex + 3] = quads[i].mask.color;

                    uv[vertIndex] = new Vector2(0, 0);
                    uv[vertIndex + 1] = new Vector2(0, 1);
                    uv[vertIndex + 2] = new Vector2(1, 0);
                    uv[vertIndex + 3] = new Vector2(1, 1);

                    trisIndex += 6;
                    vertIndex += 4;
                }
                
                Mesh mesh = new Mesh
                {
                    indexFormat = use32BitInt ? UnityEngine.Rendering.IndexFormat.UInt32 : default,
                    vertices = vert,
                    triangles = tris, 
                    colors = col,
                    uv = uv
                };

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                return mesh;
            }
            catch (Exception e)
            {
                greedyJob.quads.Dispose();
                greedyJob.quadIndex.Dispose();
                Debug.LogError("ERROR BUILDING MESH! \n" + e.Message + e.StackTrace);
            }

            return null;
        }

        public void Dispose()
        {
            handle.Complete();
            
            if (greedyJob.blocks.IsCreated)
                greedyJob.blocks.Dispose();
            if (greedyJob.quads.IsCreated)
                greedyJob.quads.Dispose();
            if (greedyJob.quadIndex.IsCreated)
                greedyJob.quadIndex.Dispose();
            GC.SuppressFinalize(this);
        }
        
        ~MeshBuilder()
        {
            Dispose();
        }
    }
}