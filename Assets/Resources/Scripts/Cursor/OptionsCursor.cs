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
public class OptionsCursor:BoardCursorWar {
	private string spritePath;
	private float xOff;
	private float yOff;
	private float xDiff;
	private float yDiff;
	private int spriteSheetPos;
	#region "Setup"
	override public void Setup(TweenHandler t, int maxdepth, bool show = true) {
		isShown = show;
		th = t;
	}
	public void SetupMenu(string sp, float px, float py, float dx, float dy, int initX, int initY, int frame = -1, float dtweenchange = -1.0f) {
		if(dtweenchange > 0.0f) { myDTween = dtweenchange; }
		spritePath = sp;
		spriteSheetPos = frame;
		InitializeMembers();
		moveDelay = 0;
		x = initX;
		y = initY;
		xOff = px;
		yOff = py;
		xDiff = dx;
		yDiff = dy;
		UpdateCursorPos(true);
		FinishUpdate();
	}
	override protected void InitGraphics() {
		if(spriteSheetPos < 0) {
			cursor = GetGameObject(Vector3.zero, "", (string.IsNullOrEmpty(spritePath)?null:Resources.Load<Sprite>(spritePath)), false, "HUDText");
		} else {
			Sprite[] cursors = Resources.LoadAll<Sprite> (spritePath);
			cursor.GetComponent<SpriteRenderer>().sprite = cursors[spriteSheetPos];
			cursor = GetGameObject(Vector3.zero, "", cursors[spriteSheetPos], false, "HUDText");
		}
	}
	#endregion
	#region "Updating"
	public override void DoUpdate(int max = -1) {
		HandleControls();
		MainUpdate();
		if(HasMoved()) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Select); }
	}
	override protected void MainUpdate() {
		UpdateCursorPos();
		FinishUpdate();
	}
	#endregion
	public bool leftP() { return left(); }
	public bool rightP() { return right(); }
	public bool upP() { return up(); }
	public bool downP() { return down(); }
	override public Vector3 getPos(int x, int y) { return new Vector3(xOff + x * xDiff, yOff + y * yDiff); }
}