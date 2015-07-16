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
public class BoardWarPuzzlePlayer:BoardWar {
	public int unlockedRow;
	public override void DoShift() {
		if(unlockedRow < 0) { base.DoShift(); return; }
		if(shifting == 0) { return; }
		if(shiftall) {
			if(isShown) { PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRows); }
			if(shifting > 0) {
				cursor.shiftX((cursor.getX() == width - 1) ? (-width + 1) : 1);
				cursor.DoUpdate();
				ShiftRow(unlockedRow, 1);
				changes.Add(new MirrorChangeShift(unlockedRow, 1));
			} else if(shifting < 0) {
				cursor.shiftX((cursor.getX() == 0) ? (width - 1) : -1);
				cursor.DoUpdate();
				ShiftRow(unlockedRow, -1);
				changes.Add(new MirrorChangeShift(unlockedRow, -1));
			}
		} else if(cursor.getY() == unlockedRow) {
			if(isShown) { PD.sounds.SetSoundAndPlay(SoundPaths.S_ShiftRow); }
			if(shifting > 0) {
				ShiftRow(cursor.getY(), 1);
				changes.Add(new MirrorChangeShift(cursor.getY(), 1));
			} else if(shifting < 0) {
				ShiftRow(cursor.getY(), -1);
				changes.Add(new MirrorChangeShift(cursor.getY(), -1));
			}
		} else {
			PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny);
		}
		actionDelay = PD.KEY_DELAY;
	}
}