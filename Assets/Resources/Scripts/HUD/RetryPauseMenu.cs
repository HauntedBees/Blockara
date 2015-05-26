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
public class RetryPauseMenu:MenuController {
	public int state;
	protected GameObject menu;
	protected GameObject[] menuButtons;
	protected TextMesh[] textMeshes;
	protected Sprite[] buttonSheet;
	public void Update() {
		menuButtons[selectedIdx].GetComponent<SpriteRenderer>().sprite = buttonSheet[0];
		HandleMouse();
		if(cursor.launch() || (this is RetryMenu && cursor.pause())) { state = cursor.getY() + 1; }
		cursor.DoUpdate();
		selectedIdx = cursor.boardheight - cursor.getY() - 1;
		menuButtons[selectedIdx].GetComponent<SpriteRenderer>().sprite = buttonSheet[1];
	}
	protected void InitButtonSprites() { buttonSheet = Resources.LoadAll<Sprite>(SpritePaths.LongButtons); }
	protected void AddButton(int idx, float x, float y, string text, FontData f) {
		menuButtons[idx - 1] = GetGameObject(new Vector3(x, y - 0.1f), text, buttonSheet[0], true, "Pause HUD Buttons");
		textMeshes[idx] = GetMeshText(new Vector3(x, y), text, f);
	}
	public void CleanUp() {
		for(int i = 0; i < textMeshes.Length; i++) { Destroy(textMeshes[i].gameObject); }
		for(int i = 0; i < menuButtons.Length; i++) { Destroy(menuButtons[i]); }
		cursor.Wipe();
		Destroy(menu);
		menu = null; clicker = null; cursor = null;
	}
}