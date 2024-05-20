using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour 
{
	[Header("State Game")]
	public  bool isWin;
	[HideInInspector] 
	public int CurrentLevel;

	[HideInInspector]
	public GameObject Jelly;
	private float rationComplete;
	public Vector3 jellyCallPosition; // position of jelly for instantiate it when start level 


	UiGame uiGame { get { return this.gameObject.GetComponent<UiGame>(); } }

	 void OnEnable()
	{
		
		GameManager.instance.OnFinshLevelEvent += uiGame.Exit;

		Jelly = GameData.Instance.jellyObject;
		Instantiate(Jelly, jellyCallPosition,Jelly.transform.rotation);
		Debug.Log("ins");
		GameData.Instance.EquipMyNeedle();

		rationComplete = 0;

		uiGame.SetParameters_UILevel();
	}
	// NOOOTE: IS THIS FUNCTION WORK ALWAYS WHEN ENABLE AND DISABLE ? CHECK
	void Start()
	{
		
	}
	void Update()
	{
		if (!GameManager.instance.isStartPlaying)
			return;

		if(rationComplete >= 50)
		{
			if (GameManager.instance.isStartPlaying)
			{
				uiGame.ShowNextButton();
				isWin = true;
			}	
		}
	}
	private void OnDisable()
	{
		GameManager.instance.OnFinshLevelEvent -= uiGame.Exit;
	}

	
}




