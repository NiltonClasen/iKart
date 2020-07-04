using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    /// Alvo
    public Transform target;

    /// Posição do deslocamento
    public Vector3 offsetPosition;

    public float damp = 0.2f;

    public void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 newPosition = target.TransformPoint(offsetPosition);
        transform.position = Vector3.Lerp(transform.position, newPosition, damp);
        transform.LookAt(target);
    }
}
