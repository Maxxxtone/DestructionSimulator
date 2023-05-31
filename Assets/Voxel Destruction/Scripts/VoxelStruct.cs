using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelDestruction 
{
    public class VoxelStruct : MonoBehaviour
    {
        private Vector3Int length;

        private MeshBuilder builder;
        public VoxelObject voxOrigin;
        
        public void BuildObject(VoxReader.Voxel[] data, Material material, Allocator fragAllocator, bool useDelay,
            VoxelObject origin)
        {
            if (useDelay)
                StartCoroutine(BuildModel(data, material, fragAllocator, origin));
            else
            { 
                //Build Voxel Mesh out of Data

                Vector3 min = GetMinV3(data);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new VoxReader.Voxel(data[i].Position - min, data[i].Color);
                }

                length = GetLength(data) + Vector3Int.one;
                
                if (length == Vector3Int.zero)
                    Debug.LogError("Fragment Length can not be zero", this);
                
                Voxel[] final = new Voxel[length.x * length.y * length.z];

                for (int i = 0; i < final.Length; i++)
                {
                    final[i] = new Voxel(Color.black, false);
                }
                
                for (int i = 0; i < data.Length; i++)
                {
                    try
                    {
                        final[To1D(Vector3Int.FloorToInt(data[i].Position))] = new Voxel(data[i].Color, true);
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"Error: Position {data[i].Position}, Final Index {To1D(Vector3Int.FloorToInt(data[i].Position))}, Data Index {i}, Final Length {length.x * length.y * length.z}");
                    }
                }

                builder = new MeshBuilder();
                
                int4 arrayLength = new int4(length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 2 * 6,
                    length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 4 * 6);
                arrayLength.xyzw /= 2;
                
                builder.StartMeshDrawing(new VoxelData(length, final), arrayLength, fragAllocator, false);
                
                GameObject modelChild = new GameObject("VoxelModel " + 1);
                modelChild.transform.parent = transform;
                modelChild.transform.localPosition = Vector3.zero;
                modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
                modelChild.transform.localScale = Vector3.one;
                MeshFilter filter = modelChild.AddComponent<MeshFilter>();
                
                modelChild.AddComponent<MeshRenderer>();
                modelChild.GetComponent<MeshRenderer>().material = material;
                
                MeshCollider coll = modelChild.AddComponent<MeshCollider>();
                coll.convex = true;
                
                Mesh mesh = builder.GetVoxelObject(false);

                builder.Dispose();
                
                if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
                {
                    Destroy(gameObject);
                    return;
                }

                filter.mesh = mesh;
                coll.sharedMesh = mesh;

                if (origin == null)
                    Destroy(this);
                else
                {
                    voxOrigin = origin;
                }
            }
        }

        private IEnumerator BuildModel(VoxReader.Voxel[] data, Material material, Allocator fragAllocator, VoxelObject origin)
        {
            //Build Voxel Mesh out of Data

            Vector3 min = GetMinV3(data);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new VoxReader.Voxel(data[i].Position - min, data[i].Color);
            }

            length = GetLength(data) + Vector3Int.one;
            
            if (length == Vector3Int.zero)
                Debug.LogError("Fragment Length can not be zero", this);
            
            Voxel[] final = new Voxel[length.x * length.y * length.z];

            for (int i = 0; i < final.Length; i++)
            {
                final[i] = new Voxel(Color.black, false);
            }
            
            for (int i = 0; i < data.Length; i++)
            {
                try
                {
                    final[To1D(Vector3Int.FloorToInt(data[i].Position))] = new Voxel(data[i].Color, true);
                }
                catch (Exception)
                {
                    Debug.LogError($"Error: Position {data[i].Position}, Final Index {To1D(Vector3Int.FloorToInt(data[i].Position))}, Data Index {i}, Final Length {length.x * length.y * length.z}");
                }
            }

            builder = new MeshBuilder();
            
            int4 arrayLength = new int4(length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 2 * 6,
                length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 4 * 6);
            arrayLength.xyzw /= 2;
            
            builder.StartMeshDrawing(new VoxelData(length, final), arrayLength, fragAllocator, false);
            
            GameObject modelChild = new GameObject("VoxelModel " + 1);
            modelChild.transform.parent = transform;
            modelChild.transform.localPosition = Vector3.zero;
            modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
            modelChild.transform.localScale = Vector3.one;
            MeshFilter filter = modelChild.AddComponent<MeshFilter>();
            
            modelChild.AddComponent<MeshRenderer>();
            modelChild.GetComponent<MeshRenderer>().material = material;
            
            MeshCollider coll = modelChild.AddComponent<MeshCollider>();
            coll.convex = true;

            yield return null;
            
            Mesh mesh = builder.GetVoxelObject(false);

            builder.Dispose();
            
            if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
            {
                Destroy(gameObject);
                yield break;
            }

            filter.mesh = mesh;
            coll.sharedMesh = mesh;
            
            transform.root.gameObject.SetActive(false);
                
            if (origin == null)
                Destroy(this);
            else
            {
                voxOrigin = origin;
            }
        }
        
        private void OnApplicationQuit()
        {
            if (builder != null)
            {
                builder.Dispose();
            }
        }

        private Vector3 GetMinV3(VoxReader.Voxel[] array)
        {
            Vector3 min = array[0].Position;
            
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i].Position.x < min.x)
                    min.x = array[i].Position.x;
                if (array[i].Position.y < min.y)
                    min.y = array[i].Position.y;
                if (array[i].Position.z < min.z)
                    min.z = array[i].Position.z;
            }

            return min;
        }
        
        private Vector3Int GetLength(VoxReader.Voxel[] array)
        {
            Vector3Int max = Vector3Int.one;
            
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Position.x > max.x)
                    max.x = (int)array[i].Position.x;
                if (array[i].Position.y > max.y)
                    max.y = (int)array[i].Position.y;
                if (array[i].Position.z > max.z)
                    max.z = (int)array[i].Position.z;
            }

            return max;
        }
        
        private int To1D(Vector3Int index)
        {
            return index.x + length.x * (index.y + length.y * index.z);
        }
    }
}