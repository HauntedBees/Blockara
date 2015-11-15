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
public class AICore {
	public BoardWar myBoard, enemyBoard;
	public BoardCursorActualCore c;
	public int delay = 5, confused;
	protected int state, delayLowerbound = 5, delayUpperbound = 10, difficulty;
	protected bool inactive, easyMode;
	public AICore(BoardWar mine, BoardWar theirs, BoardCursorActualCore cursor, int difficulty = 0) {
		myBoard = mine;
		easyMode = false;
		enemyBoard = theirs;
		c = cursor;
		confused = 0;
		setUpDifficulty(difficulty);
		AdditionalSetup();

	}
	public void EasyModo() { easyMode = true; }
	public void ToggleInactive() { inactive = !inactive; }
	public void boostDifficulty(int d) {
		setUpDifficulty(difficulty + d);
		if(easyMode) { delayLowerbound *= 6; delayUpperbound *= 6; }
	}
	virtual protected void setUpDifficulty(int d) {
		difficulty = d;
		int inversePeakDifficulty = 9 - d;
		delayLowerbound = Mathf.FloorToInt(5 + inversePeakDifficulty * 4.0f);
		delayUpperbound = Mathf.FloorToInt(10 + inversePeakDifficulty * 7.5f);
	}
	virtual protected void AdditionalSetup() { }
	virtual public AIAction TakeAction() { return null; }
	protected int whatIsBeatenWhat(int i) {
		if(i == 2) { return 0; }
		return (i + 1);
	}
	protected int whatBeatsWhat(int i) {
		if(i == 0) { return 2; }
		return (i - 1);
	}
	protected bool beats(int mine, int theirs) {
		if(theirs > 2) { return true; }
		if(mine == 2 && theirs == 0) { return true; }
		return mine == (theirs - 1);
	}
	public void forceState(int s) { state = s; }

	protected struct depthInfoResults {
		public depthInfo[] res;
		public int Count;
		public depthInfoResults(depthInfo[] r, int c) { res = r; Count = c; }
	}
	protected struct depthInfo {
		public int x;
		public int topy;
		public int type;
		public int depth;
		public int distance;
		public depthInfo(int _x, int _type, int _depth, int _distance, int _topy) { x = _x; type = _type; depth = _depth; distance = _distance; topy = _topy; }
	}
	protected depthInfoResults getTypesAndDepths(BoardWar board, bool includeOneLength, bool mirrorX, int currentX = -1) {
		depthInfo[] vals = new depthInfo[board.width];
		int count = 0;
		for(int x = 0; x < board.width; x++) { 
			depthInfo res = getTypeAndDepth(board, x);
			if(mirrorX) { res.x = board.width - 1 - x; }
			if(currentX >= 0) { res.distance = distanceBetweenTwoXCoords(x, currentX, board.width); }
			if(res.depth > 1 || includeOneLength) { vals[count++] = res; }
		}
		return new depthInfoResults(vals, count);
	}

	protected int GetDistanceBetweenPoints(int x1, int x2) {
		if(x1 == x2) { return 0; }
		if(x1 > x2) { return -GetDistanceBetweenPoints(x2, x1); }
		int dx = x2 - x1;
		if(dx > Mathf.FloorToInt(myBoard.width / 2)) { return GetDistanceBetweenPoints(x1 + myBoard.width, x2); }
		return dx;
	}

	private depthInfo getTypeAndDepth(BoardWar board, int x) {
		int topy = board.GetHighestYAtX(x);
		int type = board.GetTopValueAtX(x);
		int depth = 1;
		for(int y = topy - 1; y >= 0; y--) {
			Tile val = board.GetValueAtXY(x, y);
			if(!val.IsDead() && val.GetColorVal() != type) { break; }
			depth++;
		}
		return new depthInfo(x, type, depth, 0, topy);
	}
	protected int distanceBetweenTwoXCoords(int a, int b, int width) {
		int dx = a - b;
		int half = Mathf.FloorToInt(width / 2.0f);
		if(dx > half) { dx = width - dx; }
		return dx;
	}
	protected int distDirection(int myPos, int theirPos, int width) {
		if(myPos == theirPos) { return 0; }
		int half = Mathf.FloorToInt(width / 2.0f);
		if((myPos > half && theirPos > half) || (myPos < half && theirPos < half)) {
			int dx = myPos - theirPos;
			if(dx > 0) { return -1; } else { return 1; }
		} else if(theirPos == half) {
			return 1;
		} else {
			int dx = myPos - theirPos;
			if(dx < 0) { return -1; } else { return 1; }
		}
	}
}