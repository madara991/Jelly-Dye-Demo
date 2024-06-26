using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameData : MonoBehaviour
{
	public static GameData Instance;
	
	[SerializeField] 
	private DataLevelOS[] dataLevelSO;

	
	private GameObject _myNeedle;
	public GameObject[] allNeedls;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else Destroy(this.gameObject);

	}
	// ((On Join)) Data Of Level
	public int levelJoined          { get; private set; }
	public GameObject jellyObject   { get; private set; }
    public Sprite jellySprite       { get; private set; }
	public int colorsNumber         { get; private set; }
	public Color[] injectionColors  { get; private set; }


	public Dictionary<int, bool> luckLevelDictionary = new Dictionary<int, bool>(); // <Level , luck or unluck>
	
	public void SetNeedlesIndux(int indux)
	{
		_myNeedle = allNeedls[indux];
	}
	public GameObject GetNeedle() // Get Needle that Equip
	{
		return _myNeedle;
	}
	public void EquipMyNeedle() 
	{
		if (_myNeedle == null)
			_myNeedle = allNeedls[0];

		foreach (var needle in allNeedls)
		{
			needle.SetActive(needle == _myNeedle);
		}
	} 
	public void SetLevelIndux(int indux)
	{
		levelJoined = indux;
	}
	public int GetLevelIndux()
	{
		return levelJoined;
	}


	
	public void SetupDataLevel() // Setup (data level) before join level
	{
		int levelNumber = GetLevelIndux();

		levelJoined = levelNumber;

		jellyObject     = dataLevelSO[levelNumber - 1].JellyObject;
		jellySprite     = dataLevelSO[levelNumber - 1].JellySprtie;
		colorsNumber    = dataLevelSO[levelNumber - 1].ColorsNumber;
		injectionColors = dataLevelSO[levelNumber - 1].InjectionColors;
	} 

	
}
