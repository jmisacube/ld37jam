using UnityEngine;

public class StartScreen : MonoBehaviour
{
	public void OnStartClick()
    {
        Application.LoadLevel(1);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
