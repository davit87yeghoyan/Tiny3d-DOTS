using Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Runtime.Systems
{
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class TriggerSystem : SystemBase
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
        private struct TriggerSystemEvent : ITriggerEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<Parent> Parents;
            [ReadOnly] public BufferFromEntity<Child> ChildBufferFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Translation> Translations;
            [ReadOnly] public ComponentDataFromEntity<GoldTagComponent> GoldTagComponents;
            [ReadOnly] public ComponentDataFromEntity<ColorBoxTagComponent> ColorBoxTagComponents;
            [ReadOnly] public ComponentDataFromEntity<ColorBoxComponent> ColorBoxComponents;
            [ReadOnly] public ComponentDataFromEntity<DynamicColorBoxTagComponent> DynamicColorBoxTagComponents;
            [ReadOnly] public ComponentDataFromEntity<CollidedTagComponent> CollidedTagComponents;
            [ReadOnly] public ComponentDataFromEntity<TeleportTagComponent> TeleportTagComponents;


            public Entity EntityCamera;
            public EntityCommandBuffer CommandBuffer;

            public void Execute(TriggerEvent triggerEvent)
            {
               
                if (CollidedTagComponents.HasComponent(triggerEvent.EntityB)) return;

                if (GoldTagComponents.HasComponent(triggerEvent.EntityA))
                {
                    OnTriggerGold(triggerEvent.EntityA);
                }  
                
                if (GoldTagComponents.HasComponent(triggerEvent.EntityB))
                {
                    OnTriggerGold(triggerEvent.EntityB);
                } 
                
                
                
                if (TeleportTagComponents.HasComponent(triggerEvent.EntityB))
                {
                    OnTriggerTeleport(triggerEvent);
                }


                if (ColorBoxTagComponents.HasComponent(triggerEvent.EntityB))
                {
                    OnTriggerBoxColor<ColorBoxTagComponent>(triggerEvent);
                }

                if (DynamicColorBoxTagComponents.HasComponent(triggerEvent.EntityB))
                {
                    OnTriggerBoxColor<DynamicColorBoxTagComponent>(triggerEvent);
                }
            }

            private void OnTriggerTeleport(TriggerEvent triggerEvent)
            {
                ColorsType colorsType = ColorBoxComponents[triggerEvent.EntityB].ColorsType;
                CommandBuffer.AddComponent(EntityCamera, new ChangePlayerColorComponent{ColorsType = colorsType});
              
                for (var i = 0; i < ChildBufferFromEntity[triggerEvent.EntityB].Length; i++)
                    CommandBuffer.DestroyEntity(ChildBufferFromEntity[triggerEvent.EntityB][i].Value);
                CommandBuffer.DestroyEntity(triggerEvent.EntityB);
            }

            private void OnTriggerGold(Entity entity)
            {
                CommandBuffer.RemoveComponent<PhysicsCollider>(entity);

                Entity parent = Parents[entity].Value;
                float3 posGold = Translations[parent].Value;
                float3 posCam = Translations[EntityCamera].Value;

                var scale = new float3(1, 1, 1);
                CommandBuffer.AddComponent(parent, new NonUniformScale {Value = scale});
                CommandBuffer.AddComponent(parent, new GoldToScoreComponent()
                {
                    Lerp = 0,
                    GoldFixPosition = posGold,
                    CameraFixPosition = posCam,
                    StartScale = scale
                });
            }

            private void OnTriggerBoxColor<T>(TriggerEvent triggerEvent)
            {
                //CommandBuffer.RemoveComponent<PhysicsCollider>(triggerEvent.EntityB);
                CommandBuffer.AddComponent(triggerEvent.EntityB, new DispatchEventBoxComponent());
                CommandBuffer.AddComponent(triggerEvent.EntityB, new CollidedTagComponent());
            }
        }


        protected override void OnUpdate()
        {
            Entity entity = GetSingletonEntity<CameraTagComponent>();

            Dependency = new TriggerSystemEvent
            {
                Parents = GetComponentDataFromEntity<Parent>(true),
                ChildBufferFromEntity = GetBufferFromEntity<Child>(true),
                Translations = GetComponentDataFromEntity<Translation>(true),
                GoldTagComponents = GetComponentDataFromEntity<GoldTagComponent>(true),
                ColorBoxTagComponents = GetComponentDataFromEntity<ColorBoxTagComponent>(true),
                ColorBoxComponents = GetComponentDataFromEntity<ColorBoxComponent>(true),
                DynamicColorBoxTagComponents = GetComponentDataFromEntity<DynamicColorBoxTagComponent>(true),
                CollidedTagComponents = GetComponentDataFromEntity<CollidedTagComponent>(true),
                TeleportTagComponents = GetComponentDataFromEntity<TeleportTagComponent>(true),
                CommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer(),
                EntityCamera = entity,
            }.Schedule(_stepPhysicsWorldSystem.Simulation, ref _buildPhysicsWorldSystem.PhysicsWorld, Dependency);

            _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}