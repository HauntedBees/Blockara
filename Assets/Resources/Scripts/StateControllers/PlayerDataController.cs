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
public class PlayerDataController:LeftButtonsMenuController {
	private int charIdx, p1_delay, bioCount;
	private bool bioInfoHidden, inSoundTest;
	private OptionsSelector navigationArrows;
	private SoundTest soundTest;
	private PlayerDataTextWriter writer;
	public void Start() {
		StateControllerInit(false);
		charIdx = 0;

		inSoundTest = false;
		XmlNode top = GetXMLHead();
		float x = -2.85f, topy = 0.85f;
		
		selectedIdx = 5;
		sidepanels = new GameObject[7][];
		sidepanels[0] =	GetButton(x, topy, 		  GetXmlValue(top, "overall"), PD.mostCommonFont);
		sidepanels[1] =	GetButton(x, topy - 0.3f, GetXmlValue(top, "quickplay"), PD.mostCommonFont);
		sidepanels[2] =	GetButton(x, topy - 0.6f, GetXmlValue(top, "arcade"), PD.mostCommonFont);
		sidepanels[3] =	GetButton(x, topy - 0.9f, GetXmlValue(top, "campaign"), PD.mostCommonFont);
		sidepanels[4] =	GetButton(x, topy - 1.2f, GetXmlValue(top, "bios"), PD.mostCommonFont);
		sidepanels[5] =	GetButton(x, topy - 1.5f, GetXmlValue(top, "soundtest"), PD.mostCommonFont);
		sidepanels[6] =	GetButton(x, topy - 1.8f, GetXmlValue(top, "back"), PD.mostCommonFont);

		cursor = GetMenuCursor(1, 7, null, x, -topy, 0.0f, 0.3f, 0, 6);
		CreateInfoPane(0.8f, 0.0f);
	}
	private void CreateInfoPane(float x, float y) {
		GameObject infoPane = GetGameObject(new Vector3(x, y), "infoPane", Resources.Load<Sprite>(SpritePaths.BigOptionInfoBox));

		FontData font = PD.mostCommonFont.Clone();
		font.scale = 0.06f;
		TextMesh headerText = GetMeshText(new Vector3(x, y + 1.7f), "honk", font);
		font.scale = 0.04f;
		TextMesh infoPaneTextCenter = GetMeshText(new Vector3(x, y + 1.2f), "honk", font);
		TextMesh infoPaneTextLeft = GetMeshText(new Vector3(x - 1.0f, y + 1.2f), "honk", font);
		TextMesh infoPaneTextRight = GetMeshText(new Vector3(x + 1.0f, y + 1.2f), "honk", font);

		navigationArrows = gameObject.AddComponent<OptionsSelector>();
		navigationArrows.Setup(x - 2.35f, y - 1.6f, 0.0f, true);
		navigationArrows.SetWidth(4.7f);
		navigationArrows.UpdatePosition(0);

		soundTest = gameObject.AddComponent<SoundTest>();
		soundTest.Setup(0.7f, 0.6f, infoPane);
		soundTest.ToggleVisibility(false);

		List<XmlNode> bios = GetFilteredBiosList();
		GameObject characters = GetGameObject(new Vector3(x - 1.4f, y), "BioChar", null, false, "HUD");
		GameObject goBack = GetGoBackImage(x, y);
		writer = new PlayerDataTextWriter(headerText, infoPaneTextCenter, infoPaneTextLeft, infoPaneTextRight, bios, GetXMLHead(), characters, goBack, PD);
		bioCount = bios.Count - 1;
	}
	private List<XmlNode> GetFilteredBiosList() {
		XmlNodeList allBios = GetXMLHead("/characterBios", "bios").SelectNodes("character");
		List<XmlNode> availableBios = new List<XmlNode>();
		SaveData sd = PD.GetSaveData();
		for(int i = 0; i < allBios.Count - 1; i++) {
			if(sd.HasBeatenGameWithCharacter(allBios[i].Attributes["name"].InnerText)) {
				availableBios.Add(allBios[i]);
			}
		}
		if(availableBios.Count < 12) { availableBios.Add(allBios[12]); }
		return availableBios;
	}
	private bool UpdateSoundTest(bool useKeys) {
		bool res = soundTest.HandleMouseInput(clicker);
		if(!soundTest.DoUpdate(useKeys, cursor.GetController())) { SignalFailure(); inSoundTest = false; }
		if(PD.usingMouse) {
			cursor.DoUpdate();
			int cy = cursor.getY();
			UpdateCursorPosition(cy, true);
			if(clicker.isDown() && GetClickSelection(clicker.getPosition()) >= 0) {
				inSoundTest = false;
				UpdatePanel(cy);
			}
		}
		return res;
	}
	public void Update() {
		TweenAndMouse();
		if(inSoundTest) { UpdateSoundTest(true); return; }
		cursor.DoUpdate();
		int cy = cursor.getY();
		UpdateCursorPosition(cy);
		if(cy == 2 && --p1_delay <= 0) {
			int dx = 0;
			if(PD.usingMouse) { dx = HandleBioClicker(); }
			bool keyWasPressed = false;
			if(cursor.shiftRight() || cursor.shiftAllRight() || PD.controller.Nav_Right()) { 
				dx++; 
				keyWasPressed = true;
				navigationArrows.HighlightArrow(true);
			} else if(cursor.shiftLeft() || cursor.shiftAllLeft() || PD.controller.Nav_Left()) { 
				dx--; 
				keyWasPressed = true;
				navigationArrows.HighlightArrow(false);
			} else {
				if(!PD.usingMouse) { navigationArrows.ClearArrows(); }
			}
			charIdx += dx;

			if(charIdx < 0) { charIdx = bioCount; } else if(charIdx > bioCount) { charIdx = 0; }

			if(dx != 0) { SignalMovement(); writer.LoadCharacterBio(charIdx, PD); }

			if(keyWasPressed) { p1_delay = 20; }
		} else if(cy == 1) {
			bool res = UpdateSoundTest(false);
			if(res || cursor.launchOrPause() || (clicker.getPositionInGameObject(sidepanels[5][0]).z > 0.0f && clicker.isDown())) {
				if(!res) { SignalSuccess(); }
				inSoundTest = true;
			}
		}
		if(cursor.back() || (cursor.getY() == 0 && (cursor.launchOrPause() || (clicker.isDown() && PD.usingMouse)))) { SignalSuccess(); PD.GoToMainMenu(); }
	}
	private void UpdateCursorPosition(int pos, bool earlyExit = false) {
		int oldIdx = selectedIdx;
		selectedIdx = 6 - pos;
		if(selectedIdx != oldIdx) { sidepanels[oldIdx][1].SetActive(false); }
		sidepanels[selectedIdx][1].SetActive(true);
		sidepanels[selectedIdx][1].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, GetButtonOpacity());
		if(selectedIdx == oldIdx) { return; }
		if(earlyExit) { return; }
		UpdatePanel(pos);
	}
	private void UpdatePanel(int pos) {
		switch(pos) {
			case 6: writer.SetToTotalPanel(); ToggleBioInfo(false); soundTest.ToggleVisibility(false); break;
			case 5: writer.SetToHighScorePanel(PersistData.GT.QuickPlay); ToggleBioInfo(false); soundTest.ToggleVisibility(false); break;
			case 4: writer.SetToHighScorePanel(PersistData.GT.Arcade); ToggleBioInfo(false); soundTest.ToggleVisibility(false); break;
			case 3: writer.SetToHighScorePanel(PersistData.GT.Campaign); ToggleBioInfo(false); soundTest.ToggleVisibility(false); break;
			case 2: writer.SetToBiosPanel(charIdx, PD); ToggleBioInfo(true); soundTest.ToggleVisibility(false); break;
			case 1: writer.SetToSoundTest(); ToggleBioInfo(false); soundTest.ToggleVisibility(true); break;
			case 0: writer.SetToBackPanel(); ToggleBioInfo(false); soundTest.ToggleVisibility(false); break;
		}
	}
	private void ToggleBioInfo(bool show) {
		if(show) {
			if(!bioInfoHidden) { return; }
			navigationArrows.SetVisibility(true);
			writer.ToggleBioInfo(true);
			bioInfoHidden = false;
		} else {
			if(bioInfoHidden) { return; }
			navigationArrows.SetVisibility(false);
			writer.ToggleBioInfo(false);
			bioInfoHidden = true;
		}
	}
	
	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
		int p = GetClickSelection(clicker.getPosition());
		if(p >= 0) { cursor.setY(p); }
		return false;
	}
	private int GetClickSelection(Vector2 p) {
		if(p == Vector2.zero || p.x > -2.0f) { return -1; }
		float y = p.y;
		if(y > 0.75) { return 6; }
		if(y > 0.45) { return 5; }
		if(y > 0.15) { return 4; }
		if(y > -0.15) { return 3; }
		if(y > -0.45) { return 2; }
		if(y > -0.75) { return 1; }
		return 0;
	}
	private int HandleBioClicker() {
		int dx = 0;
		if(clicker.getPositionInGameObject(navigationArrows.leftArrow).z == 1) {
			navigationArrows.HighlightArrow(false);
			dx = -1;
		} else if(clicker.getPositionInGameObject(navigationArrows.rightArrow).z == 1) {
			navigationArrows.HighlightArrow(true);
			dx = 1;
		} else {
			navigationArrows.ClearArrows();
		}
		if(!clicker.isDown()) { return 0; }
		return dx;
	}
}