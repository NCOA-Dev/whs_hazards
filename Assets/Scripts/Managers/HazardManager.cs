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

    [Header("Major Panels")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private GameObject riskPanel;
    [SerializeField] private GameObject responsePanel;
    [SerializeField] private GameObject completionPanel;

    [Header("Other UI")]
    [SerializeField] private TMP_Text riskHazardText;
    [SerializeField] private TMP_Text responseHazardText;
    [SerializeField] private UIPointerHandler[] responseButtonsCont;
    [SerializeField] private UIPointerHandler[] responseButtonsRep;
    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private GameObject dateText;

    [Header("Storage Room")]
    [SerializeField] private List<string> hazardDescsStorage;
    [SerializeField] private int[] riskAnswersStorage;
    [SerializeField] private Vector2[] responseAnswersStorage;
    private List<string> hazardDescsStorageCopy;
    private int[] riskAttemptsStorage;
    private Vector2[] responseAttemptsStorage;

    // Current room tracking
    private int currentProgress = 0;
    private int selectedRow = 0;

    [Header("Scene Tracking Variables")]
    [Tooltip("Location name for the scene of equal array index to scene index. Menu scenes should be blank.")]
    [SerializeField] private string[] locationNames;

    // Response tracking variables
    private bool selectedResCont = false;
    private bool selectedResRep = false;
    private string contText = "";
    private string repText = "";

    // Results {risk, res1, res2}
    private bool[,] storageResults; 

    void Start()
    {
        // Clone storage hazard texts
        hazardDescsStorageCopy = new List<string>(hazardDescsStorage);

        // Instantiate attempt arrays
        riskAttemptsStorage = new int[riskAnswersStorage.Length];
        responseAttemptsStorage = new Vector2[responseAnswersStorage.Length];

        // Instantiate results arrays
        storageResults = new bool[riskAnswersStorage.Length, 3];

        // Fill attempt arrays with -1 to track if attempted
        for (int i = 0; i < riskAttemptsStorage.Length; i++)
		{
            riskAttemptsStorage[i] = -1;
            responseAttemptsStorage[i].x = -1;
            responseAttemptsStorage[i].y = -1;
        }

        StartRoom(0);
    }

    public void StartRoom(int sceneIndex)
	{
        // Set name of room
        locationText.text = locationNames[sceneIndex];

        // Date text visible on non-menu scenes
        dateText.SetActive(sceneIndex > 0);
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

    // Called by Okay button on completion panel
    public void SubmitCompletionPanel()
    {
        StartCoroutine(WaitThenActivate(0.2f, completionPanel, false));
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

        // Record answer
        riskAttemptsStorage[IndexOf(selectedRow)] = severity;

        // Evaluate results
        bool correct = severity == riskAnswersStorage[IndexOf(selectedRow)];
        storageResults[IndexOf(selectedRow), 0] = correct; // Store result

        // If not being called by cancel button, set text
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

            // Only set color if not being called by cancel button
            riskBoxes[selectedRow].ActivateColor(!correct); // Change color to red if incorrect
        }

        StartCoroutine(WaitThenActivate(0.2f, riskPanel, false));
        CheckIfFinished();
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

    // Response select workaround for UnityEvents only accepting one parameter
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
                {  // Incorrect - make text red and store incorrect result
                    contText = Redify(contText);
                    storageResults[IndexOf(selectedRow), 1] = false;
                }
                else
                { // Correct - store correct result
                    storageResults[IndexOf(selectedRow), 1] = true;
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
                { // Incorrect - make text red and store incorrect result
                    repText = Redify(repText);
                    storageResults[IndexOf(selectedRow), 2] = false;
                }
                else
                { // Correct - store correct result
                    storageResults[IndexOf(selectedRow), 2] = true;
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

            // Change color to red if incorrect
            bool correct = storageResults[IndexOf(selectedRow), 1] &&
                           storageResults[IndexOf(selectedRow), 2];
            responseBoxes[selectedRow].ActivateColor(!correct);

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
        CheckIfFinished();
    }
	#endregion

	#region private
	// Duplicate report info for each room
    private void CreateReports()
	{

	}

	// Called when an answer is submitted 
	private void CheckIfFinished()
	{
        bool allAttempted = true;
        bool allCorrect = true;

        for (int i = 0; i < riskAttemptsStorage.Length; i++)
		{ // Check if any question is not yet attempted
            if (riskAttemptsStorage[i] == -1 ||
                responseAttemptsStorage[i].x == -1 ||
                responseAttemptsStorage[i].y == -1)
			{
                allAttempted = false;
			}
		}
        
        for (int i = 0; i < storageResults.GetLength(0); i++)
        { // Check if all questions are correct
            for (int j = 0; j < storageResults.GetLength(1); j++)
			{
                if (!storageResults[i, j])
				{
                    allCorrect = false;
                }
			}
		}

        if (allAttempted)
		{
            // do something to finish 
            Debug.Log("All questions attempted.");
		}

        if (allCorrect)
		{
            Debug.LogWarning("All questions correct.");

            // Reveal completion panel
            StartCoroutine(WaitThenActivate(1f, completionPanel, true));
            StartCoroutine(WaitThenActivate(1f, riskPanel, false));
            StartCoroutine(WaitThenActivate(1f, responsePanel, false));
        }
    }

    // Return the description list index of a hazard text from the given row
    private int IndexOf(int row)
	{
        string text = hazardTextBoxes[row].text;
        return hazardDescsStorageCopy.IndexOf(text);
    }
     
    // Returns a given string, but made dark red with html/TMP tags
    private string Redify(string text)
	{
        return "<color=#9D0000>" + text + "</color>";
	}

    private IEnumerator WaitThenActivate(float secs, GameObject obj, bool activ)
	{
        // Block clicks during wait time
        clickBlocker.SetActive(true);
        yield return new WaitForSeconds(secs);
        obj.SetActive(activ);
        clickBlocker.SetActive(false);
	}
	#endregion
}
