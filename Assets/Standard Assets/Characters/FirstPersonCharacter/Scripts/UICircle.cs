using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class UICircle : MonoBehaviour
{
    [SerializeField] private Sprite ChargedSprite, SpentSprite;
    private bool isCharged;
    private Image uiImage;

    void Start ()
    {
        isCharged = true;
        uiImage = GetComponent<Image>();
	}

    public void setCharged(bool b)
    {
        if (isCharged == b)
            return;

        if (isCharged)
            uiImage.sprite = SpentSprite;
        else
            uiImage.sprite = ChargedSprite;

        isCharged = b;
    }
}
