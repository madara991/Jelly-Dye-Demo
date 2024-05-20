using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataLevel",menuName ="Jelly Game /New Data Level")]
public class DataLevelOS: ScriptableObject
{
	public int levelNumber;
	public GameObject JellyObject;
	public Sprite JellySprtie;
	public int ColorsNumber;
	public Color[] InjectionColors;

}
