using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExcitementBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxExcitement (float excitement) {
        slider.maxValue = excitement;
        slider.value = excitement;
    }
    public void SetExcitement (float excitement) {
        slider.value = excitement;
    }
}
