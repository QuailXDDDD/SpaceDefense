using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float destroyDelay = 0.5f; // Adjust this value

    void Start()
    {
        // This will destroy the GameObject after 'destroyDelay' seconds.
        // Set 'destroyDelay' to match the duration of your explosion animation or particle system.
        Destroy(gameObject, destroyDelay);
    }
}
