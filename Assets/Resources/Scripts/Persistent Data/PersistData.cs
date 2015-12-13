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
public class PersistData:MonoBehaviour {
	public enum GT { QuickPlay = 0, Arcade = 1, Campaign = 2, Versus = 3, Training = 4, Challenge = 5, PlayerData = 6, Options = 7 }
	public enum GS { Intro = 0, MainMenu = 1, CharSel = 2, Game = 3, PuzSel = 4, CutScene = 5, HighScore = 6, PlayerData = 7, Credits = 8, WinnerIsYou = 9, Options = 10,
					 RoundWinner = 11, OpeningScene = 12 }
	public enum C { Null = -1, George = 0, Milo, Devin, MJ, Andrew, Joan, Depeche, Lars, Laila, AliceAna, White, September, Everyone, FuckingBalloon }
	public C p1Char, p2Char;
	private GS currentScreen;
	public GT gameType;
	public int unlockNew, demoPlayers, level, puzzleType, initialDifficulty, difficulty, rounds, currentRound, rowCount, rowCount2, totalRoundTime, totalP1RoundScore, totalP2RoundScore, winType, runningScore, runningTime, prevMainMenuLocationX, prevMainMenuLocationY, balloonType;
	public bool won, useSpecial, isTutorial, isDemo, override2P, isTransitioning, aboutToFightAFuckingBalloon, usingMouse;
	public bool usingGamepad1, usingGamepad2;
	public List<bool> playerOneWonRound;
	public List<int> playerRoundScores, playerRoundTimes;
	public InputMethod controller, controller2;
	private SaveData saveInfo;
	public FontData mostCommonFont;
	public AudioContainerContainer sounds;
	public GameObject universalPrefab;
	public string culture = "en";
	public int KEY_DELAY;
	void Start() {
		Object.DontDestroyOnLoad(this);
		universalPrefab = Resources.Load<GameObject>("Prefabs/Tile");
		Object.DontDestroyOnLoad(universalPrefab);
		Texture2D t = Resources.Load<Texture2D>(SpritePaths.MouseCursor);
		Cursor.SetCursor(t, Vector2.zero, CursorMode.ForceSoftware);
		prevMainMenuLocationX = -1;
		prevMainMenuLocationY = 4;
		p1Char = C.Null;
		p2Char = C.Null;
		usingGamepad1 = false;
		usingGamepad2 = false;
		initialDifficulty = 4;
		difficulty = 4;
		unlockNew = 0;
		isDemo = false;
		dontFade = false;
		isTransitioning = false;
		mostCommonFont = new FontData(TextAnchor.UpperCenter, TextAlignment.Center, 0.03f);
		SetupFadeVars();
		LoadGeemu();
		SaveGeemu();
		KEY_DELAY = saveInfo.savedOptions["keydelay"];
		SetRes();
		override2P = false;
		timeA = Time.timeSinceLevelLoad;
	}
	#region "Tile Bank"
	public List<Tile> TileBank;
	public void InitTileBank() { TileBank = new List<Tile>(); }
	public void ClearTileBank() { for(int i = 0; i < TileBank.Count; i++) { TileBank[i].CleanGameObjects(true); Destroy(TileBank[i].gameObject); } TileBank.Clear(); }
	#endregion
	#region "Controller Setup"
	private bool IsKeyboardRegisteringAsGamepad() { return Input.GetJoystickNames()[0].IndexOf("abcdefg") >= 0; }
	public int GetGamepadsPresent() {
		string[] gamepads = Input.GetJoystickNames();
		if(gamepads.Length == 0) { return 0; }
		bool keyboardPresent = IsKeyboardRegisteringAsGamepad();
		if(gamepads.Length == 1 && keyboardPresent) { return 0; }
		return keyboardPresent ? (gamepads.Length - 1) : gamepads.Length;
	}
	public void UpdateGamepad(int player, int buttonIdx) {
		int analogIdx = buttonIdx + 1;
		string buttonPrefix = (35 + buttonIdx * 2).ToString();
		string analogPrefix = "joy" + analogIdx;
		saveInfo.UpdateGamepadNumber(player, buttonPrefix, analogPrefix);
		SaveGeemu();
	}

	public bool IsKeyDownOrButtonPressed() {
		for(int i = 0; i < 4; i++) {
			if(Input.GetKeyDown((KeyCode)(350 + 20 * i))) { return true; }
			if(Input.GetKeyDown((KeyCode)(357 + 20 * i))) { return true; }
		}
		return Input.inputString.Length > 0 || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
	}
	private InputVal GetInputVal(string binding) {
		if(binding.Contains(":")) {
			string[] split = binding.Split(new char[]{':'});
			return new InputVal_Axis(split[0], int.Parse(split[1]));
		} else {
			return new InputVal_Key(int.Parse(binding));
		}
	}
	public string GetP1InputName(InputMethod.KeyBinding binding) { return GetInputVal(saveInfo.controlBindingsP1[(int)binding]).GetName(); }
	public string GetP2InputName(InputMethod.KeyBinding binding) { return GetInputVal(saveInfo.controlBindingsP2[(int)binding]).GetName(); }
	public int ReturnLaunchOrPauseOrNothingIsPressed() {
		for(int i = 0; i < 4; i++) {
			if(Input.GetKeyDown((KeyCode)(350 + 20 * i))) { return 1; }
			if(Input.GetKeyDown((KeyCode)(357 + 20 * i))) { return 2; }
		}
		InputVal launch = GetInputVal(saveInfo.GetBinding(InputMethod.KeyBinding.launch, 0, usingGamepad1));
		InputVal pause = GetInputVal(saveInfo.GetBinding(InputMethod.KeyBinding.pause, 0, usingGamepad1));
		if(Input.GetMouseButtonDown(0) || launch.KeyDown()) { return 1; }
		if(pause.KeyDown()) { return 2; }
		return 0;
	}
	public InputMethod GetP1Controller() {
		if(ReturnLaunchOrPauseOrNothingIsPressed() > 0) { return new Input_Computer(); }
		return null;
	}

	public InputMethod detectInput_P2() {
		if(usingGamepad2) {
			for(int i = 0; i < 4; i++) {
				if(Input.GetKeyDown((KeyCode)(350 + 20 * i))) {
					UpdateGamepad(1, i);
					break;
				}
			}
		}
		InputVal launch = GetInputVal(saveInfo.GetBinding(InputMethod.KeyBinding.launch, 1, usingGamepad2));
		InputVal pause = GetInputVal(saveInfo.GetBinding(InputMethod.KeyBinding.pause, 1, usingGamepad2));
		if(launch.KeyDown() || pause.KeyDown()) { return new Input_Computer(); }
		return null;
	}
	#endregion
	#region "Transitions"
	private bool isFading, holdFade;
	private int fadeDir;
	private Texture2D fade;
	private float fadeSpeed, fadeAlpha;
	public bool dontFade;
	private void SetupFadeVars() {
		isFading = false;
		fadeSpeed = 1.5f;
		fadeAlpha = 0.0f;
		holdFade = false;
		fadeDir = -1;
		fade = Resources.Load<Texture2D>(SpritePaths.FullBlackCover);
	}
	float timeA;
	int fps, lastFPS;
	public void Update() { if(Time.timeSinceLevelLoad - timeA <= 1) { fps++; } else { lastFPS = fps+1; timeA = Time.timeSinceLevelLoad; fps = 0; } }
	public void OnGUI() {
		//GUI.Label(new Rect(20,20,60,40), "fps: " + lastFPS + "\r\nGO: " + GameObject.FindObjectsOfType(typeof(GameObject)).Length);
		if(isFading) {
			fadeAlpha += fadeDir * fadeSpeed * Time.deltaTime;
			fadeAlpha = Mathf.Clamp01(fadeAlpha);
			if(fadeAlpha == 0.0f || fadeAlpha == 1.0f) { isFading = false; }
			Color g = GUI.color;
			g.a = fadeAlpha;
			GUI.color = g;
			GUI.depth = -1000;
			GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), fade);
		} else if(holdFade) {
			GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), fade);
		}

	}

	public void SaveAndQuit(int time) { saveInfo.addPlayTime(gameType, time); SaveGeemu(); Application.Quit(); }
	public void SaveAndReset(int time) { saveInfo.addPlayTime(gameType, time); SaveGeemu(); ChangeScreen(GS.Game); }
	public void SaveAndMainMenu(int time) { saveInfo.addPlayTime(gameType, time); SaveGeemu(); GoToMainMenu(); }
	public void GoToMainMenu() { ChangeScreen(GS.MainMenu); }
	public void ChangeScreen(GS type) {
		if(isTransitioning) { return; }
		if(currentScreen == GS.Game) { ClearTileBank(); }
		isTransitioning = true;
		StartFade(1);
		StartCoroutine(ChangeScreenInner(type));
	}
	private System.Collections.IEnumerator ChangeScreenInner(GS type) { yield return new WaitForSeconds(0.3f); currentScreen = type; Application.LoadLevel((int) type); }
	public void OnLevelWasLoaded() {
		isTransitioning = false;
		if(dontFade) { dontFade = false; return; }
		if(currentScreen == GS.Game) { InitTileBank(); }
		StartFade(-1);
		SetupSound();
	}
	private void StartFade(int direction) { 
		holdFade = direction > 0;
		isFading = true; 
		fadeDir = direction; 
		fadeAlpha = fadeDir>0?0.0f:1.0f;
	}
	#endregion
	#region "Main Menu"
	public void MoveToDemo() {
		isDemo = true;
		isTutorial = false;
		rounds = 0; totalRoundTime = 0; totalP1RoundScore = 0; totalP2RoundScore = 0;
		gameType = GT.QuickPlay;
		useSpecial = Random.value > 0.7f;
		p1Char = (C) Random.Range(0, 10);
		p2Char = (C) Random.Range(0, 10);
		rowCount = Random.Range(4, 8);
		demoPlayers = (Random.value < 0.6f)?1:2;
		ChangeScreen(GS.Game);
	}
	public void MoveOutOfDemo() {
		p1Char = C.Null;
		p2Char = C.Null;
		rowCount = 6;
		GoToMainMenu();
	}
	public void MainMenuConfirmation(GT t) { gameType = t; level = 0; runningScore = 0; ChangeScreen(GetMenuNextState()); }
	private GS GetMenuNextState() {
		switch(gameType) {
			case GT.PlayerData: return GS.PlayerData;
			case GT.QuickPlay: return GS.CharSel;
			case GT.Arcade: return GS.CharSel;
			case GT.Campaign: return GS.CharSel;
			case GT.Versus: return GS.CharSel;
			case GT.Training: return GS.CharSel;
			case GT.Challenge: return GS.PuzSel;
			case GT.Options: return GS.Options;
		}
		return 0;
	}
	#endregion
	#region "Sound"
	public void SetupSound() {
		sounds = new GameObject("AudioContainers").AddComponent<AudioContainerContainer>();
		sounds.Init(saveInfo.savedOptions["vol_m"] / 100.0f, saveInfo.savedOptions["vol_s"] / 100.0f, saveInfo.savedOptions["vol_v"] / 100.0f, voicePitch);
		if(currentScreen == GS.Options || currentScreen == GS.PlayerData || currentScreen == GS.PuzSel || currentScreen == GS.CharSel) {
			sounds.SetMusicAndPlay(SoundPaths.M_Menu);
		} else if(currentScreen == GS.CutScene) {
			sounds.SetMusicAndPlay(SoundPaths.M_Cutscene);
			sounds.HalveMusicVolume();
		} else if(currentScreen == GS.Credits) {
			sounds.SetMusicAndPlay(SoundPaths.M_Credits, false);
		}
	}
	public float voicePitch = 1.0f;
	public void InhaleHelium() { if(voicePitch == 1.0f) { voicePitch = 1.5f; } else { voicePitch = 1.0f; } }
	#endregion
	#region "Character Select"
	public void Debug_ForceWin(string name) { saveInfo.saveArcadeVictory(name, 2); }
	public string GetPlayerSpritePathFromInt(int i, bool isBackground = false) { return GetPlayerSpritePath((C)i, isBackground); }
	public void SetPlayer1(int i, bool anotherEasterEgg = false) { 
		p1Char = (C)i;
		if(anotherEasterEgg) {
			p1Char = C.FuckingBalloon;
			balloonType = Random.Range(0, 3);
		} else {
			saveInfo.incrementCharacterFrequency(GetPlayerSpritePath(p1Char));
		}
		SaveGeemu();
	}
	public void SetPlayer2(int i) { p2Char = (C)i; }
	public string GetPlayerName(C p) { return System.Enum.GetName(typeof(C), p); }
	public int GetPlayerSpriteStartIdx(C p) { 
		switch(p) {
			case C.AliceAna: return 9;
			case C.Andrew: return 4; 
			case C.Depeche: return 6;
			case C.Devin: return 2;
			case C.George: return 0;
			case C.Joan: return 5;
			case C.Laila: return 8;
			case C.Lars: return 7;
			case C.Milo: return 1;
			case C.MJ: return 3;
			case C.September: return 11;
			case C.White: return 10;
		}
		return Random.Range(0, 12);
	}
	public string GetPlayerSpritePath(C p, bool isBackground = false, bool isMusic = false) { 
		switch(p) {
			case C.AliceAna: return "AliceAna";
			case C.Andrew: return "Andrew"; 
			case C.Depeche: return "Depeche";
			case C.Devin: return "Devin";
			case C.George: return "George";
			case C.Joan: return "Joan";
			case C.Laila: return "Laila";
			case C.Lars: return "Lars";
			case C.Milo: return "Milo";
			case C.MJ: return "MJ";
			case C.September: return isMusic?"White":"September";
			case C.White: return "White";
			case C.FuckingBalloon: return "MasterAlchemist";
			case C.Everyone: return "Everyone";
		}
		return isBackground?"Default":"George";
	}
	public string GetPlayerDisplayName(string p) { 
		switch(p) {
			case "AliceAna": return "Alice/Ana";
			case "Depeche": return "MODE";
			case "MJ": return "M.J.";
		}
		return p;
	}
	public void CharacterSelectConfirmation(bool moveToBalloon = false) {
		runningScore = 0; runningTime = 0;
		if(moveToBalloon) { MoveToBalloonBattle(); return; }
		if(gameType == GT.QuickPlay && p2Char == C.Null) { p2Char = (C) Random.Range(0, 10); } // TODO: REMOVE THE NULL CHECK LATER
		if(gameType == GT.Arcade) { GetNextOpponent(); ChangeScreen(GS.CutScene); }
		else { ChangeScreen(GS.Game); }
	}
	#endregion
	#region "Arcade Mode"
	public void GetNextOpponent() {
		bool isBossChar = p1Char == C.White || p1Char == C.September;
		if(!isBossChar && level == 5) { p2Char = C.White; return; }
		if(!isBossChar && level >= 6) { p2Char = C.September; return; }
		C[] c;
		switch(p1Char) {
			case C.George: c = new C[] {C.Joan, C.Lars, C.Devin, C.Laila, C.Depeche}; break;
			case C.AliceAna: c = new C[] {C.Lars, C.Joan, C.Milo, C.Devin, C.Andrew}; break;
			case C.Devin: c = new C[] {C.MJ, C.Lars, C.Laila, C.AliceAna, C.Milo}; break;
			case C.Lars: c = new C[] {C.Depeche, C.MJ, C.Andrew, C.Devin, C.Laila}; break;
			case C.Andrew: c = new C[] {C.MJ, C.Depeche, C.George, C.Joan, C.AliceAna}; break;
			case C.Joan: c = new C[] {C.Laila, C.Milo, C.George, C.Depeche, C.MJ}; break;
			case C.Depeche: c = new C[] {C.AliceAna, C.Milo, C.Laila, C.Andrew, C.George}; break;
			case C.Milo: c = new C[] {C.George, C.AliceAna, C.Joan, C.Andrew, C.Devin}; break;
			case C.Laila: c = new C[] {C.MJ, C.Depeche, C.Devin, C.AliceAna, C.Lars}; break;
			case C.MJ: c = new C[] {C.Andrew, C.Lars, C.George, C.Milo, C.Joan}; break;
			default: c = new C[] {C.George, C.Milo, C.Devin, C.MJ, C.Andrew, C.Joan, C.Depeche, C.Lars, C.Laila, C.AliceAna}; break;
		}
		p2Char = c[level];
	}
	#endregion
	#region "Game"
	public void MoveToBalloonBattle() {
		gameType = GT.Arcade;
		rowCount = 6;
		rowCount2 = 6;
		isTutorial = false;
		aboutToFightAFuckingBalloon = false;
		balloonType = Random.Range(0, 3);
		p2Char = C.FuckingBalloon;
		difficulty = 13;
		ChangeScreen(GS.Game);
	}
	public void MoveFromHighScoreScreen() {
		runningScore = 0;
		level = 0;
		if(aboutToFightAFuckingBalloon) {
			MoveToBalloonBattle();
		} else {
			ChangeScreen(GS.CharSel);
		}
	}
	public void MoveFromWinScreen() { winType = 0; ChangeScreen(GS.HighScore); }
	public int GetScore(int depth, int length, float bonus, int d = -1) { return Mathf.FloorToInt(((d<0?difficulty:d) * 0.25f) * bonus * (depth * (depth>1?150:100) + length * 10)); }

	private bool AreAdditionalMatchesAreRedundant() {
		int p1Wins = 0, p2Wins = 0, halfRounds = Mathf.FloorToInt(rounds/2.0f);
		for(int i = 0; i < playerOneWonRound.Count; i++) { if(playerOneWonRound[i]) { p1Wins++; } else { p2Wins++; } }
		return (p1Wins > halfRounds || p2Wins > halfRounds);
	}
	public void DoWin(int score, int time, bool lost, bool updateData = true) {
		if(isTransitioning) { return; }
		if(updateData) { runningTime = time; runningScore = score; }
		won = !lost;
		if(p2Char == C.FuckingBalloon) {
			if(won) { saveInfo.savedOptions["beatafuckingballoon"] = 1; }
			saveInfo.addPlayTime(gameType, runningTime);
			if(winType > 0) {
				won = true;
				int prevComplet = saveInfo.CalculateGameCompletionPercent();
				saveInfo.saveArcadeVictory(name, winType);
				int newComplet = saveInfo.CalculateGameCompletionPercent();
				if(prevComplet < 50 && newComplet >= 50) {
					unlockNew = 1;
				} else if(prevComplet < 100 && newComplet == 100) {
					unlockNew = 2;
				}
			}
			SaveGeemu();
			GoToMainMenu();
			return;
		}
		if(rounds > 0) {
			totalRoundTime += time;
			bool endGame = false;
			playerOneWonRound.Add(won);
			if(won) { playerRoundTimes.Add(time); playerRoundScores.Add(score); }
			if(++currentRound <= rounds) {
				runningScore = 0;
				runningTime = 0;
				endGame = AreAdditionalMatchesAreRedundant();
				if(!endGame) { SaveAndReset(time); return; }
			}
			if(endGame || currentRound > rounds) {
				playerRoundTimes.Sort();
				playerRoundScores.Sort();
				runningTime = (playerRoundTimes.Count > 0) ? playerRoundTimes[0] : 0;
				runningScore = (playerRoundScores.Count > 0) ? playerRoundScores[playerRoundScores.Count - 1] : 0;
				if(rounds > 1) { ChangeScreen(GS.RoundWinner); }
			}
		}
		bool advanceToWinScreenFromPuzzleScreen = false;
		if(gameType == GT.QuickPlay || gameType == GT.Campaign) {
			saveInfo.addPlayTime(gameType, runningTime);
			SaveGeemu();
			ChangeScreen(GS.HighScore);
			return;
		} else if(gameType == GT.Challenge) {
			int prevComplet = saveInfo.CalculateGameCompletionPercent();
			if(won) { saveInfo.addToPuzzles(level, runningScore, runningTime); }
			SaveGeemu();
			int newComplet = saveInfo.CalculateGameCompletionPercent();
			if(prevComplet < 50 && newComplet >= 50) {
				unlockNew = 1;
				advanceToWinScreenFromPuzzleScreen = true;
			} else if(prevComplet < 100 && newComplet == 100) {
				unlockNew = 2;
				advanceToWinScreenFromPuzzleScreen = true;
			}
		} 
		if(gameType != GT.Arcade) {
			saveInfo.addPlayTime(gameType, runningTime);
			SaveGeemu();
			runningScore = 0;
			runningTime = 0;
			if(gameType == GT.Challenge && advanceToWinScreenFromPuzzleScreen) {
				p1Char = unlockNew == 1 ? C.White : C.September;
				p2Char = p1Char;
				ChangeScreen(GS.WinnerIsYou); 
				return;
			}
			ChangeScreen(gameType==GT.Challenge?GS.PuzSel:GS.CharSel); 
			return;
		}
		if(lost) {
			saveInfo.addPlayTime(gameType, runningTime);
			SaveGeemu();
			string name = GetPlayerSpritePath(p1Char);
			if(winType > 0) { 
				won = true;
				int prevComplet = saveInfo.CalculateGameCompletionPercent();
				saveInfo.saveArcadeVictory(name, winType);
				int newComplet = saveInfo.CalculateGameCompletionPercent();
				if(prevComplet < 50 && newComplet >= 50) {
					unlockNew = 1;
				} else if(prevComplet < 100 && newComplet == 100) {
					unlockNew = 2;
				}
			}
			SaveGeemu();
			ChangeScreen(won?GS.WinnerIsYou:GS.HighScore);
			return;
		}
		int dragonScore = 100 * GetScore(2, 5, 1.0f, initialDifficulty), puhLoonScore = dragonScore * 2;
		if(p1Char == C.FuckingBalloon) { dragonScore = runningScore + 100; puhLoonScore = runningScore + 100; }
		if(level % 2 == 0) { difficulty++; }
		if(p1Char == C.White || p1Char == C.September) {
			if(level == 9) {
				ChangeScreen(GS.WinnerIsYou);
				winType = 2;
				saveInfo.addPlayTime(gameType, runningTime);
				SaveGeemu();
				string name = GetPlayerSpritePath(p1Char);
				saveInfo.saveArcadeVictory(name, winType);
				SaveGeemu();
				ChangeScreen(GS.WinnerIsYou);
			} else {
				level++;
				ChangeScreen(GS.CutScene);
			}
		} else {
			if(level == 7 && runningScore >= puhLoonScore) { aboutToFightAFuckingBalloon = true; level++; }
			else if(level == 5 && runningScore >= dragonScore) {
				level = 7;
				difficulty++;
			} else { level++; }
			ChangeScreen(GS.CutScene);
		}
	}
	#endregion
	#region "Saving/Loading/Options"
	public bool IsFirstTime() { return saveInfo.firstTime; }
	public void SaveGeemu() { saveInfo.Save(); }
	public void LoadGeemu() { 
		if(saveInfo == null) {
			SaveIOCore s = new SaveIO_PC();
			saveInfo = new SaveData(s);
		} 
		SaveData res = saveInfo.Load();
		if(res != null) { saveInfo = res; saveInfo.ApplyPatch(); }
	}
	public void WipeData() { saveInfo.EraseSaveData(); saveInfo.Save(); }
	public SaveData GetSaveData() { return saveInfo; }
	public bool UseHighContrastCursor() { return saveInfo.savedOptions["emphasizecursor"] == 1; }
	public bool IsColorBlind() { return saveInfo.savedOptions["colorblind"] == 1; }
	public bool IsLeftAlignedHUD() { return saveInfo.savedOptions["hudplacement"] == 0; }
	public void SetOption(string s, int v) { if(!saveInfo.savedOptions.ContainsKey(s)) { saveInfo.savedOptions.Add(s, v); } else { saveInfo.savedOptions[s] = v; } }
	public void SetRes() { Screen.SetResolution(saveInfo.savedOptions["width"], saveInfo.savedOptions["height"], saveInfo.savedOptions["fullscreen"] == 1); }
	public void SaveName(string n) { if(n.Length != 3) { return; } saveInfo.highScoreName = n; SaveGeemu(); }
	public void SaveScore(string n, int s) { saveInfo.addToHighScore(n, s, gameType); SaveGeemu(); }
	public void SaveTime(string n, int s) { saveInfo.addToTime(n, s, gameType); SaveGeemu(); }
	public void SetCharSelOptionVal(string key, int val) { saveInfo.gameOptionDefaults[key] = val; }
	public int GetCharSelOptionVal(string key) {
		if(saveInfo.gameOptionDefaults == null) { saveInfo.setupGameOptionDefaults(); SaveGeemu(); }
		return saveInfo.gameOptionDefaults[key];
	}
	public void SetKeyBinding(int player, int key, string val) {
		if(player == 0) {
			if(usingGamepad1) {
				saveInfo.controlBindingsGamepadP1[key] = val;
			} else {
				saveInfo.controlBindingsP1[key] = val;
			}
		} else {
			if(usingGamepad2) {
				saveInfo.controlBindingsGamepadP2[key] = val;
			} else {
				saveInfo.controlBindingsP2[key] = val;
			}
		}
		SaveGeemu();
	}
	public bool IsKeyInUse(int key) {
		string skey = key.ToString();
		foreach(string v in saveInfo.controlBindingsP1.Values) { if(v == skey) { return true; } }
		return false;
	}
	public Dictionary<int, string> GetKeyBindings(int player = 0) {
		if(saveInfo.controlBindingsP1 == null) { saveInfo.SetupDefaultKeyControls(); SaveGeemu(); }
		if(saveInfo.controlBindingsGamepadP1 == null) { saveInfo.SetupDefaultPadControls(); SaveGeemu(); }
		if(player == 0) {
			return usingGamepad1 ? saveInfo.controlBindingsGamepadP1 : saveInfo.controlBindingsP1;
		} else {
			return usingGamepad2 ? saveInfo.controlBindingsGamepadP2 : saveInfo.controlBindingsP2;
		}
	}
	#endregion
	#region "Puzzles"
	public int GetPuzzleLevel() { return level; }
	public void SetPuzzleLevel(int i) { level = i; }
	public void LowerPuzzleLevel() { level--; }
	#endregion
}