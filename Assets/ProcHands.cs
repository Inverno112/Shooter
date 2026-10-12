using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ProcHands : MonoBehaviour
{
    [Header("Rig Targets")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Target Destinations")]
    [SerializeField] private Transform leftDestination;
    [SerializeField] private Transform rightDestination;

    [Header("Settings")]
    [SerializeField] private bool followRotation = true;
    [SerializeField]TwoBoneIKConstraint ikLeft;
    [SerializeField]TwoBoneIKConstraint ikRight;
    

    private void FixedUpdate()
    {
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        if (leftHandTarget != null && leftDestination != null)
        {
            leftHandTarget.position = leftDestination.position;
            if (followRotation)
                leftHandTarget.rotation = leftDestination.rotation;
        }

        if (rightHandTarget != null && rightDestination != null)
        {
            rightHandTarget.position = rightDestination.position;
            if (followRotation)
                rightHandTarget.rotation = rightDestination.rotation;
        }
    }

    public void SetLeftDestination(Transform destination)
    {
        leftDestination = destination;       
    }

    public void SetRightDestination(Transform destination)
    {
        rightDestination = destination;
    }

    public void SetLeftDestinationPosition(Vector3 position, Quaternion rotation)
    {
        if (leftHandTarget == null)
            return;

        leftHandTarget.position = position;
        if (followRotation)
            leftHandTarget.rotation = rotation;
    }

    public void SetRightDestinationPosition(Vector3 position, Quaternion rotation)
    {
        if (rightHandTarget == null)
            return;

        rightHandTarget.position = position;
        if (followRotation)
            rightHandTarget.rotation = rotation;
    }

    internal void SetLeftWeight(float v)
    {
        ikLeft.weight=(v);
    }
    internal void SetRightWeight(float v)
    {
        ikRight.weight=(v);
    }
}
