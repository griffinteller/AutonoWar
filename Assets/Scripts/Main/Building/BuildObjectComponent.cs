using UnityEngine;

namespace Main.Building
{
    public abstract class BuildObjectComponent : MonoBehaviour
    {

        public bool removable = true;

        public abstract ConnectionDescription GetConnection(Vector3 hitPointWorldSpace);

        public abstract Vector3 GetConnectingFaceOutwardsDirection();

        public abstract float GetRadius();

    }
}