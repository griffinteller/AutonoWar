using UnityEngine;

namespace GameTerrain.Flat
{
    public class FlatTerrainQuad : TerrainQuad
    {
        public float Width;

        public override float VertexDistance => Width / (VerticesPerSide - 1);

        public FlatTerrainQuad(float width, byte degree, TriangleCache triangleCache, float[][] heightmap = null)
        {
            Width = width;
            Degree = degree;
            Heightmap = heightmap;
            TriangleCacheObject = triangleCache;
            
            if (heightmap == null)
                InitializeHeightmap();
        }

        protected override Vector3[] GenerateBaseVertices()
        {
            var verts = new Vector3[TotalNumberOfVertices];
            var verticesPerSide = VerticesPerSide;
            var distanceBetweenVerts = Width / verticesPerSide;
                
            for (var row = 0; row < verticesPerSide; row++)
            for (var col = 0; col < verticesPerSide; col++)
                verts[row * verticesPerSide + col] = new Vector3(
                    -Width / 2 + distanceBetweenVerts * col,
                    0,
                    Width / 2 + distanceBetweenVerts * row);

            return verts;
        }

        protected override Vector3[] GenerateBaseNormals()
        {
            var normals = new Vector3[TotalNumberOfVertices];
                
            for (var i = 0; i < normals.Length; i++)
                normals[i] = Vector3.up;

            return normals;
        }

        protected override Vector2[] GenerateUv()
        {
            return GenerateStandardUv(VerticesPerSide);
        }
    }
}