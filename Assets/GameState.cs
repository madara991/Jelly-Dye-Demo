using StableFluids;
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
	
	public GameObject jellyClone;
	public static float rationComplete;
	public Vector3 jellyCallPosition; // position of jelly for instantiate it when start level 
	
	UiGame uiGame { get { return this.gameObject.GetComponent<UiGame>(); } }

	 void OnEnable()
	 {
		GameManager.instance.OnFinshLevelEvent += uiGame.Exit;
		Jelly = GameData.Instance.jellyObject;
		Instantiate(Jelly, jellyCallPosition,Jelly.transform.rotation);
		jellyClone = GameObject.FindGameObjectWithTag("jelly");

		var camereaRenderer = GameObject.FindGameObjectWithTag("camera rendering");
		camereaRenderer.GetComponent<Fluid>().TargetObject = jellyClone;
		camereaRenderer.GetComponent<Fluid>().SetTextureInitlil();
		
		GameData.Instance.EquipMyNeedle();

		rationComplete = 0;

		uiGame.SetParameters_UILevel();
	}
	private void Start()
	{
		// we make "GameState" (enable , disable) when join level , so we dont need start function 
	}
	void Update()
	{
		if (!GameManager.instance.isStartPlaying)
			return;

		if(rationComplete >= 3)
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




