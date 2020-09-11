using UnityEngine;

namespace GameTerrain.Torus
{
    public class TorusTerrainQuadRenderer : TerrainQuadRenderer
    {
        public byte degree;
        public float majorRadius;
        public float minorRadius;
        public float majorArc; // degrees
        public float minorArc;
        public float minorAngle; // 0 is outside, 180 is inside
        
        protected override TerrainQuad GenerateQuad(TriangleCache cache)
        {
            var quad = new TorusTerrainQuad(degree, majorRadius, minorRadius, majorArc, minorArc, minorAngle, cache);
            quad.Generate();

            return quad;
        }
    }
}