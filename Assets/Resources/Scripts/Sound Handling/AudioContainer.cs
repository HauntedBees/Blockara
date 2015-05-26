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
public class AudioContainer {
	private GameObject parent;
	private AudioSource source;
	private float pitch;
	public AudioContainer(GameObject g, float volume, float p = 1.0f) {
		parent = g;
		source = parent.AddComponent<AudioSource>();
		source.volume = volume;
		pitch = p;
	}
	public void SetClip(AudioClip c, bool looping = false) { source.clip = c; source.pitch = pitch;  source.loop = looping; }
	public void Play() { source.pitch = pitch; source.Play(); }
	public void Pause() { source.Pause(); }
	public void Stop() { source.Stop(); }
	public bool isPlaying() { return source.isPlaying; }
	public void SetVolume(float v) { source.volume = v; }
	public void HalveVolume() { source.volume /= 2; }
	public float GetTrackLength() { return source.clip.length; }
	public float GetTrackPlayTime() { return source.time; }
}