using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntervalPublisher : MonoBehaviour
{
    public string topic;
    public ROSConnection ros;
    public float publishIntervalSeconds = 0.1f;
    float nextPublishTimestamp;
    protected virtual string RegisterMessageName => "";

    protected virtual void Start()
    {
        if(RegisterMessageName != "")
            ros.RegisterPublisher(topic, RegisterMessageName);
        StartCoroutine(PublishLoop());
    }

    IEnumerator PublishLoop()
    {
        nextPublishTimestamp = Time.time + publishIntervalSeconds;
        while (true)
        {
            yield return new WaitForSeconds(nextPublishTimestamp - Time.time);
            DoPublish();
            nextPublishTimestamp += publishIntervalSeconds;
        }
    }

    public virtual void DoPublish()
    {

    }
}
