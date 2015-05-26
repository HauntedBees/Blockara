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
public class BoardCursorBot:BoardCursorActualCore {
	#region "Members"
	private AICore AI;
	private AIAction memAction;
	#endregion
	#region "Setup"
	public void CreateAI(BoardWar myBoard, BoardWar theirBoard, int type, int difficulty = 0) {
		switch(type) {
			case (int)PersistData.GT.Training: AI = new TrainingAI(myBoard, theirBoard, this); break;
			case (int)PersistData.GT.Challenge: AI = new TrainingAI(myBoard, theirBoard, this); break;
			default: AI = new AIversion2(myBoard, theirBoard, this, difficulty); break;
		}
		if(type == (int)PersistData.GT.Training && difficulty == 2) { AI.forceState(3); }
		if(PD.GetSaveData().savedOptions["easymode"] == 1) { AI.EasyModo(); AI.boostDifficulty(difficulty); }
	}
	public void LevelUpAI(int boost = 1) { AI.boostDifficulty(boost); }
	public void Blind(int length) { AI.confused = length; }
	#endregion
	#region "Updating"
	public override void DoUpdate() {
		AIAction action = null;
		if(--AI.confused > 0) { AI.forceState(999); }
		if(--AI.delay <= 0) { action = AI.TakeAction(); }
		memAction = null;
		if(action != null) {
			if(action.newx >= 0) { x = action.newx; } else { x += action.dx; }
			if(x >= boardwidth) { x = 0; } else if(x < 0) { x = boardwidth - 1; }
			if(action.newy >= 0) { y = action.newy; } else { y += action.dy; }
			this.memAction = action;
		}
		MainUpdate();
	}
	override public void FreezyPop(int i) { AI.delay = i; }
	#endregion
	#region "Control Execution"
	public void forceAIState(int i) { if(i < 0) { AI.ToggleInactive(); return; } AI.delay = 0; AI.forceState(i); }
	public override bool shiftLeft() { return (memAction != null && memAction.shift < 0 && !memAction.shiftall); }
	public override bool shiftRight() { return (memAction != null && memAction.shift > 0 && !memAction.shiftall); }
	public override bool shiftAllLeft() { return (memAction != null && memAction.shift < 0 && memAction.shiftall); }
	public override bool shiftAllRight() { return (memAction != null && memAction.shift > 0 && memAction.shiftall); }
	public override bool launch() { return (memAction != null && memAction.launch); }
	#endregion
}