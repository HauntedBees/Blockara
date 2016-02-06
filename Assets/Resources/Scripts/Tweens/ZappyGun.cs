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
public class ZappyGun:MonoBehaviour {
	private const float ZAPSPEED = 0.1f, ZAP_SIZE = 0.2f;
	private List<GameObject> zappers;
	private List<EENdeets> EENs;
	private Sprite EEN;
	private float maxY;
	private bool up;
	private int tileType;
	private PersistData PD;
	public bool dead;
	public void Init(PersistData p, int type, int length, Vector3 topPos, float topDest, bool mirror = false) {
		PD = p;
		dead = false; tileType = type;
		maxY = topDest + (mirror ? ((length - 1) * Consts.TILE_SIZE + 0.6f) : Consts.TILE_SIZE);
		up = topDest > topPos.y;
		Sprite[] sheet = Resources.LoadAll<Sprite> (SpritePaths.Zapper);
		zappers = new List<GameObject>();
		Vector3 newPos = topPos;
		GameObject top = GetGameObject(newPos);
		top.renderer.sortingLayerName = "Zapper";
		top.GetComponent<SpriteRenderer>().sprite = sheet[type];
		top.transform.parent = gameObject.transform;
		zappers.Add(top);
		for(int i = 2; i < length; i++) {
			newPos.y -= ZAP_SIZE;
			GameObject middlin = GetGameObject(newPos);
			middlin.GetComponent<SpriteRenderer>().sprite = sheet[type + 3];
			middlin.renderer.sortingLayerName = "Zapper";
			middlin.transform.parent = gameObject.transform;
			zappers.Add(middlin);
		}
		newPos.y -= ZAP_SIZE;
		GameObject bottom = GetGameObject(newPos);
		bottom.GetComponent<SpriteRenderer>().sprite = sheet[type + 6];
		bottom.transform.parent = gameObject.transform;
		bottom.renderer.sortingLayerName = "Zapper";
		zappers.Add(bottom);
		EENs = new List<EENdeets>();
		EEN = Resources.LoadAll<Sprite>(SpritePaths.Launch_Particles)[type];
	}
	private struct EENdeets {
		public int xDir, yDir, rotDir;
		public GameObject g;
		public EENdeets(GameObject g, int x, int y, int r) { this.g = g; xDir = x; yDir = y; rotDir = r; }
	}

	public void Update() {
		UpdateEENs();
		if(dead || zappers.Count == 0) { return; }
		float py = zappers[0].transform.position.y;
		if(Random.value > 0.6f) {
			float dx = Random.Range(-0.1f, 0.1f);
			GameObject o = GetGameObject(new Vector3(zappers[0].transform.position.x + dx, zappers[0].transform.position.y));
			o.renderer.sortingLayerName = "HUD";
			o.GetComponent<SpriteRenderer>().sprite = EEN;
			int x = dx<0?-1:1;
			EENs.Add(new EENdeets(o, x, 0, x));
		}
		if((up && py > maxY) || (!up && py < maxY)) {
			DeleteZaps();
		} else {
			MoveZaps();
		}
	}
	private GameObject GetGameObject(Vector3 pos) {
		GameObject g = PD.GetBankObject();
		if(g == null) {
			g = Instantiate(PD.universalPrefab, pos, Quaternion.identity) as GameObject;
		} else {
			g.transform.position = pos;
		}
		return g;
	}
	private void UpdateEENs() {
		if(EENs.Count == 0) { return; }
		bool anyAlive = false;
		for(int i = 0; i < EENs.Count; i++) {
			EENdeets e = EENs[i];
			Vector3 pos = e.g.transform.position;
			switch(tileType) {
				case 0: pos.y -= 0.005f; break;
				case 1: pos.y += 0.005f; pos.x += Random.Range(-0.01f, 0.01f); break;
				case 2: pos.x += 0.0005f * e.xDir; pos.y += 0.0005f * (up?-1:1); e.g.transform.Rotate(0.0f, 0.0f, 1.0f * e.rotDir); break;
				}
			Color o = e.g.GetComponent<SpriteRenderer>().color;
			o.a *= 0.9f;
			if(o.a > 0.05f) { anyAlive = true; }
			e.g.GetComponent<SpriteRenderer>().color = o;
			e.g.transform.position = pos;
		}
		if(!anyAlive) {
			for(int i = 0; i < EENs.Count; i++) { CleanUpObject(EENs[i].g); }
			EENs.Clear();
			dead = true;
		}
	}
	private void CleanUpObject(GameObject g) { PD.AddToBank(g); }
	private void DeleteZaps() { for(int i = 0; i < zappers.Count; i++) { CleanUpObject(zappers[i]); } zappers.Clear(); }
	private void MoveZaps() {
		for(int i = 0; i < zappers.Count; i++) {
			GameObject zap = zappers[i];
			Vector3 pos = zap.transform.position;
			pos.y += (up ? 1.0f : -1.0f) * ZAPSPEED;
			zap.transform.position = pos;
		}
	}
}