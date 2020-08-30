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
            var totalMin = GetMinWorldPointOfChildMeshes(gameObject);
            var totalMax = GetMaxWorldPointOfChildMeshes(gameObject);

            return totalMax - totalMin;
        }

        public static Vector3 GetMinWorldPointOfChildMeshes(GameObject gameObject)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            var totalMin = GetMinWorldPointOfMesh(meshFilters[0]);

            for (var i = 1; i < meshFilters.Length; i++)
            {
                var min = GetMinWorldPointOfMesh(meshFilters[i]);

                totalMin = Vector3.Min(totalMin, min); ;
            }

            return totalMin;
        }
        
        public static Vector3 GetMaxWorldPointOfChildMeshes(GameObject gameObject)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            var totalMax = GetMaxWorldPointOfMesh(meshFilters[0]);

            for (var i = 1; i < meshFilters.Length; i++)
            {
                var max = GetMaxWorldPointOfMesh(meshFilters[i]);

                totalMax = Vector3.Max(totalMax, max); ;
            }

            return totalMax;
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