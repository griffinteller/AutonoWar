using System;
using System.Collections.Generic;
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
            HashSet<TorusTerrainQuadRenderer> needToRefresh = new HashSet<TorusTerrainQuadRenderer>();
            
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
                needToRefresh.Add(renderer);
            }
            
            for (int longitude = 0; longitude < _renderers.Length; longitude++)
            for (int latitude = 0; latitude < _renderers[0].Length; latitude++)
            {
                TorusTerrainQuadRenderer renderer = _renderers[longitude][latitude];
                byte mask = GetMipmapMask(longitude, latitude);
                
                if (renderer.MipmapMask == mask)
                    continue;

                renderer.MipmapMask = mask;
                needToRefresh.Add(renderer);
            }
            
            foreach (TorusTerrainQuadRenderer renderer in needToRefresh)
                renderer.RefreshMesh();
        }

        private byte GetMipmapMask(int longitude, int latitude)
        {
            int longitudes = _renderers.Length;
            int latitudes = _renderers[0].Length;
            
            byte mask = 0;
            int degree = _renderers[longitude][latitude].degree;

            if (_renderers[longitude][(latitude + 1) % latitudes].degree < degree)
                mask |= 1;

            if (_renderers[(longitude + 1) % longitudes][latitude].degree < degree)
                mask |= 2;
            
            if (_renderers[longitude][(latitude - 1 + latitudes) % latitudes].degree < degree) // negative modulos are broken
                mask |= 4;
            
            if (_renderers[(longitude - 1 + longitudes) % longitudes][latitude].degree < degree)
                mask |= 8;

            return mask;
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