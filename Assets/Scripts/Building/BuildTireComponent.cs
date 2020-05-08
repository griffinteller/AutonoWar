using UnityEngine;

namespace Building
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
            
            return Vector3.left;
            
        }

        public override float GetRadius()
        {

            return collider.size.x / 2;

        }

        public override RobotPart GetRobotPartDescription(Vector3 robotCenterOfMass)
        {

            return new RobotPart(transform, robotCenterOfMass, PartType.Tire, partName, mass, tireName);

        }

        public override void LoadInfoFromPartDescription(RobotPart part)
        {
            base.LoadInfoFromPartDescription(part);
            tireName = part.name;
        }
    }
}