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
public class BoardWar:BoardWarCore {
	private const int TIMER_BONUS = 120;
	private int countdownFromLaunch;
	protected BoardCursorActualCore cursor;
	public int score, misses, shifting;
	public float actionDelay;
	public bool dead, shiftall, usingTouchControls;
	public LaunchInfo launchInfo;
	private BoardMirror mirror;
	protected List<MirrorChange> changes;
	private int[] keyStates;
	private int highestRowWithTiles;
	protected BlockNexter bn;
	public struct LaunchInfo {
		public int x, len, type, topy;
		public float bonus;
		public bool launching;
		public LaunchInfo(int _x, int l, int t, int y, float b, bool launch = true) { x = _x; len = l; type = t; topy = y; bonus = b; launching = launch; }
	}
	virtual public void Setup(BoardCursorActualCore c, TweenHandler t, BlockHandler bh, Vector2 nexterPos, bool smallNext, bool show = true, bool touch = false) {
		GetPersistData();
		usingTouchControls = touch;
		topoffset = 1;
		shiftall = false; dead = false; cursor = c; deathTile = 3; isShown = show; th = t;
		launchInfo = new LaunchInfo(-1, 0, deathTile, 0, 1.0f, false);
		countdownFromLaunch = 0; actionDelay = 0;
		changes = new List<MirrorChange>();
		keyStates = new int[4] {-1, -1, -1, -1};
		tileSheet = GetTileSheet();
		shapeSheet = GetShapeSheet();
		overlaySprite = GetOverlaySprite();
		bn = ScriptableObject.CreateInstance<BlockNexter>();
		if(isShown) { bn.SetupUniversalPrefabAndSheet(PD.universalPrefab, tileSheet, overlaySprite); }
		bn.Initialize(bh, nexterPos.x, nexterPos.y, player, isShown, smallNext, ((smallNext || !PD.IsLeftAlignedHUD()) && player == 1) ? 1 : -1);
		SetupTilesList();
		bn.SetupTileGraphics();
		CreateBG();
		chain = 0; misses = 0;
	} 
	protected void CreateBG() {
		if(!isShown) { return; }
		GetGameObject(GetScreenPosFromXY(0, 0) - new Vector3(0.13f, 0.13f), "Background", Resources.Load<Sprite>(SpritePaths.BGTileBack), false, "BG1");
	}
	public void SetMirror(BoardMirror m) { mirror = m; }
	public BoardMirror GetMirror() { return mirror; }
	override protected int GetTileColor(int pos = -1) { return bn.GetTileForPlayer(); }
	private void ClearKeyStates() { for(int i = 0; i < 4; i++) { keyStates[i] = -1; } }
	public bool IsKeyPressAccepted() { actionDelay -= Time.deltaTime * 60.0f;  return 0 >= actionDelay; }
	virtual public void DoUpdate() {
		launchInfo.launching = false; shifting = 0; shiftall = false; countdownFromLaunch--;
		ClearChanges();
		actionDelay -= Time.deltaTime * 60.0f;
		if(actionDelay < 0 && !usingTouchControls) {
			bool shiftKeyDown = false;
			if(cursor.launch()) {
				SetLaunchInfoForLaunch();
				actionDelay = PD.KEY_DELAY;
			} else if(cursor.shiftLeft()) {
				shiftKeyDown = true;
				keyStates[0]++;
				if(keyStates[0] == 0 || keyStates[0] > Consts.DELAY_INT) { shifting = -1; }
			} else if(cursor.shiftRight()) {
				shiftKeyDown = true;
				keyStates[1]++;
				if(keyStates[1] == 0 || keyStates[1] > Consts.DELAY_INT) { shifting = 1;  }
			} else if(cursor.shiftAllRight()) {
				shiftKeyDown = true;
				keyStates[2]++;
				if(keyStates[2] == 0 || keyStates[2] > Consts.DELAY_INT) { shiftall = true; shifting = 1; }
			} else if(cursor.shiftAllLeft()) {
				shiftKeyDown = true;
				keyStates[3]++;
				if(keyStates[3] == 0 || keyStates[3] > Consts.DELAY_INT) { shiftall = true; shifting = -1; }
			}
			if(!shiftKeyDown) { ClearKeyStates(); }
		}
	}
	public virtual void DoShift() {
		if(shifting == 0) { return; }
		if(shiftall) {
			if(isShown) { PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRows); }
			if(shifting > 0) {
				cursor.shiftX((cursor.getX() == width - 1) ? (-width + 1) : 1);
				cursor.DoUpdate();
				for(int i = 0; i < height; i++) {
					ShiftRow(i, 1);
					changes.Add(new MirrorChangeShift(i, 1));
				}
			} else if(shifting < 0) {
				cursor.shiftX((cursor.getX() == 0) ? (width - 1) : -1);
				cursor.DoUpdate();
				for(int i = 0; i < height; i++) {
					ShiftRow(i, -1);
					changes.Add(new MirrorChangeShift(i, -1));
				}
			}
		} else {
			if(isShown) { PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRow); }
			if(shifting > 0) {
				ShiftRow(cursor.getY(), 1);
				changes.Add(new MirrorChangeShift(cursor.getY(), 1));
			} else if(shifting < 0) {
				ShiftRow(cursor.getY(), -1);
				changes.Add(new MirrorChangeShift(cursor.getY(), -1));
			}
		}
		actionDelay = PD.KEY_DELAY;
	}
	public bool DidLastMoveBlockCurrentAttack() { return actionDelay > 2; }
	public bool CanBeKilled(int x, int length) { return (GetHighestYAtX(x) < 0 && length > 2); }
	virtual protected bool DoesLaunchHit(int launch, int x, int y) {
		Tile victim = tiles[GetListPosFromXY(x, y)];
		int victimVal = victim.GetColorVal();
		return (victim.IsDead()) || (launch == (victimVal - 1)) || (launch == 2 && victimVal == 0);
	}
	public int GetHitDepth(int x, int length, int type, int givenTopHeight = -10) {
		int topHeight = (givenTopHeight == -10)?GetHighestYAtX(x):givenTopHeight;
		int halfHeight = Mathf.FloorToInt(height/2);
		if(topHeight < 0) { return -1; }
		if(length <= 1 || (length < 3 && topHeight < halfHeight)) { return topHeight; }
		if(!DoesLaunchHit(type, x, topHeight)) { return topHeight; }
		topHeight = GetHighestYAtX(x, topHeight - 1);
		
		if(topHeight < 0) { return -1; }
		if(length < 5) { return topHeight; }
		if(!DoesLaunchHit(type, x, topHeight)) { return topHeight; }
		topHeight = GetHighestYAtX(x, topHeight - 1);
		
		if(topHeight < 0) { return -1; }
		if(length < 6) { return topHeight; }
		if(!DoesLaunchHit(type, x, topHeight)) { return topHeight; }
		topHeight = GetHighestYAtX(x, topHeight - 1);
		return topHeight;
	}
	virtual public int TakeDamage(int x, int length, int type) {
		ignoreDamageSound = false;
		int initHeight = GetHighestYAtX(x);
		if(GetHighestYAtX(x) < 0 && length > 2) { BeDefeated(); return 10; }
		int amount = GetHitDepth(x, length, type);
		for(int i = height - 1; i > amount; i--) { DestroyBlock(x, i); changes.Add(new MirrorChangeRemoveTile(x, i)); }
		if(CheckIfCantWin()) { BeDefeated(); return 10; }
		int depth = initHeight - amount;
		if(depth > 0 && !ignoreDamageSound) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Damage); }
		UpdateHighestRowWithTiles();
		return depth;
	}
	
	public int GetHighestRowWithTiles() {
		if(highestRowWithTiles == 0) { UpdateHighestRowWithTiles(); }
		return highestRowWithTiles;
	}
	protected void UpdateHighestRowWithTiles() {
		for(int y = 0; y < height; y++) {
			bool allEmpty = true;
			for(int x = 0; x < width; x++) {
				int idx = GetListPosFromXY(x, y);
				Tile t = tiles[idx];
				if(!t.IsDead()) { 
					allEmpty = false; 
					break;
				}
			}
			if(!allEmpty) { highestRowWithTiles = y; }
		}
	}
	protected bool CheckIfCantWin() {
		for(int x = 0; x < width; x++) { if(GetHighestYAtX(x) > 1) { return false; } }
		return true;
	}
	public void BeDefeated() { dead = true; }
	public bool IsDead() { return dead; }
	virtual public int[] GetLaunchDetails(int x) {
		int length = 0, type = -1, topy = height - topoffset;
		for(int y = topy; y >= 0; y--) {
			Tile t = tiles[GetListPosFromXY(x, y)];
			if(t.IsDead()) { continue; }
			int tVal = t.GetColorVal();
			if(type >= 0 && tVal != type) { break; }
			else if(type < 0) { type = tVal; topy = y; }
			length++;
		}
		return new int[] {length, type, topy};
	}
	public void SetLaunchInfoForLaunch() {
		int x = cursor.getX();
		int[] lengthTypeTopy = GetLaunchDetails(x);
		int length = lengthTypeTopy[0];
		int type = lengthTypeTopy[1];
		int topy = lengthTypeTopy[2];
		if(length < 2) { HandleShitRow(); return; }
		float launchBonus = launchInfo.bonus;
		if(countdownFromLaunch > 0) { chain++; launchBonus += 0.2f; } else { chain = 0; launchBonus = 1.0f; }
		launchInfo = new LaunchInfo(x, length, type, topy, launchBonus);
	}
	public void UpdateBlockNexter(int length) {
		bn.HighlightNextTiles(length);
	}
	public void AcceptLaunch() {
		LaunchTiles(launchInfo.len, launchInfo.topy, launchInfo.x);
		bn.ShiftTileGraphics(launchInfo.len);
		changes.Add(new MirrorChangeLaunch(launchInfo.len, launchInfo.topy, launchInfo.x));
		countdownFromLaunch = TIMER_BONUS;
	}
	private void HandleShitRow() {
		int y = cursor.getY();
		int type = -1;
		for(int x = 0; x < width; x++) {
			Tile t = tiles[GetListPosFromXY(x, y)];
			int tVal = t.GetColorVal();
			if(t.IsDead()) { return; }
			if(type < 0) { type = tVal; }
			if(tVal != type) { return; }
		}
		LaunchTiles(1, y, cursor.getX());
		bn.ShiftTileGraphics(1);
		changes.Add(new MirrorChangeLaunch(1, y, cursor.getX()));
	}
	public override Vector3 GetScreenPosFromXY(int x, int y) { return new Vector3((xOffset + x) * Consts.TILE_SIZE, Consts.YBOTTOM + y * Consts.TILE_SIZE); }
	public List<MirrorChange> GetChanges() { return changes; }
	private void ClearChanges() { changes.Clear(); }
	public void AddToScore(int n) { score += n; if(score >= 10000000) { score = 9999999; } }
	public int GetScore() { return score; }
	public int GetTopValueAtX(int x) {
		int topy = GetHighestYAtX(x);
		if(topy < 0) { return -1; }
		return tiles[GetListPosFromXY(x, topy)].GetColorVal();
	}

	private List<Tile> recoveryDisplays;
	public void HandleRecoveryDisplay() {
		if(recoveryDisplays == null) { recoveryDisplays = new List<Tile>(); } else { WipeRecoveryDisplays(); }
		for(int x = 0; x < width; x++) {
			int[] deets = GetLaunchDetails(x);
			int length = deets[0], topy = deets[2], type = deets[1];
			if(length < 2) { continue; }
			int[] newVals = new int[height];
			for(int y = topy - length; y >= 0; y--) {
				Tile t = GetValueAtXY(x, y);
				if(t.IsDead()) {
					newVals[y + length] = -1;
				} else {	
					newVals[y + length] = t.GetColorVal();
				}
			}
			for(int y = length - 1; y >= 0; y--) { newVals[y] = type; }
			for(int y = topy; y >= 0; y--) {
				if(newVals[y] < 0 && !GetValueAtXY(x, y).IsDead()) { DrawDestroyDisplay(x, y); }
				else if(newVals[y] >= 0 && GetValueAtXY(x, y).IsDead()) { DrawRecoveryDisplay(x, y, type); }
			}
		}
	}
	private void WipeRecoveryDisplays() {
		for(int i = 0; i < recoveryDisplays.Count; i++) { recoveryDisplays[i].WipeOverlay(); }
		recoveryDisplays.Clear();
	}
	private void DrawRecoveryDisplay(int x, int y, int type) {
		Tile g = GetValueAtXY(x, y);
		g.MakeRecoveryTile(type);
		recoveryDisplays.Add(g);
	}
	private void DrawDestroyDisplay(int x, int y) {
		Tile cross = GetValueAtXY(x, y);
		cross.MakeDestroyTile();
		recoveryDisplays.Add(cross);
	}

}