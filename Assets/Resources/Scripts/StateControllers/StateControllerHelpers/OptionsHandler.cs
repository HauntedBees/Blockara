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
public class OptionsHandler:MenuHandler {
	private TextMesh txt_res;
	private TextMesh txt_fls;
	private TextMesh txt_vmu;
	private TextMesh txt_vso;
	private TextMesh txt_vvo;
	private TextMesh txt_clr;
	public void Setup(Vector2 res, bool full, int vmu, int vso, int vvo, int resIdx, int resCount) {
		top = GetXMLHead();
		headerText.text = GetXmlValue(top, "options");
		float x = 1.0f;
		FontData f = PD.mostCommonFont.Clone();
		f.align = TextAlignment.Right; f.anchor = TextAnchor.MiddleRight;
		texts.Add(GetMeshText(new Vector2(x,  1.0f), GetXmlValue(top, "resolution") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.8f), GetXmlValue(top, "fullscreen") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.6f), GetXmlValue(top, "musicvol") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.4f), GetXmlValue(top, "soundvol") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.2f), GetXmlValue(top, "voicevol") + ":", f));
		texts.Add(GetMeshText(new Vector2(x,  0.0f), GetXmlValue(top, "gameplay") + ":", f));
		texts.Add(GetMeshText(new Vector2(x, -0.2f), GetXmlValue(top, "clearSave") + ":", f));

		x += 0.25f;
		f.align = TextAlignment.Left; f.anchor = TextAnchor.MiddleLeft;
		txt_res = GetMeshText(new Vector3(x,  1.0f), "",       					 f);
		txt_fls = GetMeshText(new Vector3(x,  0.8f), "",       					 f);
		txt_vmu = GetMeshText(new Vector3(x,  0.6f), "",       					 f);
		txt_vso = GetMeshText(new Vector3(x,  0.4f), "",       					 f);
		txt_vvo = GetMeshText(new Vector3(x,  0.2f), "",       					 f);
		texts.Add(GetMeshText(new Vector3(x,  0.0f), GetXmlValue(top, "off"),    f));
		txt_clr = GetMeshText(new Vector3(x, -0.2f), GetXmlValue(top, "clear"),  f);
		texts.Add(GetMeshText(new Vector3(x, -0.4f), GetXmlValue(top, "apply"),  f));
		texts.Add(GetMeshText(new Vector3(x, -0.6f), GetXmlValue(top, "cancel"), f));
		
		optionInfos.Add(new OptionMinMaxInfo(1, 0, 2));
		optionInfos.Add(new OptionMinMaxInfo(1, 0, 2));
		optionInfos.Add(new OptionMinMaxInfo(1, 0, 2));
		optionInfos.Add(new OptionMinMaxInfo(0, 0, 0));
		optionInfos.Add(new OptionMinMaxInfo(vvo, 0, 100));
		optionInfos.Add(new OptionMinMaxInfo(vso, 0, 100));
		optionInfos.Add(new OptionMinMaxInfo(vmu, 0, 100));
		optionInfos.Add(new OptionMinMaxInfo(full?1:0, 0, 1));
		optionInfos.Add(new OptionMinMaxInfo(resIdx, 0, resCount));

		texts.Add(txt_res);
		texts.Add(txt_fls);
		texts.Add(txt_vmu);
		texts.Add(txt_vso);
		texts.Add(txt_vvo);
		texts.Add(txt_clr);

		x = 1.9f;
		colliders.Add(GetCollider("res", new Vector3(x, 1.0f)));
		colliders.Add(GetCollider("fls", new Vector3(x, 0.8f)));
		colliders.Add(GetCollider("vmu", new Vector3(x, 0.6f)));
		colliders.Add(GetCollider("vso", new Vector3(x, 0.4f)));
		colliders.Add(GetCollider("vvo", new Vector3(x, 0.2f)));
		colliders.Add(GetCollider("gmm", new Vector3(x, 0.0f)));
		colliders.Add(GetCollider("clr", new Vector3(x, -0.2f)));
		colliders.Add(GetCollider("apl", new Vector3(x, -0.4f)));
		colliders.Add(GetCollider("cnx", new Vector3(x, -0.6f)));
		
		SetResText(res);
		SetFlsText(full);
		SetVMuText(vmu);
		SetVSoText(vso);
		SetVVoText(vvo);
	}
	public void SetClrText(string s) { txt_clr.text = GetXmlValue(top, s); }
	public void SetResText(Vector2 res) { txt_res.text = res.x + "x" + res.y; }
	public void SetFlsText(bool isFullScreen) { txt_fls.text = GetXmlValue(top, (isFullScreen?"yes":"no")); }
	public void SetVMuText(int v) { txt_vmu.text = v + "%"; }
	public void SetVSoText(int v) { txt_vso.text = v + "%"; }
	public void SetVVoText(int v) { txt_vvo.text = v + "%"; }
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