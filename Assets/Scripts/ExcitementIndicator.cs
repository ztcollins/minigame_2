using UnityEngine;
using UnityEngine.UI;

public class ExcitementIndicator : MonoBehaviour
{
    public Slider slider;
    public Image verticalLine;

    //-0.5 to 0.5 for positon
    public void updateVerticalLinePosition(float position)
    {
        position = position - 0.5f;

        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
        float xPos = sliderWidth * position;

        verticalLine.rectTransform.anchoredPosition = new Vector2(xPos, 0f);
    }
}