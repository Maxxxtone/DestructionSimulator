using System;
using Unity.Collections;
using UnityEngine;

namespace VoxelDestruction
{
    [BurstCompatible]
    public struct TempBlock : IEquatable<TempBlock>
    {
        public Color color;
        public int active;
        public int normal;

        public bool Equals(TempBlock other)
        {
            return other.normal == normal && other.color == color;
        }

        public override string ToString()
        {
            return "Color: " + color + ", Active: " + active + ", Normal: " + normal;
        }
    }
}