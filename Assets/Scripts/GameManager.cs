using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	[Header("Referenced Variables")]
	[SerializeField] private GameObject[] saveOnLoad;
	[SerializeField] private TMP_Text stageText;
	[SerializeField] private Animator elevatorAnim;
	[SerializeField] private HazardManager hm;
	[SerializeField] private PointerButton[] elevButtons;

	[Header("Hazard Objects")]
	[SerializeField] private GameObject[] hazards;

	private int currentLvl = 0;
	private readonly int totalLevels = 3;
	private bool loading = false;

	private void Awake()
	{
		foreach (GameObject obj in saveOnLoad)
		{
			DontDestroyOnLoad(obj);
		}
	}

	private void Start()
	{
		foreach (GameObject hazard in hazards)
		{
			hazard.SetActive(false);
		}

		elevButtons[0].Activate(false);
	}

	private void Update()
	{
		// Lock cursor
		if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public void PressButton(int level)
	{
		if (!loading && currentLvl != level)
		{
			// Elevator goes up if new level is higher than previous and vice versa
			elevatorAnim.SetBool("up", level > currentLvl);

			loading = true;
			currentLvl = level;

			// Set click colour (only if confirmed click)
			elevButtons[currentLvl].ClickCol(true);

			if (elevatorAnim.GetBool("open"))
			{ // If elevator open, wait for it to close
				elevatorAnim.SetBool("open", false);
				StartCoroutine(ElevatorLoad(currentLvl, 8f, NextLevel));
			}
			else
			{ // If elevator already closed
				StartCoroutine(ElevatorLoad(currentLvl, 6f, NextLevel));
			}
		}
	}

	public void NextButton()
	{
		if (!loading && currentLvl < totalLevels)
		{
			loading = true;
			currentLvl++;
			elevButtons[currentLvl].ClickCol(true);

			if (elevatorAnim.GetBool("open"))
			{ // If elevator open, wait for it to close
				elevatorAnim.SetBool("open", false);
				StartCoroutine(ElevatorLoad(currentLvl, 4f, NextLevel));
			}
			else
			{ // If elevator already closed
				StartCoroutine(ElevatorLoad(currentLvl, 3f, NextLevel));
			}
		}
	}

	public void PrevButton()
	{
		if (!loading && currentLvl != 0)
		{
			loading = true;
			currentLvl--;
			elevButtons[currentLvl].ClickCol(true);

			if (elevatorAnim.GetBool("open"))
			{ // If elevator open, wait for it to close
				elevatorAnim.SetBool("open", false);
				StartCoroutine(ElevatorLoad(currentLvl, 4f, NextLevel));
			}
			else
			{ // If elevator already closed
				StartCoroutine(ElevatorLoad(currentLvl, 3f, NextLevel));
			}
		}
	}

	// Setup the next level
	private void NextLevel()
	{
		stageText.text = "Floor " + currentLvl;
		elevatorAnim.SetBool("open", true);
		loading = false;

		foreach (GameObject hazard in hazards)
		{
			hazard.SetActive(false);
		}
		if (currentLvl >= 1)
		{
			hazards[currentLvl - 1].SetActive(true);
		}

		// Disable pressed button and enable others
		foreach (PointerButton btn in elevButtons)
		{
			btn.Activate(true);
		}
		elevButtons[currentLvl].Activate(false);

		hm.StartRoom(currentLvl);
	}

	// Load next scene around set pauses and run the given method when done
	private IEnumerator ElevatorLoad(int index, float timeBeforeLoad, Action doAfter)
	{
		yield return new WaitForSeconds(timeBeforeLoad);

		// Load scene
		yield return new WaitForSeconds(0.1f);
		AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index + 1);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		Cursor.visible = false;

		yield return new WaitForSeconds(0.5f);
		doAfter();
	}
}
