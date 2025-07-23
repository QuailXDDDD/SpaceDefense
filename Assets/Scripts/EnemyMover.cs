using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.down;

    [Header("Stop Settings")]
    public float stopY = 3f;

    private bool stopped = false;

    void Update()
    {
        if (!stopped)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

            if (moveDirection.y < 0 && transform.position.y <= stopY)
            {
                stopped = true;
            }
        }
    }
} 