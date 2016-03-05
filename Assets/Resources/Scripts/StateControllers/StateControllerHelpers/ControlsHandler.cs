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
public class ControlsHandler:MenuHandler {
	private List<GameObject> p1, p2;
	private Sprite[] buttonsSheet, cancelSheet;
	private GameObject topCollider, cancelButton;
	private string renderLayer;
	private float p1x, p2x;
	private FontData f;
	private InputMethod.KeyBinding[] keyOrdering;
	private InputMethod fakePlayer2;
	public override void CleanUp() {
		if(p1 == null) { return; }
		for(int i = 0; i < p1.Count; i++) { Destroy(p1[i]); }
		for(int i = 0; i < p2.Count; i++) { Destroy(p2[i]); }
		if(cancelButton != null) { Destroy(cancelButton); }
		p1.Clear();
		p2.Clear();
		Destroy(topCollider);
		base.CleanUp();
	}
	public void Setup() {
		top = GetXMLHead();
		buttonsSheet = Resources.LoadAll<Sprite>(SpritePaths.InputIcons);
		cancelSheet = Resources.LoadAll<Sprite>(SpritePaths.CancelButtons);
		renderLayer = "HUDText";
		headerText.text = GetXmlValue(top, "controls");
		float x = -0.25f;
		f = PD.mostCommonFont.Clone();
		f.align = TextAlignment.Right; f.anchor = TextAnchor.MiddleRight;
		texts.Add(GetMeshText(new Vector2(x,  0.6f), "Up:", f));
		texts.Add(GetMeshText(new Vector2(x,  0.4f), "Down:", f));
		texts.Add(GetMeshText(new Vector2(x,  0.2f), "Left:", f));
		texts.Add(GetMeshText(new Vector2(x,  0.0f), "Right:", f));
		texts.Add(GetMeshText(new Vector2(x, -0.2f), "Launch/Confirm:", f));
		texts.Add(GetMeshText(new Vector2(x, -0.4f), "Shift Left:", f));
		texts.Add(GetMeshText(new Vector2(x, -0.6f), "Shift Right:", f));
		texts.Add(GetMeshText(new Vector2(x, -0.8f), "Shift All Left:", f));
		texts.Add(GetMeshText(new Vector2(x, -1.0f), "Shift All Right:", f));
		texts.Add(GetMeshText(new Vector2(x, -1.2f), "Pause:", f));
		texts.Add(GetMeshText(new Vector2(x, -1.4f), "Cancel:", f));
		f.align = TextAlignment.Left; f.anchor = TextAnchor.MiddleLeft;

		keyOrdering = new InputMethod.KeyBinding[]{InputMethod.KeyBinding.back, InputMethod.KeyBinding.pause, InputMethod.KeyBinding.shiftAR, 
			InputMethod.KeyBinding.shiftAL, InputMethod.KeyBinding.shiftR, InputMethod.KeyBinding.shiftL, InputMethod.KeyBinding.launch, 
			InputMethod.KeyBinding.right, InputMethod.KeyBinding.left, InputMethod.KeyBinding.down, InputMethod.KeyBinding.up};

		p1x = x + 0.75f;
		p1 = new List<GameObject>();
		texts.Add(GetMeshText(new Vector2(p1x, 1.0f), "P1", f));
		texts.Add(GetMeshText(new Vector2(p1x, 0.8f), "ALL", f));
		for(int y = 0; y < keyOrdering.Length; y++) { p1.Add(GetKeyBindingDisplay(PD.controller, new Vector2(p1x, -1.4f + y * 0.2f), keyOrdering[y])); }

		p2x = p1x + 1.75f;
		p2 = new List<GameObject>();
		texts.Add(GetMeshText(new Vector2(p2x, 1.0f), "P2", f));
		texts.Add(GetMeshText(new Vector2(p2x, 0.8f), "ALL", f));
		fakePlayer2 = new Input_Computer();
		fakePlayer2.Initialize(PD.GetKeyBindings(1));
		for(int y = 0; y < keyOrdering.Length; y++) { p2.Add(GetKeyBindingDisplay(fakePlayer2, new Vector2(p2x, -1.4f + y * 0.2f), keyOrdering[y])); }

		topCollider = GetCollider("top", new Vector3(1.45f, -0.2f), 1.2f, 13.5f);
	}

	public Vector2 GetTopColliderPositionX(MouseCore clicker) {
		Vector3 collider = clicker.getPositionInGameObject(topCollider);
		if(collider.z == 0) { return new Vector2(-1.0f, -1.0f); }
		return new Vector2(collider.x, 0.0f);
	}
	public Vector2 GetTopColliderPositionY(MouseCore clicker) {
		Vector3 collider = clicker.getPositionInGameObject(topCollider);
		if(collider.z == 0) { return new Vector2(-1.0f, -1.0f); }
		return new Vector2(0.0f, collider.y);
	}

	public bool ClickingCancelButton(MouseCore clicker) {
		if(cancelButton == null) { return false; }
		if(clicker.getPositionInGameObject(cancelButton).z == 0) {
			cancelButton.GetComponent<SpriteRenderer>().sprite = cancelSheet[0];
			return false;
		} else {
			cancelButton.GetComponent<SpriteRenderer>().sprite = cancelSheet[1];
			return clicker.isDown();
		}
	}
	public void SetToQuestion(int y, bool player1) {
		cancelButton = GetGameObject(new Vector3((player1?p1x:p2x) + 0.4f, -1.4f + y * 0.2f), "back", cancelSheet[0], true, "HUDText");
		cancelButton.transform.localScale = new Vector3(0.75f, 0.75f);
		if(player1) {
			Destroy(p1[y]);
			p1[y] = GetMeshText(new Vector3(p1x, -1.4f + y * 0.2f), "?", f).gameObject;
		} else {
			Destroy(p2[y]);
			p2[y] = GetMeshText(new Vector3(p2x, -1.4f + y * 0.2f), "?", f).gameObject;
		}
	}
	public void UndoQuestion(int y, bool player1) {
		if(cancelButton != null) { Destroy(cancelButton); }
		if(player1) {
			Destroy(p1[y]);
			p1[y] = GetKeyBindingDisplay(PD.controller, new Vector2(p1x, -1.4f + y * 0.2f), keyOrdering[y]);
		} else {
			Destroy(p2[y]);
			p2[y] = GetKeyBindingDisplay(fakePlayer2, new Vector2(p2x, -1.4f + y * 0.2f), keyOrdering[y]);
		}
	}

	public int DetectKeyInput() {
		for(int i = 0; i < 330; i++) { if(Input.GetKeyDown((KeyCode)i)) { return i; } }
		for(int i = 350; i < 430; i++) { if(Input.GetKeyDown((KeyCode)i)) { return i; } }
		return -1;
	}
	public bool HasReturnedToNormal() {
		for(int controller = 1; controller <= 4; controller++) {
			for(int axis = 0; axis < 13; axis++) {
				string key = "joy" + controller + "_" + axis;
				if(Input.GetAxis(key) < -0.5f || Input.GetAxis(key) > 0.5f) { return false; }
			}
		}
		return true;
	}
	public string DetectAxisInput() {
		for(int controller = 1; controller <= 4; controller++) {
			for(int axis = 0; axis < 13; axis++) {
				string key = "joy" + controller + "_" + axis;
				if(Input.GetAxis(key) < -0.75f) { return "-" + key; }
				if(Input.GetAxis(key) > 0.75f) { return "+" + key; }
			}
		}
		return "";
	}
	public void ChangeKey(int y, int newVal, bool player1) {
		if(player1) {
			InputVal newInput = new InputVal_Key((KeyCode) newVal);
			InputMethod.KeyBinding oldK = PD.controller.GetKeyInUse(newInput);
			if(oldK == keyOrdering[y] || oldK == InputMethod.KeyBinding.hidden3) {
				PD.controller.ChangeKey(keyOrdering[y], newInput);
				PD.SetKeyBinding(0, (int)keyOrdering[y], newInput.GetRawVal());
			} else {
				InputVal oldInput = PD.controller.GetInputVal(keyOrdering[y]);
				PD.controller.ChangeKey(keyOrdering[y], newInput);
				PD.SetKeyBinding(0, (int)keyOrdering[y], newInput.GetRawVal());

				PD.controller.ChangeKey(oldK, oldInput);
				PD.SetKeyBinding(0, (int)oldK, oldInput.GetRawVal());
				int idx = System.Array.IndexOf(keyOrdering, oldK);
				if(idx >= 0) { UndoQuestion(idx, true); }
			}
		} else {
			InputVal newInput = new InputVal_Key((KeyCode) newVal);
			InputMethod.KeyBinding oldK = fakePlayer2.GetKeyInUse(newInput);
			if(oldK == keyOrdering[y] || oldK == InputMethod.KeyBinding.hidden3) {
				fakePlayer2.ChangeKey(keyOrdering[y], newInput);
				PD.SetKeyBinding(1, (int)keyOrdering[y], newInput.GetRawVal());
			} else {
				InputVal oldInput = fakePlayer2.GetInputVal(keyOrdering[y]);
				fakePlayer2.ChangeKey(keyOrdering[y], newInput);
				PD.SetKeyBinding(1, (int)keyOrdering[y], newInput.GetRawVal());
				
				fakePlayer2.ChangeKey(oldK, oldInput);
				PD.SetKeyBinding(1, (int)oldK, oldInput.GetRawVal());
				int idx = System.Array.IndexOf(keyOrdering, oldK);
				if(idx >= 0) { UndoQuestion(idx, false); }
			}
		}
		UndoQuestion(y, player1);
	}
	public void ChangeAxis(int y, string newVal, bool player1) {
		int dir = newVal.IndexOf("+") == 0 ? 1 : -1;
		string key = newVal.Substring(1);
		if(player1) {
			InputVal newInput = new InputVal_Axis(key, dir);
			PD.controller.ChangeKey(keyOrdering[y], newInput);
			PD.SetKeyBinding(0, (int)keyOrdering[y], newInput.GetRawVal());
		} else {
			InputVal newInput = new InputVal_Axis(key, dir);
			fakePlayer2.ChangeKey(keyOrdering[y], newInput);
			PD.SetKeyBinding(1, (int)keyOrdering[y], newInput.GetRawVal());
		}
		UndoQuestion(y, player1);
	}


	private GameObject GetKeyBindingDisplay(InputMethod controller, Vector2 pos, InputMethod.KeyBinding key) {
		if(controller.IsAxis(key)) {
			string rawstrval = controller.GetRawKeyVal(key);
			return GetGameObject(pos + new Vector2(0.05f, 0.0f), "btn" + rawstrval, buttonsSheet[GetSheetIdxFromAxisVal(rawstrval)], false, renderLayer);
		}
		int rawval = int.Parse(controller.GetRawKeyVal(key));
		if(rawval < 350) {
			KeyCode val = (KeyCode) rawval;
			switch(val) {
				case KeyCode.UpArrow: return GetGameObject(pos + new Vector2(0.05f, 0.0f), "up", buttonsSheet[3], false, renderLayer);
				case KeyCode.DownArrow: return GetGameObject(pos + new Vector2(0.05f, 0.0f), "down", buttonsSheet[2], false, renderLayer);
				case KeyCode.LeftArrow: return GetGameObject(pos + new Vector2(0.05f, 0.0f), "left", buttonsSheet[0], false, renderLayer);
				case KeyCode.RightArrow: return GetGameObject(pos + new Vector2(0.05f, 0.0f), "right", buttonsSheet[1], false, renderLayer);
				default: return GetMeshText(pos, controller.GetFriendlyKeyName(key), f).gameObject;
			}
		}
		return GetGameObject(pos + new Vector2(0.05f, 0.0f), "btn" + rawval, buttonsSheet[GetSheetIdxFromKeyCode(rawval)], false, renderLayer);
	}
	private int GetSheetIdxFromAxisVal(string axisname) {
		string[] axisVals = axisname.Split(':');
		int dir = int.Parse(axisVals[1]);
		int justTheAxis = int.Parse(axisVals[0].Split('_')[1]);
		#if (UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
		switch(justTheAxis) { // left is negative up is negative
			case 0: if(dir<0) { return 10; } else { return 11; } // L X-axis
			case 1: if(dir<0) { return 13; } else { return 12; } // L Y-axis
			case 2: if(dir>0) { return 24; } else { return 25; } // shoulder buttons
			case 3: if(dir<0) { return 15; } else { return 16; } // R X-axis
			case 4: if(dir<0) { return 18; } else { return 17; } // R Y-axis
			case 5: if(dir<0) { return 20; } else { return 21; } // d X-axis
			case 6: if(dir<0) { return 22; } else { return 23; } // d Y-axis
		}
		#elif UNITY_STANDALONE_LINUX
		switch(justTheAxis) { // left is negative up is negative
			case 0: if(dir<0) { return 10; } else { return 11; } // L X-axis
			case 1: if(dir<0) { return 13; } else { return 12; } // L Y-axis
			case 2: return 24; // L shoulder button
			case 3: if(dir<0) { return 15; } else { return 16; } // R X-axis
			case 4: if(dir<0) { return 18; } else { return 17; } // R Y-axis
			case 5: return 25; // R shoulder button
			case 6: if(dir<0) { return 20; } else { return 21; } // d X-axis
			case 7: if(dir<0) { return 22; } else { return 23; } // d Y-axis
		}
		#endif
		return 0;
	}
	private int GetSheetIdxFromKeyCode(int keycode) {
		int newVal = (keycode - 350) % 20;
		switch(newVal) {
			case 0: return 4;
			case 1: return 5;
			case 2: return 7;
			case 3: return 6;
			case 4: return 26;
			case 5: return 27;
			case 6: return 9;
			case 7: return 8;
			case 8: return 14;
			case 9: 
			#if (UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
				return 19;
			#elif UNITY_STANDALONE_LINUX
				return 14;
			#endif
			case 10: return 19;
		}
		return 0;
	}
}