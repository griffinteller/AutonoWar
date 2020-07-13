using System;
using UnityEngine;

namespace Utility
{
    public static class MeshUtility
    {
        public static Mesh GetCombinedMeshes(MeshFilter[] meshFilters, Transform parent)
        {
            var combineInstances = new CombineInstance[meshFilters.Length];
            for (var i = 0; i < meshFilters.Length; i++)
            {
                combineInstances[i] = new CombineInstance();
                combineInstances[i].mesh = meshFilters[i].sharedMesh;

                var objectTransform = meshFilters[i].transform;

                var positionRelativeToParent =
                    parent.InverseTransformPoint(objectTransform.position);
                var rotationRelativeToParent = Quaternion.Inverse(parent.rotation) * objectTransform.rotation;
                var scaleRelativeToParent = objectTransform.localScale;

                var matrix = Matrix4x4.TRS(
                    positionRelativeToParent,
                    rotationRelativeToParent,
                    scaleRelativeToParent);

                combineInstances[i].transform = matrix;
            }

            var result = new Mesh();
            result.CombineMeshes(combineInstances);
            return result;
        }
        
        public static Vector3 GetCompoundMeshSize(GameObject gameObject)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            
            if (meshFilters.Length == 0)
                throw new ArgumentException("GameObject must have at least one mesh filter");

            var totalMin = GetMinWorldPointOfMesh(meshFilters[0]);
            var totalMax = GetMaxWorldPointOfMesh(meshFilters[0]);

            for (var i = 1; i < meshFilters.Length; i++)
            {
                var min = GetMinWorldPointOfMesh(meshFilters[i]);
                var max = GetMaxWorldPointOfMesh(meshFilters[i]);

                totalMin = Vector3.Min(totalMin, min);
                totalMax = Vector3.Max(totalMax, max);
            }

            return totalMax - totalMin;
        }
        
        private static Vector3 GetMinWorldPointOfMesh(MeshFilter meshFilter)
        {
            var localMin = meshFilter.sharedMesh.bounds.min;
            return meshFilter.transform.TransformPoint(localMin);
        }
        
        private static Vector3 GetMaxWorldPointOfMesh(MeshFilter meshFilter)
        {
            var localMax = meshFilter.sharedMesh.bounds.max;
            return meshFilter.transform.TransformPoint(localMax);
        }
    }
}