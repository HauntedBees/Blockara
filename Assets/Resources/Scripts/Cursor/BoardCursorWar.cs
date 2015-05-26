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
public class BoardCursorWar:BoardCursorActualCore {
	#region "Members"
	private List<int> keyStates;
	public bool usingTouchControls;
	#endregion
	#region "Setup"
	protected override void ExtraInit () {
		usingTouchControls = false;
		keyStates = new List<int>();
		for(int i = 0; i < 4; i++) { keyStates.Add(-1); }
	}
	#endregion
	#region "Updating"
	public override void DoUpdate() {
		if(!usingTouchControls) { HandleControls(); }
		MainUpdate();
	}
	#endregion
	#region "Control Execution"
	private void ClearKeyStates() { for(int i = 0; i < 4; i++) { keyStates[i] = -1; } }
	protected void HandleControls() {
		if(--moveDelay > 0) { return; }
		int dx = 0;
		int dy = 0;
		bool keysPressed = false;
		if(controller != null) { controller.update(); }
		if(left()) {
			keysPressed = true;
			keyStates[0]++;
			if(keyStates[0] == 0 || keyStates[0] > Consts.DELAY_INT) { dx -= 1; }
		} else if(right()) {
			keysPressed = true;
			keyStates[1]++;
			if(keyStates[1] == 0 || keyStates[1] > Consts.DELAY_INT) { dx += 1; }
		}
		if(down()) {
			keysPressed = true;
			keyStates[2]++;
			if(keyStates[2] == 0 || keyStates[2] > Consts.DELAY_INT) { dy -= 1; }
		} else if(up()) {
			keysPressed = true;
			keyStates[3]++;
			if(keyStates[3] == 0 || keyStates[3] > Consts.DELAY_INT) { dy += 1; }
		}
		x += dx;
		y += dy;
		controller.refresh();
		if(!keysPressed) { ClearKeyStates(); } else { PD.usingMouse = false; }
		if(x < 0) { x = boardwidth - 1; } else if(x >= boardwidth) { x = 0; }
		if(y < 0) { y = 0; } else if(y >= boardheight) { y = boardheight - 1; }
		if(dx != 0 || dy != 0) { moveDelay = PD.KEY_DELAY; }
	}
	#endregion
	#region "Input Handling"
	protected bool left() { return controller.Nav_Left(); }
	protected bool right() { return controller.Nav_Right(); }
	protected bool down() { return controller.Nav_Down(); }
	protected bool up() { return controller.Nav_Up(); }
	#endregion
}