using System;
using UnityEngine;
using Utility;

namespace GameTerrain.Torus
{
    public class TorusTerrainController : TerrainController
    {
        [SerializeField] private float majorRadius;
        [SerializeField] private float minorRadius;
        [SerializeField] private int minorQuadDensity;
        [SerializeField] private int majorQuadDensity;

        public Transform terrainQualityRefrencer; // the object that decides the LOD (usually main camera)

        public float MajorRadius => majorRadius;
        public float MinorRadius => minorRadius;
        public int MinorQuadDensity => minorQuadDensity;
        public int MajorQuadDensity => majorQuadDensity;
        
        public float[] lodDistances; // anything >= to will have corresponding degree
        public byte[] lodDegrees;

        private float _minorQuadArc;
        private float _majorQuadArc;

        private TorusTerrainQuadRenderer[][] _renderers;

        public void Start()
        {
            _renderers = ArrayUtility.GenerateJaggedMatrix<TorusTerrainQuadRenderer>(
                majorQuadDensity, minorQuadDensity);

            _majorQuadArc = 360f / majorQuadDensity;
            _minorQuadArc = 360f / minorQuadDensity;

            GenerateQuadRenderers();
        }

        public void Update()
        {
            foreach (TorusTerrainQuadRenderer[] longitude in _renderers)
            foreach (TorusTerrainQuadRenderer renderer in longitude)
            {
                float distance = Vector3.Distance(
                    terrainQualityRefrencer.transform.position,
                    transform.TransformPoint(renderer.LocalCenter));

                byte degree = GetDegreeFromDistance(distance);

                if (degree == renderer.degree)
                    continue;

                renderer.degree = degree;
                renderer.RefreshMesh();
            }
        }

        private byte GetDegreeFromDistance(float distance)
        {
            for (int i = 0; i < lodDistances.Length - 1; i++)
                if (distance > lodDistances[i] && distance < lodDistances[i + 1])
                    return lodDegrees[i];

            return lodDegrees[lodDegrees.Length - 1];
        }

        private void GenerateQuadRenderers()
        {
            for (int quadCol = 0; quadCol < majorQuadDensity; quadCol++) // in general, the convention will be to put
                                                                         // column first because it is the major axis
            for (int quadRow = 0; quadRow < minorQuadDensity; quadRow++)
            {
                float longitude = quadCol * _majorQuadArc;
                float latitude = quadRow * _minorQuadArc;
                
                GameObject rendererObj = new GameObject(
                    "Renderer " + quadCol + "," + quadRow, 
                    typeof(TorusTerrainQuadRenderer));

                Transform rendererTransform = rendererObj.transform;
                rendererTransform.SetParent(transform);
                rendererTransform.localPosition = Vector3.zero;
                rendererTransform.localRotation = Quaternion.identity;

                TorusTerrainQuadRenderer renderer = rendererObj.GetComponent<TorusTerrainQuadRenderer>();
                renderer.degree = lodDegrees[lodDegrees.Length - 1];
                renderer.majorArc = _majorQuadArc;
                renderer.minorArc = _minorQuadArc;
                renderer.majorRadius = majorRadius;
                renderer.minorRadius = minorRadius;
                renderer.majorAngle = quadCol * _majorQuadArc;
                renderer.minorAngle = quadRow * _minorQuadArc;

                renderer.GetComponent<MeshRenderer>().material = material;

                _renderers[quadCol][quadRow] = renderer;
            }
        }
    }
}