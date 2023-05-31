using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelDestruction
{
    [BurstCompile]
    public struct FragmentJob : IJobFor
    {
        [ReadOnly]
        public NativeArray<float3> positions;
        //[NativeDisableParallelForRestriction]
        public NativeArray<int> outputIndex;

        public float3 targetPoint;
        //Length of output array
        public int getCount;

        public void Execute(int index)
        {
            if (index == 0)
            {
                outputIndex[0] = index;
            }
            else
            {
                float distance = math.lengthsq(positions[index] - targetPoint);
                
                for (int i = 0; i < getCount; i++)
                {
                    if (outputIndex[i] == -1)
                    {
                        outputIndex[i] = index;
                        break;
                    }
                    else if (distance < math.lengthsq(positions[outputIndex[i]] - targetPoint))
                    {
                        int lastValue = outputIndex[i];
                        
                        for (int j = 1; j < getCount - i; j++)
                        {
                            int currentValue = outputIndex[i + j];
                            outputIndex[i + j] = lastValue;
                            
                            lastValue = currentValue;
                            
                            if (lastValue == -1)
                                break;
                        }
                        
                        outputIndex[i] = index;

                        break;
                    }
                }
            }
        }
    }   
}
