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
public class GameTouchHandler:All {
	private GameObject rowCollider;
	private float halfHeight, lastYPos;
	private bool isDown, lockY;
	private int height, moveDelay;
	public int shift, rowX, rowY;
	public bool launching, aboveEverything, launchLimiter;
	public void Initialize(int h, float xOffset) {
		GetPersistData();
		rowCollider = GetGameObject(new Vector3((xOffset + 3.5f) * Consts.TILE_SIZE, (Consts.YBOTTOM - 2.0f) * Consts.TILE_SIZE), "Tile Collider", Resources.Load<Sprite>(SpritePaths.TileCover), true, "Cover HUD");
		halfHeight = rowCollider.renderer.bounds.size.y / 2.0f; height = h - 1; lockY = false; moveDelay = 0;
	}
	public int HandleUpdate(MouseCore clicker) {
		if(--moveDelay > 0) { return 0; }
		launching = false;
		int prevX = rowX;
		float newYPos = SetSelectedRowAndReturnY(clicker);
		if(!clicker.isHeld()) { lockY = false; launchLimiter = false; lastYPos = newYPos; return 0; }
		int dx = rowX - prevX;
		if(dx == 0 && !launchLimiter) {
			launching = (newYPos - lastYPos) > 1.25f;
			launchLimiter = launching;
			return 0;
		}
		lastYPos = newYPos;
		if(dx != 0) { moveDelay = PD.KEY_DELAY; }
		return dx;
	}
	private float SetSelectedRowAndReturnY(MouseCore clicker) {
		Vector3 pos = clicker.getPositionInGameObject(rowCollider);
		if(pos.z == 0) { return 0.0f; }
		if(!clicker.isHeld() || !lockY) {
			rowY = Mathf.FloorToInt((pos.y + halfHeight) / Consts.TILE_SIZE);
			lockY = clicker.isHeld();
		}
		aboveEverything = rowY > height;
		rowX = Mathf.FloorToInt((pos.x + 0.85f) / Consts.TILE_SIZE);
		return ((pos.y + halfHeight) / Consts.TILE_SIZE);
	}
}