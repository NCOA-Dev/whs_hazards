using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHazard : PointerHandler
{
    [Header("Hazard Settings")]
    public string hazardName = "";
    public bool isHazard = false;
    public int severity = 1;

    //[Header("Question Manager")]
    //[SerializeField] private QnManager qm;

    // Highlight emission colours to swap between
    [SerializeField] [ColorUsage(true, true)] private Color highlightCol = Color.white;
    [SerializeField] [ColorUsage(true, true)] private Color highlightDoneCol = Color.white;
    [SerializeField] [ColorUsage(true, true)] private Color highlightWrongCol = Color.white;

    // Highlighters objects to create
    private List<GameObject> highlightObjs = new List<GameObject>();
    private List<GameObject> highlightDoneObjs = new List<GameObject>();
    private List<GameObject> highlightWrongObjs = new List<GameObject>();
    private List<GameObject> highlightHintObjs = new List<GameObject>();
    private List<GameObject> currentHighlights;

    // Whether highlight is completed
    private bool completed = false;

    // Whether QnManager is in learning mode
    private bool stopIncorrectAnwers = false;

    // Whether highlight is flashing after an incorrect answer
    private bool flashing = false;

    // Whether highlight is blue and shown as hint
    private bool hintBlue = false;

    // Layer index of object (to display through objects with depth camera)
    private readonly int highlightLayer = 8;

    [Header("Optional: View through other objects")]
    public bool seeThrough = false;

    // Scale offset of hover effect
    private Vector3 meshHoverScale = new Vector3(1.005f, 1.005f, 1.005f);

    public virtual void Start()
    {
        highlightObjs = CreateHighlightMeshes(highlightCol);
        highlightDoneObjs = CreateHighlightMeshes(highlightDoneCol);
        highlightWrongObjs = CreateHighlightMeshes(highlightWrongCol);
        currentHighlights = highlightObjs;

        // Initialize custom trigger events
        AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerEnter,
            OnHover);
        AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerExit,
            OnExitHover);
        AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerClick,
            OnClick);
    }

    public new virtual void OnHover(BaseEventData eventData)
    {
        base.OnHover(eventData);

        if (interactable && !completed)
        {
            if (hintBlue)
            {
                Highlight(false);
                currentHighlights = highlightObjs;
            }
            Highlight(true);
        }
    }

    public new virtual void OnExitHover(BaseEventData eventData)
    {
        base.OnExitHover(eventData);

        if (interactable && !completed)
        {
            Highlight(false);
            if (hintBlue)
            {
                currentHighlights = highlightHintObjs;
                Highlight(true);
            }
        }
    }

    public new virtual void OnClick(BaseEventData eventData)
    {
        if (interactable)
        {
            if (stopIncorrectAnwers) //&& !qm.IsCorrect(areaName))
            { // Prevent question being answered if incorrect in learning mode
                StartCoroutine(IncorrectFlash());
            }
            else
            { // Answer question
                //qm.AnswerQuestion(areaName);

                completed = true;
                hintBlue = false;

                Highlight(false);
                currentHighlights = highlightDoneObjs;
                Highlight(true);

                //qm.RecHighlight(this);
            }
        }

        base.OnClick(eventData);
    }

    // Undo this highlight
    public void UndoHighlight()
    {
        completed = false;

        Highlight(false);
        currentHighlights = highlightObjs;
        interactable = true;
        hintBlue = false;
    }

    // Switch this highlight on or off
    private void Highlight(bool on)
    {
        foreach (GameObject obj in currentHighlights)
        {
            if (obj)
            {
                obj.SetActive(on);
            }

            // Set layer to view through objects
            if ((seeThrough && (!on || completed)) || flashing)
            {
                obj.layer = 0;
            }
            else if (on && seeThrough)
            {
                obj.layer = highlightLayer;
            }
        }
    }

    // Initializes highlight meshes from given material (called on start)
    // Returns list of highlight objects
    private List<GameObject> CreateHighlightMeshes(Color col)
    {
        List<GameObject> newHighlightObjects = new List<GameObject>();
        MeshRenderer[] meshList = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer rend in meshList)
        {
            if (rend != null)
            {
                GameObject obj;
                obj = Instantiate(rend.gameObject, rend.transform);

                // Create a new material
				Material mat = new Material(Shader.Find("Standard"));
                obj.GetComponent<Renderer>().material = mat;

                // Remove color, smoothness, and activate set emission color
                mat.color *= 0.001f;
                mat.SetFloat("_Glossiness", 0f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", col);

                // Change to transparent material
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_Mode", 3);
                mat.EnableKeyword("_ALPHABLEND_ON");
				mat.DisableKeyword("_ALPHATEST_ON");
				mat.SetInt("_ZWrite", 0);
				mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = 3100;

                // Re-apply to refresh shader
                mat.shader = Shader.Find(mat.shader.name);

                // Remove new object's children
                foreach (Transform child in obj.transform)
                {
                    Destroy(child.gameObject);
                }

                //obj.transform.localScale *= 1.001f;
                obj.transform.rotation = obj.transform.parent.rotation;
                //obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = meshHoverScale;

                newHighlightObjects.Add(obj);
                obj.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Mesh not found");
            }
        }

        return newHighlightObjects;
    }

    // Trigger custom click event by player click
    public new void Click(BaseEventData data)
    {
        OnClick(data);
    }

    // Trigger hint highlighter
    public void Hint()
    {
        Highlight(false);
        currentHighlights = highlightHintObjs;
        Highlight(true);

        hintBlue = true;
    }

    // Reset hint highlighter and remove
    public void RemHint()
    {
        if (hintBlue)
        {
            Highlight(false);
            currentHighlights = highlightObjs;
            Highlight(true);
            Highlight(false);

            hintBlue = false;
        }
    }

    // Flash red mesh when incorrect answer is selected
    private IEnumerator IncorrectFlash()
    {
        flashing = true;
        interactable = false;

        for (int i = 0; i < 4; i++)
        {
            Highlight(false);
            yield return new WaitForSeconds(0.1f);
            currentHighlights = highlightWrongObjs;
            Highlight(true);

            yield return new WaitForSeconds(0.5f);
        }

        Highlight(false);
        currentHighlights = highlightObjs;

        interactable = true;
        flashing = false;
    }

    // Remove all incorrect and hint highlights when disabled
    private void OnDisable()
    {
        if (flashing)
        {
            Highlight(false);
            currentHighlights = highlightObjs;

            interactable = true;
            flashing = false;
        }

        RemHint();
    }
}
