using Runtime.Components;
using Runtime.Components.Buffer;
using Unity.Entities;
using MeshRenderer = Unity.Tiny.Rendering.MeshRenderer;
using SkinnedMeshRenderer = Unity.Tiny.Rendering.SkinnedMeshRenderer;

namespace Runtime.Systems
{
    

    public class ChangePlayerColorSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ChangePlayerColorComponent>();
        }
        
        protected override void OnUpdate()
        {
            ColorsType colorsType = GetSingleton<ChangePlayerColorComponent>().ColorsType;
            Entity entityMaterial = GetMaterial(colorsType);
            
            
            SetPlayerColor(entityMaterial);
            SetBoxColor(entityMaterial,colorsType);
            SetDynamicBoxColor(entityMaterial,colorsType);
            SetGameDataColor(colorsType);

            Entity cSingletonEntity = GetSingletonEntity<ChangePlayerColorComponent>();
            EntityManager.RemoveComponent<ChangePlayerColorComponent>(cSingletonEntity);
        }


        private Entity GetMaterial(ColorsType colorsType)
        {
            Entity mainEntity = GetSingletonEntity<GameDataComponent>();
            var buffer = GetBuffer<ColorsBuffer>(mainEntity);
            return buffer[(int) colorsType].Entity;
        }
        private void SetGameDataColor(ColorsType colorsType)
        {
            Entities.WithAll<GameDataComponent>()
                .WithBurst()
                .ForEach((ref GameDataComponent gameData) =>
                {
                    gameData.Color = colorsType;
                }).Schedule();
        }
        private void SetBoxColor(Entity material, ColorsType colorsType)
        {
               Entities.WithAll<ColorBoxTagComponent, CollidedTagComponent>().
                WithBurst().
                ForEach((ref ColorBoxComponent colorBoxComponent, ref MeshRenderer meshRenderer) =>
                {
                    colorBoxComponent.ColorsType = colorsType;
                    meshRenderer.material = material;
                }).Schedule();
        }
        private void SetDynamicBoxColor(Entity material, ColorsType colorsType)
        {
            Entities.WithAll<DynamicColorBoxTagComponent>().
                WithBurst().
                ForEach((ref ColorBoxComponent colorBoxComponent, ref MeshRenderer meshRenderer) =>
                {
                    colorBoxComponent.ColorsType = colorsType;
                    meshRenderer.material = material;
                }).Schedule();
        }
        private void SetPlayerColor(Entity material)
        {
            Entities.WithAll<PlayerMaterialTagComponent>()
                .WithBurst()
                .ForEach((ref SkinnedMeshRenderer meshRenderer) =>
            {
                meshRenderer.material = material;
            }).Schedule();
        }
    }
}