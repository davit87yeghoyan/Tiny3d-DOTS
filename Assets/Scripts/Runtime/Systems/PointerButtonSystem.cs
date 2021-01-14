using Runtime.Components;
using Runtime.Components.Window;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Text;
using Unity.Transforms;

namespace Runtime.Systems
{
    /// <summary>
    ///     Detect when a body is grabbed and move it along the ground
    ///     Use a raycast to find the ground position and place the object above it
    /// </summary>
    public class PointerButtonSystem : PointerSystemBase
    {
        

        protected override void OnInputUp(int pointerId, float2 inputPos)
        {
            var pointerRaycastHit = GetPointerRaycastHit(inputPos);
            var pointerEntity = pointerRaycastHit.Entity;

            if (pointerEntity == Entity.Null || !HasComponent<ButtonsComponent>(pointerEntity)) return;

            switch (GetComponent<ButtonsComponent>(pointerEntity).ButtonType)
            {
                case ButtonType.StartButton:
                    StartButton(pointerEntity);
                    break;
                case ButtonType.CloseButton:
                    CloseButton(pointerEntity);
                    break;
                case ButtonType.LevelButton:
                    LevelButton(pointerEntity);
                    break;
                default:
                    return;
            }
        }
        

        private void StartButton(Entity pointerEntity)
        {
            Entity entity = GetSingletonEntity<GameDataComponent>();
            int level = (int) GetComponent<FloatComponent>(pointerEntity).Value;
            EntityManager.AddComponentData(entity, new LoadLevelComponent() {LoadLevel = level});
            HideWindow(false);
        }
        private void CloseButton(Entity pointerEntity)
        {
            // if finished no close window
            if(HasComponent<RemoveInStartLevelComponent>(GetSingletonEntity<PlayerTagComponent>())) return;
            HideWindow(false);
        } 
        
        private void LevelButton(Entity pointerEntity)
        {
            int level = (int) GetComponent<FloatComponent>(pointerEntity).Value;
            LevelButton(level);
        }
        
        public void LevelButton(int level)
        {
            Entities.
                WithAll<WindowStartButtonTagComponent,FloatComponent>().
                ForEach((ref FloatComponent floatComponent) =>
                {
                    floatComponent.Value = level;
                }).Run();


            Entity entity = GetSingletonEntity<WindowTextTagComponent>();
            TextLayout.SetEntityTextRendererString(EntityManager, entity,GetLevelText(level));
            HideWindow(true);
        }

        
      

        private void HideWindow(bool open)
        {
            Entities.
                WithAll<WindowTagComponent>().
                ForEach((ref Translation translation) =>
                {
                    translation.Value.z = !open?int.MaxValue:0;
                }).Run();
            
            Entities.
                WithAll<CameraTagComponent>().
                ForEach((ref Translation translation) =>
                {
                    translation.Value.z += !open?5000:-5000f;
                }).Run();


            var singletonEntity = GetSingletonEntity<PlayerTagComponent>();
            if (open)
            {
                EntityManager.AddComponent<PlayerStopComponent>(singletonEntity);
                return;
            }
            EntityManager.RemoveComponent<PlayerStopComponent>(singletonEntity);
        }


        private string GetLevelText(int level)
        {
            return "Level " + level;
        }
        
    }
}
