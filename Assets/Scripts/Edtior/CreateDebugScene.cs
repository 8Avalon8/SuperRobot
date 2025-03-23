using SuperRobot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CreateDebugScene
{
    [MenuItem("超级机器人/创建调试场景")]
    public static void CreateScene()
    {
        // 创建空场景
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 创建必要的GameObject
        GameObject debugController = new GameObject("DebugController");
        debugController.AddComponent<GameDebugPanel>();
        
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        GameObject gameInitializer = new GameObject("GameInitializer");
        gameInitializer.AddComponent<GameInitializer>();
        
        // 保存场景
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/DebugScene.unity");
        
        Debug.Log("调试场景已创建");
    }
}