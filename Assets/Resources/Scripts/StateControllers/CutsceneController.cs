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
public class CutsceneController:CharDisplayController {
	private CutsceneChar playeractor, opponentactor;
	private InputCore rawInput;
	private XmlNodeList dialogArr;
	private int inputDelay, curFrame;
	private DialogContainer dialogueBox;
	private GameObject skipButton, skipText, skipMenu;
	private Sprite[] skipButtonSheet;
	private TextMesh skipMenuText;
	private bool skipMenuIsUp;
	public void Start() {
		StateControllerInit(false);

		playeractor = CreateActor(PD.GetPlayerSpritePath(PD.p1Char), new Vector3(-6.0f, 0.0f));
		PD.GetNextOpponent();
		opponentactor = CreateActor(PD.GetPlayerSpritePath(PD.p2Char), new Vector3(6.0f, 0.0f), true);
		skipMenuIsUp = false;

		XmlNodeList dialogs = GetXMLHead("/" + playeractor.GetPath(), "dialogs").SelectNodes("dialog");
		XmlNode dialog = dialogs[PD.GetPuzzleLevel()];
		dialogArr = dialog.SelectNodes("line");

		rawInput = GetInputHandler();

		dialogueBox = gameObject.AddComponent<DialogContainer>();
		dialogueBox.Setup(new Vector3(0.0f, -3.5f));

		bool isBossChar = PD.p1Char == PersistData.C.White || PD.p1Char == PersistData.C.September;
		curFrame = (!isBossChar && (PD.level == 6 || PD.level == 8))?0:2;
		StartFrame(curFrame);

		skipButtonSheet = Resources.LoadAll<Sprite>(SpritePaths.ShortButtons);
		skipButton = GetGameObject(new Vector3(8.3f, -4.75f), "Skip", skipButtonSheet[0], true, "HUD");
		FontData font = PD.mostCommonFont.Clone(); font.scale = 0.045f;
		XmlNode top = GetXMLHead();
		skipText = GetMeshText(new Vector3(8.3f, -4.61f), GetXmlValue(top, "skip"), font).gameObject;

		mouseObjects.Add(skipButton);
		mouseObjects.Add(skipText);

		skipMenu = GetGameObject(new Vector3(0.0f, 0.0f), "Skip Menu", Resources.Load<Sprite>(SpritePaths.CutsceneSkipBox), false, "HUD");

		string f = string.Format(GetXmlValue(top, "skipmessage"), "\r\n", PD.controller.GetFriendlyActionName(InputMethod.Action.launch), PD.controller.GetFriendlyActionName(InputMethod.Action.pause));
		font.scale = 0.08f;
		skipMenuText = GetMeshText(new Vector3(0.0f, 0.5f), f, font);
		skipMenuText.text = new WritingWriter().GetWrappedString(skipMenuText, f, skipMenu.renderer.bounds.size);
		skipMenuText.gameObject.SetActive(false);
		skipMenu.SetActive(false);

		PD.sounds.SetSoundVolume(PD.GetSaveData().savedOptions["vol_s"] / 350.0f);
	}
	private InputCore GetInputHandler() {
		inputDelay = 0;
		InputCore c = gameObject.AddComponent<InputCore>();
		c.SetController(PD.controller, PD.GetKeyBindings());
		return c;
	}

	public void Update() {
		UpdateMouseInput();
		bool skip = false;
		bool inSkipButton = (clicker.getPositionInGameObject(skipButton).z != 0);
		skipButton.GetComponent<SpriteRenderer>().sprite = skipButtonSheet[inSkipButton?1:0];
		if(PD.usingMouse && clicker.isDown()) { 
			if(inSkipButton) { 
				AdvanceToGameOrCredits();
			} else {
				skip = true; 
			}
		} else if(--inputDelay <= 0) {
			if(rawInput.pause()) {
				if(skipMenuIsUp) {
					AdvanceToGameOrCredits();
				} else {
					ToggleSkipMenu(true);
				}
			} else if(rawInput.launch()) {
				if(skipMenuIsUp) {
					ToggleSkipMenu(false);
				} else {
					skip = true;
				}
				inputDelay = 7;
			}
		}
		if(dialogueBox.UpdateTextAndCheckIfMovingOn(skip)) {  StartFrame(++curFrame); }
	}
	private void ToggleSkipMenu(bool show) {
		skipMenuIsUp = show;
		skipMenu.SetActive(show);
		skipMenuText.gameObject.SetActive(show);
	}
	private void AdvanceToGameOrCredits() {
		PD.sounds.SetSoundVolume(PD.GetSaveData().savedOptions["vol_s"] / 100.0f);
		bool isBossChar = PD.p1Char == PersistData.C.White || PD.p1Char == PersistData.C.September;
		if(!isBossChar && (PD.level == 6 || PD.level == 8)) { 
			if(PD.level == 6) { PD.winType = 1; } else { PD.winType = 2; }
			PD.ChangeScreen(PersistData.GS.Credits);
		} else { PD.ChangeScreen(PersistData.GS.Game); }
	}
	private void StartFrame(int frame) {
		if(frame >= dialogArr.Count) {
			AdvanceToGameOrCredits();
			return;
		}
		XmlNode line = dialogArr[frame];
		dialogueBox.StartTextFrame(line.InnerText);
		if(line.Attributes["speaker"].Value == "1") { 
			UpdateActorPoseAndTextBoxName(playeractor, line.Attributes["pose"].Value);
			if(line.Attributes["voice"] != null) { playeractor.SayThingFromXML(line.Attributes["voice"].Value); }
		} else { 
			UpdateActorPoseAndTextBoxName(opponentactor, line.Attributes["pose"].Value);
			if(line.Attributes["voice"] != null) { opponentactor.SayThingFromXML(line.Attributes["voice"].Value, true); }
		}
		if(line.Attributes["speed"] == null) { dialogueBox.UpdateFrameRate(1.0f); }
		else { dialogueBox.UpdateFrameRate(float.Parse(line.Attributes["speed"].Value)); }
	}
	private void UpdateActorPoseAndTextBoxName(CutsceneChar fucker, string pose) {
		int poseInt = int.Parse(pose);
		dialogueBox.SetName(fucker.GetName());
		fucker.SetSprite(poseInt);
	}
}