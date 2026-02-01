using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player
    public Vector3 offset;   // Offset between the player and the camera
    public float smoothSpeed = 0.125f; // Smooth movement speed

    void LateUpdate()
    {
        // Calculate the target position (ignoring player's scale changes)
        Vector3 targetPosition = player.position + offset;

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }
}
