using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sounds[] sounds;
    public static AudioManager instance;

    void Awake(){
        if(instance == null){
            instance = this;
        }
        foreach(Sounds s in sounds){
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.Volume;
            s.source.pitch = s.Pitch;
        }
    }

    public void Play(string name){
        Sounds s = Array.Find(sounds, sound => sound.Name == name);
        s.source.Play();
    }
}
