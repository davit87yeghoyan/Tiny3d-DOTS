using Runtime.Components;
using Runtime.Components.Buffer;
using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.Systems
{
    public class FinishSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<OpenWindowComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<OpenWindowComponent>(GetSingletonEntity<PlayerTagComponent>());
            Entity singletonEntity = GetSingletonEntity<GameDataComponent>();
            var buffer = GetBuffer<LevelBuffer>(singletonEntity);
            int level = GetSingleton<GameDataComponent>().Level;
            level = math.min(++level, buffer.Length);
            World.GetExistingSystem<PointerButtonSystem>().LevelButton(level);
        }
    }
}