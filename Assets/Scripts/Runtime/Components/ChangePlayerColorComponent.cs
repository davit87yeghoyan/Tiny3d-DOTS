using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct ChangePlayerColorComponent:IComponentData
    {
        public ColorsType ColorsType;
    }
}