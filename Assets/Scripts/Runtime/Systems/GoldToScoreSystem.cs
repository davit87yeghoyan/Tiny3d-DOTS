using Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny.Text;
using Unity.Transforms;

namespace Runtime.Systems
{
    public class GoldToScoreSystem : SystemBase
    {
        private Entity _entityGold;
        private Entity _entityCam;
        private Entity _text;
       

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _entityGold = GetSingletonEntity<GoldScoreTagComponent>();
            _entityCam = GetSingletonEntity<CameraTagComponent>();
            _text = GetSingletonEntity<ScoreTagComponent>();
            TextLayout.SetEntityTextRendererString(EntityManager, _text,GetSingleton<GameDataComponent>().Score.ToString());
        }


        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            NonUniformScale goldUiScale = EntityManager.GetComponentData<NonUniformScale>(_entityGold);
            float3 goldUiPosition = EntityManager.GetComponentData<LocalToWorld>(_entityGold).Position;
            float3 posCam = EntityManager.GetComponentData<Translation>(_entityCam).Value;
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var score = new NativeQueue<bool>(Allocator.TempJob);
            JobHandle jobHandle = Entities
                .WithAll<GoldToScoreComponent>()
                .WithBurst()
                .ForEach((Entity entity, DynamicBuffer<Child> children, ref Translation position, ref NonUniformScale scale, ref GoldToScoreComponent goldToScoreComponent ) =>
                {
                    goldToScoreComponent.Lerp += deltaTime;
                    float3 t3CameraFixPosition = goldToScoreComponent.GoldFixPosition + posCam - goldToScoreComponent.CameraFixPosition;
                    position.Value = math.lerp(t3CameraFixPosition,goldUiPosition,goldToScoreComponent.Lerp);
                    scale.Value = math.lerp(goldToScoreComponent.StartScale, goldUiScale.Value, goldToScoreComponent.Lerp);
                    if (goldToScoreComponent.Lerp >= 1)
                    {
                         commandBuffer.DestroyEntity(entity);
                         foreach (Child child in children)
                             commandBuffer.DestroyEntity(child.Value);
                         
                         score.Enqueue(true);
                    }
                }).Schedule(Dependency);
            
            jobHandle.Complete();
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();

            if (score.Count <= 0) return;
            
          
            Entities.
                WithAll<GameDataComponent>().
                ForEach((ref GameDataComponent gameData) =>
                {
                    gameData.Score += score.Count;
                }).Run();
            TextLayout.SetEntityTextRendererString(EntityManager, _text,GetSingleton<GameDataComponent>().Score.ToString());
            score.Clear();
        }
    }
}