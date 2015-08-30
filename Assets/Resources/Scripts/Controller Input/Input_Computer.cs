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
public class Input_Computer:InputMethod {
	public override void Initialize(Dictionary<int, string> rawBinding) {
		keyBinds = new InputSettings();
		foreach(int key in rawBinding.Keys) {
			string rawBindVal = rawBinding[key];
			int bindingAsKey = -1;
			if(int.TryParse(rawBindVal, out bindingAsKey)) {
				keyBinds.bindings.Add((KeyBinding)key, new InputVal_Key((KeyCode) bindingAsKey));
			} else {
				string[] splitVal = rawBindVal.Split(':');
				keyBinds.bindings.Add((KeyBinding)key, new InputVal_Axis(splitVal[0], int.Parse(splitVal[1])));
			}
		}
	}
	public override bool G_Launch() { return keyBinds.IsDown(KeyBinding.launch); }
	public override bool G_ShiftLeft() { return keyBinds.IsPress(KeyBinding.shiftL); }
	public override bool G_ShiftRight() { return keyBinds.IsPress(KeyBinding.shiftR); }
	public override bool G_ShiftAllLeft() { return keyBinds.IsPress(KeyBinding.shiftAL); }
	public override bool G_ShiftAllRight() { return keyBinds.IsPress(KeyBinding.shiftAR); }
	public override bool Nav_Up() { return keyBinds.IsPress(KeyBinding.up); }
	public override bool Nav_Left() { return keyBinds.IsPress(KeyBinding.left); }
	public override bool Nav_Down() { return keyBinds.IsPress(KeyBinding.down); }
	public override bool Nav_Right() { return keyBinds.IsPress(KeyBinding.right); }
	public override bool M_Confirm() { return keyBinds.IsDown(KeyBinding.launch); }
	public override bool M_Cancel() { return Input.GetKeyDown(KeyCode.Escape) || keyBinds.IsDown(KeyBinding.back); }
	public override bool Pause() { return keyBinds.IsDown(KeyBinding.pause); }
	public override bool EnableCheat1() { return keyBinds.IsDown(KeyBinding.hidden1) && keyBinds.IsPress(KeyBinding.hidden3); }
	public override bool EnableCheat2() { return keyBinds.IsDown(KeyBinding.hidden2) && keyBinds.IsPress(KeyBinding.hidden3); }
	public override string GetFriendlyActionName(Action a) {
		switch(a) {
			case Action.launch: return GetFriendlyKeyName(KeyBinding.launch);
			case Action.move: return GetMovementKeys();
			case Action.shiftAL: return GetFriendlyKeyName(KeyBinding.shiftAL);
			case Action.shiftAR: return GetFriendlyKeyName(KeyBinding.shiftAR);
			case Action.shiftL: return  GetFriendlyKeyName(KeyBinding.shiftL);
			case Action.shiftR: return  GetFriendlyKeyName(KeyBinding.shiftR);
			case Action.pause: return  GetFriendlyKeyName(KeyBinding.pause);
		}
		return "error";
	}
	private string GetMovementKeys() {
		int left, right, up, down;
		bool isAllKeys = int.TryParse(keyBinds.bindings[KeyBinding.left].GetRawVal(), out left) &&
				int.TryParse(keyBinds.bindings[KeyBinding.up].GetRawVal(), out up) &&
				int.TryParse(keyBinds.bindings[KeyBinding.down].GetRawVal(), out down) &&
				int.TryParse(keyBinds.bindings[KeyBinding.right].GetRawVal(), out right);
		if(isAllKeys) {
			if((KeyCode) left == KeyCode.LeftArrow && (KeyCode) right == KeyCode.RightArrow && 
			   (KeyCode) down == KeyCode.DownArrow && (KeyCode) up == KeyCode.UpArrow) {
				return "the arrow keys";
			} else {
				return keyBinds.bindings[KeyBinding.up].GetName() + 
						keyBinds.bindings[KeyBinding.left].GetName() + 
						keyBinds.bindings[KeyBinding.down].GetName() + 
						keyBinds.bindings[KeyBinding.right].GetName();
			}
		} else {
			return keyBinds.bindings[KeyBinding.down].GetName();
		}
	}
}