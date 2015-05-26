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
public class PuzzleSelectPreview {
	private List<GameObject> theirs, yours, extra, bg;
	private GameObject nextText, noneText;
	private Sprite[] minisheet;
	private XmlNodeList levelData;
	private int levelCount;
	public PuzzleSelectPreview(List<GameObject> t, List<GameObject> y, List<GameObject> e, List<GameObject> b, GameObject ne, GameObject no, Sprite[] m, XmlNodeList l, int lc) {
		theirs = t;
		yours = y;
		extra = e;
		bg = b;
		nextText = ne;
		noneText = no;
		minisheet = m;
		levelData = l;
		levelCount = lc;
	}
	public void SetPreviewTiles(int level) {
		if(level > levelCount) { 
			for(int i = 0; i < 64; i++) {
				yours[i].GetComponent<SpriteRenderer>().sprite = minisheet[3];
				theirs[i].GetComponent<SpriteRenderer>().sprite = minisheet[3];
				if(i < 36) { extra[i].GetComponent<SpriteRenderer>().sprite = minisheet[3]; }
			}
			foreach(GameObject b in bg) { b.SetActive(false); }
			nextText.SetActive(false);
			noneText.SetActive(false);
			return;
		} else {
			foreach(GameObject b in bg) { b.SetActive(true); }
			nextText.SetActive(true);
		}
		XmlNode l = levelData[level];
		int pcount = System.Convert.ToInt32(l.SelectSingleNode("playerheight").InnerText);
		int bcount = System.Convert.ToInt32(l.SelectSingleNode("botheight").InnerText);
		List<int> vals = l.SelectSingleNode("p1").InnerText.Split(',').Select(n => System.Convert.ToInt32(n)).ToList();
		List<int> valsBot = l.SelectSingleNode("p2").InnerText.Split(',').Select(n => System.Convert.ToInt32(n)).ToList(); 
		for(int y = 0; y < 8; y++) {
			for(int x = 0; x < 8; x++) {
				int point = y * 8 + x;
				int invpoint = 63 - point;
				if(y >= pcount) {
					yours[point].GetComponent<SpriteRenderer>().sprite = minisheet[3];
				} else {
					yours[point].GetComponent<SpriteRenderer>().sprite = minisheet[vals[point]];
				}
				if(y >= bcount) {
					theirs[invpoint].GetComponent<SpriteRenderer>().sprite = minisheet[3];
				} else {
					theirs[invpoint].GetComponent<SpriteRenderer>().sprite = minisheet[valsBot[point]];
				}
			}
		}
		int pos = pcount * 8;
		bool hasNextTiles = false;
		for(int i = 0; i < 36; i++) {
			if(pos >= vals.Count) {
				extra[i].GetComponent<SpriteRenderer>().sprite = minisheet[3];
			} else {
				hasNextTiles = true;
				extra[i].GetComponent<SpriteRenderer>().sprite = minisheet[vals[pos++]];
			}
		}
		noneText.SetActive(!hasNextTiles);
	}
}