using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HueRotation : MonoBehaviour
{
    public float hueRate = 1; // How fast the hue should change

    private Image image;
    private int currentHue;

	void Start ()
    {
        image = GetComponent<Image>();
	}
	
	void Update ()
    {
        currentHue += (int) (hueRate * Time.deltaTime);
        currentHue %= 255;
        image.color = new Color();
	}
}
