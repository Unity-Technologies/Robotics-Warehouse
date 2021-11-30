using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WarehouseResize : MonoBehaviour
{
    public bool hasResized = false;
    public bool needsMoveFix = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!hasResized)
        {
            var numChildren = transform.childCount;
            for (int x = 0; x < numChildren; x++)
            {
                var child = transform.GetChild(x);
                child.position /= 100;
                child.localScale = Vector3.one;
                if (child.gameObject.TryGetComponent<BoxCollider>(out var boxCollider))
                {
                    boxCollider.center /= 100;
                    boxCollider.size /= 100;
                }
            }

            hasResized = true;
        }
        else if (needsMoveFix)
        {
            var numChildren = transform.childCount;
            for (int x = 0; x < numChildren; x++)
            {
                var child = transform.GetChild(x);
                child.position /= 10000;
            }

            needsMoveFix = false;
        }
    }

}
