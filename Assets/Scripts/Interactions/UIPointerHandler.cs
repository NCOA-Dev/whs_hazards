using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(EventTrigger))]
public class UIPointerHandler : MonoBehaviour
{
    [Tooltip("Whether this UI item is currently interactable")]
    public bool interactable;

    [Tooltip("Only allow hover events, not clicks.")]
    public bool noClick; 
    public bool interactableOnce;
    public bool hasOutline;
    public bool changeColor = false;
    public Color origCol = Color.white;
    public Color hoverCol = Color.white;
    public Color clickCol = Color.white;
    public Color activatedCol = Color.clear;
    public Color hintCol = Color.clear;

    [Header("Events")]
    public UnityEvent doOnHover;
    public UnityEvent doOnExitHover;
    public UnityEvent doOnClick;

    [Header("Custom Click Speed")]
    public float speedMultiplier = 1.0f;

    //[Header("SFX")]
    //[SerializeField] private GameObject downSound = null;
    //[SerializeField] private GameObject upSound = null;
    //[SerializeField] private GameObject clickSound = null;
    //[SerializeField] private GameObject errorSound = null;
    //[SerializeField] private GameObject dragSound = null;
    //[SerializeField] private GameObject dropSound = null;

    private bool intOnceStart = false;
    private bool hovering = false;
    private Image img;

    private void OnEnable()
	{
        img = GetComponent<Image>();
        origCol = img.color;
    }

    // Start is called before the first frame update
    void Start()
    {
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
            EventTriggerType.PointerDown,
            OnDown);

        AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerUp,
            OnUp);

        AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerClick,
            OnClick);
    }
    private void OnDisable()
    {
        if (hovering)
        {
            Player.Instance.StopHovering();
        }
        if (img != null && origCol != null && changeColor)
		{
            img.color = origCol;
        }
    }

    public static void AddEventTriggerListener(EventTrigger trigger,
                            EventTriggerType eventType, System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }

    private void OnHover(BaseEventData eventData)
    {
        if (interactable)
        {
            if (changeColor)
            {
                img.color = hoverCol;
            }

            if (!noClick)
			{
                Player.Instance.isHovering = true;
                Player.Instance.btnSpeedMultiplier = speedMultiplier;
            }

            doOnHover.Invoke();

            hovering = true;
        }
    }

    private void OnExitHover(BaseEventData eventData)
    {
        if (hovering)
        {
            if (changeColor)
            {
                img.color = origCol;
            }

            Player.Instance.StopHovering();

            doOnExitHover.Invoke();
        }
    }

    private void OnDown(BaseEventData eventData)
    {
        if (interactable)
        {
        }
    }

    private void OnUp(BaseEventData eventData)
    {
        if (interactable)
        {
        }
    }

    private void OnClick(BaseEventData eventData)
    {
        if (interactable)
        {
            if (changeColor)
            {
                img.color = clickCol;
            }

            if (interactableOnce)
            {
                interactable = false;
                changeColor = false;
            }

            // Exclusive physics obj
            if (GetComponent<Rigidbody>())
            {
                Rigidbody rb = GetComponent<Rigidbody>();

                if (rb)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Player.Instance.cam.transform.forward * 5f, ForceMode.Impulse);
                }
            }

            doOnClick.Invoke();
        }


        if (this.isActiveAndEnabled)
        {
            StartCoroutine(InteractablePause());
        }
    }

    private void OnDrag(BaseEventData eventData)
    {
    }

    private void OnEndDrag(BaseEventData eventData)
    {
    }

    private IEnumerator InteractablePause()
    {
        interactable = false;

        yield return new WaitForSeconds(0.25f);

        if (!interactableOnce)
		{
            interactable = true;
        }
    }

    public void Click(BaseEventData data)
    {
        OnClick(data);
    }

    public void Activate(bool on)
	{
        if (on && img != null)
		{
            img.color = activatedCol;
        }
        else if (img != null)
		{
            img.color = origCol;
        }
        changeColor = !on;
        interactable = !on;
    }

    public void Hint(bool on)
    {
        if (on && img != null)
        {
            img.color = hintCol;
        }
        else if (img != null)
        {
            img.color = origCol;
        }

        changeColor = false;
        interactable = on;
    }
}
