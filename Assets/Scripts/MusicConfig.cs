using UnityEngine;

public class MusicConfig : MonoBehaviour
{
    public enum MusicMode
    {
        // Stops the current song and plays the specified clip.
        Override,
        // Keeps the current song playing.
        KeepCurrent,
        // Stops the current song.
        Stop
    }

    [Header("Music")]
    public MusicMode mode = MusicMode.Override;

    [Tooltip("Used when Mode is Override.")]
    public AudioClip musicClip;

    public bool loop = true;
}
