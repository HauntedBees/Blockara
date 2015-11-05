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
public class BoardMirror:BoardWarCore {
	#region "Members"
	private float maxX;
	private float maxY;
	public BoardWar parent;
	public BoardWar mirrorTop;
	#endregion
	#region "Setup"
	virtual public void Setup(TweenHandler th, bool show = true) {
		isMirror = true;
		deathTile = parent.deathTile;
		GetPersistData();
		this.th = th;
		tileSheet = GetTileSheet();
		shapeSheet = GetShapeSheet();
		overlaySprite = GetOverlaySprite();

		isShown = show;
		parent.SetMirror(this);
		player = parent.player;
		width = parent.width;
		height = parent.height;
		xOffset = mirrorTop.xOffset;
		topoffset = 1;

		maxX = (xOffset + width - 1) * Consts.TILE_SIZE;
		maxY = 1.85f;

		SetupTilesList();
	}
	override protected int GetTileColor(int pos = -1) { return parent.GetValueAtListPos(pos).GetColorVal(); }
	override protected int GetTileSpecialVal(int pos = -1) { return parent.GetValueAtListPos(pos).GetSpecialVal(); }
	#endregion
	#region "Updating"
	public void DoUpdate() {
		if(!isShown) { return; }
		List<MirrorChange> changes = parent.GetChanges();
		for(int i = 0; i < changes.Count; i++) {
			switch(changes[i].t) {
				case MirrorChange.changeType.shift: {
					MirrorChangeShift change = changes[i] as MirrorChangeShift;
					ShiftRow(change.y, change.shiftDir, true);
					} break;
				case MirrorChange.changeType.launch: {
					MirrorChangeLaunch change = changes[i] as MirrorChangeLaunch;
					LaunchTiles(change.length, change.topY, change.colNum, true);
					} break;
				case MirrorChange.changeType.wipe: {
					MirrorChangeWipeRow change = changes[i] as MirrorChangeWipeRow;
					for(int x = 0; x < width; x++) { DestroyBlock(x, change.y); }
					} break;
				case MirrorChange.changeType.shield: {
					MirrorChangeShield change = changes[i] as MirrorChangeShield;
					if(change.kill) { tiles[GetListPosFromXY(change.x, height - 1)].Kill(); }
					else { SetShieldGraphic(change.x, change.frame); }
					} break;
				default: {
					MirrorChangeRemoveTile change = changes[i] as MirrorChangeRemoveTile;
					DestroyBlock(change.x, change.y);
					} break;
			}
		}
	}
	public void SetShieldGraphic(int x, int frame) {
		Sprite[] shieldSheet = Resources.LoadAll<Sprite>(SpritePaths.Shield);
		tiles[GetListPosFromXY(x, height - 1)].MakeShieldTile(shieldSheet[frame]);
	}
	#endregion
	#region "Position Getters"
	public override Vector3 GetScreenPosFromXY(int x, int y) { return new Vector3(maxX - x * Consts.TILE_SIZE, maxY - y * Consts.TILE_SIZE); }
	#endregion
}