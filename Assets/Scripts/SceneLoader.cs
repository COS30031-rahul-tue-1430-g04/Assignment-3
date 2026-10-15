using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneLoader
{
	private const string scenePath = "Assets/Scenes/MainMenu.unity";
	private const string previousScenePathKey = "PreviousScenePath";
	static SceneLoader() =>
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingEditMode)
		{
			string previousScenePath = EditorSceneManager.GetActiveScene().path;
			EditorPrefs.SetString(previousScenePathKey, previousScenePath);
			EditorSceneManager.OpenScene(scenePath);
		}
		if (state == PlayModeStateChange.EnteredEditMode)
		{
			string previousScenePath = EditorPrefs.GetString(previousScenePathKey, string.Empty);
			if (!string.IsNullOrEmpty(previousScenePath))
				EditorSceneManager.OpenScene(previousScenePath);
		}
	}
}