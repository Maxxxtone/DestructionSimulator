using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelDestruction
{
     public sealed class FragmentBuilder : System.IDisposable
     {
         private FragmentJob fragmentJob;
         private JobHandle handle;
     
         public FragmentBuilder(Transform[] frag, int _getCount, Vector3 targetPoint)
         {
             fragmentJob = new FragmentJob()
             {
                 getCount = _getCount,
                 targetPoint = targetPoint,
                 outputIndex = new NativeArray<int>(_getCount, Allocator.Persistent),
                 positions = new NativeArray<float3>(frag.Length, Allocator.Persistent)
             };
     
             for (int i = 0; i < _getCount; i++)
                 fragmentJob.outputIndex[i] = -1;
     
             for (int i = 0; i < frag.Length; i++)
                 fragmentJob.positions[i] = frag[i].position;
     
             handle = fragmentJob.Schedule(frag.Length, default);
         }
     
         public int[] GetFragments()
         {
             handle.Complete();
             fragmentJob.positions.Dispose();
     
             int[] outputArray = new int[fragmentJob.outputIndex.Length];
     
             for (int i = 0; i < outputArray.Length; i++)
                 outputArray[i] = fragmentJob.outputIndex[i];
     
             fragmentJob.outputIndex.Dispose();
             
             return outputArray;
         }
     
         public void Dispose()
         {
             handle.Complete();
             
             if (fragmentJob.outputIndex.IsCreated)
                 fragmentJob.outputIndex.Dispose();
     
             if (fragmentJob.positions.IsCreated)
                 fragmentJob.positions.Dispose();
             
             System.GC.SuppressFinalize(this);
         }
         
         ~FragmentBuilder ()
         {
             Dispose();
         }
     }   
}
