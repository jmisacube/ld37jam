using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Checkpoint respawnPoint;
    private bool inGameEnd;

	void Start ()
    {
        respawnPoint = null;
        inGameEnd = false;
	}

    private void Update()
    {
        if (inGameEnd)
        {
            if (Input.GetKeyDown("return"))
            {
                inGameEnd = false;
                Cursor.visible = true;
                Application.LoadLevel(0);
            }
        }
    }

    public void gameEndScreen()
    {
        inGameEnd = true;
    }
}
