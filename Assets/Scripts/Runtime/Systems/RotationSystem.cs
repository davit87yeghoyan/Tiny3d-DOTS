using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.Systems
{
    
    public class RotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            Entities
                .WithAll<RotationComponent>()
                .WithBurst()
                .ForEach((ref Rotation rotation, in RotationComponent rotationComponent) =>
                {
                    rotation.Value = math.mul(math.normalizesafe(rotation.Value), quaternion.AxisAngle(math.left(), deltaTime*rotationComponent.Speed));
                }).ScheduleParallel();
        }
    }
}