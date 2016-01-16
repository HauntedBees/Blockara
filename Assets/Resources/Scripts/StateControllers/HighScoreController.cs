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
using System.Text.RegularExpressions;
public class HighScoreController:MenuController {
	private TextMesh currentScore, currentTime, nameEntry;
	private string scorePosText, scoreTxt, timePosText, timeTxt, currentName;
	private OptionsCursor nameCursor;
	private bool saved;
	private int[] inputName;
	private char[] characters;
	private int p1_delay;
	private Sprite[] confirmButtonSprites;
	private GameObject inputCollider, confirmButton;
	private ScoreTextFormatter writer;
	private Regex keyPress;
	public void Start() {
		StateControllerInit(false);
		SaveData sd = PD.GetSaveData();
		currentName = "---";
		characters = new char[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
		string savedName = sd.highScoreName;
		inputName = new int[] {System.Array.IndexOf(characters, savedName[0]), System.Array.IndexOf(characters, savedName[1]), System.Array.IndexOf(characters, savedName[2])};
		GetGameObject(Vector3.zero, "BG", Resources.Load<Sprite>(SpritePaths.HighScoreBG), false, "BG0");

		keyPress = new Regex("[A-Za-z0-9]");

		PD.sounds.SetMusicAndPlay(SoundPaths.M_Menu);

		writer = new ScoreTextFormatter(PD);
		List<KeyValuePair<string, int>> scores = sd.getHighScores(PD.gameType);
		int userScorePos = sd.getHighscorePos(scores, PD.runningScore, true);
		if(userScorePos < 0) { userScorePos = 100; }
		scorePosText = (userScorePos + 1).ToString() + ". "; if(userScorePos < 9) { scorePosText = " " + scorePosText; }
		scoreTxt = PD.runningScore.ToString();
		float scoreX = -2.0f;

		List<KeyValuePair<string, int>> times = sd.getBestTimes(PD.gameType);
		int userTimePos = -1;
		if(PD.gameType == PersistData.GT.Campaign) {
			userTimePos = sd.getHighscorePos(times, PD.runningTime, true);
		} else {
			userTimePos = PD.won?sd.getHighscorePos(times, PD.runningTime, false):-1;
		}
		if(userTimePos < 0) { userTimePos = 100; }
		timePosText = (userTimePos + 1).ToString() + ". "; if(userTimePos < 9) { timePosText = " " + timePosText; }
		timeTxt = writer.ConvertSecondsToMinuteSecondFormat(PD.runningTime);
		float timeX = 2.0f;

		writer.SetScoreRowAndTimeRowWidths(scores, times);

		System.Xml.XmlNode top = GetXMLHead();
		GetMeshText(new Vector3(scoreX, 1.4f), GetXmlValue(top, "highscores"), PD.mostCommonFont, "Prefabs/Text/FixedWidth");
		GetMeshText(new Vector3(timeX, 1.4f), GetXmlValue(top, "besttimes"), PD.mostCommonFont, "Prefabs/Text/FixedWidth");

		Sprite slab = Resources.Load<Sprite>(SpritePaths.HighScoreSlab);
		GetGameObject(new Vector3(scoreX, 0.15f), "Score Slab", slab);
		GetGameObject(new Vector3(timeX, 0.15f), "Time Slab", slab);

		for(int pos = 0; pos < 10; pos++) {
			if(pos < userScorePos) { GetTextRow(scores[pos], pos, scoreX); }
			else if(pos == userScorePos) { currentScore = GetTextRow(new KeyValuePair<string, int>(currentName, PD.runningScore), userScorePos, scoreX); }
			else if(pos > userScorePos) { GetTextRow(scores[pos - 1], pos, scoreX); }
			
			if(pos < userTimePos) { GetTextRow(times[pos], pos, timeX, true); }
			else if(pos == userTimePos) { currentTime = GetTextRow(new KeyValuePair<string, int>(currentName, PD.runningTime), pos, timeX, true); }
			else if(pos > userTimePos) { GetTextRow(times[pos - 1], pos, timeX, true); }
		}
		if(currentScore != null) { currentScore.color = Color.red; }
		if(currentTime != null) { currentTime.color = Color.red; }

		saved = (userScorePos > 10) && (userTimePos > 10);
		nameCursor = GetOptionsCursor(3, 1, SpritePaths.VerticalArrows, -0.17f, -0.8f, 0.17f, 0, 0, 0, 1);
		if(!saved) {
			FontData f = new FontData(TextAnchor.MiddleCenter, TextAlignment.Center);
			GetGameObject(new Vector3(0.0f, -0.65f), "Minislab", Resources.Load<Sprite>(SpritePaths.HighScoreInput));
			GetMeshText(new Vector3(0.0f, -0.5f), GetXmlValue(top, "entername"), f);
			nameEntry = GetMeshText(new Vector3(0.0f, -0.8f), savedName, f, "Prefabs/Text/FixedWidth");
			inputCollider = GetGameObject(new Vector3(0.0f, -0.8f), "Input Collider", Resources.Load<Sprite>(SpritePaths.ScoreCollider), true);
			confirmButtonSprites = Resources.LoadAll<Sprite>(SpritePaths.ConfirmButtons);
			confirmButton = GetGameObject(new Vector3(0.0f, -1.2f), "Confirm", confirmButtonSprites[0], true);
			confirmButton.SetActive(PD.usingMouse);
			mouseObjects.Add(confirmButton);
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "036", 0);
		} else {
			nameCursor.cursor.SetActive(false);
			PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "041", 0);
		}
	}
	private TextMesh GetTextRow(KeyValuePair<string, int> vals, int pos, float x, bool time = false) {
		string name = (pos + 1).ToString() + ". " + vals.Key + " ";
		if(pos < 9) { name = " " + name; }
		string scoreText = time?writer.ConvertSecondsToMinuteSecondFormat(vals.Value):vals.Value.ToString();
		return GetMeshText(new Vector3(x, 1.0f - pos * 0.2f), writer.GetRowText(name, scoreText, time), PD.mostCommonFont, "Prefabs/Text/FixedWidth");
	}


	public void Update() {
		UpdateMouseInput();
		if(saved) {
			if(confirmButton != null) { confirmButton.SetActive(false); }
			if(clicker.isDown() || nameCursor.launchOrPause()) { PD.MoveFromHighScoreScreen(); }
			return;
		}
		nameCursor.DoUpdate();
		if(PD.usingMouse) {
			MouseInput();
		}
		KeyboardInput();
	}
	private void MouseInput() {
		if(clicker.getPositionInGameObject(confirmButton).z != 0) {
			confirmButton.GetComponent<SpriteRenderer>().sprite = confirmButtonSprites[1];
			if(clicker.isDown()) {
				Save();
				confirmButton.SetActive(false);
				SignalSuccess();
			}
			return;
		} else {
			Vector3 pos = clicker.getPositionInGameObject(inputCollider);
			if(pos.z == 0) { return; }
			if(pos.x < -0.06f) { 
				nameCursor.setX(0);
				if(nameCursor.getX() != 0) { SignalMovement(); }
			} else if(pos.x < 0.1f) {
				nameCursor.setX(1);
				if(nameCursor.getX() != 1) { SignalMovement(); }
			} else {
				nameCursor.setX(2);
				if(nameCursor.getX() != 2) { SignalMovement(); }
			}
			if(clicker.isDown()) {
				int cx = nameCursor.getX();
				bool change = false;
				if(pos.y > 0.09f) { 
					change = true;
					SignalMovement();
					if(++inputName[cx] >= characters.Length) { inputName[cx] = 0; }
				} else if(pos.y < -0.09f) {
					change = true;
					SignalMovement();
					if(--inputName[cx] < 0) { inputName[cx] = characters.Length - 1; }
				}
				if(change) { nameEntry.text = GetName(); }
			}
		}
		confirmButton.GetComponent<SpriteRenderer>().sprite = confirmButtonSprites[0];
	}
	private void KeyboardInput() {
		int cx = nameCursor.getX();
		bool change = false, manualChange = false;
		if(--p1_delay <= 0) {
			if(nameCursor.upP()) {
				change = true;
				if(++inputName[cx] >= characters.Length) { inputName[cx] = 0; }
			} else if(nameCursor.downP()) {
				change = true;
				if(--inputName[cx] < 0) { inputName[cx] = characters.Length - 1; }
			} else if(keyPress.IsMatch(Input.inputString)) {
				int idx = System.Array.IndexOf(characters, Input.inputString.ToUpper()[0]);
				if(idx >= 0) { inputName[cx] = idx; }
				change = true;
				manualChange = true;
			}
			if(change) { SignalMovement(); p1_delay = 10; }
		}
		if(change) { nameEntry.text = GetName(); }
		if(nameCursor.launchOrPause() || manualChange) {
			if(cx < 2) {
				nameEntry.text = GetName();
				nameCursor.shiftX(1);
				SignalMovement();
			} else if(!manualChange) { SignalSuccess(); Save(); }
		}
	}
	private string GetName() {
		string res = "";
		for(int i = 0; i < 3; i++) { res += characters[inputName[i]]; }
		return res;
	}
	private void Save() {
		saved = true;
		nameCursor.SetVisibility(false);
		currentName = nameEntry.text;
		PD.SaveName(currentName);
		PD.SaveScore(currentName, PD.runningScore);
		PD.SaveTime(currentName, PD.runningTime);
		if(currentScore != null) { currentScore.text = writer.GetRowText(scorePosText + currentName + " ", scoreTxt, false); }
		if(currentTime != null) { currentTime.text = writer.GetRowText(timePosText + currentName + " ", timeTxt, true); }
		if(nameCursor != null) { nameCursor.SetVisibility(false); }
	}
}