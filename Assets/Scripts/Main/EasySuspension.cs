﻿using UnityEngine;

using UnityPhysics = UnityEngine.Physics;
//[ExecuteInEditMode()]
public class EasySuspension : MonoBehaviour
{
    private float _mass;

    [Range(0, 3)] public float dampingRatio = 0.8f;

    [Range(-1, 1)] public float forceShift = 0.03f;

    [Range(0, 20)] public float naturalFrequency = 10;

    public bool setSuspensionDistance = true;

    private void Start()
    {
	    // work out the stiffness and damper parameters based on the better spring model
        foreach (var wc in GetComponentsInChildren<WheelCollider>())
        {
            var spring = wc.suspensionSpring;

            spring.spring = Mathf.Pow(Mathf.Sqrt(wc.sprungMass) * naturalFrequency, 2);
            spring.damper = 2 * dampingRatio * Mathf.Sqrt(spring.spring * wc.sprungMass);

            wc.suspensionSpring = spring;

            var wheelRelativeBody = transform.InverseTransformPoint(wc.transform.position);
            var distance = GetComponent<Rigidbody>().centerOfMass.y - wheelRelativeBody.y + wc.radius;

            wc.forceAppPointDistance = distance - forceShift;

            // the following line makes sure the spring force at maximum droop is exactly zero
            if (spring.targetPosition > 0 && setSuspensionDistance)
                wc.suspensionDistance =
                    wc.sprungMass * UnityPhysics.gravity.magnitude / (spring.targetPosition * spring.spring);
        }
    }

// uncomment OnGUI to observe how parameters change

/*
	public void OnGUI()
	{
		foreach (WheelCollider wc in GetComponentsInChildren<WheelCollider>()) {
			GUILayout.Label (string.Format("{0} sprung: {1}, k: {2}, d: {3}", wc.name, wc.sprungMass, wc.suspensionSpring.spring, wc.suspensionSpring.damper));
		}

		var rb = GetComponent<Rigidbody> ();

		GUILayout.Label ("Inertia: " + rb.inertiaTensor);
		GUILayout.Label ("Mass: " + rb.mass);
		GUILayout.Label ("Center: " + rb.centerOfMass);
	}
*/
}