using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Teleporter : PointerHandler
{
	[Header("Teleporter Only Variables")]
	[SerializeField] public GameObject teleportLocation;
	[HideInInspector] public GameObject tube;

    // Start is called before the first frame update
    void Awake()
    {
		AddEventTriggerListener(
            GetComponent<EventTrigger>(),
            EventTriggerType.PointerClick,
            OnClick);

		tube = this.gameObject;
	}

	public new virtual void OnClick(BaseEventData eventData)
    {
		//if (changeColor)
		//{
		//	objMat.SetColor("_TintColor", clickCol);
		//}

		if (transform.CompareTag("Teleporter"))
		{
			Player.Instance.Teleport(this);
		}
	}

	public new virtual void Click(BaseEventData data)
	{
		OnClick(data);
	}
}
