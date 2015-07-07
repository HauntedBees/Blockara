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
public class BoardCursorCore:InputCore {
	public GameObject cursor;
	public float xOffset;
	protected int x;
	protected int y;
	protected int boardwidth;
	public int boardheight;
	protected int prevx;
	protected int prevy;
	protected int id;
	protected float myDTween;
	protected Sprite[] sheet;
	protected bool moved;
	public bool loopAround;
	public void setWidthAndHeight(int w, int h) { boardwidth = w; boardheight = h; }
	public void Wipe() {
		Destroy(cursor);
		cursor = null;
		sheet = null;
	}
	public void SetPD(PersistData p) { PD = p; }
	protected void InitializeMembers() {
		id = 0;
		x = 0;
		y = 0;
		prevx = -1;
		prevy = -1;
		moved = false;
		InitGraphics();
		ExtraInit();
	}
	virtual protected void InitGraphics() {
		sheet = Resources.LoadAll<Sprite>(PD.UseHighContrastCursor()?SpritePaths.HiContastTileCursor:SpritePaths.TileCursor);
		cursor = GetGameObject(Vector3.zero, "", sheet[0], false, "HUDText");
		if(!isShown) { cursor.SetActive(false); }
	}
	virtual protected void ExtraInit() { } 
	public void SetInitXAndID(int x) { this.x = x; this.id = x; }
	virtual public void DoUpdate() {}
	protected void UpdateCursorPos(bool skipTween = false) {
		if(!isShown) { return; }
		if(prevx == x && prevy == y) { moved = false; return; }
		moved = true;
		Vector3 pos = getPos(x, y);
		if(skipTween || sheet == null) {
			cursor.transform.position = pos;
		} else {
			th.DoTween(cursor, pos);
		}
	} 
	protected void FinishUpdate() { prevx = x; prevy = y; }
	public bool HasMoved() { return moved; }
	public void SetVisibility(bool show) { cursor.SetActive(show); }
	public int getX() { return x; }
	public int getY() { return y; }
	public void setX(int nx) { x = nx; if(x >= boardwidth) { x = boardwidth - 1; } else if(x < 0) { x = 0; } }
	public void setY(int ny) { y = ny; if(y >= boardheight) { y = boardheight - 1; } else if(y < 0) { y = 0; } }
	virtual public Vector3 getPos(int x, int y) { return new Vector3((xOffset + x) * Consts.TILE_SIZE, y * Consts.TILE_SIZE - 1.85f); }
}