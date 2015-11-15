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
using System.Xml;
public class PuzzleHUD:InGameHUD {
	private TextMesh remainingText;
	private int initRemainingMoves, remainingMoves, puzzleType;
	private MenuCursor cursor;
	public RetryMenu retryMenu;
	override protected void AdditionalSetup(Sprite tile, int players, Vector3 offset, System.Xml.XmlNode top, int additionalInfo) {
		GetPersistData();
		SetupFromPuzzleLevel(additionalInfo);
		float x = PD.IsLeftAlignedHUD() ? -1.2f : 0.6f;
		Vector3 pos = new Vector3(x, 1.03f);
		AddDamageReferenceKey();
		FontData f = new FontData(TextAnchor.MiddleLeft, TextAlignment.Left, 0.03f);
		switch(puzzleType) {
			case 0: 
				GetMeshText(pos, GetXmlValue(top, "lockedrow"), f);
				AddRowLocks();
				break;
			case 1: 
				GetMeshText(pos, GetXmlValue(top, "rotations"), f);
				break;
			case 2: 
				GetMeshText(pos, GetXmlValue(top, "launches"), f);
				break;
		}
		remainingText = GetMeshText(new Vector3(x + 0.6f, 0.84f), initRemainingMoves.ToString(), new FontData(TextAnchor.MiddleRight, TextAlignment.Right, 0.045f));
	}
	private void AddRowLocks() {
		Sprite rowLockSprite = Resources.Load<Sprite>(SpritePaths.LockedRow);
		int actualUnlockedRow = PD.rowCount - initRemainingMoves;
		for(int y = 0; y < PD.rowCount; y++) {
			if(actualUnlockedRow == y) { continue; }
			GetGameObject(new Vector3((PD.IsLeftAlignedHUD()?0.45f:-0.45f), -1.84f + y * Consts.TILE_SIZE), "lock" + y, rowLockSprite, false, "Zapper");
		}
	}
	private void AddDamageReferenceKey() {
		Vector2 pos = new Vector2(PD.IsLeftAlignedHUD()?-0.9f:0.9f, 0.42f);
		GameObject helper = GetGameObject(pos, "Damage Reference", Resources.Load<Sprite>(SpritePaths.GuideCircle + (PD.IsColorBlind()?SpritePaths.ColorblindSuffix:"")), false, "Reference");
		helper.transform.localScale = new Vector2(0.8f, 0.8f);
	}
	public int GetUnlockedRow() { if(puzzleType == 0) { return PD.rowCount - initRemainingMoves; } else { return -1; } }
	private void SetupFromPuzzleLevel(int level) {
		XmlNode top = GetXMLHead("/challenges", "challenges", false);
		XmlNode levelInfo = top.SelectNodes("challenge")[level];
		puzzleType = System.Convert.ToInt32(levelInfo.SelectSingleNode("type").InnerText);
		remainingMoves = System.Convert.ToInt32(levelInfo.SelectSingleNode("limit").InnerText) - (puzzleType == 0?0:1);
		initRemainingMoves = remainingMoves;
		PD.puzzleType = puzzleType;
		PD.rowCount = System.Convert.ToInt32(levelInfo.SelectSingleNode("playerheight").InnerText);
		PD.rowCount2 = System.Convert.ToInt32(levelInfo.SelectSingleNode("botheight").InnerText);
	}
	override public void DoUpdate(bool paused, int p1val, int p2val, bool hiddenPause = false) {
		if(gameEnd) { return; }
		HandlePause(paused);
		if(Time.time >= lastCheck + 1.0f) { UpdateTimer(); }
		if(p1Score_val != p1val) { p1Score_val = p1val; UpdateTextValueAndSize(p1ScoreText, p1Score_val); }
		if(puzzleType != 0 && p2val != 0) { remainingText.text = Mathf.Max(--remainingMoves, 0).ToString(); }
		if((p2val == 1 && remainingMoves <= 0) || (p2val == 2 && remainingMoves < 0)) { lose = true; }
	}
	public int GetRemainingMoves() { return (initRemainingMoves - remainingMoves); }
	public void DisplayGameOverRetryScreen() {
		if(retryMenu != null) { return; }
		GameObject g = new GameObject("RetryHandler");
		retryMenu = g.AddComponent<RetryMenu>();
		retryMenu.Initialize();
	}
}