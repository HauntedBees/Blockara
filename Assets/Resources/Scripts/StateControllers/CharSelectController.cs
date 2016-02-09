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
using System.Xml;
public class CharSelectController:MenuController {
	private GameObject bg1, bg2, charactersel, begin, beginText, cancel, charSprite1, charSprite2, chooseText, charName1, charName2, char2StartText;
	private OptionsSelector cursorOpDisplay;
	private OptionsCursor cursorOp;
	private MenuCursor cursor2;
	private bool conf1, conf1options, conf2;
	private Sprite[] chars, beginSheet, cancelSheet, charNames;
	private int p1_delay, p2_delay, p1eggState;
	private Rect originalRect;
	private XmlNode top;
	private List<OptionInfo> options;
	private float initX, dX;
	public void Start() {
		StateControllerInit();
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "022", 0);
		p1_delay = 0; p2_delay = 0; p1eggState = 0;
		conf1 = false; conf1options = false; conf2 = true;
		int completionPercent = PD.GetSaveData().CalculateGameCompletionPercent();
		string spPath = SpritePaths.CharSelSheet, crPath = SpritePaths.CharSelCursors;
		int crNum = 10; dX = 0.71f; initX = -3.2f;
		if(completionPercent == 100) {
			spPath = SpritePaths.CharSelSheetAll;
			crPath = SpritePaths.CharSelCursorsAll;
			crNum = 12;
			initX = -3.25f;
			dX = 0.591f;
		} else if(completionPercent >= 50) {
			spPath = SpritePaths.CharSelSheetWhite;
			crPath = SpritePaths.CharSelCursorsWhite;
			crNum = 11;
			initX = -3.21f;
			dX = 0.6455f;
		}
		charactersel = GetGameObject(new Vector3(0.0f, -0.99f), "Character Select", Resources.Load<Sprite>(spPath), true, "HUD");
		cursor = GetMenuCursor(crNum, 1, crPath, initX, -0.99f, dX, 0.0f, 0, 0, 1, 0, 0.2f);

		beginSheet = Resources.LoadAll<Sprite>(SpritePaths.ShortButtons);
		begin = GetGameObject(new Vector3(0.0f, 0.3f), "Begin", beginSheet[0], true, "HUD");
		FontData f = PD.mostCommonFont.Clone(); f.scale = 0.045f;
		XmlNode top = GetXMLHead();
		beginText = GetMeshText(new Vector3(0.0f, 0.45f), GetXmlValue(top, "begin"), f).gameObject;

		f.color = Color.white; f.scale = 0.035f;
		chooseText = GetMeshText(new Vector3(0.0f, 1.11f), GetXmlValue(top, "chooseyourcharacter"), f).gameObject;

		int yMax = InitOptionsTextAndReturnValueCount();
		cursorOp = GetOptionsCursor(1, yMax + 1, null, -0.2f, 0.45f, 0.0f, 0.25f, 0, yMax, 1);
		cursorOpDisplay = gameObject.AddComponent<OptionsSelector>();
		cursorOpDisplay.Setup(0.2f, 0.45f, 0.25f);
		cursorOpDisplay.SetVisibility(false);
		cursorOpDisplay.SetWidth(PD.gameType == PersistData.GT.Training?0.85f:0.6f);
		InitPlayer1Select();
		SetupBackgrounds();
		ToggleDisplayOptions(false);
		if(PD.gameType == PersistData.GT.Arcade) { AddVictoryIcons(); }
		else if(PD.gameType == PersistData.GT.Versus) {
			SetupP2StartText(top, f);
			conf2 = false;
		}
	}
	private void SetupP2StartText(XmlNode top, FontData f) {
		if(PD.controller2 != null) {
			FullInitP2Select();
			return;
		}
		string text = "";
		if(PD.usingGamepad2) {
			text = GetXmlValue(top, "starttext2Pgamepad");
		} else {
			text = string.Format(GetXmlValue(top, "starttext2P"), "\n", PD.GetP2InputName(InputMethod.KeyBinding.launch), PD.GetP2InputName(InputMethod.KeyBinding.pause));
		}
		char2StartText = GetMeshText(new Vector3(2.5f, 1.1f), text, f).gameObject;
	}
	private int InitOptionsTextAndReturnValueCount() {
		FontData f = PD.mostCommonFont.Clone(); 
		f.color = Color.white; f.align = TextAlignment.Right; f.anchor = TextAnchor.MiddleRight;
		top = GetXMLHead();
		options = new List<OptionInfo>();
		float bottomY = 0.7f;
		int currentY = 0;
		if(PD.gameType == PersistData.GT.QuickPlay || PD.gameType == PersistData.GT.Versus) {
			options.Add(CreateOptionSpot(f, bottomY, "rounds", OptionType.Rounds));
			bottomY += 0.25f; currentY++;
		}
		if(PD.gameType != PersistData.GT.Arcade) {
			options.Add(CreateOptionSpot(f, bottomY, "colheight", OptionType.RowDepth));
			bottomY += 0.25f; currentY++;
		}
		if(PD.gameType == PersistData.GT.Training) {
			options.Add(CreateOptionSpot(f, bottomY, "trainingmode", OptionType.TrainingMode));
			bottomY += 0.25f; currentY++;
		}
		if(PD.gameType == PersistData.GT.QuickPlay || PD.gameType == PersistData.GT.Campaign || PD.gameType == PersistData.GT.Arcade || PD.gameType == PersistData.GT.Versus) { 
			if(PD.gameType != PersistData.GT.Versus) {
				options.Add(CreateOptionSpot(f, bottomY, "difficulty", OptionType.Difficulty));
				bottomY += 0.25f; currentY++;
			}
			if(PD.gameType != PersistData.GT.Campaign) {
				options.Add(CreateOptionSpot(f, bottomY, "specialtiles", OptionType.Special));
				bottomY += 0.25f; currentY++;
			}
		}
		return options.Count;
	}

	private enum OptionType { Rounds = 0, Special = 1, RowDepth = 2, Difficulty = 3, TrainingMode = 4 }
	private struct OptionInfo {
		public GameObject titleText;
		public GameObject collider;
		public OptionType type;
		public int minVal, maxVal;
		public TextMesh tmesh;
		public int curVal;
		public OptionInfo(TextMesh text, GameObject titleText, GameObject collider, OptionType type, int curVal) {
			this.titleText = titleText;
			this.tmesh = text;
			this.collider = collider;
			this.type = type;
			this.curVal = curVal;
			switch(type) {
				case OptionType.Rounds: minVal = 1; maxVal = 9; break;
				case OptionType.Special: minVal = 0; maxVal = 1; break;
				case OptionType.RowDepth: minVal = 4; maxVal = 8; break;
				case OptionType.Difficulty: minVal = 1; maxVal = 9; break;
				case OptionType.TrainingMode: minVal = 0; maxVal = 2; break;
				default: minVal = 0; maxVal = 0; break;
			}
		} 
	}
	private void ToggleDisplayOptions(bool show) {
		begin.SetActive(show);
		beginText.SetActive(show);
		for(int i = 0; i < options.Count; i++) {
			options[i].titleText.SetActive(show);
			options[i].tmesh.gameObject.SetActive(show);
		}
		chooseText.SetActive(!show);
	}
	private OptionInfo CreateOptionSpot(FontData f, float y, string text, OptionType type) {
		int defaultValue = PD.GetCharSelOptionVal((int)type + "_" + (int)PD.gameType);
		TextMesh g = GetMeshText(new Vector3(0.0f, y), GetXmlValue(top, text), f);
		f.anchor = TextAnchor.MiddleLeft; f.align = TextAlignment.Left;
		TextMesh t = GetMeshText(new Vector3(0.4f, y), defaultValue.ToString(), f);
		if(type == OptionType.Special) { t.text = GetSpecialText(defaultValue); }
		else if(type == OptionType.TrainingMode) { t.text = GetTrainingModeText(defaultValue); }
		GameObject col = GetCollider("SpecCol", new Vector3(0.6f, y), 0.5555f, 1.15f);
		return new OptionInfo(t, g.gameObject, col, type, defaultValue);
	}

	private bool SwitchVal(int yplusone, int dir) {
		int y = yplusone - 1;
		if(dir == 0 || y < 0) { return false; }
		OptionInfo o = options[y];
		int newx = o.curVal + dir;
		if(o.type == OptionType.Rounds) { newx += dir; }
		if(newx < o.minVal || newx > o.maxVal) { SignalFailure(); return true; }
		SignalMovement();
		o.curVal = newx;
		if(o.type == OptionType.Special) { o.tmesh.text = GetSpecialText(newx); }
		else if(o.type == OptionType.TrainingMode) { o.tmesh.text = GetTrainingModeText(newx); }
		else { o.tmesh.text = newx.ToString(); }
		options[y] = o;
		return true;
	}
	private void UpdateOptionsSelectorArrowVisibility() {
		int cy = cursorOp.getY() - 1;
		if(cy < 0) {
			begin.GetComponent<SpriteRenderer>().sprite = beginSheet[1];
			cursorOpDisplay.ToggleArrowVisibility(false);
		} else {
			cursorOpDisplay.ToggleArrowVisibility(true);
			OptionInfo o = options[cy];
			cursorOpDisplay.HideAnArrowIfAtCorner(o.curVal, o.minVal, o.maxVal);
		}
	}
	private string GetSpecialText(int val) { return val == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	private string GetTrainingModeText(int val) {
		switch(val) {
			case 2: return GetXmlValue(top, "t_targets");
			case 1: return GetXmlValue(top, "t_normal");
			default: return GetXmlValue(top, "t_tutorial");
		}
	}

	private void InitPlayer1Select() {
		chars = Resources.LoadAll<Sprite>(SpritePaths.CharSelProfiles);
		charNames = Resources.LoadAll<Sprite>(SpritePaths.CharNames);
		charSprite1 = GetGameObject(new Vector3(-2.6f, 1.0f), "Player 1 Character", chars[0]);
		charSprite1.transform.localScale = new Vector3(0.7f, 0.7f);
		charName1 = GetGameObject(new Vector3(-1.6f, 0.15f), "Player 1 Name", charNames[0], false, "HUDText");
	}
	private void InitPlayer2Select() {
		charSprite2 = GetGameObject(new Vector3(2.6f, 1.0f), "Player 2 Character", chars[1]);
		charName2 = GetGameObject(new Vector3(1.6f, 0.15f), "Player 2 Name", charNames[1], false, "HUDText");
		charSprite2.transform.localScale = new Vector3(-0.7f, 0.7f);
	}
	private void SetupBackgrounds() {
		bg1 = GetGameObject(new Vector3(-2.25f, 0.5f), "Background 1", Resources.Load<Sprite> (SpritePaths.DefaultBG), false, "BG0");
		bg1.transform.localScale /= 2.5f;
		originalRect = bg1.GetComponent<SpriteRenderer>().sprite.rect;
		UpdateBackground(true);
		bg2 = GetGameObject(new Vector3(2.25f, 0.5f), "Background 2", Resources.Load<Sprite> (SpritePaths.DefaultBG), false, "BG0");
		bg2.transform.localScale /= 2.5f;
		UpdateBackground(false);
		GetGameObject(Vector3.zero, "Background Cover", Resources.Load<Sprite>(SpritePaths.BGBlackFadeCharSel), false, "BG1");
	}
	private void UpdateBackground(bool first) {
		string path = "";
		if(first) {
			path = PD.GetPlayerSpritePathFromInt(cursor.getX(), true);
		} else {
			if(cursor2 == null) { path = "Default"; }
			else { path = PD.GetPlayerSpritePathFromInt(cursor2.getX(), true); }
		}
		Sprite old = Resources.Load<Sprite> (SpritePaths.BGPath + path);
		Rect r = originalRect;
		float dx = 0.0f;
		if(!first) { dx += r.width * 0.5f; }
		Sprite cropped = Sprite.Create(old.texture, new Rect(r.x + dx, r.y, r.width * 0.5f, r.height), new Vector2(0.5f, 0.5f), 144.0f);
		if(first) {
			bg1.GetComponent<SpriteRenderer>().sprite = cropped;
		} else {
			bg2.GetComponent<SpriteRenderer>().sprite = cropped;
		}
	}
	private void AddVictoryIcons() {
		Sprite[] trophies = Resources.LoadAll<Sprite>(SpritePaths.CharSelVictoryIcons);
		List<KeyValuePair<string, int>> vals = PD.GetSaveData().characterWinsArcade;
		for(int i = 0; i < vals.Count; i++) {
			KeyValuePair<string, int> v = vals[i];
			switch(v.Key) {
				case "George": AddVictoryIcon(0, v.Value, trophies); break;
				case "Milo": AddVictoryIcon(1, v.Value, trophies); break;
				case "Devin": AddVictoryIcon(2, v.Value, trophies); break;
				case "MJ": AddVictoryIcon(3, v.Value, trophies); break;
				case "Andrew": AddVictoryIcon(4, v.Value, trophies); break;
				case "Joan": AddVictoryIcon(5, v.Value, trophies); break;
				case "Depeche": AddVictoryIcon(6, v.Value, trophies); break;
				case "Lars": AddVictoryIcon(7, v.Value, trophies); break;
				case "Laila": AddVictoryIcon(8, v.Value, trophies); break;
				case "AliceAna": AddVictoryIcon(9, v.Value, trophies); break;
				case "White": AddVictoryIcon(10, v.Value, trophies); break;
				case "September": AddVictoryIcon(11, v.Value, trophies); break;
			}
		}
	}
	private void AddVictoryIcon(int idx, int type, Sprite[] trophies) {
		if(type == 0) { return; }
		GameObject troph = GetGameObject(new Vector3(initX - dX * 0.65f + dX * (idx + 1), -1.85f), "Trophy for Character " + idx, trophies[type], false, "HUDText");
		troph.transform.localScale = new Vector3(1.5f, 1.5f);
	}

	public void Update() {
		if(isTransitioning) { return; }
		UpdateMouseInput();
		if(PD.usingMouse && isClickingBack()) { SignalSuccess(); PD.GoToMainMenu(); }
		HandlePlayer1Input();
		HandlePlayer2Input();
		if(conf1&&conf1options&&conf2) { AdvanceToGame(); }
	}
	private void AdvanceToGame() {
		isTransitioning = true;
		PD.rounds = 0;
		PD.totalRoundTime = 0;
		PD.totalP1RoundScore = 0;
		PD.totalP2RoundScore = 0;
		PD.useSpecial = false;
		PD.isTutorial = false;
		PD.rowCount = 6;
		for(int i = 0; i < options.Count; i++) {
			PD.SetCharSelOptionVal((int)options[i].type + "_" + (int)PD.gameType, options[i].curVal);
			switch(options[i].type) {
				case OptionType.Difficulty: PD.initialDifficulty = options[i].curVal; PD.difficulty = options[i].curVal; break;
				case OptionType.Rounds: PD.rounds = options[i].curVal; break;
				case OptionType.RowDepth: PD.rowCount = options[i].curVal; break;
				case OptionType.Special: PD.useSpecial = (options[i].curVal == 1); break;
				case OptionType.TrainingMode: PD.difficulty = options[i].curVal; PD.isTutorial = (options[i].curVal == 0); break;
			}
		}
		PD.playerOneWonRound = new List<bool>();
		PD.playerRoundScores = new List<int>();
		PD.playerRoundTimes = new List<int>();
		PD.currentRound = 1;
		if(PD.isTutorial) { PD.rowCount = 6; }
		PD.SetPlayer1(cursor.getX(), p1eggState == 3);
		if(cursor2 == null) { PD.SetPlayer2(-1); } else { PD.SetPlayer2(cursor2.getX()); }
		SignalSuccess();
		PD.CharacterSelectConfirmation(p1eggState == -3);
	}
	private void HandlePlayer1Input() {
		if(--p1_delay <= 0) {
			bool pressed = false;
			if(!conf1) {
				pressed = HandlePlayer1CursorInput();
			} else {
				pressed = HandleOptionsInput();
			}
			if(pressed) { p1_delay = 10; }
		}
	}
	private bool HandlePlayer1CursorInput() {
		bool pressed = false, clicked = false;
		if(PD.usingMouse) { clicked = HandleCharSelClicker(); }
		cursorOpDisplay.SetVisibility(false);
		if(cursor.back()) { SignalSuccess(); PD.GoToMainMenu(); return true; }
		cursor.DoUpdate();
		if(cursor.HasMoved()) { UpdateBackground(true); }
		charSprite1.GetComponent<SpriteRenderer>().sprite = chars[cursor.getX()];
		charName1.GetComponent<SpriteRenderer>().sprite = charNames[cursor.getX()];
		if(clicked || cursor.launchOrPause()) {
			conf1 = true;
			pressed = true;
			SignalSuccess();
			SpeakCharacterName(cursor.getX(), 0);
			if(cursor.getX() == 0 && p1eggState == 0) { p1eggState++; }
			else if(cursor.getX() == 2 && p1eggState == 1) { p1eggState++; }
			else if(cursor.getX() == 9 && p1eggState == 2) { p1eggState++; PD.sounds.SetSoundAndPlay(SoundPaths.S_Applause + Random.Range(1, 7).ToString()); }
			else if(cursor.getX() == 3 && p1eggState == 0) { p1eggState--; }
			else if(cursor.getX() == 5 && p1eggState == -1) { p1eggState--; }
			else if(cursor.getX() == 7 && p1eggState == -2) { p1eggState--; PD.sounds.SetSoundAndPlay(SoundPaths.S_Applause + Random.Range(1, 7).ToString()); }
			else { p1eggState = 0; }
			ToggleDisplayOptions(true);
			AddCancelButton();
		}
		return pressed;
	}
	private void SpeakCharacterName(int idx, int playerSelecting) {
		int narratorIndex = 24 + idx;
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + narratorIndex.ToString("d3"), 0);
		int val = Random.Range(1, 4);
		PD.sounds.QueueVoice(SoundPaths.VoicePath + PD.GetPlayerSpritePathFromInt(idx) + "/" + val.ToString("d3"));
	}
	private bool HandleOptionsInput() {
		if(cursor.back()) {
			if(!conf1options) { 
				SignalFailure();
				RemoveCancelButton(); 
				ToggleDisplayOptions(false);
				conf1 = false; 
			} else if(PD.gameType != PersistData.GT.Arcade) { 
				conf1options = false; 
				SignalFailure();
			}
			return true;
		}
		bool pressed = false;
		begin.GetComponent<SpriteRenderer>().sprite = beginSheet[0];
		if(PD.usingMouse) { HandleOptionsClicker(); }
		cursorOpDisplay.SetVisibility(true);
		cursorOp.DoUpdate();
		cursorOpDisplay.UpdatePosition(cursorOp.getY());
		pressed = SwitchVal(cursorOp.getY(), cursorOp.leftP() ? -1 : (cursorOp.rightP() ? 1 : 0));
		if(cursorOp.launchOrPause()) {
			if((PD.gameType == PersistData.GT.Versus && cursorOp.getY() == 1) || cursorOp.getY() == 0) {
				conf1options = true;
			} else {
				cursorOp.setY(0);
				pressed = true;
			}
		}
		UpdateOptionsSelectorArrowVisibility();
		return pressed;
	}
	private void HandlePlayer2Input() {
		if(cursor2 != null) {
			if(--p2_delay <= 0) {
				bool pressed = false;
				if(!conf2) {
					cursor2.DoUpdate();
					if(cursor2.HasMoved()) { UpdateBackground(false); }
					charSprite2.GetComponent<SpriteRenderer>().sprite = chars[cursor2.getX()];
					charName2.GetComponent<SpriteRenderer>().sprite = charNames[cursor2.getX()];
					if(cursor2.launchOrPause()) { SpeakCharacterName(cursor2.getX(), 1); SignalSuccess(); conf2 = true; pressed = true; } 
				} else if(cursor2.back()) {
					conf2 = false;
					pressed = true;
					SignalFailure();
				}
				if(pressed) { p2_delay = 5; }
			}
		} else if(PD.gameType == PersistData.GT.Versus && PD.controller2 == null) {
			PD.controller2 = PD.detectInput_P2();
			if(PD.controller2 != null) { 
				char2StartText.SetActive(false);
				FullInitP2Select();
				SignalSuccess();
			}
		}
	}
	private void FullInitP2Select() {
		int completionPercent = PD.GetSaveData().CalculateGameCompletionPercent();
		string crPath = SpritePaths.CharSelCursors;
		int crNum = 10;
		if(completionPercent == 100) {
			crPath = SpritePaths.CharSelCursorsAll;
			crNum = 12;
		} else if(completionPercent >= 50) {
			crPath = SpritePaths.CharSelCursorsWhite;
			crNum = 11;
		}
		cursor2 = GetMenuCursor(crNum, 1, crPath, initX, -0.99f, dX, 0.0f, 1, 0, 2, 1, 0.2f);
		InitPlayer2Select();
		UpdateBackground(false);
	}
	private void AddCancelButton() {
		float x = cursor.getPos(cursor.getX(), 0).x;
		cancelSheet = Resources.LoadAll<Sprite>(SpritePaths.CancelButtons);
		cancel = GetGameObject(new Vector3(x + 0.2f, cursor.cursor.transform.position.y + 0.85f), "Cancel", cancelSheet[0], true, "HUDText");
		cancel.SetActive(PD.usingMouse);
		mouseObjects.Add(cancel);
	}
	private void RemoveCancelButton() {
		mouseObjects.Remove(cancel);
		Destroy(cancel);
		cancel = null;
	}

	private bool HandleCharSelClicker() {
		Vector3 p = clicker.getPositionInGameObject(charactersel);
		if(p.z == 0.0f) { return false; }
		int nx = -1;
		if(cursor.GetWidth() == 10) {
			if(p.x > 2.8f) { nx = 9; }
			else if(p.x > 2.1f) { nx = 8; }
			else if(p.x > 1.4f) { nx = 7; }
			else if(p.x > 0.7f) { nx = 6; }
			else if(p.x > 0.0f) { nx = 5; }
			else if(p.x > -0.7f) { nx = 4; }
			else if(p.x > -1.4f) { nx = 3; }
			else if(p.x > -2.1f) { nx = 2; }
			else if(p.x > -2.8f) { nx = 1; } 
			else { nx = 0; }
		} else if(cursor.GetWidth() == 11) {
			if(p.x > 2.95f) { nx = 10; }
			else if(p.x > 2.3f) { nx = 9; }
			else if(p.x > 1.65f) { nx = 8; }
			else if(p.x > 1.0f) { nx = 7; }
			else if(p.x > 0.35f) { nx = 6; }
			else if(p.x > -0.3f) { nx = 5; }
			else if(p.x > -0.95f) { nx = 4; }
			else if(p.x > -1.6f) { nx = 3; }
			else if(p.x > -2.25f) { nx = 2; }
			else if(p.x > -2.9f) { nx = 1; } 
			else { nx = 0; }
		} else if(cursor.GetWidth() == 12) {
			if(p.x > 2.9f) { nx = 11; }
			else if(p.x > 2.35f) { nx = 10; }
			else if(p.x > 1.8f) { nx = 9; }
			else if(p.x > 1.2f) { nx = 8; }
			else if(p.x > 0.6f) { nx = 7; }
			else if(p.x > 0.0f) { nx = 6; }
			else if(p.x > -0.6f) { nx = 5; }
			else if(p.x > -1.2f) { nx = 4; }
			else if(p.x > -1.8f) { nx = 3; }
			else if(p.x > -2.35f) { nx = 2; }
			else if(p.x > -2.9f) { nx = 1; } 
			else { nx = 0; }
		}
		cursor.setX(nx);
		return clicker.isDown();
	}
	private void HandleOptionsClicker() {
		if(HandleCancelAndReturnIfFound()) { return; }
		if(HandleBeginAndReturnIfFound()) { return; }
		for(int y = 0; y < options.Count; y++) {
			if(options[y].collider == null) { continue; }
			Vector3 p = clicker.getPositionInGameObject(options[y].collider);
			if(p.z == 0) { continue; }
			cursorOp.setY(y + 1);
			int dx = 0;
			if(clicker.getPositionInGameObject(cursorOpDisplay.leftArrow).z == 1.0f) {
				cursorOpDisplay.HighlightArrow(false);
				dx--; 
			} else if(clicker.getPositionInGameObject(cursorOpDisplay.rightArrow).z == 1.0f) {
				cursorOpDisplay.HighlightArrow(true);
				dx++;
			} else {
				cursorOpDisplay.ClearArrows();
			}
			if(!clicker.isDown()) { break; }
			SwitchVal(y + 1, dx);
		}
	}

	private bool HandleCancelAndReturnIfFound() {
		if(cancel == null) { return false; }
		Vector3 p = clicker.getPositionInGameObject(cancel);
		if(p.z == 0) { cancel.GetComponent<SpriteRenderer>().sprite = cancelSheet[0]; return false; }
		cancel.GetComponent<SpriteRenderer>().sprite = cancelSheet[1];
		if(clicker.isDown()) {
			conf1 = false;
			conf1options = false;
			SignalFailure();
			RemoveCancelButton();
			ToggleDisplayOptions(false);
		}
		return true;
	}
	private bool HandleBeginAndReturnIfFound() {
		if(begin == null) { return false; }
		Vector3 p = clicker.getPositionInGameObject(begin);
		if(p.z == 0) { return false; }
		cursorOp.setY(0);
		begin.GetComponent<SpriteRenderer>().sprite = beginSheet[1];
		if(clicker.isDown()) {
			conf1 = true;
			conf1options = true;	
		}
		return true;
	}
}