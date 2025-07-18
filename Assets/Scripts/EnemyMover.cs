using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f; // Units per second
    public Vector3 moveDirection = Vector3.down; // Default: move down

    [Header("Stop Settings")]
    public float stopY = 3f; // The Y position where the enemy should stop

    private bool stopped = false;

    void Update()
    {
        if (!stopped)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

            // Stop when reaching or passing the stopY position (for downward movement)
            if (moveDirection.y < 0 && transform.position.y <= stopY)
            {
                stopped = true;
            }
            // If moving up, use: if (moveDirection.y > 0 && transform.position.y >= stopY)
        }
    }
} 