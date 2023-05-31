using UnityEngine;

namespace VoxelDestruction
{
    public struct Voxel
    {
        public Color color;
        public bool active;

        public Voxel(Color _color, bool _active)
        {
            color = new Color(_color.r / 255, _color.g / 255, _color.b / 255, 255);
            active = _active;
        }
        
        public Voxel(Voxel vox)
        {
            color = vox.color;
            active = vox.active;
        }
    }
}