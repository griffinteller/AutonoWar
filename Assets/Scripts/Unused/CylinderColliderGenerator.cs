using System.Linq;
using UnityEngine;

namespace Unused
{
    public class CylinderColliderGenerator : MonoBehaviour
    {
        public int density = 5;
        public PhysicMaterial material;
        public float radius = 1;
        public float width = 1;

        public void BuildCylinderBoxes()
        {
            RemoveChildren();

            var individualWidth = Mathf.Sqrt(
                Mathf.Pow(radius - radius * Mathf.Cos(Mathf.PI / density), 2)
                + Mathf.Pow(radius * Mathf.Sin(Mathf.PI / density), 2));

            var boxSize = new Vector3(width, individualWidth, radius * 2);

            for (var i = 0; i < density; i++)
            {
                var obj = Instantiate(new GameObject(), transform);
                var boxCollider = obj.AddComponent<BoxCollider>();
                boxCollider.size = boxSize;
                boxCollider.transform.localRotation =
                    Quaternion.Euler(i * 180f / density, 0, 0);
                boxCollider.material = material;
            }
        }

        public void BuildCylinderSpheres()
        {
            RemoveChildren();

            for (var i = 0; i < density; i++)
            {
                var obj = Instantiate(new GameObject(), transform);
                var sphereCollider = obj.AddComponent<SphereCollider>();
                sphereCollider.radius = width / 2;
                obj.transform.localPosition = new Vector3(0,
                    (radius - width / 2) * Mathf.Sin(i * 2 * Mathf.PI / density),
                    (radius - width / 2) * Mathf.Cos(i * 2 * Mathf.PI / density));
                sphereCollider.material = material;
            }
        }

        private void RemoveChildren()
        {
            var children = transform.Cast<Transform>().ToList();
            foreach (var child in children) DestroyImmediate(child.gameObject);
        }
    }
}