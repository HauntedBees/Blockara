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
	public GameObject block, wholeoverlay, glow;
	private SpriteRenderer block_sr, shape_sr;
	private GameObject charoverlay, shape;
	protected int colorVal, specialVal, flashState;
	private bool isEmpty;
	private int emptySpriteTile;
	private Sprite[] tileSheet, glowSheet;
	private Color ColorHalf;
	public bool isShield, clearBackOnOverlayWipe, isConcrete, isAnimating;
	public void SetupTile(PersistData P, Vector3 pos, Sprite[] ts, Sprite os, Sprite[] ss, int tVal, int sVal, bool show) {
		PD = P;
		ColorHalf = new Color(1.0f, 1.0f, 1.0f, 0.5f);
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
			block = GetTileObject(pos, "tile", ts[tValAct], "Tile");
			block_sr = block.GetComponent<SpriteRenderer>();
			charoverlay = GetTileObject(pos, "tile", os, "TileOverlay");
			charoverlay.transform.parent = block.transform;
			shape = GetTileObject(pos, "tile", ss[GetShapeIdx(tValAct, sVal)], "Shape");
			shape.transform.parent = block.transform;
			shape_sr = shape.GetComponent<SpriteRenderer>();
			if(tValAct == emptySpriteTile) { shape.SetActive(false); }
		}
		if(tVal > 2 || tVal < 0) { Kill(); }
	}
	public void StartFlash(int color, Vector3 pos) {
		if(!isShown) { return; }
		glow = GetGameObject(block.renderer.transform.position, "glow", glowSheet[color], false, "ShapeOverlay");
		SpriteRenderer glow_sr = glow.GetComponent<SpriteRenderer>();
		glow.renderer.transform.position = pos;
		glow_sr.color = Color.white;
		Sequence S = DOTween.Sequence();
		S.Append(glow_sr.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.9f), 0.15f));
		S.Append(glow_sr.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.35f).OnComplete(()=>EndFlash(glow)));
		flashState = 0;
	}
	private void EndFlash(GameObject glow) { Destroy(glow); }
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
	private GameObject GetTileObject(Vector3 pos, string name, Sprite sprite, string sortLayer) { return GetGameObject_Tile(pos, name, sprite, sortLayer); }
	public Vector3 GetTilePosition() { return block.transform.position; }
	public void SetTilePosition(Vector3 v) {
		block.transform.position = v;
		shape.transform.position = v;
		if(wholeoverlay != null) { wholeoverlay.transform.position = v; }
	}
	public void MakeRecoveryTile(int type) {
		clearBackOnOverlayWipe = true;
		block_sr.sprite = tileSheet[type];
		block_sr.color = ColorHalf;
		shape_sr.color = ColorHalf;
		if(wholeoverlay == null) {
			wholeoverlay = GetTileObject(block.transform.position, "plus", Resources.Load<Sprite>(SpritePaths.RecoveryTile), "ShapeOverlay");
		} else {
			wholeoverlay.SetActive(true);
			wholeoverlay.transform.position = block.transform.position;
			wholeoverlay.name = "plus";
			wholeoverlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(SpritePaths.RecoveryTile);
		}
	}
	public void MakeDestroyTile() {
		block_sr.color = Color.white;
		shape_sr.color = Color.white;
		if(wholeoverlay == null) {
			wholeoverlay = GetTileObject(block.transform.position, "destroy", Resources.Load<Sprite>(SpritePaths.DestroyTile), "ShapeOverlay");
		} else {
			wholeoverlay.SetActive(true);
			wholeoverlay.transform.position = block.transform.position;
			wholeoverlay.name = "destroy";
			wholeoverlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(SpritePaths.DestroyTile);
		}
	}
	public void MakeShieldTile(Sprite spr) {
		if(isShown) {
			block_sr.color = Color.clear;
			block.SetActive(true);
			charoverlay.SetActive(false);
			if(shape == null) {
				shape = GetTileObject(block.transform.position, "tile", spr, "Shape");
				shape.transform.parent = block.transform;
			}
			shape.SetActive(true);
			block_sr.color = Color.clear;
			shape_sr.sprite = spr;
		}
		isShield = true;
	}
	public void ChangeShieldTile(Sprite spr) { if(isShown) { shape_sr.sprite = spr; } }
	public void CleanGameObjects() {
		if(!isShown) { return; }
		PutGameObjectInBank(charoverlay);
		PutGameObjectInBank(block);
		PutGameObjectInBank(shape);
		if(wholeoverlay != null) { PutGameObjectInBank(wholeoverlay); }
		Destroy(this.gameObject);
		Destroy(this);
	}
	public void WipeOverlay() {
		if(wholeoverlay == null) { return; }
		if(clearBackOnOverlayWipe) {
			clearBackOnOverlayWipe = false;
			block_sr.color = Color.clear;
			if(shape != null) { shape_sr.color = Color.clear; }
		}
		wholeoverlay.SetActive(false);
	}
	public bool IsIrrelevantForSpriteUpdate() { return isConcrete || isAnimating || isEmpty || isShield || !isShown; }
	public void UpdateSprite(bool hasTop, bool hasBottom) {
		if(hasTop && hasBottom) {
			block_sr.sprite = tileSheet[colorVal];
		} else if(hasTop) {
			block_sr.sprite = tileSheet[colorVal + 3];
		} else if(hasBottom) {
			block_sr.sprite = tileSheet[colorVal + 9];
		} else {
			block_sr.sprite = tileSheet[colorVal + 6];
		}
	}
	public int GetColorVal() { return colorVal; }
	public int GetSpecialVal() { return specialVal; }
	public bool IsDead() { return isEmpty && !isShield; }
	public void MakeConcrete() { isConcrete = true; }
	public void Kill() {
		isEmpty = true;
		isShield = false;
		if(!isShown) { return; }
		shape_sr.color = Color.white;
		shape.SetActive(false);
		if(wholeoverlay != null) { wholeoverlay.SetActive(false); }
		block.SetActive(false);
		block_sr.sprite = tileSheet[emptySpriteTile];
	}
}