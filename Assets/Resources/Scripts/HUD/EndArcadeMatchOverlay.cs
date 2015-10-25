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
public class EndArcadeMatchOverlay:CharDisplayController {
	private CutsceneChar playeractor, opponentactor;
	private DialogContainer tbox;
	private int inputDelay;
	private bool isFirstLoad, isP1Speaker, didP1Win;
	public void Setup(bool won) {
		GetPersistData();
		didP1Win = won;
		playeractor = CreateActor(PD.GetPlayerSpritePath(PD.p1Char), new Vector3(-2.1f, 0.2f), false, true);
		playeractor.SetScale(0.4f);
		opponentactor = CreateActor(PD.GetPlayerSpritePath(PD.p2Char), new Vector3(2.1f, 0.2f), true, true);
		opponentactor.SetScale(0.4f);
		tbox = (DialogContainer) gameObject.AddComponent("DialogContainer");
		tbox.Setup(new Vector3(0.0f, -1.4f), true);
		isFirstLoad = true;
		PD.sounds.SetSoundVolume(PD.GetSaveData().savedOptions["vol_s"] / 350.0f);
		setText();
	}
	public bool doUpdate(bool skip) {
		if(isFirstLoad) {
			isFirstLoad = false;
			if(isP1Speaker) {
				playeractor.SayThingFromReaction(didP1Win?CutsceneChar.SpeechType.win:CutsceneChar.SpeechType.lose);
			} else {
				opponentactor.SayThingFromReaction(didP1Win?CutsceneChar.SpeechType.lose:CutsceneChar.SpeechType.win);
			}
		}
		return tbox.UpdateTextAndCheckIfMovingOn(skip);
	}
	private void setText() {
		if(PD.p2Char == PersistData.C.FuckingBalloon) {
			SetPuhloon();
			return;
		}
		XmlDocument doc = new XmlDocument();
		TextAsset ta = Resources.Load<TextAsset>("XML/" + PD.culture + "/" + playeractor.GetPath(PD.p1Char == PersistData.C.FuckingBalloon));
		doc.LoadXml(ta.text);
		XmlNode An = doc.SelectSingleNode("dialogs");
		XmlNodeList nl = An.SelectNodes("dialog");
		XmlNode dialog = nl[PD.GetPuzzleLevel()];
		XmlNodeList allLines = dialog.SelectNodes("line");
		XmlNode textNode = allLines[didP1Win?1:0];
		string textToSay = textNode.InnerText;
		if(textNode.Attributes["speaker"].Value == "1" && PD.p1Char == PersistData.C.FuckingBalloon) {
			textToSay = "p" + new string('f', Random.Range(4, 10)) + "th" + new string('e', Random.Range(4, 10)) + "nk";
		}
		tbox.StartTextFrame(textToSay);
		if(textNode.Attributes["speed"] == null) { tbox.UpdateFrameRate(1.0f); }
		else { tbox.UpdateFrameRate(float.Parse(textNode.Attributes["speed"].Value)); }
		if(textNode.Attributes["speaker"].Value == "1") { 
			setActor(playeractor, textNode.Attributes["pose"].Value);
			hideActor(opponentactor);
			isP1Speaker = true;
		} else { 
			setActor(opponentactor, textNode.Attributes["pose"].Value);
			hideActor(playeractor);
			isP1Speaker = false;
		}
	}
	private void SetPuhloon() {
		setActor(opponentactor, "1");
		hideActor(playeractor);
		isP1Speaker = false;
		tbox.StartTextFrame("pffffffffffffffheeeeeeeenk");
		tbox.SetName("The Master");
	}
	private void setActor(CutsceneChar fucker, string pose) {
		int poseInt = int.Parse(pose);
		tbox.SetName(fucker.GetName(), fucker == playeractor);
		fucker.SetSprite(poseInt);
	}
	private void hideActor(CutsceneChar fucker) { fucker.Hide(); }
}