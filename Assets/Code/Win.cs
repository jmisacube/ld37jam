using UnityEngine;
using UnityEngine.UI;

public class Win : MonoBehaviour
{
    public Timer timer;
    public Image panel;
    private GameManager manager;

    private void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            panel.color = new Color(0, 0, 0, 0.5f);
            timer.stop();
            manager.gameEndScreen();
            //Application.LoadLevel(0);
        }
    }
}
