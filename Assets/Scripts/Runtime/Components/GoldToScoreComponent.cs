using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.Components
{
    public struct GoldToScoreComponent:IComponentData
    {
        public float3 GoldFixPosition;
        public float3 CameraFixPosition;
        public float3 StartScale;
        public float Lerp;
    }
}