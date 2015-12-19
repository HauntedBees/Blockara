/*Copyright 2015 Sean Finch

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/
using UnityEngine;
using System.Collections.Generic;
public class AudioContainerContainer:MonoBehaviour { // I named the class this to spite the Free software community for not making a Free equivalent of Unity yet
	private AudioContainer music;
	private AudioContainer[] sounds, voices;
	private string nextNarratorVoice;
	private const int soundLen = 8;
	public void Init(float musVol, float sndVol, float voiVol, float voicePitch = 1.0f) {
		music = new AudioContainer(new GameObject("aud_music"), musVol);
		sounds = new AudioContainer[soundLen];
		for(int i = 0; i < soundLen; i++) { sounds[i] = new AudioContainer(new GameObject("aud_sound_" + i), sndVol); }
		voices = new AudioContainer[2];
		for(int i = 0; i < 2; i++) { voices[i] = new AudioContainer(new GameObject("aud_voice_" + i), voiVol, voicePitch); }
		nextNarratorVoice = "";
	}
	public void SetMusicAndPlay(string path, bool loop = true) {
		music.SetClip(Resources.Load<AudioClip>(path), loop);
		music.Play();
	}
	public void SetSoundAndPlay(string path) {
		int idx = 0;
		for(int i = 0; i < soundLen; i++) { if(!sounds[i].isPlaying()) { idx = i; break; } }
		SetSoundAndPlayAtIdx(path, idx);
	}
	private void SetSoundAndPlayAtIdx(string path, int idx) {
		sounds[idx].SetClip(Resources.Load<AudioClip>(path));
		sounds[idx].Play();
	}
	public void SetVoiceAndPlay(string path, int player) {
		voices[player].SetClip(Resources.Load<AudioClip>(path));
		voices[player].Play();
		if(nextNarratorVoice.Contains("#")) {
			string toDestroy = nextNarratorVoice.Split(new char[1] {'#'})[0] + "#";
			nextNarratorVoice = nextNarratorVoice.Replace(toDestroy, "");
		} else {
			nextNarratorVoice = "";
		}
	}
	public void SetPitchP2() { voices[1].SetDoug(); }
	public void Update() {
		if(nextNarratorVoice == "" || voices[0].isPlaying()) { return; }
		string nextVal = nextNarratorVoice;
		if(nextNarratorVoice.Contains("#")) {
			nextVal = nextNarratorVoice.Split(new char[1] {'#'})[0];
		}
		SetVoiceAndPlay(nextVal, 0);
	}
	public void QueueVoice(string path) { if(string.IsNullOrEmpty(nextNarratorVoice)) { nextNarratorVoice = path; } else { nextNarratorVoice += "#" + path; } }
	public void ResumeMusic() { music.Play(); }
	public void PauseMusic() { music.Pause(); }
	public void StopMusic() { music.Stop(); }
	public bool IsVoicePlaying() { return voices[0].isPlaying(); }
	public bool IsMusicPlaying() { return music.isPlaying(); }
	public float GetMusicPlaytime() { return music.GetTrackPlayTime(); }
	public float GetMusicLength() { return music.GetTrackLength(); }
	public void HalveMusicVolume() { music.HalveVolume(); }
	public void SetMusicVolume(float v) { music.SetVolume(v); }
	public void SetSoundVolume(float v) { for(int i = 0; i < soundLen; i++) { sounds[i].SetVolume(v); } }
	public void SetVoiceVolume(float v) { for(int i = 0; i < 2; i++) { voices[i].SetVolume(v); } } 
}