using Runtime.Components;
using Unity.Entities;


namespace Runtime.Systems
{
    public class GameOverSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<GameOverComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.RemoveComponent<GameOverComponent>(GetSingletonEntity<GameOverComponent>());
            int level = GetSingleton<GameDataComponent>().Level;
            World.GetExistingSystem<PointerButtonSystem>().LevelButton(level);
        }
    }
}