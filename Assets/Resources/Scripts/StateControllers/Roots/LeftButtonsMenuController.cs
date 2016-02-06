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
public class LeftButtonsMenuController:MenuController {
	protected Sprite[] leftButton;
	protected GameObject[][] sidepanels;
	protected override void StateControllerInit(bool showBack = true) {
		base.StateControllerInit(showBack);
		buttonOpacityTime = 0;
		GetGameObject(Vector3.zero, "Gradient BG Cover", Resources.Load<Sprite>(SpritePaths.BGBlackFadeLeft), false, "BG1");
		leftButton = Resources.LoadAll<Sprite>(SpritePaths.LeftButtons);
	}
	protected GameObject[] GetButton(float x, float y, string text, FontData f) {
		Vector3 pos = new Vector3(x, y);
		GameObject g = GetGameObject(pos, "Button: " + text, leftButton[0], true, "HUD");
		GameObject g2 = GetGameObject(pos, "ButtonTop: " + text, leftButton[1], false, "HUDPlusOne");
		g2.SetActive(false);
		pos.y += 0.1f;
		GetMeshText(pos, text, f);
		return new GameObject[] {g, g2};
	}
	protected void TweenAndMouse() {
		UpdateMouseInput();
		HandleMouse();
	}
	protected GameObject GetGoBackImage(float x, float y) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(SpritePaths.PersistDataBack);
		return GetGameObject(new Vector3(x, y - 0.926f), "Return", sprites[Random.Range(0, 5)], false, "HUD");
	}
	protected string GetFunFactText() {
		XmlDocument doc = new XmlDocument();
		TextAsset ta = Resources.Load<TextAsset>("XML/" + PD.culture + "/facts");
		doc.LoadXml(ta.text);
		XmlNode top = doc.SelectSingleNode("facts");
		int numFacts = top.ChildNodes.Count;
		return top.ChildNodes[Random.Range(0, numFacts)].InnerText.Replace("{0}", "\r\n");
	}
}