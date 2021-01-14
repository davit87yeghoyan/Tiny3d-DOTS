using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct FloatComponent:IComponentData
    {
        public float Value;
    }
}