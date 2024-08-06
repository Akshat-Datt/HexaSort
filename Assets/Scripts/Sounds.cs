using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
[System.Serializable]

public class Sounds
{
    public string Name;
    public AudioClip clip;
    [Range(0f, 2f)]
    public float Volume;
    [Range(0f, 3f)]
    public float Pitch;
    public AudioSource source;
}
