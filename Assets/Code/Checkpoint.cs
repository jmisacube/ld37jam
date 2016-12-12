using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private GameManager manager;
    private Transform respawnPoint;
    // We don't want players to accidentally touch and grab a previous checkpoint,
    // a simple solution is to only let checkpoints be grabbed once, and since the level is
    // linear, the last checkpoint should be the furthest one.
    private bool wasUsed;

	void Start ()
    {
        respawnPoint = gameObject.transform;
        manager = FindObjectOfType<GameManager>();
        wasUsed = false;
	}

    void OnTriggerEnter(Collider col)
    {
        if (wasUsed)
            return;

        wasUsed = true;

        if (col.gameObject.tag == "Player")
        {
            manager.respawnPoint = this;
        }
    }

    public Transform getRespawnPoint()
    {
        return respawnPoint;
    }
}
