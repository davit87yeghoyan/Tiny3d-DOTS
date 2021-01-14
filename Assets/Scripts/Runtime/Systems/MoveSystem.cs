using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.Systems
{
    
    public class MoveSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerInputComponent>();
        }

        
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            Entities
                .WithAll<PlayerTagComponent>()
                .WithNone<PlayerStopComponent,RemoveInStartLevelComponent>()
                .WithBurst()
                .ForEach((Entity entity,  ref Translation translation, ref Rotation rotation, ref PlayerInputComponent playerInput,  in PlayerSpeedComponent speedComponent) =>
                {
                    rotation.Value = quaternion.AxisAngle(new float3(0,1,0),0);
                    
                    translation.Value.x += speedComponent.Speed * deltaTime;
                    if (!playerInput.IsTouch)
                    {
                        translation.Value.z += -playerInput.InputAxis.x*speedComponent.Move * deltaTime;
                    }
                    else
                    {
                        translation.Value.z += playerInput.InputAxis.x;
                    }
                    
                }).ScheduleParallel();
        }
    }
}