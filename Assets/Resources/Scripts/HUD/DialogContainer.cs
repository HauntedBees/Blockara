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
public class DialogContainer:StateController {
	private const float DEFAULT_DISPLAY_RATE = 0.05f;
	private float display_rate;

	private TextMesh nameText, dialogText;
	
	private int stringIdx;
	private float delay;
	private string stringVal;

	private WritingWriter writer;
	private Vector2 bounds;

	public void SetName(string s) { nameText.text = s; }
	private void SetDialog(string s) { dialogText.text = s; }
	private void AddToDialog(char s) { dialogText.text += s; }
	public void Setup(Vector3 pos, bool onGameScreen = false) {
		GetPersistData();
		writer = new WritingWriter();

		GameObject textBox = GetGameObject(pos, "Text Box", Resources.Load<Sprite>(SpritePaths.DialogBox), false, "Cover HUD Dialog Box");
		if(onGameScreen) {
			Vector3 half = new Vector3(0.4f, 0.4f);
			textBox.transform.localScale = half;
		}
		bounds = new Vector3(textBox.renderer.bounds.size.x * 0.97f, textBox.renderer.bounds.size.y * 0.66f);

		Vector3 textpos = textBox.transform.position;
		textpos.x -= 7.6f * (onGameScreen?0.4f:1.0f);
		textpos.y += 1.5f * (onGameScreen?0.4f:1.0f);
		FontData f = PD.mostCommonFont.Clone();
		f.align = TextAlignment.Left; f.anchor = TextAnchor.UpperLeft; f.layerName = "Cover HUD Dialog Text";
		f.scale = onGameScreen?0.04f:0.1f;
		nameText = GetMeshText(textpos, "Ass", f);
		textpos.y -= 0.6f * (onGameScreen?0.4f:1.0f);
		textpos.x += 0.025f;
		f.scale = onGameScreen?0.04f:0.10f;
		dialogText = GetMeshText(textpos, "", f);
	}
	public bool UpdateTextAndCheckIfMovingOn(bool skip) {
		bool movingOn = false;
		if(skip) {
			if(stringIdx >= stringVal.Length) {
				movingOn = true;
			} else {
				stringIdx = stringVal.Length;
				SetDialog(stringVal);
			}
		}
		delay -= Time.deltaTime;
		if(delay <= 0) {
			int idx = ++stringIdx;
			if(idx < stringVal.Length) {
				char s = stringVal[idx];
				AddToDialog(s);
				if(stringVal[idx] != ' ') { PlaySound(); }
				delay = display_rate;
			}
		}
		return movingOn;
	}
	public void PlaySound() {
		PD.sounds.SetSoundAndPlay(SoundPaths.CutscenePath + nameText.text.Replace(".", "").Replace("/", "").Replace("MODE", "Depeche"));
	}
	public void UpdateFrameRate(float f) { display_rate = DEFAULT_DISPLAY_RATE * f; }
	public void StartTextFrame(string text) {
		delay = 0;
		stringIdx = -1;
		stringVal = writer.GetWrappedString(dialogText, text, bounds);
		SetDialog("");
	}
}