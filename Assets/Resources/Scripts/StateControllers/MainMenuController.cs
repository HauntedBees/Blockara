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
public class MainMenuController:MenuController {
	private GameObject[] menuButtons;
	private GameObject title;
	private int timeUntilDemo;
	private TextMesh pressButtonToStart;
	private TextMesh[] texts;
	private Sprite[] buttonSheet;
	private int konamiCodeState, delay;
	System.Xml.XmlNode top;
	public void Start() {
		StateControllerInit(false);
		top = GetXMLHead();
		buttonSheet = Resources.LoadAll<Sprite>(SpritePaths.LongButtons);
		Screen.showCursor = true;
		PD.level = -1;
		PD.isDemo = false;
		selectedIdx = 0;
		konamiCodeState = 0;
		delay = 0;
		SetupBGAndTitleText();
		SetupCharacter();
		if(PD.controller == null) { SetupTitle(); }
		else { SetupMenu(); }
	}
	private void SetupCharacter() {
		SaveData sd = PD.GetSaveData();
		PD.p2Char = (PersistData.C) sd.GetTitleScreenCharacter();
		int winType = sd.getPlayerWinType(PD.GetPlayerSpritePath(PD.p2Char));
		string winPath = (winType == 2)?SpritePaths.CharWins2:SpritePaths.CharWins1;
		if(PD.p2Char == PersistData.C.White || PD.p2Char == PersistData.C.September) { winPath = SpritePaths.CharSeptWhite; }
		Sprite[] sheet = Resources.LoadAll<Sprite>(winPath);
		float xOffset = (int)PD.p2Char%2==0?2.5f:-2.5f;
		if(PD.p2Char == PersistData.C.Everyone) {
			PD.sounds.SetVoiceAndPlay(SoundPaths.S_AllShout, 0);
			PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
			GetGameObject(new Vector3(0.0f, -0.5f), "A Player Is Everyone!", Resources.Load<Sprite>(SpritePaths.CharGroupShot), false, "Zapper");
		} else if(PD.p2Char != PersistData.C.Null) {
			PD.sounds.SetVoiceAndPlay(SoundPaths.VoicePath + PD.GetPlayerSpritePath(PD.p2Char) + "/" + "083", 0);
			if(PD.p2Char != PersistData.C.September && PD.p2Char != PersistData.C.White) { 
				GetGameObject(new Vector3(xOffset, -0.5f), "A Player Is You", sheet[(int)PD.p2Char], false, "Zapper");
				PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(PD.p2Char));
			} else {
				GetGameObject(new Vector3(xOffset, -0.5f), "A Player Is You", sheet[(PD.p2Char == PersistData.C.White)?0:1], false, "Zapper");
				PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
			}
		} else {
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "001", 0);
			PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_Default);
		}
	}
	private void SetupBGAndTitleText() {
		GetGameObject(Vector3.zero, "Gradient BG Cover", Resources.Load<Sprite>(SpritePaths.BGBlackFade), false, "BG1");
		title = GetGameObject(new Vector3(0.0f, 1.25f), "Blockara Title", Resources.Load<Sprite>(SpritePaths.LogoText), true);
	}
	private void SetupTitle() {
		string presstext = string.Format(GetXmlValue(top, "starttext"), PD.GetP1InputName(InputMethod.KeyBinding.launch), PD.GetP1InputName(InputMethod.KeyBinding.pause));
		float texty = -0.05f;
		if(PD.p2Char == PersistData.C.Everyone) { texty = -1.8f; }
		pressButtonToStart = GetMeshText(new Vector3(0.0f, texty), presstext, PD.mostCommonFont);
		pressButtonToStart.color = Color.white;
		timeUntilDemo = 800;
	}
	private void CleanupTitle() {
		Destroy(pressButtonToStart.gameObject);
		SetupMenu();
	}
	private void SetupMenu() {
		cursor = GetMenuCursor(2, 5, null, -0.5f, -1.32f, 0.2f, 0.2f, PD.prevMainMenuLocationX, PD.prevMainMenuLocationY);
		cursor.SetVisibility(false);
		float menuX = 0.0f, topy = 0.35f;
		menuButtons = new GameObject[9];
		texts = new TextMesh[9];
		AddButton(0, menuX - 0.8f, topy, GetXmlValue(top, "quickplay"));
		AddButton(1, menuX + 0.8f, topy, GetXmlValue(top, "versus"));
		AddButton(2, menuX - 0.8f, topy - 0.3f, GetXmlValue(top, "arcade"));
		AddButton(3, menuX + 0.8f, topy - 0.3f, GetXmlValue(top, "campaign"));
		AddButton(4, menuX - 0.8f, topy - 0.6f, GetXmlValue(top, "challenge"));
		AddButton(5, menuX + 0.8f, topy - 0.6f, GetXmlValue(top, "training"));
		AddButton(6, menuX - 0.8f, topy - 0.9f, GetXmlValue(top, "playerdata"));
		AddButton(7, menuX + 0.8f, topy - 0.9f, GetXmlValue(top, "options"));
		AddButton(8, menuX, topy - 1.2f, GetXmlValue(top, "quit"));
	}

	private void AddButton(int idx, float x, float y, string text) {
		menuButtons[idx] = GetGameObject(new Vector3(x, y - 0.1f), text, buttonSheet[0], true, "HUD");
		texts[idx] = GetMeshText(new Vector3(x, y), text, PD.mostCommonFont);
	}

	private void CleanupMenu() {
		PD.controller = null;
		Destroy(cursor.cursor);
		cursor = null;
		for(int i = 0; i < 9; i++) { Destroy(texts[i].gameObject); Destroy(menuButtons[i]); }
		texts = null;
		SetupTitle();
	}

	public void Update() {
		if(isTransitioning) { return; }
		UpdateMouseInput();
		if(PD.controller == null) { UpdateTitle(); }
		else { UpdateMenu(); }
	}
	private void UpdateTitle() {
		if(--timeUntilDemo < 0) { isTransitioning = true; PD.MoveToDemo(); }
		PD.controller = PD.GetP1Controller();
		if(PD.controller != null) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); CleanupTitle(); }
	}
	private void UpdateMenu() {
		if(cursor.back()) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny); CleanupMenu(); return; }
		KonamiCodeCheck();
		DebugShit();
		HandleMouse();
		cursor.DoUpdate();
		menuButtons[selectedIdx].GetComponent<SpriteRenderer>().sprite = buttonSheet[0];
		int cY = cursor.getY(), cX = cursor.getX();
		if(cY == 0) { cX = 0; }
		selectedIdx = 8 - cY * 2 + cX;
		menuButtons[selectedIdx].GetComponent<SpriteRenderer>().sprite = buttonSheet[1];
		if(cursor.launchOrPause()) { ConfirmSelectionAndAdvance(selectedIdx); }
	}
	private void ConfirmSelectionAndAdvance(int pos) {
		int invertPos = 8 - pos;
		PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
		PD.prevMainMenuLocationX = cursor.getX();
		PD.prevMainMenuLocationY = cursor.getY();
		switch(invertPos) {
			case 0: Application.Quit(); break;
			case 1: PD.MainMenuConfirmation(PersistData.GT.Options); break;
			case 2: PD.MainMenuConfirmation(PersistData.GT.PlayerData); break;
			case 3: PD.MainMenuConfirmation(PersistData.GT.Training); break;
			case 4: PD.MainMenuConfirmation(PersistData.GT.Challenge); break;
			case 5: PD.MainMenuConfirmation(PersistData.GT.Campaign); break;
			case 6: PD.MainMenuConfirmation(PersistData.GT.Arcade); break;
			case 7: PD.MainMenuConfirmation(PersistData.GT.Versus); break;
			case 8: PD.MainMenuConfirmation(PersistData.GT.QuickPlay); break;
			default: break;
		}
	}
	private void KonamiCodeCheck() {
		if(--delay > 0) { return; }
		if(PD.controller.Nav_Up() && konamiCodeState < 2) { delay = PD.KEY_DELAY; konamiCodeState++; return; }
		if(PD.controller.Nav_Down() && konamiCodeState < 4) { delay = PD.KEY_DELAY; konamiCodeState++; return; }
		if(PD.controller.Nav_Left() && (konamiCodeState == 4 || konamiCodeState == 6)) { delay = PD.KEY_DELAY; konamiCodeState++; return; }
		if(PD.controller.Nav_Right()) { 
			if(konamiCodeState == 5) { delay = PD.KEY_DELAY; konamiCodeState++; return; }
			if(konamiCodeState == 7) {
				delay = PD.KEY_DELAY;
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
				PD.InhaleHelium();
			}
		}
		if(PD.controller.Nav_Up() || PD.controller.Nav_Right() || PD.controller.Nav_Left() || PD.controller.Nav_Down()) { konamiCodeState = 0; }
	}
	
	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
		Vector3 inTitle = clicker.getPositionInGameObject(title);
		if(inTitle.z == 1 && Mathf.Abs(inTitle.x) > 1.5f && Mathf.Abs(inTitle.y) < 0.3f && clicker.isDown()) {
			if(PD.GetSaveData().savedOptions["beatafuckingballoon"] == 1) {
				PD.p1Char = (PersistData.C) Random.Range(0, 10);
				PD.MoveToBalloonBattle();
			}
		}
		int initv = GetClickSelection();
		if(initv < 0) { return false; }
		int v = 9 - initv;
		int x = v % 2, y = (v-x)/2;
		if(x == 0) { x = 1; } else { x = 0; }
		cursor.setX(x); cursor.setY(y);
		if(clicker.isDown()) { ConfirmSelectionAndAdvance(initv); }
		return false;
	}
	private int GetClickSelection() {
		for(int i = 0; i < 9; i++) {
			Vector3 pos = clicker.getPositionInGameObject(menuButtons[i]);
			if(pos.z == 1) { return i; }
		}
		return -1;
	}
	
	private void DebugShit() {
		if(PD.controller.EnableCheat1()) {  
			Debug.Log("CHECK!"); 
			PD.override2P = !PD.override2P;
		}
	}
}