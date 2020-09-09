using UnityEngine;

namespace GameTerrain
{
    public abstract class TerrainQuad
    {
        /*
         * Degree - 0 for 2x2, 1 for 3x3, 2 for 5x5, etc (side is 2^degree + 1)
         */

        public byte Degree { get; set; }
        public byte MipmapMask { get; set; }
        public float[][] Heightmap;
        
        public abstract Vector3[] BaseVertices { get; }
        public abstract Vector3[] BaseNormals { get; }
        public abstract Vector2[] Uv { get; }

        private Mesh _mesh = new Mesh();
        public Mesh Mesh => _mesh;

        public int VerticesPerSide => (int) (Mathf.Pow(2, Degree) + 0.5f) + 1;

        public int TotalNumberOfVertices
        {
            get
            {
                var verticesPerSide = VerticesPerSide;
                return verticesPerSide * verticesPerSide;
            }
        }

        protected TriangleCache TriangleCacheObject;

        protected void GenerateMesh()
        {
            var totalNumberOfVertices = TotalNumberOfVertices;
            var normals = BaseNormals;
            var baseVertices = BaseVertices;
            
            var verts = new Vector3[totalNumberOfVertices];

            var i = 0;
            foreach (var row in Heightmap)
            foreach (var height in row)
            {
                verts[i] = height * normals[i] + baseVertices[i];
                i++;
            }
            
            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.triangles = TriangleCacheObject.Cache[Degree][MipmapMask];
            _mesh.normals = normals;
            _mesh.uv = Uv;
        }

        protected void InitializeHeightmap()
        {
            Heightmap = new float[VerticesPerSide][];
            
            for (var i = 0; i < Heightmap.Length; i++)
                Heightmap[i] = new float[Heightmap.Length];
        }

        public static float[][] GetRandomHeights(int degree, (float, float) range)
        {
            var verticesOnASide = (int) (Mathf.Pow(2, degree) + 0.5f) + 1;

            var result = new float[verticesOnASide][];
            for (var i = 0; i < verticesOnASide; i++)
            {
                result[i] = new float[verticesOnASide];
                for (var j = 0; j < verticesOnASide; j++)
                    result[i][j] = Random.value * (range.Item2 - range.Item1) + range.Item1;
            }

            return result;
        }
    }
}