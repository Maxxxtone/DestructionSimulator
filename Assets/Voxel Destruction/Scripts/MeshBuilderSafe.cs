using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoxelDestruction
{
    public sealed class MeshBuilderSafe : System.IDisposable
    {
        private VoxelRenderJobSafe greedyJob;

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

        public void StartMeshDrawing(VoxelData chunk, Allocator allocator)
        {
            greedyJob = new VoxelRenderJobSafe()
            {
                vertices = new NativeList<Vector3>(allocator),
                triangles = new NativeList<int>( allocator),
                uvs = new NativeList<Vector2>(allocator),
                colors = new NativeList<Color>(allocator)
            };
            
            CullVoxels(chunk, out TempBlock[] blocks);

            greedyJob.modelSize = new int3(chunk.Size.x, chunk.Size.y, chunk.Size.z);
            greedyJob.blocks = new NativeArray<TempBlock>(blocks, allocator);

            handle = greedyJob.Schedule();
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
                Mesh mesh = new Mesh
                {
                    indexFormat = use32BitInt
                        ? UnityEngine.Rendering.IndexFormat.UInt32
                        : UnityEngine.Rendering.IndexFormat.UInt16,
                    vertices = greedyJob.vertices.ToArray(),
                    triangles = greedyJob.triangles.ToArray(),
                    colors = greedyJob.colors.ToArray(),
                    uv = greedyJob.uvs.ToArray()
                };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                return mesh;
            }
            catch (Exception e)
            {
                greedyJob.vertices.Dispose();
                greedyJob.triangles.Dispose();
                greedyJob.uvs.Dispose();
                greedyJob.colors.Dispose();
                Debug.LogError("ERROR BUILDING MESH! \n" + e.Message + e.StackTrace);
            }

            return null;
        }

        public void Dispose()
        {
            handle.Complete();
            
            if (greedyJob.blocks.IsCreated)
                greedyJob.blocks.Dispose();
            if (greedyJob.vertices.IsCreated)
                greedyJob.vertices.Dispose();
            if (greedyJob.triangles.IsCreated)
                greedyJob.triangles.Dispose();
            if (greedyJob.uvs.IsCreated)
                greedyJob.uvs.Dispose();
            if (greedyJob.colors.IsCreated)
                greedyJob.colors.Dispose();
            System.GC.SuppressFinalize(this);
        }

        ~MeshBuilderSafe()
        {
            Dispose();
        }
    }   
}
