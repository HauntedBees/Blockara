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
using DG.Tweening;
public class InGameHUD:All {
	public bool lose;
	protected bool pausing, gameEnd;
	protected float lastCheck;
	protected GameObject hudBox, p1Next_box;
	protected TextMesh timerText, p1ScoreText, p2ScoreText;
	protected int p1Score_val, p2Score_val, minutes, seconds;
	protected GameObject tootoriel_candyslaws;
	public TutorialHelper tutorialAssist;
	public PauseMenu pauseMenu;
	private GameObject demoText;
	private int demoFlicker;
	public int pausePresser;
	public void Setup(int players, int additionalInfo = 0) {
		GetPersistData();
		lastCheck = 0.0f; minutes = 0; seconds = 0; p1Score_val = 0;
		pausing = false; gameEnd = false; lose = false;

		XmlNode top = GetXMLHead();
		Vector3 timerPos, p1ScorePos, p1NextPos;
		if(players == 1) {
			float x = PD.IsLeftAlignedHUD() ? -0.9f : 0.9f;
			timerPos = new Vector3(x, 1.1f); 
			p1ScorePos = new Vector3(x - 0.3f, 1.4f); 
			p1NextPos = new Vector3(x, -1.1f);
		} else {
			timerPos = new Vector3(0.0f, 1.0f); 
			p1ScorePos = new Vector3(-0.3f, 1.3f); 
			p1NextPos = new Vector3(0.0f, -1.1f);
		}
		Sprite infoBoxSprite = Resources.Load<Sprite> (SpritePaths.InfoBox);

		hudBox = GetGameObject(timerPos, "Time Box", infoBoxSprite, false, "HUD");

		FontData f = new FontData(TextAnchor.MiddleLeft, TextAlignment.Left, 0.03f), 
				  f2 = new FontData(TextAnchor.MiddleRight, TextAlignment.Right, 0.045f);

		GetMeshText(timerPos + new Vector3(-0.3f, 0.67f), GetXmlValue(top, "time"), f);
		timerText = GetMeshText(timerPos + new Vector3(0.3f, 0.5f), "0:00", f2);

		GetMeshText(p1ScorePos, GetXmlValue(top, players==1?"score":"p1score"), f);
		p1ScoreText = GetMeshText(timerPos + new Vector3(0.3f, 0.13f), PD.runningScore.ToString(), f2);
		UpdateTextValueAndSize(p1ScoreText, PD.runningScore);

		p1Next_box = GetGameObject(p1NextPos, "Next 1 Box", Resources.Load<Sprite> (SpritePaths.InfoBox), false, "HUD");
		GetMeshText(p1NextPos + new Vector3(-0.3f, 0.67f), GetXmlValue(top, "next"), f);

		Vector3 offset = new Vector3(players==2?-0.15f:-0.45f, -0.05f);
		AdditionalSetup(infoBoxSprite, players, offset, top, additionalInfo);
	}
	virtual protected void AdditionalSetup(Sprite tile, int players, Vector3 offset, XmlNode top, int additionalInfo) {
		AddDamageReferenceKey(players);
		if(PD.isDemo) { demoText = GetGameObject(Vector3.zero, "DEMO", Resources.Load<Sprite>(SpritePaths.DemoText), false, "Pause HUD Cursor"); demoFlicker = 60; }
		if(players == 2) { Setup2Player(tile, top); }
		else if(additionalInfo == 1) { SetupTutorial(offset); }
	}
	private void AddDamageReferenceKey(int numPlayers) {
		Vector2 pos = (numPlayers == 2)?new Vector2(0.0f, 0.35f):new Vector2(PD.IsLeftAlignedHUD()?-0.9f:0.9f, 0.7f);
		GameObject helper = GetGameObject(pos, "Damage Reference", Resources.Load<Sprite>(SpritePaths.GuideCircle + (PD.IsColorBlind()?SpritePaths.ColorblindSuffix:"")), false, "Reference");
		if(numPlayers == 2) { helper.transform.localScale = new Vector2(0.75f, 0.75f); }
	}
	private void SetupTutorial(Vector3 offset) {
		Vector3 tootroielposietion = new Vector3(2.4f, 0.15f);
		tootoriel_candyslaws = GetGameObject(tootroielposietion, "Tutorial Box", Resources.Load<Sprite>(SpritePaths.TutorialBox), false, "HUD");
		TextMesh t = GetMeshText(tootroielposietion + new Vector3(0.0f, -0.1f), "", new FontData(TextAnchor.UpperCenter, TextAlignment.Center, 0.025f));
		GameObject g = new GameObject("tutorialHandler");
		tutorialAssist = g.AddComponent<TutorialHelper>();
		tutorialAssist.Init(t, tootoriel_candyslaws);
	}
	private void Setup2Player(Sprite infoBoxSprite, XmlNode top) {
		p2ScoreText = GetMeshText(new Vector3(0.3f, 0.75f), "0", new FontData(TextAnchor.MiddleRight, TextAlignment.Right, 0.045f));
		p2Score_val = 0;
		GetMeshText(new Vector3(-0.3f, 0.95f), GetXmlValue(top, "p2score"), new FontData(TextAnchor.MiddleLeft, TextAlignment.Left, 0.03f));
	}
	
	public virtual void DoUpdate(bool paused, int p1val, int p2val, bool hiddenPause = false) {
		if(gameEnd) { return; }
		HandlePause(paused);
		UpdateTimer();
		if(p1Score_val != p1val) { p1Score_val = p1val; UpdateTextValueAndSize(p1ScoreText, p1Score_val); }
		if(p2ScoreText != null && p2Score_val != p2val) { p2Score_val = p2val; UpdateTextValueAndSize(p2ScoreText, p2Score_val); }
		if(demoText != null && --demoFlicker < 0) { demoFlicker = 30; demoText.SetActive(!demoText.activeSelf); }
	}
	protected void HandlePause(bool newPauseState, bool hidden = false) {
		if(hidden) { return; }
		if(newPauseState && !pausing) {
			GameObject g = new GameObject("pauseHandler");
			pauseMenu = g.AddComponent<PauseMenu>();
			pauseMenu.Initialize(pausePresser);
		} else if(!newPauseState && pausing) {
			pauseMenu.CleanUp();
			Destroy(pauseMenu.gameObject);
			pausePresser = 0;
		}
		pausing = newPauseState;
	}
	protected void UpdateTimer() {
		if(pausing || Time.time < lastCheck + 1.0f) { return; }
		lastCheck = Time.time;
		if(++seconds >= 60) { seconds = 0; minutes++; }
		string res = minutes + (seconds<10?":0":":") + seconds;
		if(minutes == 100 && seconds == 0) {
			Vector3 t = timerText.transform.localScale;
			t.x *= 0.8f;
			timerText.transform.localScale = t;
		} else if(minutes == 1000 && seconds == 0) {
			Vector3 t = timerText.transform.localScale;
			t.x *= 0.7f;
			timerText.transform.localScale = t;
		}
		if(minutes >= 1000) { res = "go outside"; }
		timerText.text = res;
	}
	protected void UpdateTextValueAndSize(TextMesh tmesh, int val) {
		tmesh.text = val.ToString();
		float scalex = 0.045f;
		if(val > 100000000) { scalex = 0.02f; }
		else if(val > 1000000) { scalex = 0.025f; }
		else if(val > 100000) { scalex = 0.035f; }
		else if(val > 10000) { scalex = 0.04f; }
		Vector3 t = tmesh.transform.localScale;
		t.x = scalex;
		tmesh.transform.localScale = t;
	}

	public void ShowVictoryText(int winPlayer, Vector3 p1Offset, Vector3 p2Offset, bool showP2) {
		if(gameEnd) { return; }
		pausing = true; gameEnd = true;

		Sprite[] particleSprites = Resources.LoadAll<Sprite>(SpritePaths.Launch_Particles);
		int d = Random.Range(0, 3), d2 = d + (winPlayer == 1 ? 1 : -1);
		if(d2 == 3) { d2 = 0; } else if(d2 < 0) { d2 = 2; }
		Color full = new Color(1.0f, 1.0f, 1.0f, 1.0f), empty = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		for(int i = 0; i < 50; i++) {
			float y = -0.1f + Random.Range(0.0f, 0.2f), y2 = -0.1f + Random.Range(0.0f, 0.2f);

			GameObject particle = GetGameObject_Tile(new Vector3(-4.0f + 8.0f * i/100, y), "particle", particleSprites[d], "Cover HUD Dialog Box");
			SpriteRenderer particle_sr = particle.GetComponent<SpriteRenderer>();
			particle_sr.color = empty;
			Sequence s = DOTween.Sequence();
			s.Append(particle_sr.DOColor(full, 0.2f).SetDelay(i / 100.0f));
			s.Join(particle.transform.DOMoveY(y * 1.5f, 0.2f));
			s.Append(particle_sr.DOColor(empty, 0.5f));
			s.Join(particle.transform.DOMoveY(y * 2.5f, 0.5f));

			GameObject particle2 = GetGameObject_Tile(new Vector3(4.0f - 8.0f * i/100, y), "particle2", particleSprites[d2], "Cover HUD Dialog Box");
			SpriteRenderer particle2_sr = particle2.GetComponent<SpriteRenderer>();
			particle2_sr.color = empty;
			Sequence s2 = DOTween.Sequence();
			s2.Append(particle2_sr.DOColor(full, 0.2f).SetDelay(i / 100.0f));
			s2.Join(particle2.transform.DOMoveY(y2 * 1.5f, 0.2f));
			s2.Append(particle2_sr.DOColor(empty, 0.5f));
			s2.Join(particle2.transform.DOMoveY(y2 * 2.5f, 0.5f));
		}
		for(int i = 50; i < 100; i++) {
			float y = -0.1f + Random.Range(0.0f, 0.2f);
			GameObject particle = GetGameObject_Tile(new Vector3(winPlayer == 1 ? (-4.0f + 8.0f * i/100) : (4.0f - 8.0f * i/100), y), "particle", particleSprites[winPlayer==1?d:d2], "Cover HUD Dialog Box");
			SpriteRenderer particle_sr = particle.GetComponent<SpriteRenderer>();
			particle_sr.color = empty;
			Sequence s = DOTween.Sequence();
			s.Append(particle_sr.DOColor(full, 0.2f).SetDelay(i / 100.0f));
			s.Join(particle.transform.DOMoveY(y * 1.5f, 0.2f));
			s.Append(particle_sr.DOColor(empty, 0.5f));
			s.Join(particle.transform.DOMoveY(y * 2.5f, 0.5f));
		}
		Sprite[] textSprites = Resources.LoadAll<Sprite>(SpritePaths.Texts);
		GameObject text1 = GetGameObject(p1Offset, "P1 End Condition", (winPlayer == 1 ? textSprites[0]:textSprites[1]), false, "Cover HUD Dialog Text");
		SpriteRenderer text1_sr = text1.GetComponent<SpriteRenderer>();
		text1_sr.color = empty;
		text1_sr.DOColor(full, 0.6f).SetDelay(showP2 ? 0.2f : 0.4f);
		if(showP2) {
			GameObject text2 = GetGameObject(p2Offset, "P2 End Condition", (winPlayer == 2 ? textSprites[0]:textSprites[1]), false, "Cover HUD Dialog Text");
			SpriteRenderer text2_sr = text2.GetComponent<SpriteRenderer>();
			text2_sr.color = empty;
			text2_sr.DOColor(full, 0.6f).SetDelay(0.2f);
			if(tutorialAssist == null) { PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + (Random.value>0.5f?"045":"046"), 0); }
		} else {
			if(tutorialAssist == null) { PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + (winPlayer == 1 ? "037" : "038"), 0); }
		}
	}
	public int GetTimeInSeconds() { return minutes * 60 + seconds; }
	public void SetTimeWithSeconds(int time) { 
		minutes = Mathf.FloorToInt(time / 60.0f);
		seconds = time % 60;
		string res = minutes + (seconds<10?":0":":") + seconds;
		if(minutes >= 1000) { res = "go outside"; }
		timerText.text = res;
	}
}