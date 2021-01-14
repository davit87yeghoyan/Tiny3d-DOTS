using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerInputComponent : IComponentData
    {
        public float2 InputAxis;
        public bool IsTouch;
    }
}
