using System;
using UnityEngine;
using Utility;

namespace GameTerrain.Torus
{
    public class TorusTerrainController : TerrainController
    {
        [SerializeField] private float majorRadius;
        [SerializeField] private float minorRadius;
        [SerializeField] private byte quadDegree;
        [SerializeField] private int minorQuadDensity;
        [SerializeField] private int majorQuadDensity;

        public Transform terrainQualityRefrencer; // the object that decides the LOD (usually main camera)

        public float MajorRadius => majorRadius;
        public float MinorRadius => minorRadius;
        public byte QuadDegree => quadDegree;
        public int MinorQuadDensity => minorQuadDensity;
        public int MajorQuadDensity => majorQuadDensity;
        
        

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
                renderer.degree = quadDegree;
                renderer.majorArc = _majorQuadArc;
                renderer.minorArc = _minorQuadArc;
                renderer.majorRadius = majorRadius;
                renderer.minorRadius = minorRadius;
                renderer.majorAngle = quadCol * _majorQuadArc;
                renderer.minorAngle = quadRow * _minorQuadArc;

                renderer.GetComponent<MeshRenderer>().material = material;
            }
        }
    }
}