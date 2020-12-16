using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationPicker : MonoBehaviour
{
    private Vector3 _offset;
    private float mZCoord;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 GetMouseWorldPos(){
        var mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    
    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        _offset = transform.position - GetMouseWorldPos();
        // var screenPoint = Camera.main.WorldToScreenPoint(Input.mousePosition);
        // _offset = Input.mousePosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
        // Debug.Log($"on mouse down location picker {_offset}");
    }
    
    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + _offset;
    }
}
