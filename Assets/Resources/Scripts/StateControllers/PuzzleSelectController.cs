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
public class PuzzleSelectController:MenuController {
	public static int totalPuzzleCount = 32;
	private GameObject menu;
	private GameObject info;
	private TextMesh infotext;
	private XmlNodeList levelData;
	private int levelCount;
	private int curLevel;
	private PuzzleSelectPreview previewer;
	private XmlNode navinfo;
	public void Start() {
		StateControllerInit();
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "021", 0);
		menu = GetGameObject(new Vector3(-1.75f, 0.0f), "Puzzle Select Menu", Resources.Load<Sprite>(SpritePaths.PuzzleListBox), true);
		info = GetGameObject(new Vector3(1.75f, 0.0f), "Puzzle Select Info", Resources.Load<Sprite>(SpritePaths.PuzzleInfoBox));
		navinfo = GetXMLHead();

		FontData f = PD.mostCommonFont.Clone();
		infotext = GetMeshText(info.transform.position + new Vector3(0.0f, -0.32f), "", f);

		levelData = GetXMLHead("/challenges", "challenges", false).SelectNodes("challenge");
		levelCount = levelData.Count - 1; // first entry is tutorial

		int actlevel = PD.GetPuzzleLevel();
		if(actlevel == 0) { actlevel = GetFirstIncompletePuzzle(); } else { actlevel++; }
		int y = Mathf.FloorToInt((actlevel - 1) / 4), x = actlevel - y * 4 - 1; y = 7 - y;
		cursor = GetMenuCursor(4, 8, SpritePaths.PuzzleCursor, -2.65f, -1.0f, 0.6f, 0.3f, x, y);
		cursor.loopAround = true;

		PD.rounds = 0;
		PD.totalRoundTime = 0;
		PD.useSpecial = false;

		previewer = GetPreviewer();
		AddVictoryIcons();
	}
	private int GetFirstIncompletePuzzle() {
		SaveData saveInfo = PD.GetSaveData();
		for(int i = 1; i < PuzzleSelectController.totalPuzzleCount; i++) { if(!saveInfo.puzzleScores.ContainsKey(i)) { return i; } }
		return 0;
	}
	private PuzzleSelectPreview GetPreviewer() {
		List<GameObject> theirs = new List<GameObject>();
		List<GameObject> yours = new List<GameObject>();
		List<GameObject> extra = new List<GameObject>();
		List<GameObject> bgs = new List<GameObject>();
		Sprite[] minisheet = Resources.LoadAll<Sprite>(SpritePaths.PuzzleSmallBlocks);
		Sprite bg = Resources.Load<Sprite>(SpritePaths.PuzzleSmallBG);
		float startx = 1.335f, starty_theirs = 0.905f, starty_yours = 0.025f, startx_extra = 2.05f, starty_extra = 0.05f, delta = 0.08f, bgx = startx - delta * 0.5f;
		for(int y = 0; y < 8; y++) {
			bgs.Add(GetGameObject(new Vector3(bgx, starty_theirs + delta * y), "bg", bg, false, "HUD"));
			bgs.Add(GetGameObject(new Vector3(bgx, starty_yours + delta * y), "bg", bg, false, "HUD"));
			for(int x = 0; x < 8; x++) {
				theirs.Add(GetGameObject(new Vector3(startx + delta * x, starty_theirs + delta * y), "theirTile", minisheet[3], false, "HUDText"));
				yours.Add(GetGameObject(new Vector3(startx + delta * x, starty_yours + delta * y), "yourTile", minisheet[3], false, "HUDText"));
			}
		}
		for(int y = 8; y < 11; y++) { bgs.Add(GetGameObject(new Vector3(bgx, starty_yours + delta * y), "bg", bg, false, "HUD")); }
		for(int x = 0; x < 3; x++) {
			for(int y = 11; y >= 0; y--) {
				extra.Add(GetGameObject(new Vector3(startx_extra + delta * x, starty_extra + delta * y), "extraTile", minisheet[3], false, "HUDText"));
			}
		}
		XmlNode top = GetXMLHead();
		FontData f = new FontData(TextAnchor.MiddleLeft, TextAlignment.Left, 0.02f);
		TextMesh next = GetMeshText(new Vector3(2.0f, 1.05f), GetXmlValue(top, "next") + ":", f);
		f.scale = 0.018f;
		TextMesh none = GetMeshText(new Vector3(2.0f, 0.9f), GetXmlValue(top, "none"), f);
		none.gameObject.SetActive(false);
		return new PuzzleSelectPreview(theirs, yours, extra, bgs, next.gameObject, none.gameObject, minisheet, levelData, levelCount);
	}
	private void AddVictoryIcons() {
		List<int> victories = GetCompletedPuzzles();
		if(victories.Count == 0) { return; }
		Sprite trophy = Resources.LoadAll<Sprite>(SpritePaths.CharSelVictoryIcons)[2];
		for(int i = 0; i < victories.Count; i++) {
			int v = victories[i] - 1, y = Mathf.FloorToInt(v/4), x = v - y * 4;
			GetGameObject(new Vector3(-2.45f + 0.6f * x, -1.05f + 0.3f * (7 - y)), "trophy" + v, trophy, false, "HUD");
		}
	}
	public List<int> GetCompletedPuzzles() {
		SaveData saveInfo = PD.GetSaveData();
		List<int> keys = new List<int>();
		foreach(int k in saveInfo.puzzleScores.Keys) { keys.Add(k); }
		return keys;
	}

	public void Update() {
		UpdateMouseInput();
		if(cursor.back() || isClickingBack()) { SignalSuccess(); PD.GoToMainMenu(); return; }
		if(cursor.launchOrPause() || HandleMouse()) { SignalSuccess(); ConfirmSelection(); }
		cursor.DoUpdate();
		int l = GetLevelFromCursorPos();
		if(curLevel == l) { return; }
		curLevel = l;
		previewer.SetPreviewTiles(curLevel);
		infotext.text = GetLevelText(curLevel);
	}
	private int GetLevelFromCursorPos() { return (7 - cursor.getY()) * 4 + cursor.getX() + 1; }
	private void ConfirmSelection() {
		if(GetLevelFromCursorPos() > levelCount) { return; }
		PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
		int level = GetLevelFromCursorPos();
		PD.SetPuzzleLevel(level);
		PD.p1Char = (PersistData.C)Random.Range(0,11);
		XmlNode levelInfo = levelData[level];
		PD.rowCount = System.Convert.ToInt32(levelInfo.SelectSingleNode("playerheight").InnerText);
		PD.rowCount2 = System.Convert.ToInt32(levelInfo.SelectSingleNode("botheight").InnerText);
		PD.ChangeScreen(PersistData.GS.Game);
	}
	private string GetLevelText(int level) {
		if(level > levelCount) { return "plz hold"; }
		XmlNode l = levelData[level];
		int type = System.Convert.ToInt32(l.SelectSingleNode("type").InnerText);
		int limit = System.Convert.ToInt32(l.SelectSingleNode("limit").InnerText);
		int pcount = System.Convert.ToInt32(l.SelectSingleNode("playerheight").InnerText);
		int bcount = System.Convert.ToInt32(l.SelectSingleNode("botheight").InnerText);
		string res;
		switch(type) {
			case 0:
			res = string.Format(GetXmlValue(navinfo, "rowlimit"), numberToOrdinalString(limit));
				break;
			case 1:
				res = string.Format(GetXmlValue(navinfo, "rotatelimit"), limit);
				break;
			case 2:
				res = string.Format(GetXmlValue(navinfo, "launchlimit"), limit);
				break;
			default:
				res = "Fix your XML!";
				break;
		}
		res += "\n\n" + GetXmlValue(navinfo, "playerrows") + ": " + pcount;
		res += "\n" + GetXmlValue(navinfo, "targetrows") + ": " + bcount;
		string typeName = "";
		switch(type) {
			case 1: typeName = "fewestrotations"; break;
			case 2: typeName = "fewestlaunches"; break;
		}
		res += string.Format(GetPuzzleData(level, type), GetXmlValue(navinfo, "bestscore"), (type==0?"":GetXmlValue(navinfo, typeName)));
		return res;
	}
	private string numberToOrdinalString(int l) {
		switch(l) {
			case 1: return "1st";
			case 2: return "2nd";
			case 3: return "3rd";
			default: return l.ToString() + "th";
		}
	}
	public string GetPuzzleData(int l, int type) {
		SaveData saveInfo = PD.GetSaveData();
		if(!saveInfo.puzzleScores.ContainsKey(l)) { return ""; }
		return "\n\n{0}: " + saveInfo.puzzleScores[l] + ((type == 0)?"":("\n{1}: " + saveInfo.puzzleTimes[l]));
	}
	private string ConvertSecondsToMinuteSecondFormat(int time) {
		int seconds = time % 60;
		int minutes = Mathf.FloorToInt(time / 60.0f);
		return (minutes<10?"0":"") + minutes + ":" + (seconds<10?"0":"") + seconds;
	}

	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
		Vector2 p = GetClickSelection(clicker.getPosition());
		if(p.x >= 0) { cursor.setX((int)p.x); cursor.setY((int)p.y); }
		return p.x >= 0 && clicker.isDown();
	}
	private Vector2 GetClickSelection(Vector2 p) {
		if(p == Vector2.zero) { return new Vector2(-1.0f, -1.0f); }
		if(clicker.getPositionInGameObject(menu).z == 0.0f) { return new Vector2(-1.0f, -1.0f); }
		float ny = p.y - menu.transform.localPosition.y;
		int y = 0;
		if(ny > 0.97) { y = 7; }
		else if(ny > 0.67) { y = 6; }
		else if(ny > 0.37) { y = 5; }
		else if(ny > 0.07) { y = 4; }
		else if(ny > -0.23) { y = 3; }
		else if(ny > -0.53) { y = 2; }
		else if(ny > -0.73) { y = 1; }
		float nx = p.x - menu.transform.localPosition.x;
		int x = 0;
		if(nx > 0.65) { x = 3; }
		else if(nx > 0.0) { x = 2; }
		else if(nx > -0.65) { x = 1; }
		return new Vector2(x, y);
	}
}
