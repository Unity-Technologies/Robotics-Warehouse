using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRobotCollision : MonoBehaviour
{
    public List<string> collision = new List<string>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Entered goal: {other} of {other.transform.root}");
        this.collision.Add(other.gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        this.collision.Remove(other.gameObject.name);
    }
}