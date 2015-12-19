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
using System.Linq;
using System.Xml;
public class BlockHandler {
	private List<int> vals;
	private List<int> valsBot;
	private int[] playerInts;
	private PersistData.GT gameType;
	private bool isTutorial;
	private int balloonType, balloonNum;
	public BlockHandler(PersistData PD, int lv) {
		gameType = PD.gameType;
		balloonNum = (PD.p2Char == PersistData.C.FuckingBalloon)?2:(PD.p1Char == PersistData.C.FuckingBalloon)?1:-1;
		balloonType = PD.balloonType;
		isTutorial = PD.isTutorial;
		if(gameType == PersistData.GT.Challenge) {
			SetupPlayerInts();
			SetupTilesForPuzzle(lv);
		} else if(gameType != PersistData.GT.Training || !isTutorial) {
			vals = new List<int>();
			SetupPlayerInts();
			for(int i = 0; i < 50; i++) { vals.Add(Random.Range(0, 3)); }
		} else {
			if(isTutorial) {
				SetupPlayerInts();
				SetupTilesForPuzzle(0);
			}
		}
	}
	private void SetupPlayerInts() { playerInts = new int[] {0, 0}; }
	public int GetPlayerColor(int player) {
		if(balloonNum == player) { return balloonType; }
		if(gameType == PersistData.GT.Campaign && player == 2) { return Random.Range(0, 3); }
		if(gameType == PersistData.GT.Challenge) { return GetTileForPlayerPuzzle(player); }
		if(gameType == PersistData.GT.Training && isTutorial) { return GetTileForPlayerPuzzle(player); }
		int idx = player - 1;
		int pos = playerInts[idx];
		if(pos >= vals.Count) {
			int minPos = GetMinPos();
			if(minPos > 0) {
				while(minPos-- > 0) { vals.RemoveAt(0); vals.Add(Random.Range(0, 3)); }
				minPos = GetMinPos();
				playerInts[0] -= minPos;
				playerInts[1] -= minPos;
			} else {
				for(int i = 0; i < 10; i++) { vals.Add(Random.Range(0, 3)); }
			}
		}
		pos = playerInts[idx]++;
		return vals[pos];
	}
	private void SetupTilesForPuzzle(int lv) { // 0 is tutorial
		XmlDocument doc = new XmlDocument();
		TextAsset ta = Resources.Load<TextAsset>("XML/challenges");
		doc.LoadXml(ta.text);
		XmlNode An = doc.SelectSingleNode("challenges");
		XmlNodeList nl = An.SelectNodes("challenge");
		XmlNode l = nl[lv];
		vals = l.SelectSingleNode("p1").InnerText.Split(',').Select(n => System.Convert.ToInt32(n)).ToList();
		valsBot = l.SelectSingleNode("p2").InnerText.Split(',').Select(n => System.Convert.ToInt32(n)).ToList();
	}
	private int GetTileForPlayerPuzzle(int player) {
		if(player == 1) {
			if(playerInts[0] >= vals.Count) { return 3; } 
			int res = vals[playerInts[0]++];
			if(res == -1) { if(Random.value < 0.5f) { return 1; } else { return 2; } }
			if(res == -2) { if(Random.value < 0.5f) { return 2; } else { return 0; } }
			if(res == -3) { if(Random.value < 0.5f) { return 1; } else { return 0; } }
			if(res < 4) { return res; }
			return Random.Range(0, 3);
		}
		if(playerInts[1] >= valsBot.Count) { return Random.Range(0, 3); }
		return valsBot[playerInts[1]++];
	}
	private int GetMinPos() { return (playerInts[0] < playerInts[1]) ? playerInts[0] : playerInts[1]; }
}