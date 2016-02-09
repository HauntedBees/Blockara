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
using System.Collections;
public class IntroController:StateController {
	private int countdown;
	private bool hasBeed, hasSaved;
	private GameObject bee;
	public void Start() {
		Screen.showCursor = false;
		bee = GameObject.Find("beeLogo") as GameObject;
		bee.SetActive(false);
		countdown = 160; 
		hasBeed = false;
		hasSaved = false;
		GetPersistData();
		StartCoroutine(InitSave());
	}
	private IEnumerator InitSave() {
		yield return new WaitForSeconds(1.5f);
		hasSaved = true;
		bee.SetActive(true);
	}
	public void Update() {
		if(!hasSaved) { return; }
		if(!hasBeed) {
			PD.SetupSound();
			PD.sounds.SetVoiceAndPlay(SoundPaths.A_BEEEEEE, 0);
			hasBeed = true;
		}
		int input = PD.ReturnLaunchOrPauseOrNothingIsPressed();
		if(--countdown < 0 || input > 0) { if(input != 2) { PD.ChangeScreen(PersistData.GS.OpeningScene); } else { PD.GoToMainMenu(); } }
	}
}