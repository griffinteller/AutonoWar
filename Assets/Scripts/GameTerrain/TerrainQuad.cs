using UnityEngine;

namespace GameTerrain
{
    public abstract class TerrainQuad
    {
        /*
         * Degree - 1 for 3x3, 2 for 5x5, etc (side is 2^degree + 1) (minmum 1)
         */

        public byte Degree { get; set; }
        public byte MipmapMask { get; set; }
        public float[][] Heightmap;

        public Vector3[] BaseVertices { get; private set; }
        public Vector3[] BaseNormals { get; private set; }
        public Vector2[] Uv { get; private set; }

        public abstract float VertexDistance { get; }

        public Mesh Mesh { get; } = new Mesh();

        public int VerticesPerSide => (int) (Mathf.Pow(2, Degree) + 0.5f) + 1;

        public int TotalNumberOfVertices
        {
            get
            {
                int verticesPerSide = VerticesPerSide;
                return verticesPerSide * verticesPerSide;
            }
        }

        protected TriangleCache TriangleCacheObject;

        protected abstract Vector3[] GenerateBaseVertices();
        protected abstract Vector3[] GenerateBaseNormals();
        protected abstract Vector2[] GenerateUv();

        public void Generate()
        {
            BaseVertices = GenerateBaseVertices();
            BaseNormals = GenerateBaseNormals();
            Uv = GenerateUv();
            RefreshMesh();
        }

        protected void InitializeHeightmap()
        {
            Heightmap = new float[VerticesPerSide][];
            
            for (int i = 0; i < Heightmap.Length; i++)
                Heightmap[i] = new float[Heightmap.Length];
        }

        public static float[][] GetRandomHeights(int degree, (float, float) range)
        {
            int verticesOnASide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;

            var result = new float[verticesOnASide][];
            for (var i = 0; i < verticesOnASide; i++)
            {
                result[i] = new float[verticesOnASide];
                for (var j = 0; j < verticesOnASide; j++)
                    result[i][j] = Random.value * (range.Item2 - range.Item1) + range.Item1;
            }

            return result;
        }

        public void RefreshMesh()
        {
            var totalNumberOfVertices = TotalNumberOfVertices;

            var verts = new Vector3[totalNumberOfVertices];

            var i = 0;
            foreach (var row in Heightmap)
            foreach (var height in row)
            {
                verts[i] = height * BaseNormals[i] + BaseVertices[i];
                i++;
            }
            
            Mesh.Clear();
            Mesh.vertices = verts;
            Mesh.triangles = TriangleCacheObject.Cache[Degree][MipmapMask];
            Mesh.uv = Uv;
            Mesh.RecalculateNormals();
        }
        
        protected static Vector2[] GenerateStandardUv(int verticesPerSide)
        {
            var uv = new Vector2[verticesPerSide * verticesPerSide];

            for (var row = 0; row < verticesPerSide; row++)
            for (var col = 0; col < verticesPerSide; col++)
                uv[row * verticesPerSide + col] = new Vector2(
                    (float) col / verticesPerSide,
                    1 - row / (float) verticesPerSide);

            return uv;
        }
    }
}