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
public class MenuHandler:All {
	protected TextMesh headerText;
	protected List<TextMesh> texts;
	protected List<GameObject> colliders;
	protected System.Xml.XmlNode top;
	protected List<OptionMinMaxInfo> optionInfos;
	protected struct OptionMinMaxInfo { public int cval, min, max; public OptionMinMaxInfo(int cw, int mi, int ma) { cval = cw; min = mi; max = ma; } }
	public void InitializeMembers(TextMesh h, PersistData p) {
		headerText = h;
		PD = p;
		texts = new List<TextMesh>();
		colliders = new List<GameObject>();
		optionInfos = new List<OptionMinMaxInfo>();
	}
	public virtual void CleanUp() {
		if(texts == null) { return; }
		for(int i = 0; i < texts.Count; i++) { Destroy(texts[i].gameObject); }
		for(int i = 0; i < colliders.Count; i++) { Destroy(colliders[i]); }
		texts.Clear();
		colliders.Clear();
		optionInfos.Clear();
	}
	public void UpdateCursorPosition(int cy, int cx) { optionInfos[cy] = new OptionMinMaxInfo(cx, optionInfos[cy].min, optionInfos[cy].max); }
	public void UpdateCursorGraphic(OptionsSelector c, int cy) { c.HideAnArrowIfAtCorner(optionInfos[cy].cval, optionInfos[cy].min, optionInfos[cy].max); }
}