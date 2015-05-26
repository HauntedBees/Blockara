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
using System.Linq; // for debug
public class AIversion2:AICore {
	private int repeats0, repeats1, repeats2, repeats25;
	private int repeatTheshold;
	private int yourX, theirInvertedX, y_row, yourType, theirType, cur_row, dxdir, shiftdir, randoWhatsie;
	private float paddingForPathetic, selfAwareness;
	private int[] lastTenStates;
	private int stateArrayIdx;
	private bool stateArrayFull;
	private bool showFullDebug;
	public AIversion2(BoardWar mine, BoardWar theirs, BoardCursorActualCore cursor, int difficulty = 0):base(mine, theirs, cursor, difficulty) {}
	override protected void AdditionalSetup() {
		state = 0; repeats0 = 0; repeats1 = 0; repeats2 = 0; repeats25 = 0;
		randoWhatsie = 0;
		lastTenStates = new int[10];
		stateArrayIdx = 0;
		stateArrayFull = false;
		showFullDebug = false;
	}
	override protected void setUpDifficulty(int d) {
		difficulty = d;
		if(d == 13) {
			delayLowerbound = 5;
			delayUpperbound = 5;
			paddingForPathetic = 0.3f;
			selfAwareness = 0.9f;
		} else if(d < 3) {
			int inversePeakDifficulty = 5 - d;
			delayLowerbound = Mathf.FloorToInt(30 + inversePeakDifficulty * 4.0f);
			delayUpperbound = Mathf.FloorToInt(60 + inversePeakDifficulty * 9.5f);
			paddingForPathetic = Mathf.Clamp01(inversePeakDifficulty / 4.0f);
			selfAwareness = d / 7.0f;
		} else {
			int inversePeakDifficulty = 9 - d;
			if(inversePeakDifficulty < 0) {
				int dd = 12 - d;
				if(dd < 0) { dd = 0; }
				float ddf = dd / 6.0f;
				delayLowerbound = Mathf.FloorToInt(5 + ddf * 0.1f);
				delayUpperbound = Mathf.FloorToInt(5 + ddf * 0.7f);
			} else {
				delayLowerbound = Mathf.FloorToInt(5 + inversePeakDifficulty * 4.0f);
				delayUpperbound = Mathf.FloorToInt(10 + inversePeakDifficulty * 7.5f);
			}
			paddingForPathetic = Mathf.Clamp01(inversePeakDifficulty / 6.0f);
			selfAwareness = d / 7.0f;
		}
		repeatTheshold = Mathf.Max(4, Mathf.FloorToInt(8.0f - d / 2.0f));
	}
	private void AddStateAndDetectLoop() {
		if(state == 999 || state == lastTenStates[stateArrayIdx]) { return; }
		if(stateArrayIdx == 9) {
			stateArrayIdx = 0;
			stateArrayFull = true;
		} else { stateArrayIdx++; }
		lastTenStates[stateArrayIdx] = state;
		if(!stateArrayFull) { return; }
		for(int i = 0; i < 10; i++) {
			string val = "" + lastTenStates[i] + lastTenStates[(i+1)%10] + lastTenStates[(i+2)%10] + lastTenStates[(i+3)%10] + lastTenStates[(i+4)%10] + lastTenStates[(i+5)%10];
			for(int j = i + 1; j < 10; j++) {
				string nval = "" + lastTenStates[j] + lastTenStates[(j+1)%10] + lastTenStates[(j+2)%10] + lastTenStates[(j+3)%10] + lastTenStates[(j+4)%10] + lastTenStates[(j+5)%10];
				if(nval == val) {
					if(showFullDebug) Debug.Log ("FOUND A LOOP!");
					stateArrayIdx = 0;
					stateArrayFull = false;
					lastTenStates = new int[10];
					lastTenStates[0] = 0;
					state = 999;
					return;
				}
			}
		}
	}
	private void DetectTopTwoRowsAreShit() {
		if(IsRowAllSame(myBoard.height - 1)) {
			y_row = myBoard.height - 1;
			state = 5;
		} else if(IsRowAllSame(myBoard.height - 2)) {
			y_row = myBoard.height - 2;
			state = 5;
		}
	}
	private bool IsRowAllSame(int y) {
		Tile t = myBoard.GetValueAtXY(0, y);
		if(t.IsDead()) { return false; }
		int tileType = t.GetColorVal();
		for(int x = 1; x < myBoard.width; x++) {
			Tile nt = myBoard.GetValueAtXY(x, y);
			if(nt.IsDead()) { return false; }
			if(nt.GetColorVal() != tileType) { return false; }
		}
		return true;
	}
	override public AIAction TakeAction() {
		if(inactive) { return new AIAction(0, 0); }
		if(Input.GetKeyDown(KeyCode.M)) { showFullDebug = !showFullDebug; Debug.Log("now: " + showFullDebug); }
		AddStateAndDetectLoop();
		DetectTopTwoRowsAreShit();
		if(Input.GetKey(KeyCode.Alpha1)) {
			Debug.Log ("state: " + state);
			Debug.Log (" yourX: " + yourX);
			Debug.Log (" theirInvertedX: " + theirInvertedX);
			Debug.Log (" y_row: " + y_row);
			Debug.Log (" yourType: " + yourType);
			Debug.Log (" theirType: " + theirType);
			Debug.Log (" cur_row: " + cur_row);
			Debug.Log (" dxdir: " + dxdir);
			Debug.Log (" shiftdir: " + shiftdir);
		}
		delay = Random.Range(delayLowerbound, delayUpperbound);
		AIAction res;
		switch(state) {
			case 0: res = s0_0(); break;
			case 1: res = s0_1(yourX, theirInvertedX, yourType, theirType, dxdir, shiftdir); break;
			case 2: res = s0_2(yourX, theirInvertedX, yourType, theirType, shiftdir); break;
			case 3: res = s0_3(); break;
			case 10: res = s1(); break;
			case 20: res = s2_0(); break;
			case 21: res = s2_1(y_row, yourX, yourType, dxdir); break;
			case 22: res = s2_2(y_row, yourX, yourType, dxdir); break;
			case 23: res = s2_3(y_row, yourX, yourType, dxdir, shiftdir, cur_row); break;
			case 24: res = s2_4(y_row, yourX, yourType); break;
			case 25: res = s2_5(yourType, dxdir); break;
			case 30: res = s3_0(); break;
			case 31: res = s3_1(y_row, yourType, shiftdir); break;
			case 4: res = s4(); break;
			case 5: res = s5(); break;
			default: res = sOhFuckLetsGoRandom(); break;
		}
		if(c.getY() == 0 && res.dy < 0) { res.dy = 0; }
		else if(c.getY() == (myBoard.height - myBoard.topoffset) && res.dy > 0) { res.dy = 0; }
		return res;
	}
	
	private struct distXData { public int x, dx; public distXData(int _x, int _dx) { x = _x; dx = _dx; } }
	private struct typeDepthData { public int type, depth; public typeDepthData(int _type, int _depth) { type = _type; depth = _depth; } }
	private struct columnData { public int x, type, height, dx; public columnData(int _x, int _type, int _height, int _dx) { x = _x; type = _type; height = _height; dx = _dx; } }

	private AIAction sOhFuckLetsGoRandom() { // FUCK YOU I'M RANDOM
		if(randoWhatsie == 0) { y_row = 1; } // Debug.Log("I'M GOG DAMN NUTTY!");
		int cy = c.getY();
		if(cy == (myBoard.height - myBoard.topoffset)) { y_row = -1; } else if(cy == 0) { y_row = 1; }
		AIAction res = new AIAction(Random.value >= 0.75f ? 1: 0, Random.value >= 0.5f ? y_row : 0, Random.value >= 0.25f ? 1: 0, Random.value >= 0.5f);
		if(randoWhatsie++ > Random.Range(4, 20) && confused <= 0) {
			state = 10;
			return s1();
		}
		return res;
	}
	private AIAction s5() { // there's a row of all the same tile on top
		//Debug.Log("S5 THE SCIENCE GYVE");
		if(y_row > c.getY()) { return new AIAction(0, 1); }
		else if(y_row < c.getY()) { return new AIAction(0, -1); }
		state = 0;
		return new AIAction(0, 0, 0, true);
	}
	private AIAction s4() { // OH FUCK I CAN'T EVEN DO ANYTHING; TOP ROW IS PROBABLY ALL DICKERS
		//Debug.Log("I'M AT 4");
		int yTop = GetHighestYWithValues();
		if(GetAllTypesOnRow(yTop).Count == 1) {
			int cy = c.getY();
			if(cy != yTop) {
				shiftdir = 0;
				if(cy > yTop) { return new AIAction(0, -1); }
				return new AIAction(0, 1);
			}
			state = 10;
			return s1();
		}
		state = 999;
		return sOhFuckLetsGoRandom();
	}
	private AIAction s3_1(int y, int typeM, int dshift) { // SET UP A COLUMN
		int cx = c.getX(), cy = c.getY();
		if(cy != y) {
			shiftdir = 0;
			if(cy > y) { return new AIAction(0, -1); }
			return new AIAction(0, 1);
		}
		if(myBoard.GetValueAtXY(cx, cy).GetColorVal() != typeM) {
			if(dshift == 0) {
				for(int x = 0; x < myBoard.width; x++) {
					if(myBoard.GetValueAtXY(x, cy).GetColorVal() == typeM) {
						int d = GetDistanceBetweenPoints(cx, x);
						shiftdir = d == 0 ? 0 : d / Mathf.Abs(d);
						dshift = shiftdir;
						break;
					}
				}
				if(dshift == 0) {
					state = 4;
					return s4();
				}
			}
			return new AIAction(0, 0, dshift);
		}
		int count = 0;
		for(int ty = myBoard.GetHighestYAtX(cx); ty >= 0; ty--) {
			int t = myBoard.GetValueAtXY(cx, ty).GetColorVal();
			if(t == myBoard.deathTile) { continue; }
			if(t != typeM) { break; }
			count++;
		}
		if(count > 1) {
			state = 10;
			return s1();
		} else {
			if(cy == 0) {
				state = 999;
				return sOhFuckLetsGoRandom();
			} else { 
				y_row--;
				shiftdir = 0;
				return new AIAction(0, -1);
			}
		}
	}

	private AIAction s3_0() { // YOU CAN'T HURT THE ENEMY AT ALL, JUST TRY LAUNCHING WHATEVER
		//Debug.Log("I'M AT 3");
		int cy = c.getY();
		int yTop = GetHighestYWithValues();
		if(yTop == 0) {
			state = 999;
			return sOhFuckLetsGoRandom();
		}
		if(cy != yTop) { 
			if(cy > yTop) { return new AIAction(0, -1); }
			return new AIAction(0, 1);
		}
		List<int> toppuns = GetAllTypesOnRow(yTop);
		List<int> nextuns = GetAllTypesOnRow(yTop - 1);
		if(nextuns.Count == 0) {
			state = 999;
			return sOhFuckLetsGoRandom();
		}
		for(int i = 0; i < toppuns.Count; i++) {
			if(!nextuns.Contains(toppuns[i])) { continue; }
			yourType = toppuns[i];
			break;
		}
		yTop = cy;
		shiftdir = 0;
		state = 31;
		return s3_1(yTop, yourType, shiftdir);
	}
	private AIAction s2_5(int typeM, int xdir) { // LINE UP TILES
		int cx = c.getX(), cy = c.getY();
		if(myBoard.GetValueAtXY(cx, cy).GetColorVal() != typeM) {
			if(repeats25++ > 4) {
				state = 1;
				return s1();
			}
			if(xdir == 0) {
				List<distXData> res = new List<distXData>();
				for(int x = 0; x < myBoard.width; x++) {
					if(myBoard.GetValueAtXY(x, cy).GetColorVal() == typeM) { res.Add(new distXData(x, GetDistanceBetweenPoints(cx, x))); }
				}
				if(res.Count == 0) {
					state = 999;
					return sOhFuckLetsGoRandom();
				}
				res.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
				distXData chosenOne = res[Random.Range(0, Mathf.FloorToInt(paddingForPathetic * res.Count))];
				dxdir = chosenOne.dx / Mathf.FloorToInt(chosenOne.dx);
				xdir = dxdir;
			}
			return new AIAction(0, 0, xdir);
		} else {
			int depth = 0;
			for(int y = myBoard.height - myBoard.topoffset; y >= 0; y--) {
				int t = myBoard.GetValueAtXY(cx, y).GetColorVal();
				if(t == myBoard.deathTile) { continue; }
				if(t != typeM) { break; }
				depth++;
			}
			if(showFullDebug) Debug.Log("depth is " + depth);
			if(depth < 2) {
				if(cy == 0) {
					state = 999;
					return sOhFuckLetsGoRandom();
				} else { 
					xdir = 0;
					shiftdir = 0;
					return new AIAction(0, -1);
				}
			}
			List<typeDepthData> acceptableTypes = GetAcceptableTypesAndTheirMinimumDepths();
			List<distXData> res2 = new List<distXData>();
			for(int x = 0; x < acceptableTypes.Count; x++) {
				if(!beats(yourType, acceptableTypes[x].type) || depth < acceptableTypes[x].depth) { continue; }
				res2.Add(new distXData(x, GetDistanceBetweenPoints(cx, enemyBoard.width - 1 - x)));
			}
			if(res2.Count == 0) {
				if(showFullDebug) Debug.Log("I can beat anything with that!");
				if(cy == 0) {
					state = 999;
					return sOhFuckLetsGoRandom();
				} else { 
					xdir = 0;
					shiftdir = 0;
					return new AIAction(0, -1);
				}
			}
			res2.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
			distXData chosenOne = res2[Random.Range(0, Mathf.FloorToInt(paddingForPathetic * res2.Count))];
			theirInvertedX = myBoard.width - 1 - chosenOne.x;
			if(showFullDebug) Debug.Log("I can beat the tile at " + theirInvertedX + " with that!");
			theirType = whatIsBeatenWhat(yourType);
			shiftdir = chosenOne.dx == 0 ? 0 : -chosenOne.dx / Mathf.Abs(chosenOne.dx);
			state = 2;
			return new AIAction(0, 0);
		}
	}
	private AIAction s2_4(int y, int xM, int typeM) { // MOVE BACK TO GOOD ROW
		int cy = c.getY();
		if(cy != y) { return new AIAction(0, -1); }
		state = 25;
		dxdir = 0;
		shiftdir = 0;
		return new AIAction(0, -1);
	}
	private AIAction s2_3(int y, int xM, int typeM, int xdir, int dshift, int cury) { // EMPTY OUT ROW
		int type = myBoard.GetValueAtXY(xM, cury).GetColorVal();
		if(type != myBoard.deathTile) {
			if(dshift != 0) { return new AIAction(0, 0, dshift); }
			int maxDist = 5;
			for(int x = 0; x < myBoard.width; x++) {
				int d = GetDistanceBetweenPoints(xM, x);
				if(Mathf.Abs(d) < maxDist) { shiftdir = d; }
			}
			return new AIAction(0, 0);
		}
		if(cury == (myBoard.height - myBoard.topoffset)) {
			state = 24;
			return s2_4(y, xM, typeM);
		} else {
			cur_row++;
			return new AIAction(0, 1);
		}
	}
	private AIAction s2_2(int y, int xM, int typeM, int xdir) { // DRAW A CLEAR LINE ABOVE YOU
		int cx = c.getX();
		if(cx != xM) { return new AIAction(xdir, 0); }
		cur_row = y_row + 1;
		state = 23;
		shiftdir = 0;
		return new AIAction(0, 1);
	}
	private AIAction s2_1(int y, int xM, int typeM, int xdir) { // THE ROW YOU ARE ON HAS SHIT YOU CAN DAMAGE FUCKERS
		int cx = c.getX();
		if(cx != xM) { return new AIAction(xdir, 0); }
		y_row -= 1;
		state = 25;
		dxdir = 0;
		return new AIAction(0, -1);
	}
	private AIAction s2_0() { // FIND SOMETHING TO CREATE
		int cy = c.getY();
		List<int> types = new List<int>();
		for(int x = 0; x < myBoard.width; x++) {
			int type = myBoard.GetValueAtXY(x, cy).GetColorVal();
			if(types.Contains(type)) { continue; }
			types.Add(type);
		}
		if(showFullDebug) Debug.Log("types on top row: " + string.Join(", ", types.Select(x => x.ToString()).ToArray()));
		if(types.Count == 1 && types[0] == myBoard.deathTile) {
			y_row--;
			if(cy == 0) {
				state = 999;
				return sOhFuckLetsGoRandom();
			} else {
				return new AIAction(0, -1);
			}
		}
		List<int> beatableXs = new List<int>();
		List<int> winningTypes = new List<int>();
		for(int x = 0; x < enemyBoard.width; x++) {
			int type = enemyBoard.GetTopValueAtX(x);
			for(int i = 0; i < types.Count; i++) {
				if(types[i] != myBoard.deathTile && beats(types[i], type)) {
					int wintype = whatBeatsWhat(type);
					beatableXs.Add(enemyBoard.width - 1 - x);
					if(!winningTypes.Contains(wintype)) { winningTypes.Add(wintype); }
				}
			}
		}
		if(showFullDebug) Debug.Log("types I should pick: " + string.Join(", ", winningTypes.Select(x => x.ToString()).ToArray()));
		if(beatableXs.Count == 0) {
			if(types.Contains(myBoard.deathTile)) {
				y_row--;
				if(cy == 0) {
					state = 999;
					return sOhFuckLetsGoRandom();
				} else {
					return new AIAction(0, -1);
				}
			} else {
				state = 30;
				return s3_0();
			}
		}
		int cx = c.getX();
		List<columnData> res = new List<columnData>();
		for(int x = 0; x < myBoard.width; x++) { 
			int t = myBoard.GetValueAtXY(x, cy).GetColorVal();
			if(winningTypes.Contains(t)) { res.Add(new columnData(x, t, 0, GetDistanceBetweenPoints(cx, x))); }
		}
		if(res.Count > 0) {
			res.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
			columnData chosenOne = res[Random.Range(0, Mathf.FloorToInt(paddingForPathetic * (res.Count - 1)))];
			if(showFullDebug) Debug.Log("i will go to [" + chosenOne.x + "]: type = " + chosenOne.type);
			yourType = chosenOne.type;
			yourX = chosenOne.x;
			dxdir = chosenOne.dx == 0 ? 0 : chosenOne.dx / Mathf.Abs(chosenOne.dx);
			if(cy == (myBoard.height - myBoard.topoffset)) {
				state = 21;
			} else {
				state = 22;
			}
		}
		return new AIAction(0, 0);
	}
	private AIAction s1() { // LAUNCH
		yourX = 0; theirInvertedX = 0; y_row = 0; yourType = 0; theirType = 0; cur_row = 0; dxdir = 0; shiftdir = 0; randoWhatsie = 0;
		state = 0;
		repeats0 = 0; repeats1 = 0; repeats2 = 0; repeats25 = 0;
		return new AIAction(0, 0, 0, true);
	}
	private AIAction s0_3() { // MOVE TO TOP ROW
		int cy = c.getY();
		if(cy == (myBoard.height - myBoard.topoffset)) {
			state = 20;
			y_row = myBoard.height - myBoard.topoffset;
			return s2_0();
		}
		return new AIAction(0, 1);
	}
	private AIAction s0_2(int xM, int xE, int typeM, int typeE, int dshift) { // SHIFT ALL TO A PREPARED ROW
		int cx = c.getX();
		if(cx == xE) { state = 1; return s1(); }
		if(!StillHasTypeAtPos(enemyBoard, myBoard.width - 1 - xE, typeE)) { state = 0; }
		if(!StillHasTypeAtPos(myBoard, cx, typeM)) {
			int newx_e = GetNewTargetX(xM, typeM);
			if(repeats2++ > repeatTheshold || newx_e < 0) { repeats2 = 0; state = 0; }
			theirInvertedX = newx_e;
		}
		if(dshift == 0) { dshift = 1; }
		return new AIAction(0, 0, dshift, false, true);
	}
	private AIAction s0_1(int xM, int xE, int typeM, int typeE, int xdir, int dshift) { // MOVE TO A PREPARED ROW
		int cx = c.getX();
		if(cx == xM) {
			state = 2;
			return s0_2(xM, xE, typeM, typeE, dshift);
		}
		AIAction res = new AIAction(0, 0);
		if(cx != xM) { res.dx = xdir; }
		if(!StillHasTypeAtPos(enemyBoard, myBoard.width - 1 - xE, typeE)) { state = 0; }
		if(selfAwareness > Random.value && !StillHasTypeAtPos(myBoard, cx, typeM)) {
			int newx_e = GetNewTargetX(xM, typeM);
			if(repeats1++ > repeatTheshold || newx_e < 0) { repeats1 = 0; state = 0; }
			theirInvertedX = newx_e;
		}
		return res;
	}
	private AIAction s0_0() { // SEE IF ANYTHING EXISTING WORKS
		if(showFullDebug) Debug.Log("AT 0");
		if(repeats0++ > repeatTheshold) {
			if(showFullDebug) Debug.Log("THIS IS TOO MUCH");
			state = 3;
			return s0_3();
		}
		List<typeDepthData> acceptableTypes = GetAcceptableTypesAndTheirMinimumDepths();
		List<columnData> res = new List<columnData>();
		int cx = c.getX();
		for(int x = 0; x < myBoard.width; x++) {
			int topYAtX = myBoard.GetHighestYAtX(x);
			if(topYAtX <= 0) { continue; }
			int typeAtPos = myBoard.GetValueAtXY(x, topYAtX).GetColorVal();
			int height = 1;
			for(int y = topYAtX - 1; y >= 0; y--) {
				if(myBoard.GetValueAtXY(x, y).GetColorVal() != typeAtPos) { break; }
				height++;
			}
			if(height == 1) { continue; }
			columnData cda = new columnData(x, typeAtPos, height, GetDistanceBetweenPoints(cx, x));
			if(CanHitOpponentWithThis(acceptableTypes, cda)) { res.Add(cda); }
		}
		if(showFullDebug) Debug.Log("RES: " + res.Count);
		if(res.Count == 0) {
			if(showFullDebug) Debug.Log("THERE IS NOTHING HERE FOR ME");
			state = 3;
			return s0_3();
		}
		state = 1;
		res.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
		int choice = Random.Range(0, Mathf.FloorToInt(paddingForPathetic * res.Count));
		int chances = 0;
		List<distXData> res2 = new List<distXData>();
		columnData chosenOne = res[choice];	
		yourX = chosenOne.x;
		yourType = chosenOne.type;
		if(showFullDebug) Debug.Log("CHOSEN ONE: " + chosenOne.x);
		while(chances++ < 3 && choice < res.Count) {
			res2.Clear();
			for(int x = 0; x < acceptableTypes.Count; x++) {
				if(!beats(yourType, acceptableTypes[x].type)) { continue; }
				res2.Add(new distXData(x, GetDistanceBetweenPoints(chosenOne.x, myBoard.width - 1 - x)));
			}
			if(res2.Count != 0) { break; }
		}
		if(showFullDebug) Debug.Log("RES2: " + res2.Count);
		if(res2.Count == 0) {
			if(showFullDebug) Debug.Log("THERE IS NOTHING HERE FOR ME!");
			state = 3;
			return s0_3();
		}
		res2.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
		distXData chosenOne2 = res2[Random.Range(0, Mathf.FloorToInt(paddingForPathetic * res2.Count))];
		theirInvertedX = myBoard.width - 1 - chosenOne2.x;
		theirType = whatIsBeatenWhat(yourType);
		dxdir = chosenOne.dx == 0 ? 0 : chosenOne.dx / Mathf.Abs(chosenOne.dx);
		shiftdir = chosenOne2.dx == 0 ? 0 : chosenOne2.dx / Mathf.Abs(chosenOne2.dx);
		if(showFullDebug) Debug.Log("@");
		return s0_1(yourX, theirInvertedX, yourType, theirType, dxdir, shiftdir);
	}
	
	private List<int> GetAllTypesOnRow(int y) {
		List<int> ts = new List<int>();
		for(int x = 0; x < myBoard.width; x++) {
			int t = myBoard.GetValueAtXY(x, y).GetColorVal();
			if(t != myBoard.deathTile) { ts.Add(t); }
		}
		return ts;
	}
	private int GetHighestYWithValues() {
		return GetHighestYWithValuesInner(myBoard.height - myBoard.topoffset);
	}
	private int GetHighestYWithValuesInner(int y) {
		for(int x = 0; x < myBoard.width; x++) {
			if(!myBoard.GetValueAtXY(x, y).IsDead()) { return y; }
		}
		return GetHighestYWithValuesInner(y - 1);
	}
	private int GetNewTargetX(int myX, int myType) {
		List<distXData> res = new List<distXData>();
		for(int x = 0; x < enemyBoard.width; x++) {
			if(beats(myType, enemyBoard.GetTopValueAtX(x))) { res.Add(new distXData(x, GetDistanceBetweenPoints(myX, enemyBoard.width - 1 - x))); }
		}
		if(res.Count == 0) { return -1; }
		res.Sort((a, b) => Mathf.Abs(a.dx).CompareTo(Mathf.Abs(b.dx)));
		distXData chosenOne = res[Random.Range(0, Mathf.FloorToInt(paddingForPathetic * res.Count))];
		return enemyBoard.width - 1 - chosenOne.x;
	}
	private bool CanHitOpponentWithThis(List<typeDepthData> acceptableTypes, columnData me) {
		for(int x = 0; x < acceptableTypes.Count; x++) { if(beats(me.type, acceptableTypes[x].type) && me.height >= acceptableTypes[x].depth) { return true; } }
		return false;
	}
	private List<typeDepthData> GetAcceptableTypesAndTheirMinimumDepths() {
		List<typeDepthData> acceptableTypes = new List<typeDepthData>();
		int halfHeight = Mathf.FloorToInt(enemyBoard.height/2);
		for(int x = 0; x < enemyBoard.width; x++) {
			int topYAtX = enemyBoard.GetHighestYAtX(x);
			if(topYAtX < 0) { 
				acceptableTypes.Add(new typeDepthData(enemyBoard.deathTile, 3));
				continue;
			}
			int typeAtPos = enemyBoard.GetValueAtXY(x, topYAtX).GetColorVal();
			if(topYAtX > halfHeight) {
				acceptableTypes.Add(new typeDepthData(typeAtPos, 2));
			} else {
				acceptableTypes.Add(new typeDepthData(typeAtPos, 3));
			}
		}
		return acceptableTypes;
	}
	private bool StillHasTypeAtPos(BoardWar board, int x, int type) { return board.GetTopValueAtX(x) == type; }
}