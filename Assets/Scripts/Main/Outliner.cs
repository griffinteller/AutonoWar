using System;
using System.Threading.Tasks;
using UnityEngine;
using Utility;

namespace Main
{
    public class Outliner : MonoBehaviour
    {

        public float baseWidth = 0.1f;
        public Color color = Color.cyan;
        public float updateInterval = 0.1f;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private MeshFilter[] _childMeshFilters;
        private Transform[] _childMeshTransforms;
        private Material _outlineMaterial;
        private Transform _transformCache;

        private int _colorId;
        private int _outlineWidthId;
        private int _objectWidthId;

        private float _lastUpdate;

        private const string ShaderName = "Unlit/Silhouette";

        private Task _currentDrawTask;

        private static readonly Action<object> DrawAction = outliner =>
        {
            var instance = (Outliner) outliner;

            
        };

        public void Draw()
        {
            
            var combinedMesh = MeshUtility.GetCombinedMeshes(
                _childMeshFilters,
                transform);

            _meshFilter.mesh = combinedMesh;

            _outlineMaterial.SetColor(_colorId, color);
            _outlineMaterial.SetFloat(_outlineWidthId, baseWidth);
            _outlineMaterial.SetFloat(_objectWidthId, combinedMesh.bounds.size.magnitude);

            _meshRenderer.material = _outlineMaterial;
            
        }

        public void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();

            if (!_meshRenderer)
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (!_meshFilter)
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            
            _outlineMaterial = new Material(Shader.Find(ShaderName));
            _colorId = Shader.PropertyToID("_Color");
            _outlineWidthId = Shader.PropertyToID("_OutlineWidth");
            _objectWidthId = Shader.PropertyToID("_ObjectWidth");

            _lastUpdate = -updateInterval;

            _childMeshFilters = MetaUtility.GetComponentsInProperChildren<MeshFilter>(gameObject);
            _childMeshTransforms = new Transform[_childMeshFilters.Length];
            
            _currentDrawTask = Task.Factory.StartNew(DrawAction, this);

        }

        public void Update()
        {
            if (Time.time - _lastUpdate < updateInterval 
                || _currentDrawTask.Status == TaskStatus.Running)
                return;

            _lastUpdate = Time.time;

            _transformCache = transform;
            for (var i = 0; i < _childMeshFilters.Length; i++)
                _childMeshTransforms[i] = _childMeshFilters[i].transform;
            
            
            _currentDrawTask = Task.Factory.StartNew(DrawAction, this);
        }
    }
}