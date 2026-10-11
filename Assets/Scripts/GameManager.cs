using System;
using UnityEngine;


[CreateAssetMenu(fileName = "GameManager", menuName = "ScriptableObjects/GameManager", order = 1)]
public class GameManager : ScriptableObject
{
	public enum GameState
	{
		Gameplay,
		AssetMenu,
		Menu
	}

	[SerializeField]
	private GameState _currentState = GameState.Gameplay;
	private GameState _previousState;
	public GameState CurrentState
	{
		get => _currentState;
		set
		{
			OnGameStateChanged?.Invoke(value);
			_currentState = value;
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