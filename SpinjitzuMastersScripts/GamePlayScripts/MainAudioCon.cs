using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainAudioCon : MonoBehaviour
{
    public AudioClip[] gameSoundClips, masterWoClips, mainBackGroundMusicClips, bossClips, winClips;
    public AudioSource  soundFx, bossSounds, masterWoSounds, mainBackGroundMusic, gameSounds, spinJitzuSound;
    public void PlayBGAudio(float vol, int clips)
    {
        mainBackGroundMusic.volume = vol;
        mainBackGroundMusic.clip = mainBackGroundMusicClips[clips];
        mainBackGroundMusic.Play();
    }
    public void PlayWinClips(int clips)
    {
        masterWoSounds.clip = winClips[clips];
        masterWoSounds.Play();
    }
    public void PlayWuAudio(int clips)
    {
        masterWoSounds.clip = masterWoClips[clips];
        masterWoSounds.Play();
    }
    public void PlayGameSounds(int clips, bool loop)
    {
        if(clips<= gameSoundClips.Length)
        {
            gameSounds.clip = gameSoundClips[clips];
            gameSounds.Play();
        }        
    }
    public void BossSoundPlay(int clip, bool random)
    {
        if (random)
        {
            clip = Random.Range(0, bossClips.Length);
        }
        bossSounds.clip = bossClips[clip];
        bossSounds.Play();
    }
}
