using UnityEngine;

public class MusicConfig : MonoBehaviour
{
    public enum MusicMode
    {
        Override,
        KeepCurrent,
        Stop
    }

    [Header("Music")]
    public MusicMode mode = MusicMode.Override;

    [Tooltip("Used when Mode is Override.")]
    public AudioClip musicClip;

    public bool loop = true;
}
