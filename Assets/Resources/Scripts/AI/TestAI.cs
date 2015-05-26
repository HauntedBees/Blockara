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
public class TestAI:AICore {
	private int columnFocus;
	private int desiredType;
	private int shiftDir;
	public TestAI(BoardWar mine, BoardWar theirs, BoardCursorActualCore cursor):base(mine, theirs, cursor) {}
	override protected void AdditionalSetup() {
		columnFocus = Random.Range(0, enemyBoard.width - 1);
		state = 1;
	}
	override public AIAction TakeAction() {
		delay = Random.Range(delayLowerbound, delayUpperbound);

		int cursorX = c.getX();
		int cursorY = c.getY();

		int x = 0; int y = 0; int shift = 0; bool launch = false;
		int r = Random.Range(0, 9);
		switch(state) {
			case 0:
				columnFocus = Random.Range (0, enemyBoard.width - 1);
				state = 1;
				break;
			case 1:
			if(cursorY == (myBoard.height - myBoard.topoffset) || r < 5 && cursorX != columnFocus) {
					x = (cursorX < columnFocus) ? 1 : -1;
			} else if(cursorY < (myBoard.height - myBoard.topoffset)) {
					y++;
				}
			if((cursorY + y) == (myBoard.height - myBoard.topoffset) && (cursorX + x) == columnFocus) {
					desiredType = whatBeatsWhat(enemyBoard.GetTopValueAtX(enemyBoard.width - columnFocus - 1));
					shiftDir = 0;
					state = 2;
			} else if(r < 1) { state = 0; } else { }
				break;
			case 2:
				if(noOptions(cursorY) || r < 4) {
					if(rowIsEmpty(cursorY)) { state = 3; }
					else if(r < 1) { state = 0; }
					else { launch = true; state = 0; }
				} else {
					if(shiftDir == 0) { shiftDir = getShiftDirection(cursorX, cursorY); }
					if(desiredType == myBoard.GetValueAtXY(cursorX, cursorY).GetColorVal()) {
						state = 3;
					} else {
						shift = shiftDir;
					}
				}
				break;
			case 3:
				if(cursorY > 0) {
					if(r < 1) {
						launch = true;
						state = 0;
					} else {
						y = -1;
						state = 2;
					}
				} else {
					launch = true;
					state = 0;
				}
				break;
			case 4:
				launch = true;
				state = 0;
				break;
		}
		if((cursorX + x) < 0) { x *= -1; } else if((cursorX + x) >= enemyBoard.width) { x *= -1; }
		return new AIAction(x, y, shift, launch);
	}
	private bool noOptions(int y) {
		for(int x = 0; x < myBoard.width; x++) { if(myBoard.GetValueAtXY(x, y).GetColorVal() == desiredType) { return false; } }
		return true;
	}
	private bool rowIsEmpty(int y) {
		for(int x = 0; x < myBoard.width; x++) { if(myBoard.GetValueAtXY(x, y).GetColorVal() != 3) { return false; } }
		return true;
	}
	private int getShiftDirection(int x, int y) {
		int val = myBoard.GetValueAtXY(x, y).GetColorVal();
		if(val == desiredType) { return 0; }
		int nx = x - 1;
		while(nx >= 0) {
			if(myBoard.GetValueAtXY(x, y).GetColorVal() == desiredType) { return -1; }
			nx--;
		}
		return 1;
	}
}