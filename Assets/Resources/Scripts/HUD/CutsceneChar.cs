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
public class CutsceneChar {
	public enum Reaction { firstStrike, hit, hit2, hit3, block, miss2, combo2, combo3, win }
	public enum SpeechType { doDamage, nonDamagePositive, takeDamage, nonDamageNegative, lose, win }
	private string _name, _path;
	private Vector3 sheetScale;
	private GameObject _obj;
	private Sprite[] _sheet;
	private int _player;
	private PersistData _PD;
	public int loseFrame;
	public bool bobbing, hidden;
	private bool inGame; 
	private float localy;
	public CutsceneChar(string n, GameObject o, Sprite[] s, int p, PersistData PD, bool gameChar = false) {
		if(n == "Doug") { PD.AlterSound(); }
		_name = PD.GetPlayerDisplayName(n);
		_path = n;
		_obj = o;
		_sheet = s;
		_player = p;
		_PD = PD;
		inGame = gameChar;
		sheetScale = new Vector3(_obj.transform.localScale.x, _obj.transform.localScale.y);
		loseFrame = 5;
		hidden = false;
		bobbing = false;
		localy = o.transform.localPosition.y;
	}
	public string GetPath(bool isBalloonCutscene = false) { return isBalloonCutscene?"White":_path; }
	public string GetName() { return _name=="MasterAlchemist"?"The Master":_name; }
	public void Hide() { _obj.transform.localPosition = new Vector3(-100f, -100f); hidden = true; }
	public CutsceneChar SetSortingLayer(string s) { _obj.GetComponent<SpriteRenderer>().sortingLayerName = s; return this; }

	private float tweenHeight = 0.2f, tweenLength = 0.3f;
	public void Bob() {
		if(!bobbing) { _obj.transform.DOComplete(); return; }
		Sequence bobSequence = DOTween.Sequence();
		bobSequence.Append(_obj.transform.DOLocalMoveY(localy - tweenHeight, 0.01f).SetDelay(tweenLength));
		bobSequence.Append(_obj.transform.DOLocalMoveY(localy, 0.01f).SetDelay(tweenLength));
		bobSequence.OnComplete(Bob);
	}
	private float inGameTweenSpeed = 0.7f;
	private void LowerTweenSpeed() { inGameTweenSpeed = Mathf.Max(inGameTweenSpeed - 0.025f, 0.2f); }
	public void InGameBob() {
		if(!bobbing) { return; }
		Sequence bobSequence = DOTween.Sequence();
		bobSequence.Append(_obj.transform.DOLocalMoveY(localy - 0.05f, 0.01f).SetDelay(inGameTweenSpeed));
		bobSequence.Append(_obj.transform.DOLocalMoveY(localy, 0.01f).SetDelay(inGameTweenSpeed));
		bobSequence.OnComplete(InGameBob);
	}
	public void FlickerColor(int colortype) {
		Color c = colortype==0?Color.blue:(colortype==1?Color.red:Color.green);
		Sequence s = DOTween.Sequence();
		s.Append(_obj.renderer.material.DOColor(c, 0.15f));
		s.Append(_obj.renderer.material.DOColor(Color.white, 0.15f));
	}
	public CutsceneChar SetSprite(int idx) {
		bobbing = false;
		if(_obj.GetComponent<SpriteRenderer>().sprite == null) { ChangeSprite(idx); return this; }
		if(_obj.GetComponent<SpriteRenderer>().sprite == _sheet[idx]) { return this; }
		Vector3 newScale = new Vector3(sheetScale.x * 0.8f, sheetScale.y * 1.2f);
		Sequence s = DOTween.Sequence();
		s.Append(_obj.transform.DOScale(newScale, 0.05f).OnComplete(()=>ChangeSprite(idx)));
		s.Append(_obj.transform.DOScale(sheetScale, 0.05f));
		s.Append(_obj.transform.DOScale(sheetScale, Random.Range(0.1f, 0.25f)));
		s.OnComplete(ReturnBob);
		return this;
	}
	private void ReturnBob() { bobbing = true; if(inGame) { InGameBob(); } else { Bob(); } }
	private void ChangeSprite(int idx) { _obj.GetComponent<SpriteRenderer>().sprite = _sheet[idx]; }

	public CutsceneChar SetScale(float f) {
		_obj.transform.localScale = _obj.transform.localScale * f;
		sheetScale = new Vector3(_obj.transform.localScale.x, _obj.transform.localScale.y);
		return this;
	}
	public CutsceneChar SetTint(Color c) { _obj.renderer.material.SetColor("_Color", c); return this; }
	virtual public void DoReaction(Reaction r, bool sender) {
		switch(r) {
		case Reaction.firstStrike:
			if(sender) { SetSprite(1); } else { SetSprite(7); LowerTweenSpeed(); }
			break;
		case Reaction.block:
			if(sender) { SetSprite(4); } else { SetSprite(1); }
			break;
		case Reaction.combo2:
			if(sender) { SetSprite(2); } else { SetSprite(7); LowerTweenSpeed(); }
			break;
		case Reaction.combo3:
			if(sender) { SetSprite(2); } else { SetSprite(6); LowerTweenSpeed(); }
			break;
		case Reaction.hit: 
			if(sender) { SetSprite(1); } else { SetSprite(4); LowerTweenSpeed(); }
			break;
		case Reaction.hit2:
			if(sender) { SetSprite(2); } else { SetSprite(5); LowerTweenSpeed(); }
			break;
		case Reaction.hit3:
			if(sender) { SetSprite(2); } else { SetSprite(6); LowerTweenSpeed(); }
			break;
		case Reaction.miss2:
			if(sender) { SetSprite(3); } else { SetSprite(1); } 
			break;
		case Reaction.win: 
			if(sender) { SetSprite(2); } else { SetSprite(loseFrame); }
			break;
		}
	}
	public void SayThingFromReaction(SpeechType type) {
		if(hidden) { return; }
		if(_player == 1 && _PD.gameType == PersistData.GT.Training) { return; }
		int startidx, endidx;
		string storedPath = _path;
		if(_path == "Everyone") {
			System.Array values = System.Enum.GetValues(typeof(PersistData.C));
			storedPath = _PD.GetPlayerSpritePath((PersistData.C) values.GetValue(Random.Range(0, values.Length)));
		}
		if(storedPath == "MasterAlchemist") {
			startidx = 0; endidx = 17;
		} else if(storedPath == "White" || _path == "September") {
			switch(type) {
				case SpeechType.doDamage: startidx = 1; endidx = 9; break;
				case SpeechType.nonDamagePositive: startidx = 3; endidx = 9; break;
				case SpeechType.takeDamage: startidx = 9; endidx = 17; break;
				case SpeechType.nonDamageNegative: startidx = 12; endidx = 17; break;
				case SpeechType.win: startidx = 30; endidx = 31; break;
				case SpeechType.lose: startidx = 31; endidx = 32; break;
				default: startidx = 1; endidx = 2; break;
			}
		} else {
			switch(type) {
				case SpeechType.doDamage: startidx = 4; endidx = 20; break;
				case SpeechType.nonDamagePositive: startidx = 9; endidx = 20; break;
				case SpeechType.takeDamage: startidx = 20; endidx = 36; break;
				case SpeechType.nonDamageNegative: startidx = 25; endidx = 36; break;
				case SpeechType.win: startidx = 70; endidx = 76; break;
				case SpeechType.lose: startidx = 76; endidx = 82; break;
				default: startidx = 4; endidx = 5; break;
			}
		}
		int idx = Random.Range(startidx, endidx);
		SayThingFromXML(idx.ToString("D3"), false, storedPath);
	}
	public void SayThingFromXML(string path, bool forcePlayer1 = false, string newPath = "") {
		string sayPath = newPath;
		if(string.IsNullOrEmpty(sayPath)) { sayPath = _path; }
		if(sayPath == "Doug") { sayPath = new string[]{"George", "Milo", "Devin", "Andrew", "Lars", "Alice/Ana", "MasterAlchemist"}[Random.Range(0, 7)]; }
		if(sayPath == "MasterAlchemist") { path = Random.Range(0, 17).ToString("D3"); }
		if(sayPath == "September" && path == "017" && Random.value < 0.05f) { path = "018"; }
		_PD.sounds.SetVoiceAndPlay(SoundPaths.VoicePath + sayPath + "/" + path, forcePlayer1?0:_player);
	}
}