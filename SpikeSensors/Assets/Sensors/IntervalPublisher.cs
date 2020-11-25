using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntervalPublisher : MonoBehaviour
{
    public ROSConnection ros;
    public float publishIntervalSeconds = 0.1f;
    float nextPublishTimestamp;

    void Start()
    {
        nextPublishTimestamp = Time.time + publishIntervalSeconds;
        StartCoroutine(PublishLoop());
    }

    IEnumerator PublishLoop()
    {
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
