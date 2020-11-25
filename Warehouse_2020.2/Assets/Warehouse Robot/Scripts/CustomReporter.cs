using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Perception.GroundTruth;

[RequireComponent(typeof(PerceptionCamera))]
public class CustomReporter : MonoBehaviour
{
    public RobotAgent camBot;                       // Key robot with attached Perception Camera

    MetricDefinition cornerMetricDefinition;
    MetricDefinition rotationMetricDefinition;
    MetricDefinition velocityMetricDefinition;
    MetricDefinition targetMetricDefinition;
    SensorHandle cameraSensorHandle;

    public void Start()
    {
        //Metrics and annotations are registered up-front
        cornerMetricDefinition = DatasetCapture.RegisterMetricDefinition(
            "Next corner",
            "The next node in the NavMeshAgent path",
            Guid.NewGuid());
        rotationMetricDefinition = DatasetCapture.RegisterMetricDefinition(
            "Current rotation",
            "The current rotation of this bot",
            Guid.NewGuid());
        velocityMetricDefinition = DatasetCapture.RegisterMetricDefinition(
            "Current velocity",
            "The current velocity of this bot",
            Guid.NewGuid());
        targetMetricDefinition = DatasetCapture.RegisterMetricDefinition(
            "Current target",
            "The current target location of this bot",
            Guid.NewGuid());
    }

    public void Update()
    {
        // Report metrics if bot exists
        if (camBot != null){
            // Report next path corner in Vector3
            if (camBot.m_path.corners.Length > 1){
                var corner = camBot.m_path.corners[1];
                DatasetCapture.ReportMetric(cornerMetricDefinition,
                    $@"[{{ ""x"": {corner.x}, ""y"": {corner.y}, ""z"": {corner.z} }}]");
            } 
            // Report current rotation in Vector3
            var rot = camBot.transform.localEulerAngles;
            DatasetCapture.ReportMetric(rotationMetricDefinition,
                $@"[{{ ""x"": {rot.x}, ""y"": {rot.y}, ""z"": {rot.z} }}]");
            // Report current velocity in Vector3
            var vel = camBot.GetComponent<NavMeshAgent>().velocity;
            DatasetCapture.ReportMetric(velocityMetricDefinition,
                $@"[{{ ""x"": {vel.x}, ""y"": {vel.y}, ""z"": {vel.z} }}]");

            var target = camBot.GetComponent<NavMeshAgent>().destination;
            DatasetCapture.ReportMetric(targetMetricDefinition,
                $@"[{{ ""x"": {target.x}, ""y"": {target.y}, ""z"": {target.z} }}]");
        }
    }
}