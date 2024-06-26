using StableFluids;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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

	public Image origenalImage;
	public Image EditedImage;

	public int numberColors;
	public Color[] ColorsContainers;
	public Transform contactColors;
	public GameObject colorContainer_Pref;
	public Color colorEquiped;
	public void SetParameters_UILevel()
	{

		levelText.text = "Level " + (GameData.Instance.GetLevelIndux() + 1).ToString();
		jellySprite.sprite = GameData.Instance.jellySprite;
		numberColors = GameData.Instance.colorsNumber;

		origenalImage.sprite = GameData.Instance.jellySprite;

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
			int indux = i;
			container.GetComponent<Button>().onClick.AddListener(() => ChoiceColor(indux));
			
			
		}
		
	}
	
	public void ShowNextButton()
	{
		NextLevelButton.SetActive(true);
	}

	// button event
	public void OnShowWinnerPanale()
	{
		EditedImage.sprite = GetComponent<ScreenCapture>().CaptureScreenshot();

	}
    // linked with OnFinshEvent action
	public void Exit()
	{
		Debug.Log("Exit");
		GameManager.instance.isStartPlaying = false;
		gameState.CurrentLevel = GameData.Instance.GetLevelIndux();

		if (gameState.isWin)
		{
			GameData.Instance.luckLevelDictionary[gameState.CurrentLevel] = true;
			luckLevel();
			gameState.isWin = false;

		}

		mainMenu.SetActive(true);

		if (gameState.jellyClone != null)
			Debug.Log("is exist");
		Destroy(gameState.jellyClone);

		for (int i = 0; i < contactColors.childCount; i++)
		{
			Destroy(contactColors.GetChild(i).gameObject);
		}

		
		this.gameObject.SetActive(false);
	}
	public void OnFinshLevel()
	{
		Debug.Log("OnFinshLevel method called");
		GameManager.instance.OnFinshLevelEvent?.Invoke();
	}
	
	public void ChoiseNeedle(int indux)
	{
		GameData.Instance.SetNeedlesIndux(indux);
		Debug.Log("choice needle: " + indux);
		Debug.Log("my needle now: " + GameData.Instance.GetNeedle().name);
	}
	public void ChoiceColor(int indux)
	{
		var colorChoised = ColorsContainers[indux];
		var myNeedle = GameData.Instance.GetNeedle();
		myNeedle.GetComponent<PlayerControler>().colorEquiped = colorChoised;
		myNeedle.GetComponent<PlayerControler>().isReloading = true;
		GameObject.FindGameObjectWithTag("camera rendering").GetComponent<Fluid>().fluidColor = ColorsContainers[indux];
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
