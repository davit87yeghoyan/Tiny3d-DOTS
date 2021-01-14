using Unity.Entities;

namespace Runtime.Components
{
    [GenerateAuthoringComponent]
    public struct ButtonsComponent:IComponentData
    {
        public ButtonType ButtonType;
    }


    public enum ButtonType
    {
        StartButton,
        CloseButton,
        LevelButton
    }
}