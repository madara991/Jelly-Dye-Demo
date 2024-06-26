using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public  static GameManager instance;
    public GameState gameState;

    public Action OnStartLevelEvent;
    public Action OnFinshLevelEvent; 

	public bool isStartPlaying;

	void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

    }
    private void OnEnable()
    {
        Invoke("AddOnStartDataLevel", 0.1f); // For Avoid Errors of frames work ( Awake and OnEnable functiions)
	}
    void AddOnStartDataLevel() => OnStartLevelEvent += GameData.Instance.SetupDataLevel;

    // Process when Press button
    public void OnStartLevel(int level)
    {
        isStartPlaying = true;

		GameData.Instance.SetLevelIndux(level);
		OnStartLevelEvent?.Invoke();

        gameState.gameObject.SetActive(true);
	}
    
    private void OnDisable()
	{
		OnStartLevelEvent -= GameData.Instance.SetupDataLevel;
	}

}
