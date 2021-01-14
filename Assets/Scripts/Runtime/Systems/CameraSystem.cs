using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.Systems
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class CameraSystem : SystemBase
    {
        private readonly float3 _camPosition = new float3(-1.2f, 0.55f, -0.51f);

        protected override void OnCreate()
        {
            EntityQuery query = GetEntityQuery(
                ComponentType.Exclude<RemoveInStartLevelComponent>(), 
                ComponentType.ReadOnly<PlayerTagComponent>()
                );
            RequireForUpdate(query);
        }


        protected override void OnUpdate()
        {
            Entity entityPlayer = GetSingletonEntity<PlayerTagComponent>();

            float3 playerPos = EntityManager.GetComponentData<Translation>(entityPlayer).Value;
            playerPos.z = 0;
            Entities
                .WithAll<CameraTagComponent>()
                .WithoutBurst()
                .ForEach((Entity entity, ref Translation position) => { position.Value = _camPosition + playerPos; }).Run();
        }
    }
}