using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSGeometry;
using RosMessageTypes.Sensor;

public class ImuSensor : IntervalPublisher
{
    protected override string RegisterMessageName => Imu.RosMessageName;

    public Rigidbody rb;
    Vector3 lastKnownVelocity;
    float lastKnownTime;

    public override void DoPublish()
    {
        Vector3 currentVelocity = rb.velocity;

        if (lastKnownTime != 0)
        {
            Imu msg = new Imu();

            float deltaTime = Time.time - lastKnownTime;
            Vector3 acceleration = (currentVelocity - lastKnownVelocity) / lastKnownTime;
            Vector3 gravity = Vector3.down*9.81f;

            msg.header = new RosMessageTypes.Std.Header();
            msg.linear_acceleration = (acceleration+gravity).To<FLU>();
            msg.orientation = rb.transform.rotation.To<FLU>();
            msg.angular_velocity = rb.angularVelocity.To<FLU>();

            ros.Send(topic, msg);
        }

        lastKnownTime = Time.time;
        lastKnownVelocity = currentVelocity;
    }
}
