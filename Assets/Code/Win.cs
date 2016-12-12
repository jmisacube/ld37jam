using UnityEngine;

public class Win : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Quit. ACTIVATE");
            Application.Quit();
        }
    }
}
