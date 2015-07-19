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
public class MatchOverController:CharDisplayController {
	private ParticleSystem particles;
	private ParticleSystem.Particle[] pars;
	private CutsceneChar winner, loser;
	private int applauseTimer;
	public void Start() {
		StateControllerInit(false);
		GameObject g = GameObject.Find("Confetti") as GameObject;
		particles = g.GetComponent<ParticleSystem>();
		pars = new ParticleSystem.Particle[particles.maxParticles];

		int p1Wins = 0, p2Wins = 0;
		for(int i = 0; i < PD.playerOneWonRound.Count; i++) { if(PD.playerOneWonRound[i]) { p1Wins++; } else { p2Wins++; } }
		PersistData.C winChar = p1Wins>p2Wins?PD.p1Char:PD.p2Char;
		PersistData.C loseChar = p1Wins<p2Wins?PD.p1Char:PD.p2Char;
		GetGameObject(Vector3.zero, "BG", Resources.Load<Sprite>(SpritePaths.BGPath + PD.GetPlayerSpritePath(p1Wins>p2Wins?PD.p1Char:PD.p2Char, true)), false, "BG0");

		PD.sounds.SetMusicAndPlay(SoundPaths.M_Title_DerivPath + PD.GetPlayerSpritePath(winChar));

		winner = CreateActor(PD.GetPlayerSpritePath(winChar), new Vector3(-2.06f, -0.5f));
		winner.SetScale(0.4f).SetSprite(2).SetSortingLayer("BG1");

		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + (Random.value > 0.5f ? "039" : "040"), 0);
		int narratorIndex = 24 + (int) winChar;
		PD.sounds.QueueVoice(SoundPaths.NarratorPath + narratorIndex.ToString("d3"));
		int val = Random.Range(70, 76);
		PD.sounds.QueueVoice(SoundPaths.VoicePath + PD.GetPlayerSpritePath(winChar) + "/" + val.ToString("d3"));
		PD.sounds.SetSoundVolume(PD.GetSaveData().savedOptions["vol_s"] / 115.0f);

		loser = CreateActor(PD.GetPlayerSpritePath(loseChar), new Vector3(2.81f, -1.25f), true);
		loser.SetSprite(loser.loseFrame).SetScale(0.2f).SetSortingLayer("BG1").SetTint(new Color(0.5f, 0.5f, 0.5f));
		GetGameObject(new Vector3(1.3f, 0.7f), "infoBox", Resources.Load<Sprite>(SpritePaths.DetailsBox));
		System.Xml.XmlNode top = GetXMLHead();
		FontData f = PD.mostCommonFont.Clone();
		f.scale = 0.07f;
		float x = 1.3f;
		GetMeshText(new Vector3(x, 1.5f), string.Format(GetXmlValue(top, "winstatement"), p1Wins>p2Wins?1:2), f);
		x = 1.2f; f.align = TextAlignment.Right; f.anchor = TextAnchor.MiddleRight; f.scale = 0.035f;
		GetMeshText(new Vector3(x, 0.9f), GetXmlValue(top, "wins") + ":", f);
		GetMeshText(new Vector3(x, 0.65f), GetXmlValue(top, "losses") + ":", f);
		GetMeshText(new Vector3(x, 0.4f), GetXmlValue(top, "totaltime") + ":", f);
		GetMeshText(new Vector3(x, 0.15f), GetXmlValue(top, "p1score") + ":", f);
		GetMeshText(new Vector3(x, -0.1f), GetXmlValue(top, "p2score") + ":", f);
		x = 1.7f; f.align = TextAlignment.Left; f.anchor = TextAnchor.MiddleLeft;
		GetMeshText(new Vector3(x, 0.9f), Mathf.Max(p1Wins, p2Wins).ToString(), f);
		GetMeshText(new Vector3(x, 0.65f), Mathf.Min(p1Wins, p2Wins).ToString(), f);
		GetMeshText(new Vector3(x, 0.4f), new ScoreTextFormatter().ConvertSecondsToMinuteSecondFormat(PD.totalRoundTime), f);
		GetMeshText(new Vector3(x, 0.15f), PD.totalP1RoundScore.ToString(), f);
		GetMeshText(new Vector3(x, -0.1f), PD.totalP2RoundScore.ToString(), f);
		applauseTimer = Random.Range(200, 220);
	}
	public void Update() {
		if(--applauseTimer == 0) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Applause + Random.Range(1, 7).ToString()); }
		if(PD.controller.G_Launch() || PD.controller.Pause() || clicker.isDown()) {
			PD.sounds.SetSoundVolume(PD.GetSaveData().savedOptions["vol_s"] / 150.0f);
			if(PD.gameType == PersistData.GT.Versus) {
				PD.ChangeScreen(PersistData.GS.CharSel);
			} else {
				PD.ChangeScreen(PersistData.GS.HighScore);
			}
		}
		int pCount = particles.GetParticles(pars);
		for(int i = 0; i < pCount; i++) {
			if(Mathf.Abs(pars[i].startLifetime - pars[i].lifetime) < 0.05f) {
				pars[i].color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
				pars[i].size *= Random.Range(0.8f, 1.2f);
			}
		}
		particles.SetParticles(pars, pCount);
	}
}