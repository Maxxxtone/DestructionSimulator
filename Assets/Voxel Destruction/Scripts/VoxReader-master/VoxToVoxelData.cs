using System;
using UnityEngine;
using VoxReader.Interfaces;

namespace VoxelDestruction
{
     public static class VoxToVoxelData
     {
         public static VoxelData GenerateVoxelData(IModel model)
         {
             Vector3Int length = Vector3Int.FloorToInt(model.Size);
             
             Voxel[,,] voxels = new Voxel[length.x, length.y, length.z];
     
             for (int x = 0; x < length.x; x++)
             {
                 for (int y = 0; y < length.y; y++)
                 {
                     for (int z = 0; z < length.z; z++)
                     {
                         voxels[x, y, z] = new Voxel(Color.black, false);
                     }
                 }
             }
     
             for (int i = 0; i < model.Voxels.Length; i++)
             {
                 try
                 {
                     voxels[(int)model.Voxels[i].Position.x, (int)model.Voxels[i].Position.y, (int)model.Voxels[i].Position.z] =
                         new Voxel(model.Voxels[i].Color, true);
                 }
                 catch (Exception e)
                 {
                     Debug.LogWarning($"Error sorting Voxel Array! \n Voxel: {i}, Array Position: {(int)model.Voxels[i].Position.x}/{(int)model.Voxels[i].Position.y}/{(int)model.Voxels[i].Position.z}" + e.Message + e.StackTrace);
                     throw;
                 }
             }
     
             Voxel[] final = new Voxel[length.x * length.y * length.z];
     
             for (int x = 0; x < length.x; x++)
             {
                 for (int y = 0; y < length.y; y++)
                 {
                     for (int z = 0; z < length.z; z++)
                     {
                         final[To1D(new Vector3(x, y, z), length)] = voxels[x, y, z];
                     }
                 }
             }

             return new VoxelData(length, final);
         }
         
         private static int To1D(Vector3 index, Vector3 length)
         {
             return (int)(index.x + length.x * (index.y + length.y * index.z));
         }
     }
}