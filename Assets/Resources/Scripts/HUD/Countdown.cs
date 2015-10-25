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
public class Countdown:ScriptableObject {
	public bool goDisplayed;
	private bool isDoneAnimating;
	private PersistData PD;
	private GameObject display;
	private Sprite[] textSprites;
	private int countdownState;
	private float lastCheck;
	public void Setup(PersistData P) {
		PD = P;
		lastCheck = Time.time;
		goDisplayed = false;
		isDoneAnimating = false;
		countdownState = 0;
		display = Instantiate(PD.universalPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		display.renderer.sortingLayerName = "Cover HUD Dialog Text";
		display.name = "Countdown";
		textSprites = Resources.LoadAll<Sprite>(SpritePaths.Texts);
		display.GetComponent<SpriteRenderer>().sprite = textSprites[2];
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "019", 0);
	}
	public void DoUpdate() {
		if(isDoneAnimating) { return; }
		if(Time.time >= lastCheck + (countdownState == 3?1.0f:1.5f)) { 
			lastCheck = Time.time;
			if(++countdownState < 2) {
				display.GetComponent<SpriteRenderer>().sprite = textSprites[3];
				PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "020", 0);
				goDisplayed = true;
				if(PD.gameType == PersistData.GT.Versus) {
					switch(Random.Range (0, 3)) {
						case 0: PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(PD.p1Char, false, true)); break;
						case 1: PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(PD.p2Char, false, true)); break;
						default: PD.sounds.SetMusicAndPlay(SoundPaths.M_InGame); break;
					}
				} else if(PD.gameType == PersistData.GT.Arcade && PD.level == 4) {
					PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(PD.p2Char));
				} else if(PD.gameType == PersistData.GT.Arcade && PD.level > 5 && PD.difficulty > 8) {
					PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
				} else if((PD.gameType == PersistData.GT.Arcade && PD.level > 5) || PD.difficulty > 6) {
					PD.sounds.SetMusicAndPlay(SoundPaths.M_InGame_Intense);
				} else {
					PD.sounds.SetMusicAndPlay(SoundPaths.M_InGame);
				}
			} else {
				isDoneAnimating = true;
				Destroy(display);
			}
		}
	}
}