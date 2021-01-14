using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct LoadLevelComponent:IComponentData
    {
        public int LoadLevel;
    }
}
