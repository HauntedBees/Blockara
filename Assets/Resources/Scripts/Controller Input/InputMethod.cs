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
using System.Collections.Generic;
public class InputMethod {
	public uint controllerNum;
	protected InputSettings keyBinds;
	virtual public void Initialize(Dictionary<int, string> rawBinding) { }
	virtual public void update() { }
	virtual public void refresh() { }
	virtual public bool G_Launch() { return false; }
	virtual public bool G_ShiftLeft() { return false; }
	virtual public bool G_ShiftRight() { return false; }
	virtual public bool G_ShiftAllLeft() { return false; }
	virtual public bool G_ShiftAllRight() { return false; }
	virtual public bool Nav_Left() { return false; }
	virtual public bool Nav_Right() { return false; }
	virtual public bool Nav_Up() { return false; }
	virtual public bool Nav_Down() { return false; }
	virtual public bool M_Confirm() { return false; }
	virtual public bool M_Cancel() { return false; }
	virtual public bool Pause() { return false; }
	virtual public bool EnableCheat1() { return false; }
	virtual public bool EnableCheat2() { return false; }
	virtual public string GetFriendlyActionName(Action a) { return ""; }
	public string GetFriendlyKeyName(KeyBinding k) { return keyBinds.GetName(k); }
	public string GetRawKeyVal(KeyBinding k) { return keyBinds.GetRawVal(k); }
	public InputVal GetInputVal(KeyBinding k) { return keyBinds.bindings[k]; }
	public bool IsAxis(KeyBinding k) { return keyBinds.IsAxis(k); }
	public void ChangeKey(KeyBinding k, InputVal v) { keyBinds.bindings[k] = v; }
	public KeyBinding GetKeyInUse(InputVal v) {
		string vRaw = v.GetRawVal();
		foreach(KeyBinding k in keyBinds.bindings.Keys) {
			if(keyBinds.bindings[k].GetRawVal() == vRaw) { return k; }
		}
		return KeyBinding.hidden3;
	}

	public enum Action { move, shiftL, shiftR, shiftAL, shiftAR, launch, pause }
	public enum KeyBinding { up = 0, down = 1, left = 2, right = 3, shiftL = 4, shiftR = 5, shiftAL = 6, shiftAR = 7, launch = 8, pause = 9, back = 10, hidden1 = 11 , hidden2 = 12, hidden3 = 13 }
}