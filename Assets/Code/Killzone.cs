using UnityEngine;

public class Killzone : MonoBehaviour
{
    private GameManager manager;

    private void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Transform t = manager.respawnPoint.getRespawnPoint();

            if (t == null)
            {
                col.gameObject.transform.position = new Vector3();
                return;
            }

            col.gameObject.transform.position = t.position;
            col.gameObject.transform.rotation = t.rotation;
        }
    }
}
