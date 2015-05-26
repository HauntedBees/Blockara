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
using DG.Tweening;
using UnityEngine;
public class Tile:ObjCore {
	public GameObject block, shape, overlay, glow;
	protected int colorVal, specialVal, flashState;
	private bool isEmpty;
	private int emptySpriteTile;
	private Sprite[] tileSheet, glowSheet;
	public bool isShield, clearBackOnOverlayWipe, isConcrete;
	public void SetupTile(PersistData P, Vector3 pos, Sprite[] ts, Sprite[] ss, int tVal, int sVal, bool show) {
		PD = P;
		tileSheet = ts;
		glowSheet = Resources.LoadAll<Sprite>(SpritePaths.Glows);
		emptySpriteTile = 3;
		isShown = show;
		isEmpty = false;
		colorVal = tVal;
		specialVal = sVal;
		clearBackOnOverlayWipe = false;
		isShield = false;
		flashState = -1;
		if(isShown) {
			int tValAct = (tVal < 0?emptySpriteTile:tVal);
			if(block == null) {
				block = GetTileObject(pos, "tile", ts[tValAct]);
			} else {
				block.SetActive(true);
				block.transform.position = pos;
				block.GetComponent<SpriteRenderer>().sprite = ts[tValAct];
			}
			if(shape == null) {
				shape = GetTileObject(pos, "tile", ss[GetShapeIdx(tValAct, sVal)], "Shape");
			} else {
				shape.SetActive(true);
				shape.transform.position = pos;
				shape.GetComponent<SpriteRenderer>().sprite = ss[GetShapeIdx(tValAct, sVal)];
			}
			Color c = block.renderer.material.color;
			c.a = 1.0f;
			block.renderer.material.color = c;
			shape.renderer.material.color = c;
			if(tValAct == emptySpriteTile) { shape.SetActive(false); }
		}
		if(tVal > 2 || tVal < 0) { Kill(); }
	}
	public void StartFlash(int color, Vector3 pos) {
		if(!isShown) { return; }
		if(glow == null) { 
			glow = GetTileObject(block.renderer.transform.position, "glow", glowSheet[color], "ShapeOverlay");
		} else {
			glow.SetActive(true);
			glow.GetComponent<SpriteRenderer>().sprite = glowSheet[color];
		}
		glow.renderer.transform.position = pos;
		glow.renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		Sequence S = DOTween.Sequence();
		S.Append(glow.renderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.9f), 0.15f));
		S.Append(glow.renderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.35f).OnComplete(()=>EndFlash(glow)));
		flashState = 0;
	}
	private void EndFlash(GameObject glow) { glow.SetActive(false); }
	private int GetShapeIdx(int type, int special) {
		int sP = type;
		if(special <= 0) {
			if(sP > 2) {
				sP = 33;
			} else if(player == 2) {
				if(PD.p2Char == PersistData.C.White) { sP += 3; }
				else if(PD.p2Char == PersistData.C.September) { sP += 6; }
			}
		} else {
			sP += 3 * special;
		}
		return sP;
	}
	private GameObject GetTileObject(Vector3 pos, string name, Sprite sprite, string sortLayer = "") { return GetGameObject(pos, name, sprite, false, sortLayer); }
	public Vector3 GetTilePosition() { return block.transform.position; }
	public void SetTilePosition(Vector3 v) {
		block.transform.position = v;
		shape.transform.position = v;
		if(overlay != null) { overlay.transform.position = v; }
	}
	public void MakeRecoveryTile(int type) {
		Color c = block.renderer.material.color;
		c.a = 0.5f;
		clearBackOnOverlayWipe = true;
		block.GetComponent<SpriteRenderer>().sprite = tileSheet[type];
		block.renderer.material.color = c;
		shape.renderer.material.color = c;
		if(overlay == null) {
			overlay = GetTileObject(block.transform.position, "plus", Resources.Load<Sprite>(SpritePaths.RecoveryTile), "ShapeOverlay");
		} else {
			overlay.SetActive(true);
			overlay.transform.position = block.transform.position;
			overlay.name = "plus";
			overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(SpritePaths.RecoveryTile);
		}
	}
	public void MakeDestroyTile() {
		Color c = block.renderer.material.color;
		c.a = 1.0f;
		block.renderer.material.color = c;
		shape.renderer.material.color = c;
		if(overlay == null) {
			overlay = GetTileObject(block.transform.position, "destroy", Resources.Load<Sprite>(SpritePaths.DestroyTile), "ShapeOverlay");
		} else {
			overlay.SetActive(true);
			overlay.transform.position = block.transform.position;
			overlay.name = "destroy";
			overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(SpritePaths.DestroyTile);
		}
	}
	public void MakeShieldTile(Sprite spr) {
		if(isShown) {
			Color c = block.renderer.material.color;
			c.a = 0.0f;
			block.renderer.material.color = c;
			shape.SetActive(true);
			c.a = 1.0f;
			shape.renderer.material.color = c;
			shape.GetComponent<SpriteRenderer>().sprite = spr;
		}
		isShield = true;
	}
	public void ChangeShieldTile(Sprite spr) { if(isShown) { shape.GetComponent<SpriteRenderer>().sprite = spr; } }
	public void CleanGameObjects(bool forceErase = true) {
		if(!isShown) { return; }
		if(forceErase) {
			Destroy(block);
			Destroy(shape);
			if(overlay != null) { Destroy(overlay); }
			Destroy(this.gameObject);
		} else {
			block.SetActive(false);
			shape.SetActive(false);
			if(overlay != null) { overlay.SetActive(false); }
			PD.TileBank.Add(this);
			this.gameObject.SetActive(false);
		}
	}
	public void WipeOverlay(bool forceErase = false) {
		if(overlay == null) { return; }
		if(clearBackOnOverlayWipe) {
			clearBackOnOverlayWipe = false;
			Color c = block.renderer.material.color;
			c.a = 0.0f;
			block.renderer.material.color = c;
			shape.renderer.material.color = c;
		}
		if(forceErase) {
			Destroy(overlay);
		} else {
			overlay.SetActive(false);
		}
	}
	public int GetColorVal() { return colorVal; }
	public int GetSpecialVal() { return specialVal; }
	public bool IsDead() { return isEmpty; }
	public void MakeConcrete() { isConcrete = true; }
	public void Kill(bool forceErase = false) {
		isEmpty = true;
		isShield = false;
		if(!isShown) { return; }
		if(forceErase) {
			Destroy(shape);
			if(overlay != null) { Destroy(overlay); }
		} else {
			Color c = block.renderer.material.color;
			c.a = 1.0f;
			shape.renderer.material.color = c;
			shape.SetActive(false);
			if(overlay != null) { overlay.SetActive(false); }
		}
		block.SetActive(false);
		block.GetComponent<SpriteRenderer>().sprite = tileSheet[emptySpriteTile];
	}
}