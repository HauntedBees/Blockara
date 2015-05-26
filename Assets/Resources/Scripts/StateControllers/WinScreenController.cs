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
	public void Start() {
		StateControllerInit(false);
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
		GameObject pose = GetGameObject(new Vector3(0.0f, -1.0f), "win", sheet[(int)PD.p1Char], false, "HUDText");
		pose.renderer.transform.localScale = new Vector3(0.5f, 0.5f);
		GetGameObject(new Vector3(0.0f, 1.0f), "win2", winText, false, "HUDText");
	}
	public void Update() { UpdateMouseInput(); if(clicker.isDown() || PD.controller.Pause() || PD.controller.G_Launch() || PD.controller.M_Confirm()) { PD.MoveFromWinScreen(); } }
}