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
public struct FontData {
	public TextAnchor anchor;
	public TextAlignment align;
	public Color color;
	public float scale;
	public string layerName;
	public FontData(TextAnchor an, TextAlignment al, float s = 0.035f) {
		anchor = an;
		align = al;
		scale = s;
		color = Color.black;
		layerName = "HUDText";
	}
	public FontData Clone() {
		FontData n = new FontData(anchor, align, scale);
		n.color = color;
		n.layerName = layerName;
		return n;
	}
}