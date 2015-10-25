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
public class WinScreenController:StateController {
	private int unlockTimer;
	private bool isUnlock;
	private GameObject winTextGO, pose;
	public void Start() {
		StateControllerInit(false);
		if(PD.p1Char == PersistData.C.FuckingBalloon) { PD.MoveFromWinScreen(); return; }
		if(PD.gameType == PersistData.GT.Challenge) {
			SetUpUnlock(true);
			return;
		}
		Sprite[] sheet, winTexts = Resources.LoadAll<Sprite>(SpritePaths.WinnerTexts);
		Sprite winText;
		if(PD.winType == 2) {
			sheet = Resources.LoadAll<Sprite>(SpritePaths.CharWins2);
			winText = winTexts[0];
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "042", 0);
		} else {
			sheet = Resources.LoadAll<Sprite>(SpritePaths.CharWins1);
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "043", 0);
			winText = winTexts[1];
		}
		Sprite charSpr;
		if(PD.p1Char == PersistData.C.September) {
			charSpr = Resources.LoadAll<Sprite>(SpritePaths.CharSeptWhite)[1];
		} else if(PD.p1Char == PersistData.C.White) {
			charSpr = Resources.LoadAll<Sprite>(SpritePaths.CharSeptWhite)[0];
		} else {
			charSpr = sheet[(int)PD.p1Char];
		}
		pose = GetGameObject(new Vector3(0.0f, -1.0f), "win", charSpr, false, "HUDText");
		pose.renderer.transform.localScale = new Vector3(0.5f, 0.5f);
		winTextGO = GetGameObject(new Vector3(0.0f, 1.0f), "win2", winText, false, "HUDText");
	}
	public void Update() {
		if(PD.p1Char == PersistData.C.FuckingBalloon) { return; }
		UpdateMouseInput();
		if(isUnlock) {
			if(unlockTimer-- > 0) { return; }
			if(clicker.isDown() || PD.controller.Pause() || PD.controller.G_Launch() || PD.controller.M_Confirm()) {
				if(PD.gameType == PersistData.GT.Challenge) { PD.ChangeScreen(PersistData.GS.PuzSel); }
				else { PD.MoveFromWinScreen(); }
			}
		} else {
			if(clicker.isDown() || PD.controller.Pause() || PD.controller.G_Launch() || PD.controller.M_Confirm()) {
				if(PD.unlockNew > 0) { SetUpUnlock(false); }
				else { PD.MoveFromWinScreen(); }
			}
		}
	}
	private void SetUpUnlock(bool fromPuzzle) {
		if(!fromPuzzle) {
			Destroy(pose);
			Destroy(winTextGO);
		}
		Sprite[] sheet = Resources.LoadAll<Sprite>(SpritePaths.CharSeptWhite);
		Sprite[] winText = Resources.LoadAll<Sprite>(SpritePaths.NewUnlocks);
		
		pose = GetGameObject(new Vector3(0.0f, -1.0f), "win", sheet[PD.unlockNew - 1], false, "HUDText");
		pose.renderer.transform.localScale = new Vector3(0.5f, 0.5f);
		winTextGO = GetGameObject(new Vector3(0.0f, 1.0f), "win2", winText[PD.unlockNew - 1], false, "HUDText");
		PD.sounds.SetSoundAndPlay(SoundPaths.S_Applause + Random.Range(1, 7).ToString());
		PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
		isUnlock = true;
		unlockTimer = 60;
		PD.unlockNew = 0;
	}
}