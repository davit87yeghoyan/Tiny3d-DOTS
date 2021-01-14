using Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Runtime.Systems
{
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class TriggerFinishSystem : SystemBase
    {
        private BuildPhysicsWorld _buildPhysicsWorldSystem;
        private EndSimulationEntityCommandBufferSystem _entityCommandBufferSystem;
        private StepPhysicsWorld _stepPhysicsWorldSystem;

        protected override void OnCreate()
        {
            _buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        [BurstCompile]
        private struct TriggerFinishSystemEvent : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<FinishTagComponent> FinishTagComponents;
            public Entity Player;
            public EntityCommandBuffer CommandBuffer;

            public void Execute(TriggerEvent triggerEvent)
            {
                if (!FinishTagComponents.HasComponent(triggerEvent.EntityB)) return;
                {
                    CommandBuffer.AddComponent<RemoveInStartLevelComponent>(Player);
                    CommandBuffer.AddComponent<OpenWindowComponent>(Player);
                    CommandBuffer.RemoveComponent<PhysicsCollider>(triggerEvent.EntityB);
                }
            }
        }


        protected override void OnUpdate()
        {

            Dependency = new TriggerFinishSystemEvent()
            {
                FinishTagComponents = GetComponentDataFromEntity<FinishTagComponent>(true),
                CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer(),
                Player = GetSingletonEntity<PlayerTagComponent>(),
            }.Schedule(_stepPhysicsWorldSystem.Simulation, ref _buildPhysicsWorldSystem.PhysicsWorld, Dependency);

            _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}