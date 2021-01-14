using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct ColorBoxComponent:IComponentData
    {
        public ColorsType ColorsType;
    }
}