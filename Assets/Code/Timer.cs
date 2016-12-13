using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float time;
    private Text timerText;
    private bool running;

	void Start ()
    {
        timerText = GetComponent<Text>();
        running = true;
	}
	
	void Update ()
    {
        if (! running)
        {
            return;
        }

        time += Time.deltaTime;

        int minutes = (int) (time / 60);
        double seconds = (time - minutes * 60);
        timerText.text = minutes + ":" + string.Format("{0:00.000}", seconds);
        
	}

    public void stop()
    {
        running = false;
    }
}
