using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPanel : MonoBehaviour
{
    public GameObject[] hazardTexts;
    public TMP_Text[] hazardTextBoxes;
    public UIPointerHandler[] riskBoxes;
    public UIPointerHandler[] responseBoxes;

    public void SetActive(bool activ)
	{
        gameObject.SetActive(activ);
	}
}
