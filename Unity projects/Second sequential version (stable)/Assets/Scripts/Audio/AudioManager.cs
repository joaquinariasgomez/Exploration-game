using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public Sound[] sounds;

    public AstronautManager astronautManager;
    public static AudioManager instance;
    private GameObject[] astronauts;

    private int stepCounter = 0;

    private void AssignSoundToObject(Sound sound)
    {
        if (sound.type == "Step")       //Astronaut's Step
        {
            foreach (GameObject astronaut in astronauts)
            {
                astronaut.AddComponent<AudioSource>();

                astronaut.GetComponents<AudioSource>()[stepCounter].clip = sound.clip;
                astronaut.GetComponents<AudioSource>()[stepCounter].volume = sound.volume;
                astronaut.GetComponents<AudioSource>()[stepCounter].pitch = sound.pitch;
                astronaut.GetComponents<AudioSource>()[stepCounter].loop = sound.loop;
                astronaut.GetComponents<AudioSource>()[stepCounter].spatialBlend = 1;
            }
            stepCounter++;
        }
        /*else
        {
            sound.source = gameObject.AddComponent<AudioSource>();  //Añadir al mundo en sí
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }*/
    }

	// Use this for initialization
	void Awake () {

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);  //Para que no se corte entre carga de escenas

        astronauts = astronautManager.astronauts;

		foreach(Sound sound in sounds)
        {
            AssignSoundToObject(sound);
        }
	}

    private void Start()
    {
        //Play("MainTheme");
    }

    public void PlayStep(int astronautId, int stepId)
    {
        AudioSource step = astronauts[astronautId].GetComponents<AudioSource>()[stepId];
        step.Play();
    }

    public void StopStep(int astronautId, int stepId)
    {
        AudioSource step = astronauts[astronautId].GetComponents<AudioSource>()[stepId];
        step.Stop();
    }

    public bool isPlayingStep(int astronautId, int stepId)
    {
        AudioSource step = astronauts[astronautId].GetComponents<AudioSource>()[stepId];
        return step.isPlaying;
    }

    /*public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public bool isPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.isPlaying;
    }*/
}
