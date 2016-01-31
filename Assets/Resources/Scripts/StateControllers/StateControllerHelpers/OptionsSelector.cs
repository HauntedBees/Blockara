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
public class OptionsSelector:All {
	public GameObject leftArrow, rightArrow;
	private Sprite spriteActive, spriteInactive;
	private float xPos, yBottom, width, dy;
	private int yIdx;
	public void Setup(float x, float y, float delta, bool big = false) {
		GetPersistData();
		spriteActive = Resources.LoadAll<Sprite>(SpritePaths.RightArrows)[big?1:3];
		spriteInactive = Resources.LoadAll<Sprite>(SpritePaths.RightArrows)[big?0:2];
		xPos = x;
		yBottom = y;
		dy = delta;
		yIdx = -1;
		leftArrow = GetGameObject(new Vector3(xPos, yBottom + yIdx * dy), "leftArrow", spriteInactive, true, "HUD");
		leftArrow.transform.localScale = new Vector3(-1.0f, 1.0f);
		rightArrow = GetGameObject(new Vector3(xPos, yBottom + yIdx * dy), "rightArrow", spriteInactive, true, "HUD");
	}
	public void ChangeParams(float x, float y, float d) { xPos = x; yBottom = y; dy = d; }
	public void ResetToStartPos() { yIdx = -1; }
	public void SetWidth(float w) { width = w; }
	public void UpdatePosition(int newYIdx, bool force = false) {
		if(yIdx == newYIdx && !force) { return; }
		yIdx = newYIdx;
		leftArrow.transform.position = new Vector3(xPos, yBottom + yIdx * dy);
		rightArrow.transform.position = new Vector3(xPos + width, yBottom + yIdx * dy);
		ClearArrows();
	}
	public void ToggleArrowVisibility(bool show) {
		leftArrow.SetActive(show);
		rightArrow.SetActive(show);
	}
	public void HideAnArrowIfAtCorner(int val, int min, int max) {
		leftArrow.GetComponent<SpriteRenderer>().color = (val != min) ? Color.white : Color.grey;
		rightArrow.GetComponent<SpriteRenderer>().color = (val != max) ? Color.white : Color.grey;
	}
	public void HighlightArrow(bool right) {
		if(right) {
			rightArrow.GetComponent<SpriteRenderer>().sprite = spriteActive;
			leftArrow.GetComponent<SpriteRenderer>().sprite = spriteInactive;
		} else {
			leftArrow.GetComponent<SpriteRenderer>().sprite = spriteActive;
			rightArrow.GetComponent<SpriteRenderer>().sprite = spriteInactive;
		}
	}
	public void ClearArrows() {
		leftArrow.GetComponent<SpriteRenderer>().sprite = spriteInactive;
		rightArrow.GetComponent<SpriteRenderer>().sprite = spriteInactive;
	}
	public void SetVisibility(bool vis) {
		leftArrow.SetActive(vis);
		rightArrow.SetActive(vis);
	}
	public void SetRightVisibility() {
		leftArrow.SetActive(false);
		rightArrow.SetActive(true);
	}
}