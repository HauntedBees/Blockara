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
[System.Serializable]
public class SaveData {
	private const int currentVersion = -1;
	private const string saveFilePath = "/blockara.sav";
	public bool firstTime = false;
	private int version;
	public List<KeyValuePair<string, int>> highScoresQuickPlay, highScoresArcade, highScoresCampaign;
	public List<KeyValuePair<string, int>> shortestTimesQuickPlay, shortestTimesArcade, longestTimesCampaign;
	public List<KeyValuePair<string, int>> characterFreqs, characterWinsArcade;
	public List<KeyValuePair<string, int>> timePlayed, remainingInfo;
	public Dictionary<int, int> puzzleScores, puzzleTimes;
	public Dictionary<string, int> savedOptions, gameOptionDefaults;
	public Dictionary<int, string> controlBindingsP1, controlBindingsP2, controlBindingsGamepadP1, controlBindingsGamepadP2;
	public string highScoreName;
	public string culture;
	private SaveIOCore saveHandler;
	public SaveData(SaveIOCore s) { saveHandler = s; }
	public int GetTitleScreenCharacter() {
		List<int> idxes = new List<int>();
		idxes.Add(-1);
		for(int i = 0; i < characterFreqs.Count; i++) { if(characterFreqs[i].Value > 0) { idxes.Add(i); } }
		for(int i = 0; i < characterWinsArcade.Count; i++) { if(characterWinsArcade[i].Value == 2) { idxes.Add(11); break; } else if(characterWinsArcade[i].Value == 1 && !idxes.Contains(10)) { idxes.Add(10); } }
		if(CalculateGameCompletionPercent() == 100) { idxes.Add(12); }
		return idxes[Random.Range(0, idxes.Count)];
	}
	public int CalculateGameCompletionPercent() {
		float puzzlePercent = (float)getPuzzlesCompleted() / PuzzleSelectController.totalPuzzleCount;
		float campaignPercent = getArcadeVictorySummation() / 20.0f;
		return Mathf.FloorToInt(70 * campaignPercent + 30 * puzzlePercent);
	}
	public string GetMostPopularCharacter() {
		string character = "George";
		int highest = 0;
		for(int i = 0; i < characterFreqs.Count; i++) {
			if(characterFreqs[i].Value > highest) {
				highest = characterFreqs[i].Value;
				character = characterFreqs[i].Key;
			}
		}
		if(character == "AliceAna") { character = "Alice/Ana"; } else if(character == "MJ") { character = "M.J."; } else if(character == "Depeche") { character = "MODE"; }
		return character;
	}
	public bool HasBeatenGameWithCharacter(string character) {
		string actualCharacter = character;
		if(character == "White") {
			for(int i = 0; i < characterWinsArcade.Count; i++) { if(characterWinsArcade[i].Value >= 1) { return true; }}
			return false;
		} else if(character == "September") {
			for(int i = 0; i < characterWinsArcade.Count; i++) { if(characterWinsArcade[i].Value == 2) { return true; }}
			return false;
		}
		if(character == "Alice/Ana") { actualCharacter = "AliceAna"; } else if(character == "M.J.") { actualCharacter = "MJ"; } else if(character == "MODE") { actualCharacter = "Depeche"; }
		for(int i = 0; i < characterWinsArcade.Count; i++) { if(characterWinsArcade[i].Key == actualCharacter && characterWinsArcade[i].Value > 0) { return true; }}
		return false;
	}
	public void incrementCharacterFrequency(string character) {
		for(int i = 0; i < characterFreqs.Count; i++) {
			if(characterFreqs[i].Key == character) { characterFreqs[i] = new KeyValuePair<string, int>(characterFreqs[i].Key, characterFreqs[i].Value + 1); return; }
		}
	}
	public int getArcadeVictories() {
		for(int i = 0; i < characterWinsArcade.Count; i++) {
			if(characterWinsArcade[i].Key == "Total") { return characterWinsArcade[i].Value; }
		}
		return 0;
	}
	public int getArcadeVictorySummation() {
		int res = 0;
		for(int i = 0; i < characterWinsArcade.Count; i++) {
			if(characterWinsArcade[i].Key == "Total" || characterWinsArcade[i].Key == "White" || characterWinsArcade[i].Key == "September") { continue; }
			res += characterWinsArcade[i].Value;
		}
		return res;
	}
	public int getPlayerWinType(string character) {
		for(int i = 0; i < characterWinsArcade.Count; i++) { if(characterWinsArcade[i].Key == character) { return characterWinsArcade[i].Value; } }
		return 0;
	}
	public void saveArcadeVictory(string character, int val) {
		for(int i = 0; i < characterWinsArcade.Count; i++) {
			if(characterWinsArcade[i].Key == "Total") {
				KeyValuePair<string, int> res = new KeyValuePair<string, int>("Total", characterWinsArcade[i].Value + 1);
				characterWinsArcade[i] = res;
			} else if(characterWinsArcade[i].Key == character) {
				KeyValuePair<string, int> res = new KeyValuePair<string, int>(character, Mathf.Max(val, characterWinsArcade[i].Value));
				characterWinsArcade[i] = res;
			}
		}
	}

	public void addToPuzzles(int puzzle, int score, int time) {
		if(puzzleScores.ContainsKey(puzzle)) {
			if(score > puzzleScores[puzzle]) { puzzleScores[puzzle] = score; }
		} else { puzzleScores.Add(puzzle, score); }
		if(puzzleTimes.ContainsKey(puzzle)) {
			if(time < puzzleTimes[puzzle]) { puzzleTimes[puzzle] = time; }
		} else { puzzleTimes.Add(puzzle, time); }
	}
	public int getPuzzlesCompleted() { return puzzleScores.Count; }

	public void addToTime(string name, int time, PersistData.GT mode) {
		switch(mode) {
			case PersistData.GT.Arcade: addToHighScoreDict(ref shortestTimesArcade, name, time, false); break;
			case PersistData.GT.Campaign: addToHighScoreDict(ref longestTimesCampaign, name, time); break;
			case PersistData.GT.QuickPlay: addToHighScoreDict(ref shortestTimesQuickPlay, name, time, false); break;
		}
	}

	public void addPlayTime(PersistData.GT mode, int time) {
		string name = "";
		switch(mode) {
			case PersistData.GT.Arcade: name = "Arcade"; break;
			case PersistData.GT.Campaign: name = "Campaign"; break;
			case PersistData.GT.Challenge: name = "Puzzle"; break;
			case PersistData.GT.QuickPlay: name = "Quick Play"; break;
			case PersistData.GT.Training: name = "Training"; break;
			case PersistData.GT.Versus: name = "Versus"; break;
		}
		for(int i = 0; i < timePlayed.Count; i++) {
			if(timePlayed[i].Key == name) { 
				KeyValuePair<string, int> res = new KeyValuePair<string, int>(timePlayed[i].Key, timePlayed[i].Value + time);
				timePlayed[i] = res;
				break;
			}
		}
	}
	public int getPlayTime(string gametype) {
		for(int i = 0; i < timePlayed.Count; i++) {
			if(timePlayed[i].Key == gametype) { return timePlayed[i].Value; }
		}
		return 0;
	}

	
	public List<KeyValuePair<string, int>> getHighScores(PersistData.GT mode) {
		switch(mode) {
		case PersistData.GT.Arcade: return highScoresArcade;
		case PersistData.GT.Campaign: return highScoresCampaign;
		case PersistData.GT.QuickPlay: return highScoresQuickPlay;
		}
		return null;
	}
	public List<KeyValuePair<string, int>> getBestTimes(PersistData.GT mode) {
		switch(mode) {
		case PersistData.GT.Arcade: return shortestTimesArcade;
		case PersistData.GT.Campaign: return longestTimesCampaign;
		case PersistData.GT.QuickPlay: return shortestTimesQuickPlay;
		}
		return null;
	}
	public void addToHighScore(string name, int score, PersistData.GT mode) {
		switch(mode) {
			case PersistData.GT.Arcade: addToHighScoreDict(ref highScoresArcade, name, score); break;
			case PersistData.GT.Campaign: addToHighScoreDict(ref highScoresCampaign, name, score); break;
			case PersistData.GT.QuickPlay: addToHighScoreDict(ref highScoresQuickPlay, name, score); break;
		}
	}
	private void addToHighScoreDict(ref List<KeyValuePair<string, int>> scores, string name, int score, bool highGood = true) {
		int spliceIndex = getHighscorePos(scores, score, highGood);
		if(spliceIndex < 0) { return; }
		scores.Insert(spliceIndex, new KeyValuePair<string, int>(name, score));
		scores.RemoveAt(scores.Count - 1);
	}
	public int getHighscorePos(List<KeyValuePair<string, int>> scores, int score, bool highGood) {
		int spliceIndex = -1;
		for(int i = 0; i < scores.Count; i++) {
			KeyValuePair<string, int> pair = scores[i];
			if(highGood && pair.Value >= score || !highGood && pair.Value <= score) { continue; }
			spliceIndex = i;
			break;
		}
		return spliceIndex;
	}

	public void Save() {
		saveHandler.Save(this, saveFilePath);
	}
	public SaveData Load(bool wipeEverything = false) {
		if(wipeEverything) { FirstLoad(); return null; }
		SaveData res = null;
		try {
			res = saveHandler.Load(saveFilePath);
		} catch(System.Exception e) {
			Debug.Log("WIPE THAT SMILDER OFF YOUR DILDER: " + e.Message);
		}
		if(res == null) { FirstLoad(); } else { res.firstTime = false; }
		return res;
	}
	public void ApplyPatch() {
		if(version != currentVersion) {
			// patch deets go here
		}
	}
	public void EraseSaveData() { FirstLoad(); }
	private void FirstLoad() {
		firstTime = true;
		setupSavedOptions();
		SetupDefaultKeyControls();
		setupGameOptionDefaults();
		puzzleScores = new Dictionary<int, int>();
		puzzleTimes = new Dictionary<int, int>();
		XmlDocument doc = new XmlDocument();
		TextAsset ta = Resources.Load<TextAsset>("XML/defaultValues");
		doc.LoadXml(ta.text);
		XmlNode head = doc.SelectSingleNode("initVals");
		loadScores(ref highScoresQuickPlay, head, "highScoresQuickPlay", "name");
		loadScores(ref highScoresArcade, head, "highScoresArcade", "name");
		loadScores(ref highScoresCampaign, head, "highScoresCampaign", "name");
		loadScores(ref shortestTimesQuickPlay, head, "shortestTimesQuickPlay", "name");
		loadScores(ref shortestTimesArcade, head, "shortestTimesArcade", "name");
		loadScores(ref longestTimesCampaign, head, "longestTimesCampaign", "name");
		loadScores(ref characterFreqs, head, "characterFreqs", "name");
		loadScores(ref characterWinsArcade, head, "characterWinsArcade", "name");
		loadScores(ref timePlayed, head, "timePlayed", "mode");
		highScoreName = "AAA";
		version = currentVersion;
	}
	private void setupSavedOptions() {
		savedOptions = new Dictionary<string, int>();
		savedOptions.Add("width", Screen.width);
		savedOptions.Add("height", Screen.height);
		savedOptions.Add("fullscreen", Screen.fullScreen?1:0);
		savedOptions.Add("vol_m", 80);
		savedOptions.Add("vol_s", 90);
		savedOptions.Add("vol_v", 90);
		savedOptions.Add("colorblind", 0);
		savedOptions.Add("hudplacement", 1);
		savedOptions.Add("emphasizecursor", 0);
		savedOptions.Add("touchcontrols", 0);
		savedOptions.Add("easymode", 0);
		savedOptions.Add("keydelay", 7);
		savedOptions.Add("beatafuckingballoon", 0);
	}
	public void setupGameOptionDefaults() {
		gameOptionDefaults = new Dictionary<string, int>();
		gameOptionDefaults.Add("0_0", 3);
		gameOptionDefaults.Add("0_3", 3);
		gameOptionDefaults.Add("1_0", 0);
		gameOptionDefaults.Add("1_1", 0);
		gameOptionDefaults.Add("1_3", 0);
		gameOptionDefaults.Add("2_0", 6);
		gameOptionDefaults.Add("2_2", 6);
		gameOptionDefaults.Add("2_3", 6);
		gameOptionDefaults.Add("2_4", 6);
		gameOptionDefaults.Add("3_0", 4);
		gameOptionDefaults.Add("3_1", 4);
		gameOptionDefaults.Add("3_2", 4);
		gameOptionDefaults.Add("4_4", 0);
	}
	public void SetupDefaultKeyControls() {
		controlBindingsP1 = new Dictionary<int, string>();
		controlBindingsP1.Add((int)InputMethod.KeyBinding.down, ((int)KeyCode.DownArrow).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.up, ((int)KeyCode.UpArrow).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.left, ((int)KeyCode.LeftArrow).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.right, ((int)KeyCode.RightArrow).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.shiftL, ((int)KeyCode.Z).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.shiftR, ((int)KeyCode.X).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.shiftAL, ((int)KeyCode.A).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.shiftAR, ((int)KeyCode.S).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.pause, ((int)KeyCode.Return).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.launch, ((int)KeyCode.Space).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.back, ((int)KeyCode.Backspace).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.hidden1, ((int)KeyCode.H).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.hidden2, ((int)KeyCode.J).ToString());
		controlBindingsP1.Add((int)InputMethod.KeyBinding.hidden3, ((int)KeyCode.Y).ToString());
		controlBindingsP2 = new Dictionary<int, string>();
		controlBindingsP2.Add((int)InputMethod.KeyBinding.down, ((int)KeyCode.Keypad2).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.up, ((int)KeyCode.Keypad8).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.left, ((int)KeyCode.Keypad4).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.right, ((int)KeyCode.Keypad6).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.shiftL, ((int)KeyCode.Keypad1).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.shiftR, ((int)KeyCode.Keypad3).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.shiftAL, ((int)KeyCode.Keypad7).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.shiftAR, ((int)KeyCode.Keypad9).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.pause, ((int)KeyCode.KeypadEnter).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.launch, ((int)KeyCode.Keypad5).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.back, ((int)KeyCode.KeypadMinus).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.hidden1, ((int)KeyCode.H).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.hidden2, ((int)KeyCode.J).ToString());
		controlBindingsP2.Add((int)InputMethod.KeyBinding.hidden3, ((int)KeyCode.Y).ToString());
	}
	public void SetupDefaultPadControls() {
		controlBindingsGamepadP1 = new Dictionary<int, string>();
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.down, "joy1_6:-1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.up, "joy1_6:1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.left, "joy1_5:-1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.right, "joy1_5:1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.shiftL, "354");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.shiftR, "355");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.shiftAL, "joy1_2:1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.shiftAR, "joy1_2:-1");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.pause, "357");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.launch, "350");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.back, "351");
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.hidden1, ((int)KeyCode.H).ToString());
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.hidden2, ((int)KeyCode.J).ToString());
		controlBindingsGamepadP1.Add((int)InputMethod.KeyBinding.hidden3, ((int)KeyCode.Y).ToString());
		controlBindingsGamepadP2 = new Dictionary<int, string>();
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.down, "joy2_6:-1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.up, "joy2_6:1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.left, "joy2_5:-1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.right, "joy2_5:1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.shiftL, "374");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.shiftR, "375");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.shiftAL, "joy2_2:1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.shiftAR, "joy2_2:-1");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.pause, "377");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.launch, "370");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.back, "371");
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.hidden1, ((int)KeyCode.H).ToString());
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.hidden2, ((int)KeyCode.J).ToString());
		controlBindingsGamepadP2.Add((int)InputMethod.KeyBinding.hidden3, ((int)KeyCode.Y).ToString());
	}
	public void UpdateGamepadNumber(int controller, string buttonPrefix, string analogPrefix) {
		if(controlBindingsGamepadP1 == null) { SetupDefaultPadControls(); }
		if(controller == 0) {
			List<int> keys = new List<int>(controlBindingsGamepadP1.Keys);
			foreach(int key in keys) {
				string old = controlBindingsGamepadP1[key];
				if(old.StartsWith("joy")) {
					controlBindingsGamepadP1[key] = analogPrefix + "_" + old.Split('_')[1];
				} else {
					controlBindingsGamepadP1[key] = buttonPrefix + old.Substring(2);
				}
			}
		} else {
			List<int> keys = new List<int>(controlBindingsGamepadP2.Keys);
			foreach(int key in keys) {
				string old = controlBindingsGamepadP2[key];
				if(old.StartsWith("joy")) {
					controlBindingsGamepadP2[key] = analogPrefix + "_" + old.Split('_')[1];
				} else {
					controlBindingsGamepadP2[key] = buttonPrefix + old.Substring(2);
				}
			}
		}
	}
	public string GetBinding(InputMethod.KeyBinding key, int player, bool usingGamepad) {
		if(usingGamepad) {
			if(player == 0) { return controlBindingsGamepadP1[(int) key]; }
			return controlBindingsGamepadP2[(int) key];
		} else {
			if(player == 0) { return controlBindingsP1[(int) key]; }
			return controlBindingsP2[(int) key];
		}
	}

	private void loadScores(ref List<KeyValuePair<string, int>> scoreBoard, XmlNode head, string headNodeName, string innerNodeName) {
		scoreBoard = new List<KeyValuePair<string, int>>();
		XmlNode An = head.SelectSingleNode(headNodeName);
		XmlNodeList nl = An.SelectNodes("score");
		foreach(XmlNode score in nl) {
			string name = score.Attributes[innerNodeName].InnerText;
			int val = System.Convert.ToInt32(score.InnerText);
			scoreBoard.Add(new KeyValuePair<string, int>(name, val));
		}
	}
}