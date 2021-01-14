using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerSpeedComponent:IComponentData
    {
        public float Speed;
        public float Move;
    }
}