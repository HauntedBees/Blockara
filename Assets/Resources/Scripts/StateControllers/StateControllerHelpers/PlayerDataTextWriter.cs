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
public class PlayerDataTextWriter:ScoreTextFormatter {
	private TextMesh headerText;
	private TextMesh infoPaneTextCenter, infoPaneTextLeft, infoPaneTextRight, infoPaneFunFact;
	private SaveData sd;
	private List<XmlNode> characterBios;
	private XmlNode navDetails;
	private GameObject characters, goBack;
	private Sprite[] charSheet;
	private Vector2 bounds;
	public PlayerDataTextWriter(TextMesh h, TextMesh c, TextMesh l, TextMesh r, TextMesh ff, List<XmlNode> cb, XmlNode top, GameObject ch, GameObject gb, PersistData p) {
		headerText = h;
		infoPaneTextCenter = c;
		infoPaneTextLeft = l;
		infoPaneTextRight = r;
		infoPaneFunFact = ff;
		sd = p.GetSaveData();
		characterBios = cb;
		characters = ch;
		goBack = gb;
		goBack.SetActive(false);
		navDetails = top;
		charSheet = Resources.LoadAll<Sprite>(SpritePaths.CharFullShots);
		PD = p;
		bounds = new Vector2(2.4f, 3.0f);
	}
	private string GetXmlValue(string id) {
		XmlNode elem = navDetails.SelectSingleNode(id);
		return (elem == null ? "ERROR LOL" : elem.InnerText);
	}
	public void SetToTotalPanel() {
		infoPaneTextRight.gameObject.transform.localScale = new Vector3(0.04f, 0.04f);
		headerText.text = GetXmlValue("gamedata");
		goBack.SetActive(false);
		
		int qpt = sd.getPlayTime("Quick Play");
		int at = sd.getPlayTime("Arcade");
		int ct = sd.getPlayTime("Campaign");
		int vt = sd.getPlayTime("Versus");
		int tt = sd.getPlayTime("Training");
		int pt = sd.getPlayTime("Puzzle");
		int total = qpt + at + ct + vt + tt + pt;
		
		string res = GetXmlValue("totalplaytime") + ": " + ConvertSecondsToMinuteSecondFormat(total, true);
		
		res += "\n" + GetXmlValue("quickplaytime") + ": " + ConvertSecondsToMinuteSecondFormat(qpt, true);
		res += "\n" + GetXmlValue("arcadetime") + ": " + ConvertSecondsToMinuteSecondFormat(at, true);
		res += "\n" + GetXmlValue("campaigntime") + ": " + ConvertSecondsToMinuteSecondFormat(ct, true);
		res += "\n" + GetXmlValue("versustime") + ": " + ConvertSecondsToMinuteSecondFormat(vt, true);
		res += "\n" + GetXmlValue("trainingtime") + ": " + ConvertSecondsToMinuteSecondFormat(tt, true);
		res += "\n" + GetXmlValue("challengetime") + ": " + ConvertSecondsToMinuteSecondFormat(pt, true);
		
		res += "\n\n" + GetXmlValue("puzzlescompleted") + ": " + sd.getPuzzlesCompleted() + "/" + PuzzleSelectController.totalPuzzleCount;
		res += "\n" + GetXmlValue("arcademodevictories") + ": " + sd.getArcadeVictories();
		res += "\n" + GetXmlValue("favoritecharacter") + ": " + sd.GetMostPopularCharacter();
		
		res += "\n" + GetXmlValue("gamecompletionpercent") + ": " + sd.CalculateGameCompletionPercent() + "%";
		
		infoPaneTextCenter.text = res;
		infoPaneFunFact.text = "";
		infoPaneTextLeft.text = "";
		infoPaneTextRight.text = "";
	}
	
	public void SetToHighScorePanel(PersistData.GT type) {
		infoPaneTextRight.gameObject.transform.localScale = new Vector3(0.04f, 0.04f);
		goBack.SetActive(false);
		switch(type) {
			case PersistData.GT.QuickPlay: headerText.text = GetXmlValue("quickplay"); break;
			case PersistData.GT.Arcade: headerText.text = GetXmlValue("arcade"); break;
			case PersistData.GT.Campaign: headerText.text = GetXmlValue("campaign"); break;
		}
		infoPaneFunFact.text = "";
		infoPaneTextCenter.text = "";
		LoadHighScores(type);
	}
	private void LoadHighScores(PersistData.GT type) {
		List<KeyValuePair<string, int>> scores = sd.getHighScores(type);
		List<KeyValuePair<string, int>> times = sd.getBestTimes(type);
		SetScoreRowAndTimeRowWidths(scores, times);
		string resScores = GetXmlValue("highscores") + "\n";
		string resTimes = GetXmlValue("besttimes") + "\n";
		for(int i = 0; i < scores.Count; i++) {
			string scoreName = (i + 1).ToString() + ". " + scores[i].Key + " ";
			string timeName = (i + 1).ToString() + ". " + times[i].Key + " ";
			if(i < 9) {
				scoreName = " " + scoreName;
				timeName = " " + timeName;
			}
			string scoreText = scores[i].Value.ToString();
			string timeText = ConvertSecondsToMinuteSecondFormat(times[i].Value);
			resScores += GetRowText(scoreName, scoreText, false) + "\n";
			resTimes += GetRowText(timeName, timeText, true) + "\n";
		}
		infoPaneFunFact.text = "";
		infoPaneTextLeft.text = resScores;
		infoPaneTextRight.text = resTimes;
	}
	public void SetToBackPanel(string t) {
		goBack.SetActive(true);
		headerText.text = GetXmlValue("returntomenu");
		infoPaneFunFact.text = t;
		infoPaneTextCenter.text = "";
		infoPaneTextLeft.text = "";
		infoPaneTextRight.text = "";
	}
	public void SetToSoundTest() {
		goBack.SetActive(false);
		headerText.text = "";
		infoPaneFunFact.text = "";
		infoPaneTextLeft.text = "";
		infoPaneTextRight.text = "";
		infoPaneTextCenter.text = "";
	}
	public void SetToBiosPanel(int pos, PersistData pers) {
		goBack.SetActive(false);
		infoPaneFunFact.text = "";
		infoPaneTextCenter.text = "";
		infoPaneTextLeft.text = "";
		LoadCharacterBio(pos, pers);
	}
	public void LoadCharacterBio(int charIdx, PersistData pers) {
		XmlNode bio = characterBios[charIdx];
		headerText.text = bio.Attributes["name"].InnerText;
		int actIdx = int.Parse(bio.Attributes["idx"].InnerText);
		string res = "";
		if(actIdx < 0) {
			res = bio.SelectSingleNode("bio").InnerText;
			characters.SetActive(false);
		} else {
			characters.SetActive(true);
			int displayIdx = actIdx >= 10 ? (actIdx + 20) : (actIdx * 3 + Random.Range(0, 1 + pers.GetSaveData().getPlayerWinType(pers.GetPlayerSpritePathFromInt(actIdx))));
			characters.GetComponent<SpriteRenderer>().sprite = charSheet[displayIdx];
			res = GetXmlValue("age") + ": " + bio.SelectSingleNode("age").InnerText;
			res += "\n\n" + GetXmlValue("likes") + ": " + bio.SelectSingleNode("likes").InnerText;
			res += "\n\n" + GetXmlValue("dislikes") + ": " + bio.SelectSingleNode("dislikes").InnerText;
			res += "\n\n" + bio.SelectSingleNode("bio").InnerText;
		}
		infoPaneTextRight.text = res;
		infoPaneTextRight.text = GetWrappedString(infoPaneTextRight, res, bounds);
	}

	
	public void ToggleBioInfo(bool show) {
		if(show) {
			characters.SetActive(true);
			infoPaneTextRight.alignment = TextAlignment.Left;
		} else {
			characters.SetActive(false);
			infoPaneTextRight.alignment = TextAlignment.Center;
		}
	}
}