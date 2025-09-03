using UnityEditor.Timeline.Actions;
using UnityEngine;
using Yarn.Unity;

public class CinematicManager : MonoBehaviour
{
    [YarnCommand("playSound")]
    public static void playSound(string soundName)
    {
        Debug.Log("测试成功" + soundName);
        var clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        }
        else
        {
            Debug.LogError($"Sound '{soundName}' not found in Resources/Sounds/");
        }
        
    }

    [YarnCommand("test")]
    public static void TestCommandStatic()
    {
        Debug.Log("测试成功");
    }
}