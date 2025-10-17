using UnityEngine;

public class TypewriterSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip typingClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private int playEveryNLetters = 2; // 每几个字播放一次

    private int letterCounter = 0;
    

    public void OnLetterPrinted()
    {
        letterCounter++;
        if (letterCounter % playEveryNLetters == 0)
        {
            typingAudioSource.PlayOneShot(typingClip, volume);
        }
    }

    public void ResetCounter()
    {
        letterCounter = 0;
    }
}