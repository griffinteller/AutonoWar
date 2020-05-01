using UnityEngine;

namespace Main.Building
{
    public class BuildTireComponent : BuildObjectComponent
    {

        public new BoxCollider collider;

        public string tireName;

        public override ConnectionDescription GetConnection(Vector3 hitPointWorldSpace)
        {
            return null;
        }

        public override Vector3 GetConnectingFaceOutwardsDirection()
        {
            
            return Vector3.back;
            
        }

        public override float GetRadius()
        {

            return collider.size.z / 2;

        }
    }
}