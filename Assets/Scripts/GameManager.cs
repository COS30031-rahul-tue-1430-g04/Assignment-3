using System;
using UnityEngine;


[CreateAssetMenu(fileName = "GameManager", menuName = "ScriptableObjects/GameManager", order = 1)]
public class GameManager : ScriptableObject
{
	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = Resources.Load<GameManager>("GameManager");
			return _instance;
		}
	}

	public enum GameState
	{
		Gameplay,
		AssetMenu,
		Menu,
		Paused
	}

	[SerializeField]
	private GameState _currentState = GameState.Gameplay;
	private GameState _previousState;
	public GameState CurrentState
	{
		get => _currentState;
		set
		{
			_currentState = value;
			OnGameStateChanged?.Invoke(_currentState);
		}
	}

	void OnValidate()
	{
		if (_currentState != _previousState)
		{
			OnGameStateChanged?.Invoke(_currentState);
			_previousState = _currentState;
		}
	}

	public event Action<GameState> OnGameStateChanged;
}