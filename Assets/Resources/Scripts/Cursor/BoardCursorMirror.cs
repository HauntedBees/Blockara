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
public class BoardCursorMirror:BoardCursorCore {
	private BoardCursorActualCore parent;
	private float maxY;
	public void Setup(BoardCursorActualCore p, TweenHandler t, bool show = true) {
		parent = p;
		isShown = show;
		th = t;
		InitializeMembers();
		x = boardwidth - 1;
		y = boardheight - 1;
		maxY = 1.85f - (boardheight - 1) * Consts.TILE_SIZE;

		UpdateMirrorCursorPos(true);
		FinishUpdate();
	}
	public override void DoUpdate() {
		cursor.GetComponent<SpriteRenderer>().sprite = sheet[parent.frozen?2:(parent.penetr?1:0)];
		x = boardwidth - 1 - parent.getX();
		y = boardheight - 1 - parent.getY();
		UpdateMirrorCursorPos();
		FinishUpdate();
	}
	private void UpdateMirrorCursorPos(bool skipTween = false) {
		if(!isShown) { return; }
		if(prevx == x && prevy == y) { return; }
		Vector3 pos =  new Vector3((xOffset + x) * Consts.TILE_SIZE, maxY + y * Consts.TILE_SIZE);
		if(skipTween) {
			cursor.transform.position = pos;
		} else {
			th.DoTween(cursor, pos);
		}
	}
}