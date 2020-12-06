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
        public float majorAngle;
        
        protected override TerrainQuad GenerateQuad(TriangleCache cache)
        {
            TorusTerrainQuad quad = new TorusTerrainQuad(
                degree, majorRadius, minorRadius, majorArc, 
                minorArc, majorAngle, minorAngle, cache);
            quad.Generate();
            return quad;
        }
    }
}