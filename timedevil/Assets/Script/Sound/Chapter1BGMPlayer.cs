using UnityEngine;

public class Chapter1BGMPlayer : MonoBehaviour
{
    public AudioClip chapter1BGM;

    private void Start()
    {
        BGMManager.instance.PlayBGM(chapter1BGM);
    }
}
