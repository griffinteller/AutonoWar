using UnityEngine;

namespace GameTerrain.Flat
{
    public class FlatTerrainQuad : TerrainQuad
    {
        public float Width;

        public override Vector3[] BaseVertices
        {
            get
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
        }

        public override Vector3[] BaseNormals
        {
            get
            {
                var normals = new Vector3[TotalNumberOfVertices];
                
                for (var i = 0; i < normals.Length; i++)
                    normals[i] = Vector3.up;

                return normals;
            }
        }

        public override Vector2[] Uv
        {
            get
            {
                var uv = new Vector2[TotalNumberOfVertices];
                var verticesPerSide = VerticesPerSide;
                
                for (var row = 0; row < verticesPerSide; row++)
                for (var col = 0; col < verticesPerSide; col++)
                    uv[row * verticesPerSide + col] = new Vector2(
                        (float) col / verticesPerSide,
                        1 - row / (float) verticesPerSide);

                return uv;
            }
        }

        public FlatTerrainQuad(float width, byte degree, TriangleCache triangleCache, float[][] heightmap = null)
        {
            Width = width;
            Degree = degree;
            Heightmap = heightmap;
            TriangleCacheObject = triangleCache;
            
            if (heightmap == null)
                InitializeHeightmap();
            
            GenerateMesh();
        }
    }
}