using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class PointerButton : PointerHandler
{
    [Header("Stage Settings")]
    public int level = 0;
    public bool nextButton = false;

    [Header("Button Settings")]
    [SerializeField] private GameObject buttonObject;
    [SerializeField] private GameManager gm;
    public Color activCol = Color.white;
    public Color deactivCol = Color.white;
    public Color clickCol = Color.white;
    public bool changeCol;

    private Material btnMat;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        if (buttonObject != null && buttonObject.GetComponent<Renderer>() != null)
		{
            btnMat = buttonObject.GetComponent<Renderer>().material;
            btnMat.color = activCol;
        }

        if (!interactable)
		{
            btnMat.color = deactivCol;
		}

        anim = GetComponent<Animator>();

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

        if (interactable)
        {
            
        }
    }

    public new virtual void OnExitHover(BaseEventData eventData)
    {
        base.OnExitHover(eventData);

        if (interactable && changeCol)
        {
            btnMat.color = activCol;
        }
    }

    public new virtual void OnClick(BaseEventData eventData)
    {
        if (interactable)
        {
            if (nextButton)
			{
                gm.NextButton();
            }
            else
			{
                gm.PressButton(level);
			}
            anim.SetTrigger("press");

            if (changeCol)
            {
                btnMat.color = clickCol;
            }

            if (interactableOnce)
            {
                interactable = false;
                changeCol = false;
            }
        }

        base.OnClick(eventData);
    }

    public new void Click(BaseEventData data)
    {
        OnClick(data);
    }
}
