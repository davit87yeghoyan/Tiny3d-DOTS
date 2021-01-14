using Runtime.Components;
using Runtime.Components.Buffer;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Runtime.Systems
{
    public class LoadLevelSystem : SystemBase
    {
        private Entity _levelEntity;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<LoadLevelComponent>();
        }

        protected override void OnUpdate()
        {
            RemoveWagonChild(); /* 1*/
            EntityManager.DestroyEntity(_levelEntity);


            Entity singletonEntity = GetSingletonEntity<LoadLevelComponent>();
            LoadLevelComponent loadLevelComponent = GetSingleton<LoadLevelComponent>();
            var buffer = GetBuffer<LevelBuffer>(singletonEntity);

            if (loadLevelComponent.LoadLevel < 0 || loadLevelComponent.LoadLevel > buffer.Length) return;
            _levelEntity = EntityManager.Instantiate(buffer[loadLevelComponent.LoadLevel - 1].Entity);
            SetPlayerToStartPosition();
            SetLevel(loadLevelComponent.LoadLevel);
            EntityManager.RemoveComponent<LoadLevelComponent>(singletonEntity);
            var entity = GetSingletonEntity<PlayerTagComponent>();
            EntityManager.RemoveComponent<RemoveInStartLevelComponent>(entity);
        }


        private void SetLevel(int level)
        {
            Entities
                .WithAll<GameDataComponent>()
                .ForEach((ref GameDataComponent gameDataComponent) => { gameDataComponent.Level = level; }).Run();
        }

        private void RemoveWagonChild()
        {
            Entity colorsGroupEntity = GetSingletonEntity<ColorsGroupTagComponent>();
            EntityManager.RemoveComponent<Child>(GetSingletonEntity<WagonTagComponent>());

            Entities
                .WithAll<CollidedTagComponent>()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref Parent parent) => { parent.Value = colorsGroupEntity; }).Run();
        }


        private void SetPlayerToStartPosition()
        {
            Entity entity = GetSingletonEntity<StartPositionTagComponent>();
            float3 startPosition = EntityManager.GetComponentData<Translation>(entity).Value;
            Entities.WithAll<PlayerTagComponent>().ForEach((ref Translation translation) => { translation.Value = startPosition; }).Run();
        }
    }
}