using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMusic : MonoBehaviour
{

    private static SoundMusic _instance;
    public static SoundMusic Instance { get { return _instance; } }

    private void Start() {
        if (SoundMusic.Instance == null) SoundMusic._instance = this;
    }

    [System.Serializable]
    public class MusicInfo {
        public string track_name_maiusc;
        public AudioClip music;
    }

    public List<MusicInfo> soundtracks;

    public AudioClip getSoundtrack(string mapName) {
        foreach(MusicInfo mi in soundtracks) {
            if (string.Equals(mi.track_name_maiusc.ToUpper(), mapName.ToUpper().Substring(0, mapName.Length-1))) return mi.music;
            else Debug.Log("TRACK CHECKING: " + mi.track_name_maiusc.ToUpper() + " | ACTUAL TRACK: " + mapName.ToUpper().Substring(0, mapName.Length - 1) + " LONG " + (mapName.Length-1));
        }
        return null;
    }

    public void play(AudioClip ac) {
        Debug.LogWarning("Asking to play " + ac.name);
        AudioSource asource = this.gameObject.GetComponent<AudioSource>();
        asource.clip = ac;
        asource.Play();
    }

}
