using System;
using System.Collections.Generic;
using System.Linq;
using Runtime;
using Runtime.Components;
using Runtime.Components.Buffer;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    
    
    
    [DisallowMultipleComponent]
    public class MainAuthoring : MonoBehaviour, IConvertGameObjectToEntity,IDeclareReferencedPrefabs
    {

        public GameObject[] levels;
        
        [SerializeField]
        public List<ColorMaterial> colors = new List<ColorMaterial>();

        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<LevelBuffer>(entity);
            foreach (var t in levels)
            {
                var levelEntity = conversionSystem.GetPrimaryEntity(t);
                buffer.Add(new LevelBuffer(){Entity = levelEntity});
            }


            var bufferColor = dstManager.AddBuffer<ColorsBuffer>(entity);
            var colorSorted = colors.OrderBy(material => (int)material.colorsType).ToArray();
            foreach (var colorMaterial in colorSorted)
            {
                var levelEntity = conversionSystem.GetPrimaryEntity(colorMaterial.material);
                bufferColor.Add(new ColorsBuffer{Entity = levelEntity});
            }
        }


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(levels);
        }
    }
    
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))] 
    public class DeclareCellMaterialReference : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((MainAuthoring mgr) =>
            {
                foreach (ColorMaterial colorMaterial in mgr.colors) 
                    DeclareReferencedAsset(colorMaterial.material);
            });
        }
    }
    
    
    [Serializable]
    public struct ColorMaterial
    {
        public ColorsType colorsType;
        public Material material;
    }
}
