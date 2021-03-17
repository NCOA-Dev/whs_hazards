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
		if (!loading)
		{
			loading = true;
			currentLvl++;
			if (currentLvl > totalLevels)
			{
				currentLvl = 1;
			}

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
		stageText.text = "Stage " + currentLvl;
		elevatorAnim.SetBool("open", true);
		loading = false;
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
