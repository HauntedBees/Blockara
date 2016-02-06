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
using System.Xml;
public class OptionsController:LeftButtonsMenuController {
	private MenuCursor cursor2, cursor3;
	private OptionsSelector cursor2Display;
	private TextMesh headerText, funFactText;
	private int prevPos;
	private Vector2[] resolutions;

	private int leftRight_delay;
	private Vector2 currentResolution;
	private int resIdx, menuPosition, controlsPos;
	private bool isFullScreen;
	private int volume_music, volume_sound, volume_voice;
	private bool confirmWipe;
	private GameObject[] side_options, side_controls, side_accessibility, side_back;
	private GameObject goBack;
	
	private OptionsHandler optionsScreen;
	private ControlsHandler controlsScreen;
	private AccessibilityHandler accessibilityScreen;
	private XmlNode top;
	public void Start() {
		StateControllerInit(false);
		confirmWipe = false;
		menuPosition = 0;
		controlsPos = 0;
		ResetOptions(false);

		float x = -2.85f, topy = 0.15f;
		cursor = GetMenuCursor(1, 4, null, x, topy - 0.6f, 0.0f, 0.3f, 0, 3);
		cursor2 = GetMenuCursor(1, 9, null, 1.9f, -0.8f, 0.0f, 0.2f, 0, 8);

		cursor3 = GetMenuCursor(2, 1, SpritePaths.RightArrows, 0.55f, 1.25f, 1.75f, 0.0f, 0, 0, 1, 2, 1.0f);
		cursor3.Rotate(-90.0f);
		cursor3.SetVisibility(false);

		cursor2Display = gameObject.AddComponent<OptionsSelector>();
		cursor2Display.Setup(1.1f, -0.6f, 0.2f);
		cursor2Display.SetVisibility(false);

		top = GetXMLHead();
		side_options = GetButton(x, topy + 0.3f, GetXmlValue(top, "options"), PD.mostCommonFont);
		side_controls = GetButton(x, topy, GetXmlValue(top, "controls"), PD.mostCommonFont);
		side_accessibility = GetButton(x, topy - 0.3f, GetXmlValue(top, "accessibility"), PD.mostCommonFont);
		side_back = GetButton(x, topy - 0.6f, GetXmlValue(top, "back"), PD.mostCommonFont);
		sidepanels = new GameObject[][] {side_back, side_accessibility, side_controls, side_options};
		selectedIdx = 3;

		CreateInfoPane(0.8f, 0.0f);
	}
	private void ResetOptions(bool andGraphics = true) {
		resolutions = new Vector2[] { new Vector2(1280, 720), new Vector2(1600, 900), new Vector2(1920, 1080) };
		currentResolution = new Vector2(Screen.width, Screen.height);
		resIdx = -1;
		for(int i = 0; i < resolutions.Length; i++) {
			if(resolutions[i].x == currentResolution.x && resolutions[i].y == currentResolution.y) { resIdx = i; break; }
		}
		if(resIdx < 0) { resIdx = 0; currentResolution = resolutions[0]; }
		isFullScreen = Screen.fullScreen;
		SaveData sd = PD.GetSaveData();
		volume_music = sd.savedOptions["vol_m"];
		PD.sounds.SetMusicVolume(volume_music / 100.0f);
		volume_sound = sd.savedOptions["vol_s"];
		PD.sounds.SetSoundVolume(volume_sound / 100.0f);
		volume_voice = sd.savedOptions["vol_v"];
		PD.sounds.SetVoiceVolume(volume_voice / 100.0f);

		if(andGraphics) {
			optionsScreen.SetResText(resolutions[resIdx]);
			optionsScreen.SetVMuText(volume_music);
			optionsScreen.SetVSoText(volume_sound);
			optionsScreen.SetVVoText(volume_voice);
			optionsScreen.SetFlsText(isFullScreen);
		}
	}
	private void CreateInfoPane(float x, float y) {
		GetGameObject(new Vector3(x, y), "infoPane", Resources.Load<Sprite>(SpritePaths.BigOptionInfoBox));
		goBack = GetGoBackImage(x, y);
		goBack.SetActive(false);
		FontData font = PD.mostCommonFont.Clone();
		font.scale = 0.06f;
		headerText = GetMeshText(new Vector3(x, y + 1.7f), "honk", font);
		font.scale = 0.035f;
		funFactText = GetMeshText(new Vector3(x, y + 1.0f), "honk", font);
		optionsScreen = gameObject.AddComponent<OptionsHandler>();
		optionsScreen.InitializeMembers(headerText, PD);
		controlsScreen = gameObject.AddComponent<ControlsHandler>();
		controlsScreen.InitializeMembers(headerText, PD);
		accessibilityScreen = gameObject.AddComponent<AccessibilityHandler>();
		accessibilityScreen.InitializeMembers(headerText, PD);
	}

	public void Update() {
		TweenAndMouse();
		switch(menuPosition) {
			case 1: UpdateAccessibility(); break;
			case 2: UpdateControls(); break;
			case 3: UpdateOptions(); break;
			default: UpdateOuter(); break;
		}
		if(menuPosition > 0 && PD.usingMouse) {
			if(clicker.getPositionInGameObject(side_back[0]).z > 0) { cursor.setY(0); }
			else if(clicker.getPositionInGameObject(side_accessibility[0]).z > 0) { cursor.setY(1); }
			else if(clicker.getPositionInGameObject(side_controls[0]).z > 0) { cursor.setY(2); }
			else if(clicker.getPositionInGameObject(side_options[0]).z > 0) { cursor.setY(3); }
			UpdateOuterMouse(clicker.isDown(), true);
		}
	}
	private void UpdateControls() { if(controlsPos == 0) { UpdateControlsTop(); } else { UpdateControlsInner(); } }
	private void UpdateControlsTop() {
		cursor2Display.SetVisibility(false);
		cursor3.SetVisibility(true);
		cursor.SetVisibility(false);
		cursor3.DoUpdate();
		if(cursor3.launchOrPause()) {
			TransitionIntoControlsInner();
		} else if(cursor3.back()) {
			LeaveControls();
		}
	}
	private void LeaveControls() {
		menuPosition = 0;
		controlsPos = 0;
		ChangeCursor2Details();
		cursor.setY(2);
		SignalFailure();
	}
	private void TransitionIntoControlsInner() {
		controlsPos = 1;
		ChangeCursor2Details(cursor3.getPos(cursor3.getX(), 0).x);
		cursor2.setY(11);
		cursor2Display.UpdatePosition(11, true);
		SignalSuccess();
	}
	private void UpdateControlsInner() {
		cursor2Display.SetRightVisibility();
		cursor3.SetVisibility(true);
		cursor.SetVisibility(false);
		bool isP1 = cursor3.getX() == 0;
		if(controlsPos == 4) {
			if(controlsScreen.HasReturnedToNormal()) { controlsPos = 3; }
		} else if(controlsPos == 3) {
			if(Input.GetKeyDown(KeyCode.Escape) || controlsScreen.ClickingCancelButton(clicker)) {
				SignalFailure();
				controlsPos = 1;
				controlsScreen.UndoQuestion(cursor2.getY(), isP1);
			} else {
				int key = controlsScreen.DetectKeyInput();
				int cy = cursor2.getY();
				if(key >= 0 && (KeyCode) key != KeyCode.Mouse0) {
					controlsScreen.ChangeKey(cursor2.getY(), key, isP1);
					if(cy == 0) {
						controlsPos = 1;
					} else {
						cursor2.setY(cy - 1);
						cursor2Display.UpdatePosition(cy - 1);
						controlsScreen.SetToQuestion(cy - 1, isP1);
					}
					return;
				}
				string axis = controlsScreen.DetectAxisInput();
				if(!string.IsNullOrEmpty(axis)) {
					controlsScreen.ChangeAxis(cursor2.getY (), axis, isP1);
					if(cy == 0) {
						controlsPos = 1;
					} else {
						cursor2.setY(cy - 1);
						cursor2Display.UpdatePosition(cy - 1);
						controlsScreen.SetToQuestion(cy - 1, isP1);
						controlsPos = 4;
					}
					return;
				}
			}
		} else if(controlsPos == 2) {
			if(Input.GetKeyDown(KeyCode.Escape)) {
				SignalFailure();
				controlsPos = 1;
				controlsScreen.UndoQuestion(cursor2.getY(), isP1);
			} else {
				int key = controlsScreen.DetectKeyInput();
				if(key >= 0 && (KeyCode) key != KeyCode.Mouse0) {
					controlsScreen.ChangeKey(cursor2.getY(), key, isP1);
					controlsPos = 1;
					cursor2.FreezyPop(PD.KEY_DELAY * 2);
					return;
				}
				string axis = controlsScreen.DetectAxisInput();
				if(!string.IsNullOrEmpty(axis)) {
					controlsScreen.ChangeAxis(cursor2.getY (), axis, isP1);
					controlsPos = 1;
					cursor2.FreezyPop(PD.KEY_DELAY * 2);
					return;
				}
			}
		} else if(controlsPos == 1) {
			cursor2.DoUpdate();
			int cy = cursor2.getY();
			cursor2Display.UpdatePosition(cy);
			if(cursor2.launchOrPause()) {
				SelectControlChange();
			} else if(cursor2.back()) {
				controlsPos = 0;
				SignalFailure();
			}
		}
	}

	private void SelectControlChange() {
		if(cursor2.getY() == 11) {
			controlsPos = 3;
			cursor2.setY(10);
			cursor2Display.UpdatePosition(10);
			controlsScreen.SetToQuestion(cursor2.getY(), cursor3.getX() == 0);
		} else {
			controlsPos = 2;
			controlsScreen.SetToQuestion(cursor2.getY(), cursor3.getX() == 0);
		}
		SignalSuccess();
	}

	private void ChangeCursor2Details(float x = 0.0f) {
		if(menuPosition == 2) {
			cursor2.setWidthAndHeight(1, 12);
			cursor2Display.ChangeParams(x - 0.25f, -1.4f, 0.2f);
			cursor2Display.SetWidth(0.0f);
		} else {
			cursor2.setWidthAndHeight(1, 9);
			cursor2Display.ChangeParams(1.1f, -0.6f, 0.2f);
		}
	}
	private void UpdateAccessibility() {
		cursor2Display.SetVisibility(true);
		cursor.SetVisibility(false);
		cursor3.SetVisibility(false);
		cursor2.DoUpdate();
		int cy = cursor2.getY();
		cursor2Display.UpdatePosition(cy);
		if(cursor2.launchOrPause()) {
			if(cy == 0) {
				menuPosition = 0;
				accessibilityScreen.Reset(PD.GetSaveData());
				cursor.setY(1);
				SignalFailure();
			} else if(cy == 1) { 
				ApplySelected_Accessibility();
			} else {
				cursor2.setY(1);
			}
		} else if(cursor2.back()) {
			menuPosition = 0;
			cursor.setY(1);
			SignalFailure();
		} else {
			if(--leftRight_delay <= 0) {
				int dx = 0;
				if(cursor.shiftRight() || cursor.shiftAllRight() || PD.controller.Nav_Right()) {
					cursor2Display.HighlightArrow(true);
					dx++;
				} else if(cursor.shiftLeft() || cursor.shiftAllLeft() || PD.controller.Nav_Left()) {
					cursor2Display.HighlightArrow(false);
					dx--;
				} else {
					if(!PD.usingMouse) { cursor2Display.ClearArrows(); }
				}
				if(dx != 0) { 
					UpdateAccessibilityOption(cy, dx);
					leftRight_delay = 15;
				}
			}
		}
		accessibilityScreen.UpdateCursorGraphic(cursor2Display, cy);
	}
	private void UpdateOptions() {
		cursor2Display.SetVisibility(true);
		cursor.SetVisibility(false);
		cursor3.SetVisibility(false);
		cursor2.DoUpdate();
		int cy = cursor2.getY();
		cursor2Display.UpdatePosition(cy);
		if(cy != 2) { confirmWipe = false; optionsScreen.SetClrText("clear"); }
		if(cursor2.launchOrPause()) {
			if(cy == 0) {
				menuPosition = 0;
				ResetOptions();
				SignalFailure();
				cursor.setY(3);
			} else if(cy == 1) { 
				ApplySelected_Options();
			} else if(cy == 2) {
				WipeSelected_Options();
			} else {
				cursor2.setY(1);
			}
		} else if(cursor2.back()) {
			SignalFailure();
			menuPosition = 0;
			cursor.setY(3);
		} else {
			if(--leftRight_delay <= 0) {
				int dx = 0;
				if(cursor.shiftRight() || cursor.shiftAllRight() || PD.controller.Nav_Right()) {
					cursor2Display.HighlightArrow(true);
					dx++;
				} else if(cursor.shiftLeft() || cursor.shiftAllLeft() || PD.controller.Nav_Left()) {
					cursor2Display.HighlightArrow(false);
					dx--;
				} else {
					if(!PD.usingMouse) { cursor2Display.ClearArrows(); }
				}
				if(dx != 0) { 
					UpdateOption(cy, dx);
					leftRight_delay = 15;
				}
			}
		}
		optionsScreen.UpdateCursorGraphic(cursor2Display, cursor2.getY());
	}
	private void UpdateAccessibilityOption(int y, int dx) {
		if(y < 2) { SignalFailure(); return; }
		if(y == 8) {
			if(!accessibilityScreen.ChangeColorblind(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 7) {
			if(!accessibilityScreen.ChangeHUDPlacement(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 6) {
			if(!accessibilityScreen.ChangeEmphasizeCursor(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 5) {
			if(!accessibilityScreen.ChangeTouchControls(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 4) {
			if(!accessibilityScreen.ChangeEasyMode(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 3) {
			if(!accessibilityScreen.ChangeKeyDelay(dx)) { SignalFailure(); } else { SignalMovement(); }
		} else if(y == 2) {
			if(!accessibilityScreen.ChangeScopophobia(dx)) { SignalFailure(); } else { SignalMovement(); }
		}
	}
	private void UpdateOption(int y, int dx) {
		if(dx == 0) { return; }
		if(y <= 3) { SignalFailure(); return; }
		if(y == 4) { 
			ChangeVolume(ref volume_voice, dx * 10);
			PD.sounds.SetVoiceVolume(volume_voice / 100.0f);
			optionsScreen.SetVVoText(volume_voice);
			optionsScreen.UpdateCursorPosition(y, volume_voice);
		} else if(y == 5) { 
			ChangeVolume(ref volume_sound, dx * 10); 
			PD.sounds.SetSoundVolume(volume_sound / 100.0f);
			optionsScreen.SetVSoText(volume_sound);
			optionsScreen.UpdateCursorPosition(y, volume_sound);
		} else if(y == 6) { 
			ChangeVolume(ref volume_music, dx * 10);
			PD.sounds.SetMusicVolume(volume_music / 100.0f);
			optionsScreen.SetVMuText(volume_music);
			optionsScreen.UpdateCursorPosition(y, volume_music);
		} else if(y == 7) {
			if(isFullScreen && dx < 0) { isFullScreen = false; SignalMovement(); }
			else if(!isFullScreen && dx > 0) { isFullScreen = true; SignalMovement(); }
			else { SignalFailure(); }
			optionsScreen.SetFlsText(isFullScreen);
			optionsScreen.UpdateCursorPosition(y, isFullScreen?1:0);
		} else {
			resIdx += dx;
			if(resIdx < 0) { SignalFailure(); resIdx = 0; } else if(resIdx >= resolutions.Length) { SignalFailure(); resIdx = resolutions.Length - 1; } else { SignalMovement(); }
			optionsScreen.SetResText(resolutions[resIdx]);
			optionsScreen.UpdateCursorPosition(y, resIdx);
		}
	}
	private void ChangeVolume(ref int volume, int delta) {
		volume += delta;
		if(volume > 100) { SignalFailure(); volume = 100; }
		else if(volume < 0) { SignalFailure(); volume = 0; }
		else { SignalMovement(); }
	}
	private void ApplySelected_Accessibility() {
		cursor.setY(1);
		menuPosition = 0;
		SignalSuccess();
		SaveAndApplyChanges_Accessibility();
	}
	private void ApplySelected_Options() {
		cursor.setY(3);
		menuPosition = 0;
		SignalSuccess();
		SaveAndApplyChanges_Options();
	}
	private void SaveAndApplyChanges_Accessibility() {
		PD.SetOption("colorblind", accessibilityScreen.val_colorblind);
		PD.SetOption("hudplacement", accessibilityScreen.val_hudplacement);
		PD.SetOption("emphasizecursor", accessibilityScreen.val_emphasizecursor);
		PD.SetOption("touchcontrols", accessibilityScreen.val_touchcontrols);
		PD.SetOption("easymode", accessibilityScreen.val_easymode);
		PD.SetOption("keydelay", accessibilityScreen.val_keydelay);
		PD.SetOption("scopophobia", accessibilityScreen.val_scopo);
		PD.KEY_DELAY = accessibilityScreen.val_keydelay;
		PD.SaveGeemu();
	}
	private void SaveAndApplyChanges_Options() {
		Vector2 resolution = resolutions[resIdx];
		PD.SetOption("width", Mathf.FloorToInt(resolution.x));
		PD.SetOption("height", Mathf.FloorToInt(resolution.y));
		PD.SetOption("fullscreen", isFullScreen?1:0);
		PD.SetOption("vol_m", volume_music);
		PD.SetOption("vol_s", volume_sound);
		PD.SetOption("vol_v", volume_voice);
		PD.SetRes();
		PD.SaveGeemu();
	}
	private void WipeSelected_Options() {
		if(confirmWipe) {
			SignalFailure();
			PD.WipeData();
			optionsScreen.SetClrText("done");
		} else {
			SignalMovement();
			confirmWipe = true;
			optionsScreen.SetClrText("really");
		}
	}
	private void UpdateOuter() {
		cursor2Display.SetVisibility(false);
		cursor2Display.ResetToStartPos();
		cursor.SetVisibility(true);
		cursor3.SetVisibility(false);
		cursor.DoUpdate();
		int oldIdx = selectedIdx;
		int cy = cursor.getY();
		selectedIdx = cy;
		if(selectedIdx != oldIdx) { sidepanels[oldIdx][1].SetActive(false); }
		sidepanels[selectedIdx][1].SetActive(true);
		sidepanels[selectedIdx][1].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, GetButtonOpacity());
		if(cursor.launchOrPause()) {
			menuPosition = cy;
			if(cy == 3) {
				cursor2.setY(8);
				cursor2Display.SetWidth(GetXmlFloat(top, "optionscalewidth"));
				SignalSuccess();
			} else if(cy == 2) {
				cursor2Display.SetWidth(GetXmlFloat(top, "optionscalewidth"));
				SignalSuccess();
			} else if(cy == 1) {
				cursor2.setY(8);
				cursor2Display.SetWidth(GetXmlFloat(top, "accessibilityscalewidth"));
				SignalSuccess();
			} else if(cy == 0) {
				SignalSuccess();
				PD.GoToMainMenu();
			}
		} else {
			UpdateCursorPosition(cy);
			if(cursor.back()) { SignalSuccess(); PD.GoToMainMenu(); }
		}
	}
	private void UpdateCursorPosition(int pos) {
		if(pos == prevPos) { return; }
		prevPos = pos;
		goBack.SetActive(false);
		funFactText.text = "";
		switch(pos) {
			case 3: 
				optionsScreen.CleanUp();
				controlsScreen.CleanUp();
				accessibilityScreen.CleanUp();
				optionsScreen.Setup(resolutions[resIdx], isFullScreen, volume_music, volume_sound, volume_voice, resIdx, resolutions.Length - 1);
				break;
			case 2:
				optionsScreen.CleanUp();
				controlsScreen.CleanUp();
				accessibilityScreen.CleanUp();
				controlsScreen.Setup();
				break;
			case 1:
				optionsScreen.CleanUp();
				controlsScreen.CleanUp();
				accessibilityScreen.CleanUp();
				accessibilityScreen.Setup();
				accessibilityScreen.Reset(PD.GetSaveData());
				break;
			case 0:
				optionsScreen.CleanUp();
				controlsScreen.CleanUp();
				accessibilityScreen.CleanUp();
				headerText.text = GetXmlValue(top, "returntomenu");
				funFactText.text = GetFunFactText();
				goBack.SetActive(true);
				break;
		}
	}
	protected override bool HandleMouse() {
		if(!PD.usingMouse) { return false; }
		if(menuPosition == 3) {
			Vector2 res = optionsScreen.GetColliderPosition(clicker);
			if(res.y < 0) { return false; }
			int i = (int) res.y;
			cursor2.setY(8 - i);
			int dx = 0;
			if(clicker.getPositionInGameObject(cursor2Display.leftArrow).z == 1.0f) {
				cursor2Display.HighlightArrow(false);
				dx--; 
			} else if(clicker.getPositionInGameObject(cursor2Display.rightArrow).z == 1.0f) {
				cursor2Display.HighlightArrow(true);
				dx++;
			} else {
				cursor2Display.ClearArrows();
			}
			if(!clicker.isDown()) { return false; }
			if(i < 6) {
				UpdateOption(8 - i, dx);
			} else if(i == 6) {
				WipeSelected_Options();
			} else if(i == 7) {
				ApplySelected_Options();
			} else {
				menuPosition = 0;
				SignalFailure();
				ResetOptions();
				cursor.setY(3);
			}
		} else if(menuPosition == 2) {
			if(clicker.getPositionInGameObject(side_back[0]).z > 0 || 
			   clicker.getPositionInGameObject(side_accessibility[0]).z > 0 ||
			   clicker.getPositionInGameObject(side_controls[0]).z > 0 ||
			   clicker.getPositionInGameObject(side_options[0]).z > 0) {
				if(clicker.isDown()) {
					LeaveControls();
					return false;
				}
			}
			if(controlsPos == 0) {
				Vector2 res = controlsScreen.GetTopColliderPositionX(clicker);
				if(res.y < 0) { return false; }
				cursor3.setX(res.x < 0 ? 0 : 1);
				if(clicker.isDown()) { TransitionIntoControlsInner(); }
			} else if(controlsPos == 1) {
				Vector2 res = controlsScreen.GetTopColliderPositionY(clicker);
				if(res.x < 0) { return false; }
				int newval = 11;
				if(res.y < -1.05f) { newval = 0; }
				else if(res.y < -0.87f) { newval = 1; }
				else if(res.y < -0.65f) { newval = 2; }
				else if(res.y < -0.48f) { newval = 3; }
				else if(res.y < -0.28f) { newval = 4; }
				else if(res.y < -0.08f) { newval = 5; }
				else if(res.y < 0.12f) { newval = 6; }
				else if(res.y < 0.32f) { newval = 7; }
				else if(res.y < 0.52f) { newval = 8; }
				else if(res.y < 0.72f) { newval = 9; }
				else if(res.y < 0.92f) { newval = 10; }
				cursor2.setY(newval);
				if(clicker.isDown()) { SelectControlChange(); }
			} else if(controlsPos == 2) {
				if(controlsScreen.ClickingCancelButton(clicker)) {
					SignalFailure();
					controlsPos = 1;
					controlsScreen.UndoQuestion(cursor2.getY(), cursor3.getX() == 0);
				}
			}
		} else if(menuPosition == 1) {
			Vector2 res = accessibilityScreen.GetColliderPosition(clicker);
			if(res.y < 0) { return false; }
			int i = (int) res.y;
			int cy = 8 - i;
			cursor2.setY(cy);
			int dx = 0;
			if(clicker.getPositionInGameObject(cursor2Display.leftArrow).z == 1.0f) {
				cursor2Display.HighlightArrow(false);
				dx--; 
			} else if(clicker.getPositionInGameObject(cursor2Display.rightArrow).z == 1.0f) {
				cursor2Display.HighlightArrow(true);
				dx++;
			} else {
				cursor2Display.ClearArrows();
			}
			if(!clicker.isDown()) { return false; }
			if(cy == 1) { 
				ApplySelected_Accessibility();
			} else if(cy == 0) {
				menuPosition = 0;
				SignalFailure();
				accessibilityScreen.Reset(PD.GetSaveData());
				cursor.setY(1);
			} else {
				UpdateAccessibilityOption(cy, dx);
			}
		} else {
			if(clicker.getPositionInGameObject(side_back[0]).z > 0) {
				cursor.setY(0);
				if(clicker.isDown()) { SignalSuccess(); PD.GoToMainMenu(); }
			} else if(clicker.getPositionInGameObject(side_accessibility[0]).z > 0) {
				cursor.setY(1);
				if(clicker.isDown()) {
					SignalSuccess();
					cursor2.setY(8);
					menuPosition = 1;
					cursor2Display.SetWidth(GetXmlFloat(top, "accessibilityscalewidth"));
				}
			} else if(clicker.getPositionInGameObject(side_controls[0]).z > 0) {
				cursor.setY(2);
				if(clicker.isDown()) {
					SignalSuccess();
					menuPosition = 2;
				}
			} else if(clicker.getPositionInGameObject(side_options[0]).z > 0) {
				cursor.setY(3);
				if(clicker.isDown()) {
					cursor2.setY(8);
					menuPosition = 3;
					cursor2Display.SetWidth(GetXmlFloat(top, "optionscalewidth"));
					SignalSuccess();
				}
			}
			return false;
		}
		return false;
	}
	private void UpdateOuterMouse(bool isClicking, bool ignoreC2Resize = false) {
		cursor.SetVisibility(true);
		sidepanels[selectedIdx][0].GetComponent<SpriteRenderer>().sprite = leftButton[0];
		int cy = cursor.getY();
		selectedIdx = cy;
		sidepanels[selectedIdx][0].GetComponent<SpriteRenderer>().sprite = leftButton[1];
		if(isClicking) {
			UpdateCursorPosition(cy);
			menuPosition = cy;
			if(cy == 3) {
				cursor2.setY(8);
				cursor2Display.SetWidth(GetXmlFloat(top, "optionscalewidth"));
				SignalSuccess();
			} else if(cy == 2) {
				if(!ignoreC2Resize) { cursor2Display.SetWidth(GetXmlFloat(top, "optionscalewidth")); }
				SignalSuccess();
			} else if(cy == 1) {
				cursor2.setY(8);
				cursor2Display.SetWidth(GetXmlFloat(top, "accessibilityscalewidth"));
				SignalSuccess();
			} else if(cy == 0) {
				SignalSuccess();
				PD.GoToMainMenu();
			}
		}
	}
}