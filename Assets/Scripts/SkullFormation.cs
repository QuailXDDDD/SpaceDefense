using UnityEngine;

public class SkullFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public GameObject enemyPrefab;
    public float spacing = 1.0f;
    public Vector3 enemyScale = new Vector3(0.5f, 0.5f, 1f);

    int[,] linePattern = new int[,] {
        {1,1,1,1,1,1,1},
        {1,1,1,1,1,1,1}
    };

    void Start()
    {
        int rows = linePattern.GetLength(0);
        int cols = linePattern.GetLength(1);
        Vector3 offset = new Vector3(-(cols - 1) * spacing / 2f, (rows - 1) * spacing / 2f, 0);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (linePattern[row, col] == 1)
                {
                    Vector3 pos = new Vector3(col * spacing, -row * spacing, 0) + offset;
                    GameObject enemy = Instantiate(enemyPrefab, transform.position + pos, Quaternion.identity, transform);
                    enemy.transform.localScale = enemyScale;
                }
            }
        }
    }
} 