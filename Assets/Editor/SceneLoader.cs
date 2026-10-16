using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneLoader
{
	private const string scenePath = "Assets/Scenes/Bootstrap.unity";
	private const string previousScenePathKey = "PreviousScenePath";
	static SceneLoader() =>
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingEditMode)
		{
			Scene currentScene = EditorSceneManager.GetActiveScene();
			if (currentScene.isDirty)
				EditorSceneManager.SaveScene(currentScene);

			EditorPrefs.SetString(previousScenePathKey, currentScene.path);
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