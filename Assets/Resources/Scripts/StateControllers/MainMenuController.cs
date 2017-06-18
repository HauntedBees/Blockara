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
	private GameObject[] menuButtons, menuButtonHighlights;
	private GameObject title, character;
	private CutsceneChar charTalker;
	private int timeUntilDemo;
	private TextMesh pressButtonToStart;
	private TextMesh[] texts;
	private Sprite[] buttonSheet;
	private int konamiCodeState;
	private float delay;
	private bool expectingGamepad, expectingP2Gamepad;
	System.Xml.XmlNode top;
	public void Start() {
		StateControllerInit(false);
		top = GetXMLHead();
		expectingGamepad = false;
		expectingP2Gamepad = false;
		buttonSheet = Resources.LoadAll<Sprite>(SpritePaths.LongButtons);
		Screen.showCursor = true;
		PD.level = -1;
		PD.isDemo = false;
		selectedIdx = 0;
		buttonOpacityTime = 0;
		konamiCodeState = 0;
		delay = 0;
		title = GetGameObject(new Vector3(0.0f, 1.25f), "Blockara Title", Resources.Load<Sprite>(SpritePaths.LogoText), true);
		SetupCharacter();
		SetupAFuckingBalloon();
		SetupCopyright();
		if(PD.controller == null) { SetupTitle(); }
		else { SetupMenu(); }
	}
	private void SetupCopyright() {
		FontData f = PD.mostCommonFont.Clone(); f.color = Color.white;
		GetMeshText(new Vector3(1.35f, -1.8f), "Copyright \u00A9 2014-2017 Sean Finch/Haunted Bees Productions", f);
	}
	private void SetupCharacter() {
		SaveData sd = PD.GetSaveData();
		PD.p2Char = (PersistData.C) sd.GetTitleScreenCharacter();
		int winOffset = PD.GetPlayerSpriteStartIdx(PD.p2Char) * 3 + sd.getPlayerWinType(PD.GetPlayerSpritePath(PD.p2Char));
		if(PD.p2Char == PersistData.C.White) { winOffset = 30; } else if(PD.p2Char == PersistData.C.September) { winOffset = 31; }
		Sprite[] sheet = Resources.LoadAll<Sprite>(SpritePaths.CharFullShots);
		bool onRight = System.Array.IndexOf(new int[] {0, 3, 4, 7, 11, 13, 15, 16, 17, 19, 20, 22, 23, 24, 27, 29, 31}, winOffset) >= 0;
		float xOffset = onRight?2.5f:-2.5f;
		if(PD.p2Char != PersistData.C.Null) {
			PD.sounds.SetVoiceAndPlay(SoundPaths.VoicePath + PD.GetPlayerSpritePath(PD.p2Char) + "/" + "083", 0);
			character = GetGameObject(new Vector3(xOffset, -0.5f), "A Player Is You", sheet[winOffset], true, "Zapper");
			charTalker = new CutsceneChar(PD.GetPlayerSpritePath(PD.p2Char), character, null, 0, PD);
			if(PD.p2Char != PersistData.C.September && PD.p2Char != PersistData.C.White) { 
				PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(PD.p2Char));
			} else {
				PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + "White");
				if(PD.p2Char == PersistData.C.September) {
					PD.sounds.SetVoiceAndPlay(SoundPaths.VoicePath + "September/042", 0);
				} else {
					PD.sounds.SetVoiceAndPlay(SoundPaths.VoicePath + "White/032", 0);
				}
			}
		} else {
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "001", 0);
			PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_Default);
		}
	}
	private void SetupAFuckingBalloon() {
		if(PD.GetSaveData().savedOptions["beatafuckingballoon"] != 1) { return; }
		Sprite psprite = Resources.LoadAll<Sprite>(SpritePaths.CharFullShots)[32];
		GameObject pleaseDadCanIHgaaveOne = GetGameObject(new Vector3(-1.8f, 1.2f), "Puhloonverlay", psprite, false, "HUDText");
		pleaseDadCanIHgaaveOne.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
		pleaseDadCanIHgaaveOne.renderer.transform.localScale = new Vector2(0.5f, 0.45f);
	}
	private void SetupTitle() {
		string presstext = "";
		int gamepadCount = PD.GetGamepadsPresent();
		if(gamepadCount > 0) {
			expectingGamepad = true;
			if(gamepadCount > 1) { expectingP2Gamepad = true; }
			presstext = GetXmlValue(top, "starttextgamepad");
		} else {
			presstext = string.Format(GetXmlValue(top, "starttext"), PD.GetP1InputName(InputMethod.KeyBinding.launch), PD.GetP1InputName(InputMethod.KeyBinding.pause));
		}
		float texty = -0.05f;
		pressButtonToStart = GetMeshText(new Vector3(0.0f, texty), presstext, PD.mostCommonFont);
		pressButtonToStart.color = Color.white;
		timeUntilDemo = 600;
	}
	private void CleanupTitle() {
		Destroy(pressButtonToStart.gameObject);
		SetupMenu();
	}
	private void SetupMenu() {
		int cursx = PD.prevMainMenuLocationX, cursy = PD.prevMainMenuLocationY;
		if(cursx < 0) {
			if(PD.IsFirstTime()) {
				cursx = 1; cursy = 2;
			} else {
				cursx = 0;
			}
		}
		cursor = GetMenuCursor(2, 5, null, -0.5f, -1.32f, 0.2f, 0.2f, cursx, cursy);
		cursor.SetVisibility(false);
		float menuX = 0.0f, topy = 0.35f, dx = 0.8f, bottomdy = 1.2f;
		menuButtons = new GameObject[9];
		menuButtonHighlights = new GameObject[9];
		texts = new TextMesh[9];
		AddButton(0, menuX - dx, topy, GetXmlValue(top, "quickplay"));
		AddButton(1, menuX + dx, topy, GetXmlValue(top, "versus"));
		AddButton(2, menuX - dx, topy - 0.3f, GetXmlValue(top, "arcade"));
		AddButton(3, menuX + dx, topy - 0.3f, GetXmlValue(top, "campaign"));
		AddButton(4, menuX - dx, topy - 0.6f, GetXmlValue(top, "challenge"));
		AddButton(5, menuX + dx, topy - 0.6f, GetXmlValue(top, PD.IsFirstTime()?"t_tutorial":"training"));
		AddButton(6, menuX - dx, topy - 0.9f, GetXmlValue(top, "playerdata"));
		AddButton(7, menuX + dx, topy - 0.9f, GetXmlValue(top, "options"));
		AddButton(8, menuX, topy - bottomdy, GetXmlValue(top, "quit"));
	}

	private void AddButton(int idx, float x, float y, string text) {
		menuButtons[idx] = GetGameObject(new Vector3(x, y - 0.1f), text, buttonSheet[0], true, "HUD");
		menuButtonHighlights[idx] = GetGameObject(new Vector3(x, y - 0.1f), text, buttonSheet[1], false, "HUDPlusOne");
		menuButtonHighlights[idx].SetActive(false);
		texts[idx] = GetMeshText(new Vector3(x, y), text, PD.mostCommonFont);
	}

	private void CleanupMenu() {
		PD.controller = null;
		PD.controller2 = null;
		Destroy(cursor.cursor);
		cursor = null;
		for(int i = 0; i < 9; i++) { Destroy(texts[i].gameObject); Destroy(menuButtons[i]); Destroy(menuButtonHighlights[i]); }
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
		PD.usingGamepad1 = false;
		if(expectingGamepad) {
			for(int i = 0; i < 4; i++) {
				if(Input.GetKeyDown((KeyCode)(350 + 20 * i))) {
					PD.UpdateGamepad(0, i);
					PD.usingGamepad1 = true;
					if(expectingP2Gamepad) { PD.usingGamepad2 = true; }
					break;
				}
			}
		}
		PD.controller = PD.GetP1Controller();
		if(PD.controller != null) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); CleanupTitle(); }
	}
	private void UpdateMenu() {
		if(Input.GetKeyDown(KeyCode.End)) { PD.ChangeScreen(PersistData.GS.Credits); }
		if(cursor.back()) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny); CleanupMenu(); return; }
		KonamiCodeCheck();
		HandleMouse();
		cursor.DoUpdate();
		int oldIdx = selectedIdx;
		int cY = cursor.getY(), cX = cursor.getX();
		if(cY == 0) { cX = 0; }
		selectedIdx = 8 - cY * 2 + cX;
		if(selectedIdx != oldIdx) { menuButtonHighlights[oldIdx].SetActive(false); }
		menuButtonHighlights[selectedIdx].SetActive(true);
		menuButtonHighlights[selectedIdx].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, GetButtonOpacity());
		if(cursor.launchOrPause()) { isTransitioning = true; ConfirmSelectionAndAdvance(selectedIdx); }
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
		delay -= Time.deltaTime * 20.0f;
		if(delay > 0) { return; }
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
	}
	
	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
		HandleCharacterClick();
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
	private void HandleCharacterClick() {
		if(character == null) { return; }
		if(clicker.getPositionInGameObject(character).z == 1 && clicker.isDown()) {
			charTalker.SayThingFromReaction(CutsceneChar.SpeechType.doDamage);
		}
	}
	private int GetClickSelection() {
		for(int i = 0; i < 9; i++) {
			Vector3 pos = clicker.getPositionInGameObject(menuButtons[i]);
			if(pos.z == 1) { return i; }
		}
		return -1;
	}
}