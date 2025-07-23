using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float destroyDelay = 0.5f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
