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
public class BoardWarPuzzle:BoardWarSpecial {
	override public void Setup(BoardCursorActualCore c, TweenHandler t, BlockHandler bh, Vector2 nexterPos, bool smallNext, bool show = true, bool touch = false) {
		base.Setup(c, t, bh, nexterPos, smallNext, show, touch);
		topoffset = 1;
		cursor.setWidthAndHeight(width, --height);
	} 
	override protected Tile CreateTile(int x, int y, int tval = -1, int sval = -1) {
		int acttval = tval, actsval = -1;
		if(tval == 10) {
			acttval = 0;
			actsval = 11;
		} else if(acttval > 3) {
			acttval -= 4;
			actsval = 9;
			if(acttval > 2) {
				acttval -= 3;
				actsval = 10;
			}
		}
		Tile t = base.CreateTile(x, y, acttval, actsval);
		if(tval == 10) { t.MakeConcrete(); }
		return t;
	}
	override public int TakeDamage(int x, int length, int type) {
		ignoreDamageSound = false;
		int initHeight = GetHighestYAtX(x);
		if(GetHighestYAtX(x) < 0 && length > 2) { BeDefeated(); return 10; }
		int amount = GetHitDepth(x, length, type);
		for(int i = height - 1; i > amount; i--) { changes.Add(new MirrorChangeRemoveTile(x, i)); DestroyBlock(x, i); }
		if(CheckIfCantWin()) { BeDefeated(); return 10; }
		int depth = initHeight - amount;
		if(depth > 0 && !ignoreDamageSound) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Damage); }
		return depth;
	}
	override protected void DestroyBlock(int x, int y) {
		int shift = HandlePuzzleTile(tiles[GetListPosFromXY(x, y)], y);
		base.DestroyBlock(x, y);
		if(shift != 0) {
			ShiftRow(y, shift);
			changes.Add(new MirrorChangeShift(y, shift));
		}
	}
	private int HandlePuzzleTile(Tile t, int y) {
		if(t.IsDead()) { return 0; }
		int val = t.GetSpecialVal();
		switch(val) {
			case 9:
				PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRow);
				return 1;
			case 10:
				PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRow);
				return -1;
		}
		return 0;
	}
}