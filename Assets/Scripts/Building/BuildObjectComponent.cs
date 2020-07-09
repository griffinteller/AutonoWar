using UnityEngine;

namespace Building
{
    public abstract class BuildObjectComponent : MonoBehaviour
    {
        public int mass;
        public string partName;

        public bool removable = true;

        public abstract ConnectionDescription GetConnection(Vector3 hitPointWorldSpace);

        public abstract Vector3 GetConnectingFaceOutwardsDirection();

        public abstract float GetRadius();

        public abstract RobotPart GetRobotPartDescription(Vector3 robotCenterOfMass);

        public virtual void LoadInfoFromPartDescription(RobotPart part)
        {
        }
    }
}