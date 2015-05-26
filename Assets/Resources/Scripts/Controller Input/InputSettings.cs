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
public class InputSettings {
	public Dictionary<InputMethod.KeyBinding, InputVal> bindings;
	public InputSettings() { bindings = new Dictionary<InputMethod.KeyBinding, InputVal>(); }
	public bool IsDown(InputMethod.KeyBinding bind) { return bindings[bind].KeyDown(); }
	public bool IsPress(InputMethod.KeyBinding bind) { return bindings[bind].KeyPress(); }
	public string GetName(InputMethod.KeyBinding bind) { return bindings[bind].GetName(); }
	public string GetRawVal(InputMethod.KeyBinding bind) { return bindings[bind].GetRawVal(); }
	public bool IsAxis(InputMethod.KeyBinding bind) { return (bindings[bind] is InputVal_Axis); }
}
public class InputVal {
	protected string rawVal;
	virtual public bool KeyDown() { return false; }
	virtual public bool KeyPress() { return false; }
	virtual public string GetName() { return ""; }
	public string GetRawVal() { return rawVal; }
}
public class InputVal_Key:InputVal {
	private KeyCode keyVal;
	public InputVal_Key(int keyCode) {
		this.rawVal = keyCode.ToString();
		this.keyVal = (KeyCode)keyCode;
	}
	public InputVal_Key(KeyCode keyCode) {
		this.rawVal = ((int)keyCode).ToString();
		this.keyVal = keyCode;
	}
	override public bool KeyDown() { return Input.GetKeyDown(keyVal); }
	override public bool KeyPress() { return Input.GetKey(keyVal); }
	override public string GetName() {
		int intCode = int.Parse(rawVal);
		if(intCode >= 350) {
			int newVal = (intCode - 350) % 20;
			switch(newVal) {
				case 0: return "A";
				case 1: return "B";
				case 2: return "X";
				case 3: return "Y";
				case 4: return "L1";
				case 5: return "R1";
				case 6: return "BACK";
				case 7: return "START";
				case 8: return "L3";
				case 9: 
					#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
					return "R3";
					#elif UNITY_STANDALONE_LINUX
					return "L3";
					#endif
				case 10: return "R3";
			}
		}
		return keyVal.ToString();
	}
}
public class InputVal_Axis:InputVal {
	private string joyName;
	private float axisHigh, axisLow;
	private int axisDir;
	private bool isDown;
	public InputVal_Axis(string joyName, int axisDir) {
		this.joyName = joyName;
		this.axisDir = axisDir;
		axisHigh = 0.75f;
		axisLow = 0.5f;
		float prevState = Input.GetAxis(joyName) * axisDir;
		if(prevState < 0.0f) { prevState = 0.0f; }
		isDown = prevState > axisHigh;
		this.rawVal = joyName + ":" + axisDir;
	}
	override public bool KeyPress() { return KeyDown(); }
	override public bool KeyDown() {
		float curState = Input.GetAxis(joyName) * axisDir;
		if(isDown) {
			if(curState < axisLow) { isDown = false; }
		} else {
			if(curState > axisHigh) { isDown = true; }
		}
		return isDown;
	}
	override public string GetName() {
		int idx = int.Parse(joyName.Split('_')[1]);
		#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
		switch(idx) { // left is negative up is negative
			case 0: return "the L-stick"; // L X-axis
			case 1: return "the L-stick"; // L Y-axis
			case 2: if(axisDir>0) { return "L2"; } else { return "R2"; } // shoulder buttons
			case 3: return "the R-stick"; // R X-axis
			case 4: return "the R-stick"; // R Y-axis
			case 5: return "the d-pad"; // d X-axis
			case 6: return "the d-pad"; // d Y-axis
		}
		#elif UNITY_STANDALONE_LINUX
		switch(idx) { // left is negative up is negative
			case 0: return "the L-stick"; // L X-axis
			case 1: return "the L-stick"; // L Y-axis
			case 2: return "L2"; // L shoulder button
			case 3: return "the R-stick"; // R X-axis
			case 4: return "the R-stick"; // R Y-axis
			case 5: return "R2"; // R shoulder button
			case 6: return "the d-pad"; // d X-axis
			case 7: return "the d-pad"; // d Y-axis
		}
		#endif
		return "";
	}
}