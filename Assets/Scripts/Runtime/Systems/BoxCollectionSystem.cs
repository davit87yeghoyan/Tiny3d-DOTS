using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.Systems
{
    
    [UpdateInGroup(typeof( PresentationSystemGroup ), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
   
    public class BoxCollectionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            GameDataComponent gameDataComponent = GetSingleton<GameDataComponent>();

            Entity wagonEntity = GetSingletonEntity<WagonTagComponent>();

            if (!EntityManager.HasComponent<Child>(wagonEntity))
            {
                EntityManager.AddBuffer<Child>(wagonEntity);
            }

            DynamicBuffer<Child> bufferChildren = EntityManager.GetBuffer<Child>(wagonEntity);
            Entities
                .WithAll<DispatchEventBoxComponent>()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref Translation position, ref Parent parent, ref PreviousParent previousParent, in ColorBoxComponent colorBoxComponent) =>
                {
                    EntityManager.RemoveComponent<DispatchEventBoxComponent>(entity);

                    if (gameDataComponent.Color == colorBoxComponent.ColorsType)
                    {
                        EntityManager.AddComponent<ToWagonTagComponent>(parent.Value);
                        return;
                    }

                    if (bufferChildren.Length == 0)
                    {
                        Entity playerEntity = GetSingletonEntity<PlayerTagComponent>();
                        EntityManager.AddComponent<GameOverComponent>(playerEntity);
                        EntityManager.AddComponent<RemoveInStartLevelComponent>(playerEntity);
                        return;
                    };
                    var last = bufferChildren[bufferChildren.Length - 1];
                    bufferChildren.RemoveAt(bufferChildren.Length - 1);
                    EntityManager.AddComponent<RemoveFromWagonTagComponent>(last.Value);
                }).Run();


            SetToWagon(bufferChildren);
            RemoveFromWagon(bufferChildren);
        }

        private void SetToWagon(DynamicBuffer<Child> bufferChildren)
        {
            var wagon = GetSingletonEntity<WagonTagComponent>();
            ComponentDataFromEntity<Translation> entityData = GetComponentDataFromEntity<Translation>(false);
            Entities
                .WithAll<ToWagonTagComponent>()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref DynamicBuffer<Child> child, ref Translation position, ref Parent parent) =>
                {
                    UpdatePositionChildren(bufferChildren,child,entityData,true);
                    bufferChildren.Add(new Child() {Value = entity});
                    position.Value = float3.zero;
                    parent.Value = wagon;
                    EntityManager.RemoveComponent<ToWagonTagComponent>(entity);
                }).Run();
        }

        private void RemoveFromWagon(DynamicBuffer<Child> bufferChildren)
        {
            Entity colorsGroupEntity = GetSingletonEntity<ColorsGroupTagComponent>();
            ComponentDataFromEntity<Translation> entityData = GetComponentDataFromEntity<Translation>(false);
            Entities
                .WithAll<RemoveFromWagonTagComponent>()
                .WithStructuralChanges()
                .ForEach((Entity entity, DynamicBuffer<Child> child, ref Translation position, ref Parent parent, ref PreviousParent previousParent) =>
                {
                    parent.Value = colorsGroupEntity;
                    UpdatePositionChildren(bufferChildren,child,entityData,false);
                    EntityManager.RemoveComponent<RemoveFromWagonTagComponent>(entity);
                    position.Value.y = float.MinValue;
                }).Run();
        }

        private void UpdatePositionChildren(DynamicBuffer<Child> bufferChildren, DynamicBuffer<Child> child, ComponentDataFromEntity<Translation> entityData, bool add)
        {
            float y = EntityManager.GetComponentData<NonUniformScale>(child[0].Value).Value.y/2;
            foreach (var bufferChild in bufferChildren)
            {
                Translation translation = entityData[bufferChild.Value];
                translation.Value.y += y*(add?1:-1);
                entityData[bufferChild.Value] = translation;
            }
        }
    }
}