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
public class BoardWarCore:ObjCore {
	public int width, height;
	public float xOffset;
	protected List<Tile> tiles;
	public Sprite[] tileSheet, shapeSheet;
	public Sprite overlaySprite;
	public int deathTile, chain, topoffset;
	protected bool ignoreDamageSound, isMirror;
	protected void SetupTilesList() {
		tiles = new List<Tile>();
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int pos = GetListPosFromXY(x, y);
				tiles.Add(CreateTile(x, y, GetTileColor(pos), GetTileSpecialVal(pos)));
			}
		}
	}
	virtual protected Tile CreateTile(int x, int y, int tval = -1, int sval = -1) {
		int sreal = (sval < 0 ? GetSpecial() : sval);
		GameObject g = new GameObject("tileHolder");
		Tile t = g.AddComponent<Tile>();
		t.SetupTile(PD, GetScreenPosFromXY(x, y), tileSheet, overlaySprite, shapeSheet, tval, sreal, isShown);
		return t;
	}
	virtual public int GetSpecial() { return 0; }
	virtual protected int GetTileColor(int pos = -1) { return 0; }
	virtual protected int GetTileSpecialVal(int pos = -1) { return GetSpecial(); }
	protected void ReplaceTileInListAndTween(int idx, Tile tile, int cropDir, Vector3 initPos, bool mirror = false, bool endPiece = false) {
		tiles[idx] = tile;
		if(isShown) {
			Vector3 xy = GetXYFromListPos(idx);
			if(initPos != Vector3.zero) { tile.SetTilePosition(initPos); }
			Vector3 destination = GetScreenPosFromXY(Mathf.FloorToInt(xy.x), Mathf.FloorToInt(xy.y));
			if(endPiece) {
				th.DoTileTween(tile, destination, false, true, true, cropDir, mirror);
			} else {
				th.DoTileTween(tile, destination);
			}
		}
	}
	virtual protected void DestroyBlock(int x, int y) {
		int pos = GetListPosFromXY(x, y);
		if(tiles[pos].IsDead()) { return; }
		int color = tiles[pos].GetColorVal();
		tiles[pos].Kill();
		int damageColor = color == 0 ? 2 : color - 1;
		for(int yn = 0; yn <= y; yn++) { tiles[GetListPosFromXY(x, yn)].StartFlash(damageColor, GetScreenPosFromXY(x, yn)); }
	}
	virtual protected void ShiftRow(int rowNum, int dir, bool mirror = false) {
		int rowStart = GetListPosFromXY(0, rowNum);
		int rowEnd = rowStart + width;
		List<Tile> dx = new List<Tile>();
		for(int x = rowStart; x < rowEnd; x++) { dx.Add(tiles[x]); }
		int length = dx.Count - 1;
		if(dir < 0) {
			for(int i = length - 1; i >= 0; i--) { ReplaceTileInListAndTween(rowStart + i, dx[i + 1], -1, Vector3.zero, mirror); }
			if(isShown) { dx[0].SetTilePosition(Vector3.zero); }
			ReplaceTileInListAndTween(rowEnd - 1, dx[0], -1, GetScreenPosFromXY(width, rowNum), mirror, true);
			if(isShown) {
				Tile r = CreateTile(0, rowNum, dx[0].GetColorVal(), dx[0].GetSpecialVal());
				if(dx[0].IsDead()) { r.Kill(); }
				th.DoTileTween(r, GetScreenPosFromXY(-1, rowNum), true, true, false, 1, mirror);
			}
		} else {
			for(int i = 1; i <= length; i++) { ReplaceTileInListAndTween(rowStart + i, dx[i - 1], 1, Vector3.zero, mirror); }
			ReplaceTileInListAndTween(rowStart, dx[length], 1, GetScreenPosFromXY(-1, rowNum), mirror, true);
			if(isShown) {
				Tile r = CreateTile(width - 1, rowNum, dx[length].GetColorVal(), dx[length].GetSpecialVal());
				if(dx[length].IsDead()) { r.Kill(); }
				th.DoTileTween(r, GetScreenPosFromXY(width, rowNum), true, true, false, -1, mirror);
			}
		}
	}
	protected void LaunchTiles(int length, int topy, int x, bool mirror = false) {
		if(isShown) {
			PD.sounds.SetSoundAndPlay(SoundPaths.S_Launch);
			for(int y = 0; y < length; y++) { tiles[GetListPosFromXY(x, topy - y)].CleanGameObjects(); }
		}
		for(int y = topy - length; y >= 0; y--) {
			int pos = GetListPosFromXY(x, y);
			ReplaceTileInListAndTween(pos + (length * width), tiles[pos], 0, Vector3.zero);
		}
		for(int y = length - 1; y >= 0; y--) { 
			int pos = GetListPosFromXY(x, y);
			Tile tile = CreateTile(x, y, GetTileColor(pos), GetTileSpecialVal(pos));
			tiles[pos] = tile;
			if(isShown) {
				th.DoTileTween(tile, tile.GetTilePosition());
				tile.SetTilePosition(GetScreenPosFromXY(x, 1 - length + y));
			}
		}
	}
	public int GetHighestYAtX(int x, int yOffset = 100) {
		int starty = (yOffset >= 100) ? (height - topoffset) : yOffset;
		for(int y = starty; y >= 0; y--) {if(!tiles[GetListPosFromXY(x, y)].IsDead()) { return y; } }
		return -1;
	}
	protected Vector3 GetXYFromListPos(int v) {
		int y = Mathf.FloorToInt (v / width), x = v - y * width;
		return new Vector3 (x, y);
	}
	public int GetListPosFromXY(int x, int y) { return (y * width) + x; }
	public Tile GetValueAtXY(int x, int y) { return GetValueAtListPos(GetListPosFromXY(x, y)); }
	public Tile GetValueAtListPos(int pos) { return tiles[pos]; }
	virtual public Vector3 GetScreenPosFromXY(int x, int y) { return Vector3.zero; }
	
	protected Sprite[] GetShapeSheet() { return Resources.LoadAll<Sprite>(SpritePaths.TileShape + (PD.IsColorBlind()?SpritePaths.ColorblindSuffix:"")); }
	protected Sprite GetOverlaySprite() { return Resources.LoadAll<Sprite>(SpritePaths.TileOverlay)[PD.GetPlayerSpriteStartIdx(player==1?PD.p1Char:PD.p2Char)]; }
	protected Sprite[] GetTileSheet() { return Resources.LoadAll<Sprite>(SpritePaths.TileBlock + (PD.IsColorBlind()?SpritePaths.ColorblindSuffix:"")); }
	public void RefreshGraphics() {
		if(!isShown) { return; }
		for(int y = height - 1; y >= 0; y--) {
			for(int x = 0; x < width; x++) {
				int idx = GetListPosFromXY(x, y);
				Tile t = tiles[idx];
				if(t.IsIrrelevantForSpriteUpdate()) { continue; }
				int previdx = idx - width, nextidx = idx + width;
				bool hasTop = true, hasBottom = true;
				if(previdx >= 0 && !tiles[previdx].IsIrrelevantForSpriteUpdate() && tiles[previdx].GetColorVal() == t.GetColorVal()) { hasBottom = false; }
				if(nextidx < tiles.Count && !tiles[nextidx].IsIrrelevantForSpriteUpdate() && tiles[nextidx].GetColorVal() == t.GetColorVal()) { hasTop = false; }
				t.UpdateSprite(isMirror?hasBottom:hasTop, isMirror?hasTop:hasBottom);
			}
		}
	}
}