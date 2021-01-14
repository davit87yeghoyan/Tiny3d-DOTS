using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct GameDataComponent:IComponentData
    {
        public int Score;
        public int Level;
        public ColorsType Color;
    }
    
}