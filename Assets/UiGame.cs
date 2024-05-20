using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiGame: MonoBehaviour 
{
	GameState gameState { get { return this.gameObject.GetComponent<GameState>(); } }
	
	[Header("UI Main Menu")]
	public  Transform[] LevelsElements;
	public GameObject[] NeedlesElemnts;
	public GameObject mainMenu;

	[Header("UI GamePlay")]
	public GameObject NextLevelButton;
	public GameObject WinnerPanal;
	
	public Text levelText;
	public Image jellySprite;

	public int numberColors;
	public Color[] ColorsContainers;
	public Transform contactColors;
	public GameObject colorContainer_Pref;
	public void SetParameters_UILevel()
	{

		levelText.text = "Level " + (GameData.Instance.GetLevelIndux() + 1).ToString();
		jellySprite.sprite = GameData.Instance.jellySprite;
		numberColors = GameData.Instance.colorsNumber;

		SetColorsContainers();

	}
	void SetColorsContainers()
	{
		numberColors = GameData.Instance.colorsNumber;
		ColorsContainers = GameData.Instance.injectionColors;


		for (int i = 0; i < numberColors; i++)
		{
			
			GameObject container = Instantiate(colorContainer_Pref, contactColors);
			container.GetComponent<Image>().color = ColorsContainers[i];
		}
	}
	public void ShowNextButton()
	{
		NextLevelButton.SetActive(true);
	}

	void ShowWinnerPanale()
	{
		WinnerPanal.SetActive(true);
		NextLevelButton.SetActive(false);

	}
    // linked with OnFinshEvent action
	public void Exit()
	{
		GameManager.instance.isStartPlaying = false;
		gameState.CurrentLevel = GameData.Instance.GetLevelIndux();

		if (gameState.isWin)
		{
			GameData.Instance.luckLevelDictionary[gameState.CurrentLevel] = true;
			luckLevel();
			gameState.isWin = false;

		}

		mainMenu.SetActive(true);

		Destroy(gameState.Jelly);

		for (int i = 0; i < contactColors.childCount; i++)
		{
			Destroy(contactColors.GetChild(i).gameObject);
		}

		
		this.gameObject.SetActive(false);
	}
	public void OnFinshLevel()
	{
		GameManager.instance.OnFinshLevelEvent?.Invoke();
	}

	public void ChoiseNeedle(int indux)
	{
		GameData.Instance.SetNeedlesIndux(indux);
	}

	void luckLevel()
	{
		int _levelCurrent = gameState.CurrentLevel;
		LevelsElements[_levelCurrent].GetChild(0).gameObject.SetActive(false);
		LevelsElements[_levelCurrent].GetChild(1).gameObject.SetActive(true);
	}
	void BuyNeedle(int Indux)
	{
		NeedlesElemnts[Indux].transform.GetChild(0).gameObject.SetActive(false);
		NeedlesElemnts[Indux].transform.GetChild(1).gameObject.SetActive(true);
	}
}
