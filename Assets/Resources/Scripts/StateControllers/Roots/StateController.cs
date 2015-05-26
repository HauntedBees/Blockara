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
public class StateController:ObjCore {
	protected MouseCore clicker;
	protected Sprite[] backSprites;
	protected GameObject backButton;
	protected List<GameObject> mouseObjects;
	protected bool isTransitioning;
	protected virtual void StateControllerInit(bool showBack = true) {
		GetPersistData();
		isTransitioning = false;
		th = ScriptableObject.CreateInstance<TweenHandler>();
		clicker = new ClickerMouse();
		mouseObjects = new List<GameObject>();
		if(showBack) {
			backSprites = Resources.LoadAll<Sprite>(SpritePaths.BackButtons);
			backButton = GetGameObject(new Vector3(-3.35f, 1.8f), "BACK", backSprites[0], true, "Pause HUD Cursor");
			mouseObjects.Add(backButton);
		}
	}
	protected virtual bool HandleMouse() { return false; }
	protected void UpdateMouseInput() {
		if(IsMouseBeingMoved()) { ToggleMouseOptionVisibility(true); }
		else if(!PD.usingMouse) { ToggleMouseOptionVisibility(false); }
	}
	private bool IsMouseBeingMoved() { return clicker.hasMoved() || clicker.isDown(); }
	private void ToggleMouseOptionVisibility(bool shown) {
		if(Screen.showCursor == shown) { return; }
		Screen.showCursor = shown;
		PD.usingMouse = shown;
		for(int i = 0; i < mouseObjects.Count; i++) { mouseObjects[i].SetActive(shown); }
	}
	protected bool isClickingBack() {
		Vector3 v = clicker.getPositionInGameObject(backButton);
		if(v.z == 0.0f) { backButton.GetComponent<SpriteRenderer>().sprite = backSprites[0]; return false; }
		backButton.GetComponent<SpriteRenderer>().sprite = backSprites[1];
		return clicker.isDown();
	}
}