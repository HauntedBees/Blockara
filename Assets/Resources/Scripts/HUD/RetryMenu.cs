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
public class RetryMenu:RetryPauseMenu {
	public void Initialize() {
		StateControllerInit(false);
		InitButtonSprites();
		state = 0;
		cursor = GetMenuCursor(1, 2, null, 0.0f, -0.08f, 0.0f, 0.2f, 0, 1, 1, -1, -1, "Pause HUD Cursor");
		menu = GetGameObject(Vector3.zero, "Retry Menu", Resources.LoadAll<Sprite>(SpritePaths.PauseMenus)[2], true, "Pause HUD");
		AddTextToMenu();
	}
	private void AddTextToMenu() {
		float x = 0.0f, topy = 0.53f;
		XmlNode top = GetXMLHead();
		FontData f = PD.mostCommonFont.Clone(); f.layerName = "Pause HUD Text";
		textMeshes = new TextMesh[3];
		menuButtons = new GameObject[2];
		textMeshes[0] = GetMeshText(new Vector3(x, topy), GetXmlValue(top, "youlose"), f);
		AddButton(1, x, topy - 0.4f, GetXmlValue(top, "retry"), f);
		AddButton(2, x, topy - 0.7f, GetXmlValue(top, "endgame"), f);
		selectedIdx = 1;
	}
	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
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