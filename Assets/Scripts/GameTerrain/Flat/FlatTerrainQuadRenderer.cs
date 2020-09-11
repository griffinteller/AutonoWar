using UnityEngine;

namespace GameTerrain.Flat
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class FlatTerrainQuadRenderer : TerrainQuadRenderer
    {
        public byte degree = 3;
        public float width = 5;
        public Vector2 randomRange;
        
        protected override TerrainQuad GenerateQuad(TriangleCache cache)
        {
            var heightmap = TerrainQuad.GetRandomHeights(degree, (randomRange.x, randomRange.y));
            
            var quad = new FlatTerrainQuad(width, degree, cache, heightmap);
            quad.Generate();
            
            return quad;
        }
    }
}