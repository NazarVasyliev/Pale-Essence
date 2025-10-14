using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Room parentRoom;

    private void OnDestroy()
    {
        FindFirstObjectByType<ScoreManager>().AddPoints(10);
        if (parentRoom != null)
        {
            parentRoom.EnemyWasDefeated(gameObject);
        }
    }
}