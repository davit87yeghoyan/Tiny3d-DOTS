using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct RotationComponent:IComponentData
    {
        public float Speed;
    }
}