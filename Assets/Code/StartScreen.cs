using UnityEngine;

public class StartScreen : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void OnStartClick()
    {
        Application.LoadLevel(1);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
