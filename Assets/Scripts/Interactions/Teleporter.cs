using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Teleporter : PointerHandler
{
	[Header("Teleporter Only Variables")]
	[SerializeField] public GameObject teleportLocation;
	[HideInInspector] public GameObject tube;

	[Header("Material Variables")]
	private Material objMat;
	public bool changeColor = true;
	public Color origCol = Color.white;
	public Color hoverCol = Color.white;
	public Color clickCol = Color.white;

	// Start is called before the first frame update
	void Awake()
    {
		objMat = GetComponent<Renderer>().material;

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

		tube = this.gameObject;
	}


    public new virtual void OnHover(BaseEventData eventData)
    {
        base.OnHover(eventData);

		if (changeColor)
		{
			objMat.SetColor("_TintColor", hoverCol);
		}

		interactable = true;
	}

    public new virtual void OnExitHover(BaseEventData eventData)
    {
        base.OnExitHover(eventData);

		if (changeColor)
		{
			objMat.SetColor("_TintColor", origCol);
		}
	}


	public new virtual void OnClick(BaseEventData eventData)
    {
		if (changeColor)
		{
			objMat.SetColor("_TintColor", origCol);
		}

		if (transform.CompareTag("Teleporter"))
		{
			Player.Instance.Teleport(this);
		}

		base.OnClick(eventData);
	}

	public new void Click(BaseEventData data)
	{
		OnClick(data);
	}
}
