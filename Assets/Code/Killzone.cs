using UnityEngine;

public class Killzone : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("YOU DEAD");
            col.gameObject.transform.position = new Vector3();
        }
    }
}
