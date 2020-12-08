using UnityEngine;

namespace GameTerrain.Torus
{
    public class TorusTerrainQuad : TerrainQuad
    {
        public float MajorRadius;
        public float MinorRadius;
        public float MajorArc; // degrees
        public float MinorArc;
        public float MinorAngle; // 0 is outside, 180 is inside
        public float MajorAngle;

        public override float VertexDistance => MajorVertexArc;
        public float MajorVertexArc => MajorArc / (VerticesPerSide - 1);
        public float MinorVertexArc => MinorArc / (VerticesPerSide - 1);

        protected override Vector3[] GenerateBaseVertices()
        {
            int verticesPerSide = VerticesPerSide;
            float majorVertexArc = MajorVertexArc;
            float minorVertexArc = MinorVertexArc;
            Vector3[] result = new Vector3[TotalNumberOfVertices];

            int i = 0;
            for (int row = 0; row < verticesPerSide; row++)
            {
                float rowAngle = MinorAngle + (-row + verticesPerSide / 2) * minorVertexArc;
                float rowRadius = MajorRadius + MinorRadius * Mathf.Cos(Mathf.Deg2Rad * rowAngle);
                float rowHeight = MinorRadius * Mathf.Sin(Mathf.Deg2Rad * rowAngle);
                for (int col = 0; col < verticesPerSide; col++)
                {
                    float colAngle = MajorAngle + (col - verticesPerSide / 2) * majorVertexArc;
                    result[i].x = rowRadius * Mathf.Cos(Mathf.Deg2Rad * colAngle);
                    result[i].y = rowHeight;
                    result[i].z = rowRadius * Mathf.Sin(Mathf.Deg2Rad * colAngle);

                    i++;
                }
            }

            return result;
        }

        protected override Vector3[] GenerateBaseNormals()
        {
            int verticesPerSide = VerticesPerSide;
            float majorVertexArc = MajorVertexArc;
            float minorVertexArc = MinorVertexArc;
            Vector3[] result = new Vector3[TotalNumberOfVertices];

            int i = 0;
            for (int row = 0; row < verticesPerSide; row++)
            {
                float rowAngle = MinorAngle + (row - verticesPerSide / 2) * minorVertexArc;
                for (int col = 0; col < verticesPerSide; col++)
                {
                    float colAngle = (-col + verticesPerSide / 2) * majorVertexArc;
                    result[i] = Quaternion.Euler(0, -colAngle, 0)
                                * Quaternion.Euler(0, 0, rowAngle)
                                * Vector3.right;
                    i++;
                }
            }

            return result;
        }

        protected override Vector2[] GenerateUv()
        {
            return GenerateStandardUv(VerticesPerSide);
        }

        public TorusTerrainQuad(byte degree, float majorRadius, float minorRadius, float majorArc, float minorArc,  
            float majorAngle, float minorAngle, TriangleCache cache, byte mipmapMask = 0, float[][] heightmap = null)
        {
            Degree = degree;
            MajorRadius = majorRadius;
            MinorRadius = minorRadius;
            MajorArc = majorArc;
            MinorArc = minorArc;
            MinorAngle = minorAngle;
            MajorAngle = majorAngle;
            MipmapMask = mipmapMask;
            TriangleCacheObject = cache;

            if (heightmap == null)
                InitializeHeightmap();
            else
                Heightmap = heightmap;
        }
    }
}