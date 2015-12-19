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
public class AccessibilityHandler:MenuHandler {
	private TextMesh txt_colorblind, txt_hudplacement, txt_emphasizecursor, txt_touchcontrols, txt_easymode, txt_keydelay, txt_scopo, txt_clr;
	public int val_colorblind, val_hudplacement, val_emphasizecursor, val_touchcontrols, val_easymode, val_keydelay, val_scopo;
	public void Setup() {
		top = GetXMLHead();
		headerText.text = GetXmlValue(top, "accessibility");
		float x = 1.0f;
		FontData f = PD.mostCommonFont.Clone();
		f.align = TextAlignment.Right; f.anchor = TextAnchor.MiddleRight;
		texts.Add(GetMeshText(new Vector2(x,  1.0f), GetXmlValue(top, "colorblind") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.8f), GetXmlValue(top, "hudplacement") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.6f), GetXmlValue(top, "emphasizecursor") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.4f), GetXmlValue(top, "touchcontrols") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.2f), GetXmlValue(top, "easymode") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.0f), GetXmlValue(top, "keydelay") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  -0.2f), GetXmlValue(top, "scopophobia") + ":", f));
		x += 0.25f;
		f.align = TextAlignment.Left; f.anchor = TextAnchor.MiddleLeft;
		txt_colorblind = GetMeshText(new Vector3(x,  1.0f), GetXmlValue(top, "off"), f);
		txt_hudplacement = GetMeshText(new Vector3(x,  0.8f), GetXmlValue(top, "right"), f);
		txt_emphasizecursor = GetMeshText(new Vector3(x,  0.6f), GetXmlValue(top, "off"), f);
		txt_touchcontrols = GetMeshText(new Vector3(x,  0.4f), GetXmlValue(top, "off"), f);
		txt_easymode = GetMeshText(new Vector3(x,  0.2f), GetXmlValue(top, "off"), f);
		txt_keydelay = GetMeshText(new Vector3(x,  0.0f), "7", f);
		txt_scopo = GetMeshText(new Vector3(x,  -0.2f), GetXmlValue(top, "off"), f);
		texts.Add(GetMeshText(new Vector3(x, -0.4f), GetXmlValue(top, "apply"),  f));
		texts.Add(GetMeshText(new Vector3(x, -0.6f), GetXmlValue(top, "cancel"), f));
		texts.Add(txt_colorblind);
		texts.Add(txt_hudplacement);
		texts.Add(txt_emphasizecursor);
		texts.Add(txt_touchcontrols);
		texts.Add(txt_easymode);
		texts.Add(txt_keydelay);
		texts.Add(txt_scopo);
		x = 1.9f;
		colliders.Add(GetCollider("cbd", new Vector3(x, 1.0f)));
		colliders.Add(GetCollider("hud", new Vector3(x, 0.8f)));
		colliders.Add(GetCollider("cur", new Vector3(x, 0.6f)));
		colliders.Add(GetCollider("tch", new Vector3(x, 0.4f)));
		colliders.Add(GetCollider("e-z", new Vector3(x, 0.2f)));
		colliders.Add(GetCollider("del", new Vector3(x, 0.0f)));
		colliders.Add(GetCollider("sco", new Vector3(x, -0.2f)));
		colliders.Add(GetCollider("apl", new Vector3(x, -0.4f)));
		colliders.Add(GetCollider("cnx", new Vector3(x, -0.6f)));
	}
	public bool ChangeColorblind(int dx) {
		if((val_colorblind == 1 && dx == 1) || (val_colorblind == 0 && dx == -1)) { return false; }
		val_colorblind += dx;
		UpdateColorBlindText();
		UpdateCursorPosition(8, val_colorblind);
		return true;
	}
	public bool ChangeHUDPlacement(int dx) {
		if((val_hudplacement == 1 && dx == 1) || (val_hudplacement == 0 && dx == -1)) { return false; }
		val_hudplacement += dx;
		UpdateHUDPlacementText();
		UpdateCursorPosition(7, val_hudplacement);
		return true;
	}
	public bool ChangeEmphasizeCursor(int dx) {
		if((val_emphasizecursor == 1 && dx == 1) || (val_emphasizecursor == 0 && dx == -1)) { return false; }
		val_emphasizecursor += dx;
		UpdateEmphasizeCursorText();
		UpdateCursorPosition(6, val_emphasizecursor);
		return true;
	}
	public bool ChangeTouchControls(int dx) {
		if((val_touchcontrols == 1 && dx == 1) || (val_touchcontrols == 0 && dx == -1)) { return false; }
		val_touchcontrols += dx;
		UpdateTouchControlText();
		UpdateCursorPosition(5, val_touchcontrols);
		return true;
	}
	public bool ChangeEasyMode(int dx) {
		if((val_easymode == 1 && dx == 1) || (val_easymode == 0 && dx == -1)) { return false; }
		val_easymode += dx;
		UpdateEasyModeText();
		UpdateCursorPosition(4, val_easymode);
		return true;
	}
	public bool ChangeKeyDelay(int dx) {
		if((val_keydelay == 20 && dx == 1) || (val_keydelay == 6 && dx == -1)) { return false; }
		val_keydelay += dx;
		UpdateKeyDelayText();
		UpdateCursorPosition(3, val_keydelay);
		return true;
	}
	public bool ChangeScopophobia(int dx) {
		if((val_scopo == 1 && dx == 1) || (val_scopo == 0 && dx == -1)) { return false; }
		val_scopo += dx;
		UpdateScopoText();
		UpdateCursorPosition(8, val_scopo);
		return true;
	}
	public void Reset(SaveData sd) {
		val_colorblind = sd.savedOptions["colorblind"];
		val_hudplacement = sd.savedOptions["hudplacement"];
		val_emphasizecursor = sd.savedOptions["emphasizecursor"];
		val_touchcontrols = sd.savedOptions["touchcontrols"];
		val_easymode = sd.savedOptions["easymode"];
		val_keydelay = sd.savedOptions["keydelay"];
		val_scopo = sd.savedOptions["scopophobia"];
		UpdateColorBlindText();
		UpdateHUDPlacementText();
		UpdateEmphasizeCursorText();
		UpdateTouchControlText();
		UpdateEasyModeText();
		UpdateKeyDelayText();
		UpdateScopoText();
		optionInfos.Add(new OptionMinMaxInfo(0, 0, 0));
		optionInfos.Add(new OptionMinMaxInfo(1, 0, 2));
		optionInfos.Add(new OptionMinMaxInfo(1, 0, 2));
		optionInfos.Add(new OptionMinMaxInfo(val_scopo, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(val_keydelay, 6, 20));
		optionInfos.Add(new OptionMinMaxInfo(val_easymode, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(val_touchcontrols, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(val_emphasizecursor, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(val_hudplacement, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(val_colorblind, 0, 1));
	}
	private void UpdateKeyDelayText() { txt_keydelay.text = val_keydelay.ToString(); }
	private void UpdateEasyModeText() { txt_easymode.text = val_easymode == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	private void UpdateEmphasizeCursorText() { txt_emphasizecursor.text = val_emphasizecursor == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	private void UpdateColorBlindText() { txt_colorblind.text = val_colorblind == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	private void UpdateTouchControlText() { txt_touchcontrols.text = val_touchcontrols == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	private void UpdateHUDPlacementText() { txt_hudplacement.text = val_hudplacement == 1 ? GetXmlValue(top, "right") : GetXmlValue(top, "left"); }
	private void UpdateScopoText() { txt_scopo.text = val_scopo == 1 ? GetXmlValue(top, "on") : GetXmlValue(top, "off"); }
	public Vector2 GetColliderPosition(MouseCore clicker) {
		if(colliders.Count != 9) { return new Vector2(-1.0f, -1.0f); }
		for(int i = 0; i < 9; i++) {
			Vector3 collider = clicker.getPositionInGameObject(colliders[i]);
			if(collider.z == 0) { continue; }
			return new Vector2(collider.x, i);
		}
		return new Vector2(-1.0f, -1.0f);
	}
}