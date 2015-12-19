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
public class BoardWarSpecial:BoardWar {
	private const int eyeTimerLength = 480, eyeFrames = 8, eyeFrameLength = 2;
	private List<Shield> shields;
	private List<GameObject> eyeballs, lockedRows;
	private List<int> eyeFramePositions;
	private Sprite[] shieldSheet, eyeSheet;
	public int gold, eyeballTimer;
	public bool justGotAShield, frozen;
	public override void Setup(BoardCursorActualCore c, TweenHandler t, BlockHandler b, Vector2 nexterPos, bool smallNext, bool show = true, bool touch = false) {
		deathTile = 7; // might be useless
		base.Setup(c, t, b, nexterPos, smallNext, show, touch);
		topoffset = 2;
		cursor.setWidthAndHeight(width, ++height);
		for (int x = 0; x < width; x++) { tiles.Add(CreateTile(x, height - 1, 4)); }
		shields = new List<Shield>();
		shieldSheet = Resources.LoadAll<Sprite>(SpritePaths.Shield);
		eyeSheet = Resources.LoadAll<Sprite>(PD.IsScopophobic()?SpritePaths.EyeSheetScopo:SpritePaths.EyeSheet);
		gold = 0; eyeballTimer = 0;
		justGotAShield = false;
		frozen = false;
	}
	override public void DoUpdate() {
		if(frozen && actionDelay <= 0) { frozen = false; CleanUpFreezs(); }
		if(eyeballTimer > 0 && eyeballs != null) {
			eyeballTimer--;
			if(eyeballTimer == 0) {
				foreach(GameObject eyeball in eyeballs) { Destroy(eyeball); }
				eyeballs.Clear(); eyeballs = null;
				eyeFramePositions.Clear(); eyeFramePositions = null;
			} else if(eyeballTimer >= (eyeTimerLength - (eyeFrames - 1) * eyeFrameLength) && eyeballTimer % eyeFrameLength == 0) {
				for(int i = 0; i < eyeballs.Count; i++) {
					eyeballs[i].GetComponent<SpriteRenderer>().sprite = eyeSheet[--eyeFramePositions[i]];
				}
			} else if(eyeballTimer <= ((eyeFrames - 1) * eyeFrameLength) && eyeballTimer % eyeFrameLength == 0) {
				if(eyeballTimer == ((eyeFrames - 1) * eyeFrameLength)) { PD.sounds.SetSoundAndPlay(SoundPaths.S_EyeClose); }
				for(int i = 0; i < eyeballs.Count; i++) {
					eyeballs[i].GetComponent<SpriteRenderer>().sprite = eyeSheet[++eyeFramePositions[i]];
				}
			}
		}
		base.DoUpdate();
	}
	override protected void ShiftRow(int rowNum, int dir, bool mirror = false) {
		if(rowNum == (height - 1)) { ShiftShields(dir); }
		base.ShiftRow(rowNum, dir, mirror);
	}
	public void CreateTileAtLocation(int x, int y) {
		int tval = Random.Range(0, 3);
		int pos = GetListPosFromXY(x, y);
		tiles[pos] = CreateTile(x, y, tval);
		UpdateHighestRowWithTiles();
	}
	override public int TakeDamage(int x, int length, int type) {
		int initHeight = GetHighestYAtX(x);
		if(GetValueAtXY(x, height - 1).isShield) {
			int damage = (length > 3)?6:((length == 3)?3:2);
			int pos = GetListPosFromXY(x, height - 1);
			bool isShieldDestroyed = SetHPAndReturnIfDestroyed(pos, -damage);
			if(isShieldDestroyed) {
				UpdateHighestRowWithTiles();
				tiles[GetListPosFromXY(x, height - 1)].Kill();
				DestroyShieldAtPos(pos);
			}
			return initHeight;
		}
		return base.TakeDamage(x, length, type);
	}
	
	private void ShiftShields(int dir) {
		for(int s = 0; s < shields.Count; s++) {
			Vector3 coords = GetXYFromListPos(shields[s].pos);
			coords.x += dir;
			if(coords.x < 0) { coords.x = width - 1; }
			else if(coords.x >= width) { coords.x = 0; }
			shields[s].pos = GetListPosFromXY(Mathf.FloorToInt(coords.x), Mathf.FloorToInt(coords.y));
		}
	}
	public void AddShield() {
		int shieldPos = -1;
		int minHealth = 6;
		for(int pos = GetListPosFromXY(0, height -1); pos < GetListPosFromXY(width, height -1); pos++) {
			if(!tiles[pos].isShield) { shieldPos = pos; break; }
			Shield s = GetShieldAtPos(pos);
			if(s != null && s.health < minHealth) {
				minHealth = s.health;
				shieldPos = pos;
			}
		}
		if(shieldPos < 0) { return; }
		if(minHealth < 6) {
			SetHPAndReturnIfDestroyed(shieldPos, 6);
		} else {
			tiles[shieldPos].MakeShieldTile(shieldSheet[0]);
			shields.Add(new Shield(shieldPos, tiles[shieldPos], shieldSheet));
			changes.Add(new MirrorChangeShield((int)GetXYFromListPos(shieldPos).x, 0));
			UpdateHighestRowWithTiles();
		}
	}
	public int GetShieldHP(int pos) {
		Shield s = GetShieldAtPos(pos);
		if(s == null) { return 0; }
		return s.health;
	}
	public bool SetHPAndReturnIfDestroyed(int pos, int hp) {
		Shield s = GetShieldAtPos(pos);
		if(s == null) { return false; }
		PD.sounds.SetSoundAndPlay(SoundPaths.S_ShieldHit);
		bool returnVal = s.SetHealthAndReturnIfDestroyed(hp);
		changes.Add(new MirrorChangeShield((int)GetXYFromListPos(pos).x, s.curframe));
		return returnVal;
	}
	private Shield GetShieldAtPos(int pos) {
		for(int s = 0; s < shields.Count; s++) {
			if(shields[s].pos == pos) { return shields[s]; }
		}
		return null;
	}
	private void DestroyShieldAtPos(int pos) {
		int idx = -1;
		for(int s = 0; s < shields.Count; s++) {
			if(shields[s].pos == pos) {
				Destroy(shields[s].shieldTile);
				tiles[pos].Kill();
				changes.Add(new MirrorChangeShield((int)GetXYFromListPos(pos).x, 0, true));
				idx = s;
				break;
			}
		}
		if(idx >= 0) { shields.RemoveAt(idx); }
		UpdateHighestRowWithTiles();
		return;
	}
	override protected Tile CreateTile(int x, int y, int tval = -1, int sval = -1) {
		Tile t = base.CreateTile(x, y, tval == 4 ? -1 : tval, sval);
		if(tval == 4) { t.Kill(); }
		return t;
	}
	override protected bool DoesLaunchHit(int launch, int x, int y) {
		Tile vtile = tiles[GetListPosFromXY(x, y)];
		int victim = vtile.GetColorVal();
		if(vtile.isConcrete) { return false; }
		if(victim >= 10) { victim = Mathf.FloorToInt(victim / 10) - 1; }
		return (victim == deathTile) || (launch == (victim - 1)) || (launch == 2 && victim == 0);
	}
	override protected void DestroyBlock(int x, int y) {
		int pos = GetListPosFromXY(x, y);
		HandleSpecialTile(tiles[pos], y);
		base.DestroyBlock(x, y);
	}
	private void HandleSpecialTile(Tile t, int y) {
		if(t.IsDead()) { return; }
		int val = t.GetSpecialVal() - 3;
		if(val < 0) { gold += Mathf.FloorToInt(200 * (height - y) * (PD.difficulty / 5.0f)); return; }
		ignoreDamageSound = true;
		switch(val) {
			case 0:
				gold += Mathf.FloorToInt(1500.0f * (height - y) * (PD.difficulty / 4.0f));
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Money);
				break;
			case 1:
				int delay = (PD.gameType == PersistData.GT.Campaign)?480:180;
				actionDelay = delay;
				cursor.FreezyPop(delay);
				FreezeGraphs();
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Freeze);
				break;
			case 2:
				for(int x = 0; x < width; x++) { base.DestroyBlock(x, y); }
				changes.Add(new MirrorChangeWipeRow(y));
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Explode);
				break;
			case 3:
				justGotAShield = true;
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Shield);
				break;
			case 4:
				EyeballsOnThePrizeBalls();
				PD.sounds.SetSoundAndPlay(SoundPaths.S_EyeOpen);
				break;
		}
	}
	private void FreezeGraphs() {
		frozen = true;
		if(!isShown) { return; }
		Sprite rowLockSprite = Resources.Load<Sprite>(SpritePaths.LockedRow);
		CleanUpFreezs();
		lockedRows = new List<GameObject>();
		for(int y = 0; y < PD.rowCount; y++) {
			Vector3 pos = GetScreenPosFromXY(3, y);
			pos.x += Consts.TILE_SIZE / 2.0f;
			lockedRows.Add(GetGameObject(pos, "lock" + y, rowLockSprite, false, "Zapper"));
		}
	}
	private void CleanUpFreezs() {
		if(lockedRows == null || lockedRows.Count == 0) { return; }
		for(int y = 0; y < lockedRows.Count; y++) { Destroy(lockedRows[y]); }
		lockedRows.Clear();
	}
	private void EyeballsOnThePrizeBalls() {
		if(eyeballTimer > 0) { return; }
		eyeballTimer = eyeTimerLength;
		if(cursor is BoardCursorBot) { ((BoardCursorBot) cursor).Blind(eyeTimerLength); }
		if(!isShown) { return; }
		eyeballs = new List<GameObject>();
		eyeFramePositions = new List<int>();
		int eyecount = eyeSheet.Length / eyeFrames - 1;
		for(int i = 0; i < 6; i++) { 
			int iFrameGETITBECAUSEEYES = Random.Range(0, eyecount) * eyeFrames + eyeFrames - 1;
			eyeFramePositions.Add(iFrameGETITBECAUSEEYES);
			GameObject g = GetGameObject(GetScreenPosFromXY(Random.Range(1, width - 1), Random.Range (0, height - 1)) + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f)), player + "eyeball" + i, eyeSheet[iFrameGETITBECAUSEEYES], false, "Cover HUD");
			g.renderer.sortingOrder = i;
			eyeballs.Add(g);
		}
	}
}