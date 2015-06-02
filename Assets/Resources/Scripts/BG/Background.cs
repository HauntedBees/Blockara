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
public class Background:All {
	private GameObject bg;
	public bool isCutscene, isWinScreen; // set in Scene
	void Start() {
		GetPersistData();
		string path = GetPath();
		Sprite bgSprite = Resources.Load<Sprite>(SpritePaths.BGPath + path);
		if(bgSprite == null) { bgSprite = Resources.Load<Sprite>(SpritePaths.DefaultBG); }
		bg = GetGameObject(Vector3.zero, "Background", bgSprite, false, "BG0");
		bg.transform.parent = gameObject.transform;
		if(isCutscene) { if(!isWinScreen) { bg.transform.localScale *= 2.5f; } } else { bg.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f); }
	}
	private string GetPath() {
		if(isWinScreen) { return PD.GetPlayerSpritePath(PD.p1Char, true); }
		if(PD.gameType == PersistData.GT.Versus) { return PD.GetPlayerSpritePath(Random.value > 0.5f?PD.p1Char:PD.p2Char, true); }
		if(PD.gameType == PersistData.GT.QuickPlay) { return PD.GetPlayerSpritePath(Random.value > 0.35f?PD.p1Char:PD.p2Char, true); }
		return PD.GetPlayerSpritePath(PD.p2Char, true);
	}
}