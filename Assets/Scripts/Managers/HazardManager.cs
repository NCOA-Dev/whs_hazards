using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HazardManager : MonoBehaviour
{
    [Header("Text Boxes")]
    [SerializeField] private GameObject[] hazardTexts;
    [SerializeField] private UIPointerHandler[] riskBoxes;
    [SerializeField] private UIPointerHandler[] responseBoxes;
    private TMP_Text[] hazardTextBoxes;

    [Header("Other UI")]
    [SerializeField] private GameObject riskPanel;
    [SerializeField] private TMP_Text riskHazardText;
    [SerializeField] private GameObject responsePanel;
    [SerializeField] private TMP_Text responseHazardText;
    [SerializeField] private UIPointerHandler[] responseButtonsCont;
    [SerializeField] private UIPointerHandler[] responseButtonsRep;
    [SerializeField] private GameObject clickBlocker;

    [Header("Storage Room")]
    [SerializeField] private List<string> hazardDescsStorage;
    [SerializeField] private int[] riskAnswersStorage;
    [SerializeField] private Vector2[] responseAnswersStorage;
    private List<string> hazardDescsStorageCopy;
    private int[] riskAttemptsStorage;
    private Vector2[] responseAttemptsStorage;

    private int currentProgress = 0;
    private int selectedRow = 0;

    // Response tracking variables
    private bool selectedResCont = false;
    private bool selectedResRep = false;
    private string contText = "";
    private string repText = "";

    void Start()
    {

        // Clone storage hazard texts
        hazardDescsStorageCopy = new List<string>(hazardDescsStorage);

        // Instantiate attempt arrays
        riskAttemptsStorage = new int[riskAnswersStorage.Length];
        responseAttemptsStorage = new Vector2[responseAnswersStorage.Length];

        // Fill attempt arrays with -1 to track if attempted
        for (int i = 0; i < riskAttemptsStorage.Length; i++)
		{
            riskAttemptsStorage[i] = -1;
            responseAttemptsStorage[i].x = -1;
            responseAttemptsStorage[i].y = -1;
        }
    }

    public void FoundHazard(string hazardDesc)
	{
        // Create list if not already created
        if (hazardTextBoxes == null || hazardTextBoxes.Length == 0)
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

	#region risk
	// Called when risk box is clicked
	public void SelectRiskBox(int row)
	{
        // Reveal risk panel
        StartCoroutine(WaitThenActivate(0.3f, riskPanel, true));

        string rowText = hazardTextBoxes[row].text;
        riskHazardText.text = rowText;

        // Keep track of which row was clicked on
        selectedRow = row;
    }

    // Called when pressed risk button (0: low, 1: med, 2: high)
    public void SelectRisk(int severity)
	{
        string riskText = "";
        Color riskCol = Color.black;
        switch (severity)
		{
            case 0:
                riskText = "Low";
                riskCol = new Color(0.95f, 0.9f, 0f); // yellow
                break;
            case 1:
                riskText = "Moderate";
                riskCol = new Color(1f, 0.5f, 0f); // orange
                break;
            case 2:
                riskText = "High";
                riskCol = new Color(0.9f, 0f, 0f); // red
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

        // Record answer
        riskAttemptsStorage[IndexOf(selectedRow)] = severity;

        if (severity != riskAnswersStorage[IndexOf(selectedRow)])
        {
            Debug.LogWarning("incorrect severity");
        }

        StartCoroutine(WaitThenActivate(0.2f, riskPanel, false));
	}
    #endregion

    #region response
    // Called when response box is clicked
    public void SelectResponseBox(int row)
    {
        // Reveal risk panel
        StartCoroutine(WaitThenActivate(0.3f, responsePanel, true));

        string rowText = hazardTextBoxes[row].text;
        responseHazardText.text = rowText;

        // Keep track of which row was clicked on
        selectedRow = row;
    }

    // Workaround for Unity Events only accepting one param
    public void SelectResponse1(int response)
	{
        SelectResponse(1, response);
    }
    public void SelectResponse2(int response)
    {
        SelectResponse(2, response);
    }
    public void ResponseCancel()
    {
        SelectResponse(-1, -1);
    }

    // Called when pressed response button (section 1, 2, or -1) and response (0, 1, 2)
    private void SelectResponse(int section, int response)
    {
        switch (section)
		{
            case -1: // Called by cancel button
                CloseResponse();
                EnableBtns(responseButtonsCont, -1);
                EnableBtns(responseButtonsRep, -1);
                break;
            case 1: // Control measure section
                selectedResCont = true;
                EnableBtns(responseButtonsCont, response);
                TMP_Text contAnsText = responseButtonsCont[response].GetComponentInChildren<TMP_Text>();
                contText = contAnsText.text;
                
                // Record answer
                responseAttemptsStorage[IndexOf(selectedRow)].x = response;

                if (response != responseAnswersStorage[IndexOf(selectedRow)].x)
                {  // Incorrect- make text red
                    contText = MakeTextRed(contText);
                }

                break;
            case 2: // Reporting method section
                selectedResRep = true;
                EnableBtns(responseButtonsRep, response);
                TMP_Text repAnsText = responseButtonsRep[response].GetComponentInChildren<TMP_Text>();
                repText = repAnsText.text;
                
                // Record answer
                responseAttemptsStorage[IndexOf(selectedRow)].y = response;

                if (response != responseAnswersStorage[IndexOf(selectedRow)].y)
                { // Incorrect - make text red
                    repText = MakeTextRed(repText);
                }
                break;
            default:
                Debug.LogError("Section " + section + " not found.");
                break;
        }


        if (selectedResCont && selectedResRep)
		{
            TMP_Text txt = responseBoxes[selectedRow].GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = string.Format("• {0} \n• {1}", contText, repText);
            }
            else
            {
                Debug.LogError("Text box not found on " + responseBoxes[selectedRow].name);
            }

            CloseResponse();
        }
    }

    private void EnableBtns(UIPointerHandler[] btns, int ignoreIndex)
	{
        for (int i = 0; i < btns.Length; i++)
		{
            if (i != ignoreIndex)
			{
                btns[i].Activate(false);
			}
		}
	}

    private void CloseResponse()
	{
        selectedResCont = false;
        selectedResRep = false;

		StartCoroutine(WaitThenActivate(0.4f, responsePanel, false));
	}
    #endregion

    // Called when an answer is submitted 
    private void CheckIfFinished()
	{
        bool finished = true;

        for (int i = 0; i < riskAttemptsStorage.Length; i++)
		{ // Check if any question is not yet attempted
            if (riskAttemptsStorage[i] == -1 ||
                responseAttemptsStorage[i].x == -1 ||
                responseAttemptsStorage[i].y == -1)
			{
                finished = false;
			}
		}

        if (finished)
		{
            // do something to finish 
            Debug.LogWarning("All questions attempted.");
		}
	}

    // Return the description list index of a hazard text from the given row
    private int IndexOf(int row)
	{
        string text = hazardTextBoxes[row].text;
        return hazardDescsStorageCopy.IndexOf(text);
    }

    private string MakeTextRed(string text)
	{
        return "<color=\"red\">" + text + "</color>";
	}

    private IEnumerator WaitThenActivate(float secs, GameObject obj, bool activ)
	{
        // Block clicks during wait time
        clickBlocker.SetActive(true);
        yield return new WaitForSeconds(secs);
        obj.SetActive(activ);
        clickBlocker.SetActive(false);

        // If being deactivated, check if finished
        if (!activ)
		{
            CheckIfFinished();
        }
	}
}
