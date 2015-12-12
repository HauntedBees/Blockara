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
		Sprite winText = Resources.LoadAll<Sprite>(SpritePaths.WinnerTexts)[PD.winType - 1];
		if(PD.winType == 2) {
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "042", 0);
		} else {
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "043", 0);
		}
		Sprite[] sheet = Resources.LoadAll<Sprite>(SpritePaths.CharFullShots);
		Sprite charSpr;
		if(PD.p1Char == PersistData.C.FuckingBalloon) {
			charSpr = sheet[32];
		} else if(PD.p1Char == PersistData.C.September) {
			charSpr = sheet[31];
		} else if(PD.p1Char == PersistData.C.White) {
			charSpr = sheet[30];
		} else {
			charSpr = sheet[(int)PD.p1Char * 3 + PD.winType];
		}
		pose = GetGameObject(new Vector3(0.0f, -0.2f), "win", charSpr, false, "HUDText");
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
		Sprite[] sheet = Resources.LoadAll<Sprite>(SpritePaths.CharFullShots);
		Sprite[] winText = Resources.LoadAll<Sprite>(SpritePaths.NewUnlocks);
		
		pose = GetGameObject(new Vector3(0.0f, -0.8f), "win", sheet[PD.unlockNew + 29], false, "HUDText");
		winTextGO = GetGameObject(new Vector3(0.0f, 1.0f), "win2", winText[PD.unlockNew - 1], false, "HUDText");
		winTextGO.transform.localScale = new Vector3(0.5f, 0.5f);
		PD.sounds.SetSoundAndPlay(SoundPaths.S_Applause + Random.Range(1, 7).ToString());
		PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
		isUnlock = true;
		unlockTimer = 60;
		PD.unlockNew = 0;
	}
}