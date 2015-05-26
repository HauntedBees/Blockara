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
public class PauseMenu:RetryPauseMenu {
	public int maxy;
	public void Initialize(int p) {
		StateControllerInit(false);
		InitButtonSprites();
		player = p;
		state = 0; maxy = PD.gameType == PersistData.GT.Challenge?3:2;
		cursor = GetMenuCursor(1, PD.gameType == PersistData.GT.Challenge?4:3, null, 0.0f, PD.gameType == PersistData.GT.Challenge?-0.48f:-0.28f, 0.0f, 0.2f, 0, maxy, p, -1, -1, "Pause HUD Cursor");
		menu = GetGameObject(Vector3.zero, "Pause Menu", Resources.LoadAll<Sprite>(SpritePaths.PauseMenus)[PD.gameType == PersistData.GT.Challenge?0:1], true, "Pause HUD");
		AddTextToMenu();
	}
	private void AddTextToMenu() {
		float x = 0.0f, topy = (PD.gameType == PersistData.GT.Challenge) ? 0.75f : 0.61f;
		XmlNode top = GetXMLHead();
		FontData f = PD.mostCommonFont.Clone(); f.layerName = "Pause HUD Text";
		textMeshes = new TextMesh[PD.gameType == PersistData.GT.Challenge?5:4];
		menuButtons = new GameObject[PD.gameType == PersistData.GT.Challenge?4:3];
		textMeshes[0] = GetMeshText(new Vector3(x, topy), GetXmlValue(top, "pause"), f);
		AddButton(1, x, topy - 0.4f, GetXmlValue(top, "resume"), f);
		if(PD.gameType == PersistData.GT.Challenge) {
			AddButton(2, x, topy - 0.7f, GetXmlValue(top, "retry"), f);
			AddButton(3, x, topy - 1.0f, GetXmlValue(top, "endgame"), f);
			AddButton(4, x, topy - 1.3f, GetXmlValue(top, "desktop"), f);
			selectedIdx = 3;
		} else {
			AddButton(2, x, topy - 0.7f, GetXmlValue(top, "endgame"), f);
			AddButton(3, x, topy - 1.0f, GetXmlValue(top, "desktop"), f);
			selectedIdx = 2;
		}
	}
	protected override bool HandleMouse() {
		if(!PD.usingMouse || player == 2) { return false; }
		bool isOverSomething = false;
		for(int i = 0; i < menuButtons.Length; i++) {
			if(clicker.getPositionInGameObject(menuButtons[i]).z == 1) {
				cursor.setY(cursor.boardheight - i - 1);
				isOverSomething = true;
				break;
			}
		}
		if(!isOverSomething) { return false; }
		if(clicker.isDown()) { state = cursor.getY() + 1; }
		return true;
	}
}