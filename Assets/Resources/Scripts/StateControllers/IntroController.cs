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
public class IntroController:StateController {
	private int countdown;
	private bool justWiped;
	private bool hasBeed;
	public void Start() {
		Screen.showCursor = false;
		countdown = 160; 
		justWiped = false;
		hasBeed = false;
		GetPersistData();
	}
	public void Update() {
		if(!hasBeed) {
			PD.SetupSound();
			PD.sounds.SetSoundAndPlay(SoundPaths.A_BEEEEEE);
			hasBeed = true;
		}
		DebugShit(); 
		int input = PD.ReturnLaunchOrPauseOrNothingIsPressed();
		if(--countdown < 0 || input > 0) { if(input != 2) {PD.ChangeScreen(PersistData.GS.OpeningScene); } else { PD.GoToMainMenu(); } }
	}
	private void DebugShit() {
		if(Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.Z)) { justWiped = true; PD.WipeData(); }
		if(justWiped) {
			GameObject bee = GameObject.Find("beeLogo") as GameObject;
			bee.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), 1.0f);
		}
	}
}