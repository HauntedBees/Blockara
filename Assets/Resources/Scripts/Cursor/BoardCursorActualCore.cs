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
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
public class BoardCursorActualCore:BoardCursorCore {
	#region "Members"
	protected GameObject white;
	protected GameObject[] whiteDepth;
	private Sequence winningTweenJustSeventeenYoureTheWinningTween, damageTween;
	protected int depth, penetrateDepth, maxWhiteDepth;
	protected float moveDelay;
	private GameTouchHandler touchHandler;
	public bool canKill, penetr, hideWhite, frozen;
	#endregion
	#region "Setup"
	virtual public void Setup(TweenHandler t, int maxdepth = 6, bool show = true) {
		isShown = show;
		th = t;
		frozen = false;
		InitializeMembers();
		
		moveDelay = 0;
		depth = 0;
		canKill = false;
		maxWhiteDepth = maxdepth;
		hideWhite = false;

		InitializeWhite();
		UpdateCursorPos(true);
		UpdateWhite(true);
		FinishUpdate();
	}
	public void AttachTouchHandler(GameTouchHandler gth) { touchHandler = gth; }
	private void InitializeWhite() {
		if(!isShown) { return; }
		white = GetGameObject(Vector3.zero, "White", Resources.Load<Sprite>(SpritePaths.White), false, "HUD");
		white.transform.localScale = new Vector2(1.0f, 1.6f + ((8 - maxWhiteDepth) * 2.0f));
		whiteDepth = new GameObject[maxWhiteDepth + 1];
		for(int i = 0; i < (maxWhiteDepth + 1); i++) {
			whiteDepth[i] = GetGameObject(Vector3.zero, "White" + i, Resources.Load<Sprite>(SpritePaths.WhiteSingle), false, "HUD");
			whiteDepth[i].SetActive(false);
		}
	}
	#endregion
	#region "Updating"
	virtual public void FreezyPop(int i) { frozen = true; moveDelay = i; if(touchHandler != null) { touchHandler.SetMoveDelay(i); } }
	virtual protected void MainUpdate() {
		if(moveDelay <= 0) { frozen = false; } 
		cursor.GetComponent<SpriteRenderer>().sprite = sheet[frozen?2:(penetr?1:0)];
		UpdateCursorPos();
		UpdateWhite();
		FinishUpdate();
	}
	private void UpdateWhite(bool skipTween = false) {
		if(!isShown) { return; }
		for(int i = 0; i < maxWhiteDepth; i++) { whiteDepth[i].SetActive(!hideWhite && i > depth); }
		if(penetrateDepth > 0) {
			if(damageTween == null || damageTween.IsComplete() || !damageTween.IsActive()) {
				damageTween = DOTween.Sequence();
				damageTween.Append(whiteDepth[depth + 1].renderer.material.DOColor(Color.black, 0.2f));
				damageTween.Append(whiteDepth[depth + 1].renderer.material.DOColor(Color.white, 0.2f));
				for(int i = depth + 2; i < (depth + 1 + penetrateDepth); i++) {
					Sequence s = DOTween.Sequence();
					s.Append(whiteDepth[i].renderer.material.DOColor(Color.black, 0.2f));
					s.Append(whiteDepth[i].renderer.material.DOColor(Color.white, 0.2f));
				}
			}
		}
		whiteDepth[maxWhiteDepth].SetActive(!hideWhite && depth < 0 && canKill);
		white.SetActive(!hideWhite);
		if(canKill) {
			if(winningTweenJustSeventeenYoureTheWinningTween == null || winningTweenJustSeventeenYoureTheWinningTween.IsComplete() || !winningTweenJustSeventeenYoureTheWinningTween.IsActive()) {
				winningTweenJustSeventeenYoureTheWinningTween = DOTween.Sequence();
				winningTweenJustSeventeenYoureTheWinningTween.Append(white.renderer.material.DOColor(Color.black, 0.2f));
				winningTweenJustSeventeenYoureTheWinningTween.Append(white.renderer.material.DOColor(Color.white, 0.2f));
				for(int i = 0; i < maxWhiteDepth; i++) {
					Sequence s = DOTween.Sequence();
					s.Append(whiteDepth[i].renderer.material.DOColor(Color.black, 0.2f));
					s.Append(whiteDepth[i].renderer.material.DOColor(Color.yellow, 0.2f));
				}
			}
		}

		if(prevx == x) { return; }
		Vector3 pos2 =  new Vector3((xOffset + x) * Consts.TILE_SIZE, 0.0f);
		if(skipTween) {
			white.transform.position = pos2;
		} else {
			th.DoTween(white, pos2);
		}
		for(int i = (maxWhiteDepth - 1); i >= 0; i--) {
			if(skipTween) {
				whiteDepth[i].transform.position = new Vector3((xOffset + x) * Consts.TILE_SIZE, 0.96f - 1.0f * Consts.TILE_SIZE + (Consts.TILE_SIZE * (5 - i)));
			} else {
				th.DoTween(whiteDepth[i], new Vector3((xOffset + x) * Consts.TILE_SIZE, 0.96f - 1.0f * Consts.TILE_SIZE + (Consts.TILE_SIZE * (5 - i))));
			}
		}
		if(skipTween) {
			whiteDepth[maxWhiteDepth].transform.position = new Vector3((xOffset + x) * Consts.TILE_SIZE, 2.2f);
		} else {
			th.DoTween(whiteDepth[maxWhiteDepth], new Vector3((xOffset + x) * Consts.TILE_SIZE, 2.2f));
		}
	}
	#endregion
	#region "Control Execution"
	public void SetDepthAndKillForDisplay(int d, bool p, int dp, bool k) { depth = d; penetr = p; penetrateDepth = dp; canKill = k; }
	public void shiftX(int x) { this.x += x; }
	#endregion
}