using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    private const int BackgroundLayer = 8;

    private Vector3? _mousePosition;


    void Update()
    {
        _mousePosition = null;
    }

    public Vector3 GetMouseDirection()
    {
        if (!_mousePosition.HasValue)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << BackgroundLayer);
            _mousePosition = hit.point;
        }

        var direction = Vector3.ProjectOnPlane(_mousePosition.Value - transform.position, Vector3.up).normalized;
        Debug.Log(_mousePosition.Value);
        Debug.Log(transform.position);
        Debug.Log(direction);
        return direction;
    }
}