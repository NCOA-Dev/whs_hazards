using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HazardManager : MonoBehaviour
{
    [Header("Text Boxes")]
    [SerializeField] private GameObject[] hazardTexts;
    [SerializeField] private UIPointerHandler[] riskBoxes;
    [SerializeField] private UIPointerHandler[] responseBoxes;
    private TMP_Text[] hazardTextBoxes;

    [Header("Other UI")]
    [SerializeField] private GameObject riskPanel;

    [Header("Storage Room")]
    [SerializeField] private List<string> hazardDescsStorage;

    private int currentProgress = 0;
    private int selectedRow = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Get all texts from the hazard text list
        hazardTextBoxes = new TMP_Text[hazardTexts.Length];
        for (int i = 0; i < hazardTexts.Length; i++)
		{
            TMP_Text text = hazardTexts[i].GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                hazardTextBoxes[i] = text;
            }
        }
    }

    public void FoundHazard(string hazardDesc)
	{
        if (hazardDescsStorage.Contains(hazardDesc))
		{
            // Remove the text to prevent repetitions
            hazardDescsStorage.Remove(hazardDesc);

            // Add the text to the clipboard
            hazardTextBoxes[currentProgress].text = hazardDesc;

            // Activate input boxes
            riskBoxes[currentProgress].interactable = true;
            responseBoxes[currentProgress].interactable = true;

            // Increment progress
            currentProgress++;
        }
        else
		{
            Debug.LogWarning(hazardDesc + " not found or already added.");
		}
	}

    // Called when risk box is clicked
    public void SelectRiskBox(int row)
	{
        // Reveal risk panel
        StartCoroutine(WaitThenActivate(0.2f, riskPanel, true));
        //riskPanel.SetActive(true);

        // Keep track of which row was clicked on
        selectedRow = row;
    }

    // Called when pressed low, mod, or high button (0: low, 1: med, 2: high)
    public void SelectRisk(int severity)
	{
        string riskText = "";
        Color riskCol = Color.black;
        switch (severity)
		{
            case 0:
                riskText = "Low";
                riskCol = Color.yellow;
                break;
            case 1:
                riskText = "Moderate";
                riskCol = new Color(1f, 0.5f, 0f);
                break;
            case 2:
                riskText = "High";
                riskCol = Color.red;
                break;         
        }

        // If not being called by cancel button
        if (severity != -1)
		{
            TMP_Text txt = riskBoxes[selectedRow].GetComponentInChildren<TMP_Text>();
            if (txt != null)
			{
                txt.text = riskText;
                txt.color = riskCol;
            }
            else
			{
                Debug.LogError("Text box not found on " + riskBoxes[selectedRow].name);
			}
		}

        //riskPanel.SetActive(false);
        StartCoroutine(WaitThenActivate(0.2f, riskPanel, false));
	}

    private IEnumerator WaitThenActivate(float secs, GameObject obj, bool activ)
	{
        yield return new WaitForSeconds(secs);
        obj.SetActive(activ);
	}
}
