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
using System;
using System.Collections.Generic;
using UnityEngine;
public class SoundTest:All {
	private const int playlistShown = 12;
	private float offsetx, offsety, keyDelay;
	private Sprite[] playPauseButtonSprite;
	private GameObject soundTestBG, playerDataBG, playPauseButton;
	private GameObject[] playlistColliders;
	private TextMesh playlist, titleText, trackinfo;
	private int topIdx, cursorIdx, playingIdx, dy;
	private int[] visualizer;
	private string trackLength;
	private bool isPlaying, isActive;
	private List<TrackInfo> playlistEntries;
	public void Setup(float ox, float oy, GameObject infobox) {
		GetPersistData();
		playingIdx = -1; isPlaying = false;
		offsetx = ox; offsety = oy; playerDataBG = infobox;
		soundTestBG = GetGameObject(new Vector3(offsetx, offsety), "SoundTestBG", Resources.Load<Sprite>(SpritePaths.SoundTest), false, "HUD");
		FontData f = new FontData(TextAnchor.UpperLeft, TextAlignment.Left, 0.025f); f.color = new Color(0.62f, 0.69f, 0.95f);
		playlist = GetMeshText(new Vector3(-2.3f + offsetx, 0.72f + offsety), "", f, "Prefabs/Text/FixedWidth");
		f.scale = 0.02f;
		keyDelay = 12;
		titleText = GetMeshText(new Vector3(0.35f + offsetx, 0.7f + offsety), ":3 hey friend", f, "Prefabs/Text/FixedWidth");
		trackinfo = GetMeshText(new Vector3(0.25f + offsetx, 0.2f + offsety), "bite my shiny taint", f, "Prefabs/Text/FixedWidth");
		InitPlaylist();
		SetPlaylistText();
		isActive = false;
		playPauseButtonSprite = Resources.LoadAll<Sprite>(SpritePaths.SoundTestControlButtons);
		playPauseButton = GetGameObject(new Vector3(1.3f + offsetx, 0.4f + offsety), "pause/play", playPauseButtonSprite[0], true, "HUDText");
	}
	public void ToggleVisibility(bool show) {
		soundTestBG.SetActive(show);
		playlist.gameObject.SetActive(show);
		titleText.gameObject.SetActive(show);
		trackinfo.gameObject.SetActive(show);
		playPauseButton.SetActive(show);
		playerDataBG.SetActive(!show);
		for(int y = 0; y < playlistShown; y++) { playlistColliders[y].SetActive(show); }
		if(show) { 
			PD.sounds.SetMusicVolume(1.0f);
			PD.sounds.StopMusic();
		} else if(isActive) {
			PD.sounds.SetMusicVolume(PD.GetSaveData().savedOptions["vol_m"] / 100.0f);
			PD.sounds.SetMusicAndPlay(SoundPaths.M_Menu);
		}
		isActive = show;
	}
	public bool HandleMouseInput(MouseCore clicker) {
		int selected = -1;
		if(clicker.getPositionInGameObject(playPauseButton).z > 0.0f && clicker.isDown()) {
			if(isPlaying) {
				PD.sounds.PauseMusic();
			} else {
				PD.sounds.ResumeMusic();
			}
			return true;
		}
		for(int y = 0; y < playlistShown; y++) { if(clicker.getPositionInGameObject(playlistColliders[y]).z > 0.0f) { selected = y; break; } }
		if(selected < 0) { return false; }
		cursorIdx = selected;
		if(clicker.isDown()) {
			if(selected == 0 && playlist.text.StartsWith("...")) { dy--; }
			else if(selected == playlistShown - 1 && playlist.text.EndsWith("...")) { dy++; }
			else { SetupTrack(); }
		}
		return clicker.isDown();
	}
	public bool DoUpdate(bool acceptKeyboardInput, InputMethod c) {
		bool forceSwitch = false;
		if(acceptKeyboardInput) {
			keyDelay -= Time.deltaTime * 60.0f;
			if(keyDelay <= 0) {
				if(c.Nav_Down()) { cursorIdx++; keyDelay = PD.KEY_DELAY; } else if(c.Nav_Up()) { cursorIdx--; keyDelay = PD.KEY_DELAY; }
				else if(c.M_Cancel()) { return false; }
				else if(c.M_Confirm() || c.Pause()) {
					if(cursorIdx == 0 && playlist.text.StartsWith("...")) { dy--; }
					else if(cursorIdx == playlistShown - 1 && playlist.text.EndsWith("...")) { dy++; }
					else if(playingIdx != (topIdx + cursorIdx)) { SetupTrack(); }
					else if(isPlaying) { PD.sounds.PauseMusic(); } else { PD.sounds.ResumeMusic(); }
					keyDelay = PD.KEY_DELAY;
				} else if(c.G_ShiftLeft()) {
					topIdx -= playlistShown;
					if(topIdx < 0) { topIdx = 0; }
					keyDelay = PD.KEY_DELAY;
					forceSwitch = true;
				} else if(c.G_ShiftRight()) {
					topIdx += playlistShown - 2;
					if(topIdx >= (playlistEntries.Count - playlistShown)) { topIdx = playlistEntries.Count - playlistShown; }
					keyDelay = PD.KEY_DELAY;
					forceSwitch = true;
				} else if(c.G_ShiftAllLeft()) {
					topIdx = 0;
					keyDelay = PD.KEY_DELAY;
					forceSwitch = true;
				} else if(c.G_ShiftAllRight()) {
					topIdx = playlistEntries.Count - playlistShown;
					keyDelay = PD.KEY_DELAY;
					forceSwitch = true;
				}
				if(cursorIdx >= playlistShown) {
					cursorIdx = playlistShown - 1;
					dy++;
				} else if(cursorIdx < 0) {
					cursorIdx = 0;
					dy--;
				}
			}
		}
		for(int y = 0; y < playlistShown; y++) { playlistColliders[y].renderer.material.color = (y == cursorIdx ? Color.white : Color.clear); }
		if(topIdx > 0 && dy < 0) { topIdx--; }
		else if(topIdx < (playlistEntries.Count - playlistShown) && dy > 0) { topIdx++; }
		else { dy = 0; }
		if(dy != 0 || forceSwitch) { SetPlaylistText(); }
		dy = 0;
		isPlaying = PD.sounds.IsMusicPlaying();
		playPauseButton.GetComponent<SpriteRenderer>().sprite = isPlaying ? playPauseButtonSprite[1] : playPauseButtonSprite[0];
		SetTrackText();
		return true;
	}

	private void SetupTrack() {
		playingIdx = cursorIdx + topIdx;
		TrackInfo val = playlistEntries[playingIdx];
		PD.sounds.SetMusicAndPlay(val.path, val.loop);
		float len = PD.sounds.GetMusicLength();
		int mins = Mathf.FloorToInt(len / 60);
		int secs = Mathf.FloorToInt(len - (mins * 60));
		trackLength = mins.ToString("d2") + ":" + secs.ToString("d2");
		visualizer = new int[] {  UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 15), UnityEngine.Random.Range(5, 15) };
		isPlaying = true;
		SetTrackText();
	}
	private void SetTrackText() {
		if(playingIdx < 0) { titleText.text = "---"; trackinfo.text = ""; return; }
		TrackInfo val = playlistEntries[playingIdx];
		titleText.text = val.title;
		float len = PD.sounds.GetMusicPlaytime();
		int mins = Mathf.FloorToInt(len / 60);
		int secs = Mathf.FloorToInt(len - mins * 60);
		trackinfo.text = mins.ToString("d2") + ":" + secs.ToString("d2") + "/" + trackLength + "\r\n\r\n - " + val.artist +  "\r\n\r\n" + GetVisualizer();
	}
	private string GetVisualizer() {
		string vis = "";
		for(int y = 0; y < visualizer.Length; y++) {
			int v = visualizer[y];
			if(UnityEngine.Random.value > 0.5f && isPlaying) { v++; } else { v--; }
			if(v < 0) { v = 0; } else if(v > 20) { v = 20; }
			visualizer[y] = v;
			vis += new string('|', v) + "\r\n";
		}
		return vis;
	}
	private void SetPlaylistText() {
		string res = "";
		int offset = 0;
		if(topIdx > 0) { offset += 1; res = "...\r\n"; }
		for(int i = topIdx + offset; i < (topIdx + playlistShown - 1); i++) {
			res += GetFuckingDickCUNTSHITFUCKGOGSHIT(i);
		}
		if(topIdx + playlistShown < playlistEntries.Count) {
			res += "...";
		} else {
			res += GetFuckingDickCUNTSHITFUCKGOGSHIT(topIdx + playlistShown - 1);
		}
		playlist.text = res;
	}
	private string GetFuckingDickCUNTSHITFUCKGOGSHIT(int idx) {
		string displayValue = (idx + 1).ToString("d3") + ". " + playlistEntries[idx].title;
		if(displayValue.Length > 18) { displayValue = displayValue.Substring(0, 15) + "..."; }
		return displayValue + "\r\n";
	}

	private struct TrackInfo {
		public string title, artist, path;
		public bool loop;
		public TrackInfo(string t, string a,  string p, bool l = true) { title = t; loop = l; artist = a; path = p; }
	}
	private void InitPlaylist() {
		playlistColliders = new GameObject[playlistShown];
		Sprite collider = Resources.Load<Sprite>(SpritePaths.SoundTestCollider);
		for(int y = 0; y < playlistShown; y++) {
			playlistColliders[y] = GetGameObject(new Vector3(-1.21f + offsetx, 0.66f + offsety - 0.12f * y), "collider", collider, true, "HUDText");
			playlistColliders[y].renderer.material.color = Color.clear;
		}
		playlistColliders[0].renderer.material.color = Color.white;
		topIdx = 0; cursorIdx = 0; dy = 0;
		playlistEntries = new List<TrackInfo>();
		string artist = "Michio Poppleton";
		playlistEntries.Add(new TrackInfo("Blockara!", artist, SoundPaths.M_Title_Default));
		playlistEntries.Add(new TrackInfo("Menu", artist, SoundPaths.M_Menu));
		playlistEntries.Add(new TrackInfo("In-Game", artist, SoundPaths.M_InGame));
		playlistEntries.Add(new TrackInfo("In-Game (HxC)", artist, SoundPaths.M_InGame_Intense));
		playlistEntries.Add(new TrackInfo("Cutscene", artist, SoundPaths.M_Cutscene));
		playlistEntries.Add(new TrackInfo("Credits", artist, SoundPaths.M_Credits));
		playlistEntries.Add(new TrackInfo("George's Ballad", artist, SoundPaths.M_Title_DerivPath + "George"));
		playlistEntries.Add(new TrackInfo("Milo's Rally", artist, SoundPaths.M_Title_DerivPath + "Milo"));
		//playlistEntries.Add(new TrackInfo("Devin's Shanty", artist, SoundPaths.M_Title_DerivPath + "HOLD"));
		playlistEntries.Add(new TrackInfo("M.J.'s Beat", artist, SoundPaths.M_Title_DerivPath + "MJ"));
		playlistEntries.Add(new TrackInfo("Andrew's Funk", artist, SoundPaths.M_Title_DerivPath + "Andrew"));
		playlistEntries.Add(new TrackInfo("Joan's Rock", artist, SoundPaths.M_Title_DerivPath + "Joan"));
		playlistEntries.Add(new TrackInfo("MODE SONG.FLAC", artist, SoundPaths.M_Title_DerivPath + "Depeche"));
		playlistEntries.Add(new TrackInfo("Lars' Rhythm", artist, SoundPaths.M_Title_DerivPath + "Lars"));
		playlistEntries.Add(new TrackInfo("Laila's Honk", artist, SoundPaths.M_Title_DerivPath + "Laila"));
		//playlistEntries.Add(new TrackInfo("Alice/Ana's Screech", artist, SoundPaths.M_Title_DerivPath + "Depeche"));
		playlistEntries.Add(new TrackInfo("Do You Remember?", artist, SoundPaths.M_Title_DerivPath + "White"));
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("George " + i.ToString("d2"), "Cal Young", SoundPaths.VoicePath + "George/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Milo " + i.ToString("d2"), "Abner Hauge", SoundPaths.VoicePath + "Milo/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Devin " + i.ToString("d2"), "Abner Hauge", SoundPaths.VoicePath + "Devin/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("M.J. " + i.ToString("d2"), "Matt Simpson", SoundPaths.VoicePath + "MJ/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Andrew " + i.ToString("d2"), "Matt Simpson", SoundPaths.VoicePath + "Andrew/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Joan " + i.ToString("d2"), "Carolina Madrid", SoundPaths.VoicePath + "Joan/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("MODE " + i.ToString("d2"), "espeak -ven+f3", SoundPaths.VoicePath + "Depeche/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Lars " + i.ToString("d2"), "Sean Mabry", SoundPaths.VoicePath + "Lars/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Laila " + i.ToString("d2"), "Carolina Madrid", SoundPaths.VoicePath + "Laila/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 83; i++) { playlistEntries.Add(new TrackInfo("Alice/Ana " + i.ToString("d2"), "Veronica Christie", SoundPaths.VoicePath + "AliceAna/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 32; i++) { playlistEntries.Add(new TrackInfo("White " + i.ToString("d2"), "Dylan Aiello", SoundPaths.VoicePath + "White/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 42; i++) { playlistEntries.Add(new TrackInfo("September " + i.ToString("d2"), "Carolina Madrid", SoundPaths.VoicePath + "September/" + i.ToString("d3"), false)); }
		for(int i = 1; i <= 46; i++) { playlistEntries.Add(new TrackInfo("Narrator " + i.ToString("d2"), "Sean Mabry", SoundPaths.VoicePath + "Narrator/" + i.ToString("d3"), false)); }
	}
}