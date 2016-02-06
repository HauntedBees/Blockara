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
public class All:MonoBehaviour {
	protected PersistData PD;
	protected void GetPersistData() {
		GameObject Persist = GameObject.Find("PersistData") as GameObject;
		PD = Persist.GetComponent<PersistData>();
	}
	#region "GameObject Getters"
	protected TextMesh GetMeshText(Vector3 pos, string text, FontData f, string prefabPath = "default") {
		float fscale = f.scale;
		if(prefabPath == "default") {
			if(fscale > 0.055f) {
				fscale /= 2;
				prefabPath = "Prefabs/Text/Size96";
			} else {
				prefabPath = "Prefabs/Text/Size48";
			}
		}
		GameObject meshText = Instantiate(Resources.Load<GameObject>(prefabPath), pos, Quaternion.identity) as GameObject;
		meshText.GetComponent<TextMesh>().text = text;
		meshText.name = "t_" + text;
		meshText.transform.localScale = new Vector3(fscale, fscale);
		meshText.GetComponent<MeshRenderer>().sortingLayerName = f.layerName;
		meshText.GetComponent<TextMesh>().alignment = f.align;
		meshText.GetComponent<TextMesh>().anchor = f.anchor;
		meshText.GetComponent<TextMesh>().color = f.color;
		return meshText.GetComponent<TextMesh>();
	}
	protected GameObject GetGameObject(Vector3 pos, string name, Sprite sprite, bool collider = false, string sortLayer = "") {
		GameObject g = Instantiate(collider?PD.universalPrefabCollider:PD.universalPrefab, pos, Quaternion.identity) as GameObject;
		g.name = name + Random.Range (0, 10000);
		if(sprite != null) { g.GetComponent<SpriteRenderer>().sprite = sprite; }
		if(sortLayer != "") { g.renderer.sortingLayerName = sortLayer; }
		if(collider) { WrapColliderToGameObject(g); }
		return g;
	}
	protected GameObject GetGameObject_Tile(Vector3 pos, string name, Sprite sprite, string sortLayer) {
		GameObject g = PD.GetBankObject();
		if(g == null) { return GetGameObject(pos, name, sprite, collider, sortLayer); }
		SpriteRenderer g_sr = g.GetComponent<SpriteRenderer>();
		g.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		g_sr.color = Color.white;
		g.transform.position = pos;
		g.name = name + Random.Range (0, 10000);
		if(sprite != null) { g_sr.sprite = sprite; }
		g.renderer.sortingLayerName = sortLayer;
		return g;
	}
	protected void PutGameObjectInBank(GameObject g) {
		if(g==null){ return; }
		g.SetActive(false);
		g.transform.parent = null;
		PD.AddToBank(g);
	}

	protected GameObject GetCollider(string name, Vector3 pos, float scaleX = 1.0f, float scaleY = 1.0f) {
		GameObject col = GetGameObject(pos, name, Resources.Load<Sprite>(SpritePaths.OptionsCollider), true, "HUD");
		col.transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
		return col;
	}
	protected void WrapColliderToGameObject(GameObject g) {
		PolygonCollider2D c = (PolygonCollider2D) g.collider2D;
		Vector2[] v = new Vector2[4];
		Vector2 min = (Vector2) g.renderer.bounds.min;
		Vector2 max = (Vector2) g.renderer.bounds.max;
		Vector2 cornerLR = min;
		Vector2 cornerUL = min;
		cornerLR.x = max.x;
		cornerUL.y = max.y;
		Vector2 offset = (Vector2) g.transform.localPosition;
		v[0] = min - offset;
		v[1] = cornerLR - offset;
		v[2] = max - offset;
		v[3] = cornerUL - offset;
		c.SetPath(0, v);
	}
	#endregion
	#region "XML"
	protected XmlNode GetXMLHead(string path = "/nav", string topNode = "words", bool useCulture = true) {
		XmlDocument doc = new XmlDocument();
		TextAsset ta = Resources.Load<TextAsset>("XML" + (useCulture?("/"+PD.culture):"") + path);
		doc.LoadXml(ta.text);
		return doc.SelectSingleNode(topNode);
	}
	protected string GetXmlValue(XmlNode top, string id) {
		XmlNode elem = top.SelectSingleNode(id);
		return (elem == null ? "ERROR LOL" : elem.InnerText);
	}
	protected float GetXmlFloat(XmlNode top, string id) {
		XmlNode elem = top.SelectSingleNode(id);
		return (elem == null ? 0.0f : float.Parse(elem.InnerText));
	}
	#endregion
}