using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSGeometry;

public class PoseSensor : IntervalPublisher
{
    protected override string RegisterMessageName => RosMessageTypes.Geometry.Transform.RosMessageName;

    public override void DoPublish()
    {
        ros.Send(topic, transform.To<FLU>());
    }
}
