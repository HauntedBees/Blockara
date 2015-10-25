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
public class GameController:CharDisplayController {
	private Countdown cd;
	private bool player1Human, player2Human, specialMode, firstLaunch, paused, gameOver, usingTouchControls;
	private int width, height, pauseTimer, endCounter, demoCountdown;
	private CutsceneChar actor1, actor2;
	private BoardWar board1, board2;
	private BoardMirror mirror1, mirror2;
	private BoardCursorActualCore cursor1, cursor2;
	private BoardCursorMirror cursormirror1, cursormirror2;
	private InGameHUD hud;
	private GameObject pauseButton, pauseText;
	private EndArcadeMatchOverlay end;
	private BlockHandler bh;
	private List<ZappyGun> zaps;
	private CampaignHandler campaign;
	private TutorialHelper tutorialAssist;
	private GameTouchHandler touchHandler;
	private Sprite[] pauseButtonSheet;
	private List<GameObject> roundLabels;
	private KeyCode medicKey;
	public void Start() {
		StateControllerInit(false);
		usingTouchControls = PD.GetSaveData().savedOptions["touchcontrols"] == 1;
		firstLaunch = true;
		player1Human = !PD.isDemo; player2Human = (PD.gameType == PersistData.GT.Versus);
		bh = new BlockHandler(PD, PD.GetPuzzleLevel());
		zaps = new List<ZappyGun>();
		SetupCountdown();
		SetupActors();
		SetupRoundDisplay();
		SetupEasterEgg();
		specialMode = PD.useSpecial;
		height = PD.isTutorial?6:PD.rowCount; width = 8;
		float p1Xoffset = (player2Human || (PD.isDemo && PD.demoPlayers == 2)) ? -10.1f : (PD.IsLeftAlignedHUD()?-1.5f:-5.5f), p2Xoffset = 3.0f;
		CreateBoards(p1Xoffset, p2Xoffset);
		SetUpHUDAndScores();
		if(PD.gameType == PersistData.GT.Challenge) { (board1 as BoardWarPuzzlePlayer).unlockedRow = (hud as PuzzleHUD).GetUnlockedRow(); }
		cursor1 = CreatePlayerCursor(player1Human, p1Xoffset, 1, board1, board2);
		cursor2 = CreatePlayerCursor(player2Human || PD.override2P, p2Xoffset, 2, board2, board1, PD.override2P);
		board1.Setup(cursor1, th, bh, (player2Human || (PD.isDemo && PD.demoPlayers == 2)) ? new Vector2(-0.2f, -0.6f) : new Vector2(PD.IsLeftAlignedHUD()?-0.725f:0.75f, -0.6f), player2Human || (PD.isDemo && PD.demoPlayers == 2), true, player1Human && usingTouchControls);
		board2.Setup(cursor2, th, bh, new Vector2(0.2f, -0.6f), true, player2Human || (PD.isDemo && PD.demoPlayers == 2));
		CreateMirrors(p1Xoffset, p2Xoffset);
		SetupMouseControls(p1Xoffset);

		if(!PD.isDemo) {
			if(PD.gameType == PersistData.GT.Campaign) { 
				campaign = new CampaignHandler(PD, board1 as BoardWarSpecial, board2 as BoardWarCampaign, 
				  			mirror2 as BoardMirrorSpecial, cursor1 as BoardCursorWar, cursor2 as BoardCursorBot, hud as CampaignHUD, GetXMLHead());
			}
			pauseButtonSheet = Resources.LoadAll<Sprite>(SpritePaths.ShortButtons);
			pauseButton = GetGameObject(player2Human ? (new Vector3(0.0f, -0.1f)):(new Vector3(2.5f, 0.7f)), "Pause Button", pauseButtonSheet[0], true, "HUD");
			pauseButton.SetActive(PD.usingMouse);
			pauseButton.transform.localScale = new Vector3(0.75f, 0.75f);
			FontData f = PD.mostCommonFont.Clone(); f.scale = 0.035f;
			pauseText = GetMeshText(player2Human ? (new Vector3(0.0f, 0.0f)):(new Vector3(2.5f, 0.8f)), GetXmlValue(GetXMLHead(), "pause"), f).gameObject;
			pauseText.SetActive(PD.usingMouse);
			pauseTimer = 0;
			mouseObjects.Add(pauseButton);
			mouseObjects.Add(pauseText);
		} else { demoCountdown = 1800; }
	}
	private void SetupCountdown() { cd = ScriptableObject.CreateInstance("Countdown") as Countdown; cd.Setup(PD); }
	private void SetupActors() {
		bool is2p = (player2Human || (PD.isDemo && PD.demoPlayers == 2));
		float posx = is2p?3.0f:2.35f, posy = is2p?-1.2f:-0.8f, scale = is2p?0.22f:0.33f;
		actor1 = CreateActor(PD.GetPlayerSpritePath(PD.p1Char), new Vector3(-posx, posy));
		actor1.SetScale(scale); actor1.SetSprite(0); actor1.bobbing = true; actor1.InGameBob();
		if(PD.gameType == PersistData.GT.Challenge) { actor1.Hide(); }
		actor2 = CreateActor(PD.GetPlayerSpritePath(PD.p2Char), new Vector3(posx, posy), true);
		actor2.SetScale(scale); actor2.SetSprite(0); actor2.bobbing = true; actor2.InGameBob();
		if(PD.gameType == PersistData.GT.Campaign || PD.gameType == PersistData.GT.Challenge || PD.gameType == PersistData.GT.Training) { actor2.Hide(); }
	}
	private void SetupRoundDisplay() {
		roundLabels = new List<GameObject>();
		if(PD.rounds == 1) { return; }
		Sprite[] roundSheet = Resources.LoadAll<Sprite>(SpritePaths.RoundStateIcons);
		if(player2Human) {
			GenerateRoundDisplayColumn(roundSheet, -0.4f, 1.7f);
			GenerateRoundDisplayColumn(roundSheet, 0.4f, 1.7f, false);
		} else {
			GenerateRoundDisplayColumn(roundSheet, PD.IsLeftAlignedHUD() ? -1.3f : 1.3f, 1.85f);
		}
	}
	private void GenerateRoundDisplayColumn(Sprite[] sheet, float x, float y, bool playerOne = true) {
		for(int i = 1; i < PD.rounds; i++) {
			int frame = 0;
			if(i < PD.currentRound) { frame = (PD.playerOneWonRound[i - 1] == playerOne) ? 1 : 2; }
			roundLabels.Add(GetGameObject(new Vector3(x, y - 0.19f * (i - 1)), "round" + i + " results for p" + (playerOne?1:2), sheet[frame], false, "HUDText"));
		}
	}
	private void CreateBoards(float p1Xoffset, float p2Xoffset) {
		paused = false; gameOver = false;
		board1 = CreateBoard(1, p1Xoffset);
		board2 = CreateBoard(2, p2Xoffset);
	}
	private void CreateMirrors(float p1Xoffset, float p2Xoffset) {
		mirror1 = CreateBoardMirror(1, board1, board2, player2Human || (PD.isDemo && PD.demoPlayers == 2));
		mirror2 = CreateBoardMirror(2, board2, board1);
		cursormirror1 = CreateMirrorCursor(p2Xoffset, 1, cursor1);
		cursormirror2 = CreateMirrorCursor(p1Xoffset, 2, cursor2);
	}
	private void SetUpHUDAndScores() {
		if(PD.gameType == PersistData.GT.Challenge) {
			GameObject g = new GameObject("PuzzleHUD");
			hud = g.AddComponent<PuzzleHUD>();
			hud.Setup(1, PD.GetPuzzleLevel());
		} else if(PD.gameType == PersistData.GT.Campaign) {
			GameObject g = new GameObject("CampaignHUD");
			hud = g.AddComponent<CampaignHUD>();
			hud.Setup(1);
		} else {
			GameObject g = new GameObject("InGameHUD");
			hud = g.AddComponent<InGameHUD>();
			if(player2Human || (PD.isDemo && PD.demoPlayers == 2)) { hud.Setup(2); }
			else if(PD.isTutorial) { 
				hud.Setup(1, 1); 
				tutorialAssist = hud.tutorialAssist;
				tutorialAssist.SetBoards(board1, board2);
				tutorialAssist.MoveHighlightToPosition(board1.GetScreenPosFromXY(4, 5)); }
			else { hud.Setup(1); }
		}
		if(PD.runningScore > 0) { board1.AddToScore(PD.runningScore); }
		if(PD.runningTime > 0) { hud.SetTimeWithSeconds(PD.runningTime); }
	}
	private void SetupEasterEgg() {
		if(PD.isDemo) { return; }
		if(!PD.IsKeyInUse((int)KeyCode.E)) { medicKey = KeyCode.E; }
		else if(!PD.IsKeyInUse((int)KeyCode.Alpha1)) { medicKey = KeyCode.Alpha1; }
		else { medicKey = KeyCode.CapsLock; }
	}
	private void SetupMouseControls(float xOffset) {
		touchHandler = gameObject.AddComponent<GameTouchHandler>();
		touchHandler.Initialize(height, xOffset);
		cursor1.AttachTouchHandler(touchHandler);
	}
	private BoardWar CreateBoard(int player, float xOffset) {
		GameObject g = new GameObject("Board " + player);
		BoardWar b;
		if(PD.gameType == PersistData.GT.Campaign) {
			if(player == 1) { b = g.AddComponent<BoardWarSpecial>(); }
			else { b = g.AddComponent<BoardWarCampaign>(); }
		} else if(specialMode) {
			b = g.AddComponent<BoardWarSpecialFull>();
		} else if(PD.gameType == PersistData.GT.Challenge) {
			if(player == 1) { 
				b = g.AddComponent<BoardWarPuzzlePlayer>();
			} else { b = g.AddComponent<BoardWarPuzzle>(); }
		} else {
			b = g.AddComponent<BoardWar>();
		}
		b.width = width; b.height = (player == 2 && PD.gameType == PersistData.GT.Challenge)?PD.rowCount2:height;
		b.player = player; b.xOffset = xOffset;
		return b;
	}
	private BoardMirror CreateBoardMirror(int player, BoardWar b1, BoardWar b2, bool show = true) {
		GameObject g = new GameObject("boardMirror" + player);
		g.AddComponent("BoardMirror");
		BoardMirror m;
		if(specialMode || (player == 2 && PD.gameType == PersistData.GT.Campaign)) { 
			m = g.AddComponent<BoardMirrorSpecial>();
		} else {
			m = g.AddComponent<BoardMirror>();
		}
		m.parent = b1; m.player = player; m.mirrorTop = b2;
		m.Setup(th, show);
		return m;
	}
	private BoardCursorMirror CreateMirrorCursor(float offset, int player, BoardCursorActualCore parent) {
		GameObject g = GetGameObject(Vector3.zero, "", null, false, "HUDText");
		BoardCursorMirror c = g.AddComponent<BoardCursorMirror>();
		c.SetPD(PD);
		c.xOffset = offset; c.player = player;
		c.setWidthAndHeight(width, height);
		c.Setup(parent, th, PD.gameType != PersistData.GT.Training && PD.gameType != PersistData.GT.Challenge && (player == 2 || player2Human || (PD.isDemo && PD.demoPlayers == 2)));
		c.SetInitXAndID(0);
		return c;
	}
	private BoardCursorActualCore CreatePlayerCursor(bool isHuman, float offset, int player, BoardWar b1, BoardWar b2, bool force2P = false) {
		BoardCursorActualCore c;
		if(isHuman || force2P) {
			if(tutorialAssist != null) {
				c = GetTutorialCursor(player, offset, width, height, th);
			} else {	
				c = GetUserCursor(player, offset, width, height, th, !force2P);
			}
			if(player == 1) { c.SetController(PD.controller, PD.GetKeyBindings()); } else { c.SetController(PD.controller2, PD.GetKeyBindings(1)); }
		} else {
			c = GetBotCursor(player, offset, width, height, th, b1, b2);
		}
		c.SetInitXAndID(0);
		return c;
	}
	private BoardCursorTutorial GetTutorialCursor(int p, float o, int w, int h, TweenHandler th) {
		GameObject g = GetGameObject(Vector3.zero, "", null, false, "HUDText");
		BoardCursorTutorial c = g.AddComponent<BoardCursorTutorial>();
		c.SetPD(PD);
		c.xOffset = o; c.player = p;
		c.setWidthAndHeight(w, h);
		c.Setup(th);
		c.setTC(tutorialAssist);
		return c;
	}
	private BoardCursorWar GetUserCursor(int p, float o, int w, int h, TweenHandler th, bool forceShow = true) {
		GameObject g = GetGameObject(Vector3.zero, "", null, false, "HUDText");
		BoardCursorWar c = g.AddComponent<BoardCursorWar>();
		c.SetPD(PD);
		c.xOffset = o; c.player = p;
		c.setWidthAndHeight(w, h);
		c.Setup(th, h, forceShow);
		if(p == 1 && usingTouchControls) { c.usingTouchControls = true; }
		return c;
	}
	private BoardCursorBot GetBotCursor(int p, float o, int w, int h, TweenHandler th, BoardWar boardA, BoardWar boardB) {
		GameObject g = GetGameObject(Vector3.zero, "", null, false, "HUDText");
		BoardCursorBot c = g.AddComponent<BoardCursorBot>();
		c.SetPD(PD);
		c.xOffset = o; c.player = p;
		c.setWidthAndHeight(w, h);
		c.Setup(th, h, false || (PD.isDemo && p <= PD.demoPlayers));
		c.CreateAI(boardA, boardB, (int)PD.gameType, (PD.isDemo?Random.Range(4,9):PD.difficulty));
		return c;
	}

	public void Update() {
		DebugShit();
		if(PD.isTransitioning) { return; }
		if(ExitDemoIfNeeded()) { return; }
		UpdateMouseInput();
		EasterEggsArentSoEasteryWhenTheCodeIsOpenSourceIsItYouFuckdummy();
		if(HandleCountdown()) { return; }
		bool clickedPause = HandlePauseButton();
		if(tutorialAssist != null) { tutorialAssist.DoUpdate(cursor1, cursor2); }
		if(PD.gameType == PersistData.GT.Campaign) {
			bool wasInCampaignShop = campaign.inCampaignShop;
			campaign.Update(PD.usingMouse && clicker.isDown(), clickedPause);
			bool returnHere = false;
			if(campaign.inCampaignShop || wasInCampaignShop != campaign.inCampaignShop) { 
				if(usingTouchControls) { ImUsingTouchControls(); }
				UpdateTweens();
				returnHere = true;
			}
			if(campaign.inCampaignShop && !wasInCampaignShop) {
				pauseText.GetComponent<TextMesh>().text = GetXmlValue(GetXMLHead(), "resumecaps");
			} else if(wasInCampaignShop && !campaign.inCampaignShop) {
				pauseText.GetComponent<TextMesh>().text = GetXmlValue(GetXMLHead(), "pause");
			}
			if(returnHere) { return; }
			if(campaign.playerDied) { HandleVictory(); }
		} else {
			if(board1.IsDead() || board2.IsDead()) { HandleVictory(); }
		}
		UpdateTweens();
		if(HandleGameOver()) { return; }
		if(!HandlePause()) {
			UpdateCursors();
			UpdateBoards();
			UpdateMirrors();
		}
		if(PD.gameType == PersistData.GT.Campaign) {
			hud.DoUpdate(paused, board1.GetScore(), (board2 as BoardWarCampaign).gold);
		} else if(PD.gameType == PersistData.GT.Challenge) {
			int res = 0;
			if(PD.puzzleType == 2) { if(board1.launchInfo.launching) { res = 1; } }
			else if(PD.puzzleType == 1) { if(board1.shifting != 0) { res = 2; } }
			hud.DoUpdate(paused, board1.GetScore(), res);
			if(hud.lose && !board2.IsDead()) { board1.BeDefeated(); }
		} else {
			hud.DoUpdate(paused, board1.GetScore(), board2.GetScore());
		}
	}
	private void EasterEggsArentSoEasteryWhenTheCodeIsOpenSourceIsItYouFuckdummy() { if(PD.isDemo) { return; } if(Input.GetKeyDown(medicKey)) { actor1.SayThingFromXML("082"); } }
	private bool HandlePauseButton() {
		if(PD.isDemo || pauseButton == null) { return false; }
		Vector3 p = clicker.getPositionInGameObject(pauseButton);
		if(p.z == 0 || paused) { if(pauseTimer-- < 0) { pauseButton.GetComponent<SpriteRenderer>().sprite = pauseButtonSheet[0]; } return false; }
		pauseButton.GetComponent<SpriteRenderer>().sprite = pauseButtonSheet[1];
		if(clicker.isDown()) {
			pauseButton.GetComponent<SpriteRenderer>().sprite = pauseButtonSheet[2];
			if(PD.gameType == PersistData.GT.Campaign && campaign.inCampaignShop) { return true; }
			pauseTimer = 5;
			HandlePause(true);
			return true;
		}
		return false;
	}
	private bool ExitDemoIfNeeded() {
		if(!PD.isDemo) { return false; }
		bool keyPress = PD.IsKeyDownOrButtonPressed() || PD.ReturnLaunchOrPauseOrNothingIsPressed() > 0;
		if(keyPress || --demoCountdown <= 0) { if(keyPress) { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); } PD.MoveOutOfDemo(); return true; }
		return false;
	}
	private bool HandleCountdown() { cd.DoUpdate(); return !cd.goDisplayed; }
	private bool HandleGameOver() {
		if(!gameOver || (PD.gameType == PersistData.GT.Challenge && board1.IsDead())) { return false; }
		if(((board1.IsKeyPressAccepted() && cursor1.launchOrPause()) || clicker.isDown()) && end == null) { 
			if(PD.gameType == PersistData.GT.Challenge) { PD.DoWin(board1.GetScore(), (hud as PuzzleHUD).GetRemainingMoves(), board1.IsDead()); }
			else { PD.DoWin(board1.GetScore(), hud.GetTimeInSeconds(), board1.IsDead()); }
		}
		return true;
	}
	private bool HandlePause(bool mouseClick = false) {
		if(isTransitioning) { return paused; }
		if(!paused) {
			if(cursor1.pause() || mouseClick) { hud.pausePresser = 1; paused = true; PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Pause); }
			else if(cursor2.pause()) { hud.pausePresser = 2; paused = true; PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Pause); }
		} else {
			if((cursor1.pause() && hud.pausePresser == 1) || (cursor2.pause() && hud.pausePresser == 2)) {
				paused = false;
				PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Unpause);
				return paused;
			}
			if(hud.pauseMenu == null) { return paused; }
			switch(hud.pauseMenu.state) {
				case 1: PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); isTransitioning = true; PD.SaveAndQuit(hud.GetTimeInSeconds()); break;
				case 2: PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
						isTransitioning = true;
						if(PD.gameType == PersistData.GT.Challenge) {
							PD.LowerPuzzleLevel();
							PD.ChangeScreen(PersistData.GS.PuzSel); 
						} else { 
							PD.SaveAndMainMenu(hud.GetTimeInSeconds());
						} 
						break;
				case 3: if(PD.gameType == PersistData.GT.Challenge) { 
							PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
							isTransitioning = true;
							PD.SaveAndReset(hud.GetTimeInSeconds()); 
						} else { 
							PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Unpause);
							paused = false;
						} 
						break;
				case 4: paused = false; PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Unpause); break;
			}
		}
		return paused;
	}
	private void HandleVictory() {
		if(gameOver) {
			if(isTransitioning) { return; }
			if(PD.gameType == PersistData.GT.Challenge && board1.IsDead()) {
				switch((hud as PuzzleHUD).retryMenu.state) {
					case 1: isTransitioning = true; PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); PD.LowerPuzzleLevel(); PD.ChangeScreen(PersistData.GS.PuzSel); break;
					case 2: isTransitioning = true; PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); PD.ChangeScreen(PersistData.GS.Game); break;
				}
				return;
			}
			if(PD.gameType != PersistData.GT.Arcade && !PD.isDemo) { return; }
			if(--endCounter > 0) { return; }
			if(end != null) {
				if(PD.isDemo) { PD.MoveOutOfDemo(); return; }
				bool lop = clicker.isDown() || (board1.actionDelay <= 0 && cursor1.launchOrPause());
				if(lop) { board1.actionDelay = PD.KEY_DELAY; }
				bool isTheEnd = end.doUpdate(lop);
				if(isTheEnd) { isTransitioning = true; PD.DoWin(board1.GetScore(), hud.GetTimeInSeconds(), board1.IsDead()); }
				return;
			}
			if(!PD.isDemo) {
				GetGameObject(Vector3.zero, "Tharsabin", Resources.Load<Sprite>(SpritePaths.TransparentBlackCover), false, "Cover HUD");
				end = gameObject.AddComponent<EndArcadeMatchOverlay>();
				end.Setup(!board1.IsDead());
			}
		}
		if(!gameOver) {
			PD.totalP1RoundScore += board1.GetScore();
			PD.totalP2RoundScore += board2.GetScore();
		}
		gameOver = true;
		paused = true;
		Vector3 p1Pos, p2Pos;
		int winningPlayer = 0;
		winningPlayer = board1.IsDead() ? 2 : 1;
		actor1.DoReaction(CutsceneChar.Reaction.win, winningPlayer == 1);
		actor2.DoReaction(CutsceneChar.Reaction.win, winningPlayer == 2);
		if(player2Human) { p1Pos = new Vector3(-1.4f, 0.0f); p2Pos = new Vector3(1.4f, 0.0f); } else { p1Pos = Vector3.zero; p2Pos = Vector3.zero; }
		hud.ShowVictoryText(winningPlayer, p1Pos, p2Pos, player2Human);
		if(PD.gameType == PersistData.GT.Arcade || PD.isDemo) { endCounter = 60; }
		else if(PD.gameType == PersistData.GT.Challenge && winningPlayer == 2) {
			(hud as PuzzleHUD).DisplayGameOverRetryScreen();
		}
	}

	private void UpdateTweens() {
		List<ZappyGun> dels = new List<ZappyGun>();
		foreach(ZappyGun z in zaps) { z.Update(); if(z.dead) { dels.Add(z); } }
		foreach(ZappyGun dead in dels) { Destroy(dead.gameObject); zaps.Remove(dead); }
		dels.Clear();
	}
	private void UpdateCursors() {
		depthPenetrateKill t1 = GetDepthAndKillForDisplay(cursor1, board1, board2);
		cursor1.SetDepthAndKillForDisplay(t1.depth, t1.penetrate, t1.penetratedepth, t1.kill);
		depthPenetrateKill t2 = GetDepthAndKillForDisplay(cursor2, board2, board1);
		cursor2.SetDepthAndKillForDisplay(t2.depth, t2.penetrate, t2.penetratedepth, t2.kill);
		cursor1.DoUpdate();
		cursor2.DoUpdate();
	}
	private void UpdateBoards() {
		board1.DoUpdate(); board2.DoUpdate();
		board1.HandleRecoveryDisplay();
		if(player2Human) { board2.HandleRecoveryDisplay(); }
		if(usingTouchControls) { ImUsingTouchControls(); }
		int strongerLaunch = GetLaunchConflictWinner(board1.launchInfo, board2.launchInfo);
		if(board1.launchInfo.launching && board2.launchInfo.launching && strongerLaunch > 0) {
			if(strongerLaunch == 1) { HandleLaunch(board1, board2, 1); }
			else { HandleLaunch(board2, board1, 2); }
		} else {
			if(board1.launchInfo.launching) { HandleLaunch(board1, board2, 1); }
			if(board2.launchInfo.launching) { HandleLaunch(board2, board1, 2); }
		}
		board1.DoShift(); board2.DoShift();
		if(specialMode) {
			if((board2 as BoardWarSpecial).justGotAShield) {
				(board2 as BoardWarSpecial).justGotAShield = false;
				(board1 as BoardWarSpecial).AddShield();
			}
			if((board1 as BoardWarSpecial).justGotAShield) {
				(board1 as BoardWarSpecial).justGotAShield = false;
				(board2 as BoardWarSpecial).AddShield();
			}
		} else if(PD.gameType == PersistData.GT.Campaign) {
			if((board2 as BoardWarSpecial).justGotAShield) {
				(board2 as BoardWarSpecial).justGotAShield = false;
				(board1 as BoardWarSpecial).AddShield();
			}
		}
	}
	private int GetLaunchConflictWinner(BoardWar.LaunchInfo l1, BoardWar.LaunchInfo l2) {
		if(l1.x != width - l2.x - 1) { return 0; }
		int t1 = l1.type, t2 = l2.type;
		if(t1 == t2) { return 0; }
		if(t1 == 0) { return t2 == 1 ? 1 : 2; }
		if(t1 == 1) { return t2 == 0 ? 2 : 1; }
		if(t1 == 2) { return t2 == 0 ? 1 : 2; }
		return 0;
	}
	private void UpdateMirrors() {
		mirror1.DoUpdate(); mirror2.DoUpdate();
		cursormirror1.DoUpdate(); cursormirror2.DoUpdate();
	}
	private void ImUsingTouchControls() {
		if(!PD.usingMouse || PD.isDemo) { return; }
		if(tutorialAssist != null) { 
			ImUsingTouchControlsInATutorial();
			return;
		}
		int change = touchHandler.HandleUpdate(clicker);
		cursor1.setY(touchHandler.rowY);
		if(touchHandler.aboveEverything) { board1.shiftall = true; }
		if(!touchHandler.aboveEverything || change == 0) { cursor1.setX(touchHandler.rowX); }
		if(change != 0) {
			board1.shifting = change;
		} else if(touchHandler.launching) {
			board1.SetLaunchInfoForLaunch();
		}
	}
	private void ImUsingTouchControlsInATutorial() {
		int change = touchHandler.HandleUpdate(clicker);
		cursor1.setY(touchHandler.rowY);
		if(touchHandler.aboveEverything) { board1.shiftall = true; }
		if(!touchHandler.aboveEverything || change == 0) { cursor1.setX(touchHandler.rowX); }
		if(change != 0 && tutorialAssist.IsActionAllowed((board1.shiftall?1:0), cursor1.getX(), cursor1.getY())) {
			board1.shifting = change;
		} else if(touchHandler.launching && tutorialAssist.IsActionAllowed(2, cursor1.getX(), cursor1.getY())) {
			board1.SetLaunchInfoForLaunch();
		}
	}
	
	private struct depthPenetrateKill {
		public int depth; public bool penetrate; public int penetratedepth; public bool kill;
		public depthPenetrateKill(int d, bool p, int dp, bool k) { depth = d; penetrate = p; penetratedepth = dp; kill = k; }
	}
	private depthPenetrateKill GetDepthAndKillForDisplay(BoardCursorCore launcherCur, BoardWar launcher, BoardWar victim) {
		int invertedx = victim.width - launcherCur.getX() - 1;
		int[] lengthType = launcher.GetLaunchDetails(launcherCur.getX());
		launcher.UpdateBlockNexter(lengthType[0]);
		int topy = victim.GetHighestYAtX(invertedx);
		int d = victim.GetHitDepth(invertedx, lengthType[0], lengthType[1], topy);
		return new depthPenetrateKill(d, d < topy, topy - d, victim.CanBeKilled(invertedx, lengthType[0]));
	}
	private void HandleLaunch(BoardWar launcher, BoardWar victim, int player) {
		BoardWar.LaunchInfo lI = launcher.launchInfo;
		int invertedX = victim.width - lI.x - 1;
		int depth = victim.TakeDamage(invertedX, lI.len, lI.type);
		HandleLaunchAnimations(launcher, victim, depth, lI.type);
		if(depth > 0) { launcher.AddToScore(PD.GetScore(depth, lI.len, lI.bonus)); }
		if(player == 1 && player1Human || player == 2 && player2Human) {
			GameObject zGo = GetGameObject(Vector3.zero, "zGo", null, false, "Zapper");
			ZappyGun z = zGo.AddComponent<ZappyGun>();
			Vector3 pos = victim.GetMirror().GetScreenPosFromXY(invertedX, victim.GetHitDepth(invertedX, lI.len, lI.type));
			z.Init(lI.type, lI.len, launcher.GetScreenPosFromXY(lI.x, lI.topy), pos.y - 0.4f);
			zaps.Add(z);
		}
		if(player == 1 && player2Human || player == 2 && player1Human) {
			GameObject zGoMirror = GetGameObject(Vector3.zero, "zGoMirror", null, false, "Zapper");
			ZappyGun mirrorZ = zGoMirror.AddComponent<ZappyGun>();
			Vector3 pos = victim.GetScreenPosFromXY(invertedX, victim.GetHitDepth(invertedX, lI.len, lI.type));
			mirrorZ.Init(lI.type, lI.len, launcher.GetMirror().GetScreenPosFromXY(lI.x, lI.topy), pos.y - 0.4f, true);
			zaps.Add(mirrorZ);
		}
		launcher.AcceptLaunch();
	}
	private void HandleLaunchAnimations(BoardWar launcher, BoardWar victim, int damageDealt, int type) {
		CutsceneChar sender, receiver;
		if(launcher.player == 1) { sender = actor1; receiver = actor2; } else { sender = actor2; receiver = actor1; }
		if(damageDealt == 0) {
			if(victim.DidLastMoveBlockCurrentAttack()) {
				sender.DoReaction(CutsceneChar.Reaction.block, true);
				receiver.DoReaction(CutsceneChar.Reaction.block, false);
			} else {
				if(++launcher.misses < 2) { return; }
				sender.DoReaction(CutsceneChar.Reaction.miss2, true);
				receiver.DoReaction(CutsceneChar.Reaction.miss2, false);
			}
			if(PD.p2Char == PersistData.C.FuckingBalloon) {
				if(Random.value > 0.65f) {
					if(Random.value > 0.98f) {
						actor1.SayThingFromReaction(CutsceneChar.SpeechType.nonDamageNegative);
					} else {
						actor2.SayThingFromReaction(CutsceneChar.SpeechType.nonDamagePositive);
					}
				}
			} else {
				if(Random.value > 0.33f) {
					if(Random.value > 0.5f) {
						sender.SayThingFromReaction(CutsceneChar.SpeechType.nonDamageNegative);
					} else {
						receiver.SayThingFromReaction(CutsceneChar.SpeechType.nonDamagePositive);
					}
				}
			}
			return;
		}
		launcher.misses = 0;
		receiver.FlickerColor(type);
		if(firstLaunch) {
			firstLaunch = false;
			sender.DoReaction(CutsceneChar.Reaction.firstStrike, true);
			receiver.DoReaction(CutsceneChar.Reaction.firstStrike, false);
		} else if(damageDealt == 3) {
			sender.DoReaction(CutsceneChar.Reaction.combo3, true);
			receiver.DoReaction(CutsceneChar.Reaction.combo3, false);
		} else if(damageDealt == 2) {
			sender.DoReaction(CutsceneChar.Reaction.combo2, true);
			receiver.DoReaction(CutsceneChar.Reaction.combo2, false);
		} else if(launcher.chain >= 2) {
			sender.DoReaction(CutsceneChar.Reaction.hit3, true);
			receiver.DoReaction(CutsceneChar.Reaction.hit3, false);
		} else if(launcher.chain == 1) {
			sender.DoReaction(CutsceneChar.Reaction.hit2, true);
			receiver.DoReaction(CutsceneChar.Reaction.hit2, false);
		} else {
			sender.DoReaction(CutsceneChar.Reaction.hit, true);
			receiver.DoReaction(CutsceneChar.Reaction.hit, false);
		}
		if(Random.value > 0.33f) {
			if(Random.value > 0.5f) {
				sender.SayThingFromReaction(CutsceneChar.SpeechType.doDamage);
			} else {
				receiver.SayThingFromReaction(CutsceneChar.SpeechType.takeDamage);
			}
		}
	}
		
	private void DebugShit() {
		if(Input.GetKeyDown(KeyCode.BackQuote)) { PD.SaveAndReset(0); return; }
		/*if(Input.GetKeyDown(KeyCode.Q)) { board1.TakeDamage(cursor1.getX(), 6, 0); }
		if(Input.GetKeyDown(KeyCode.W)) { board1.TakeDamage(cursor1.getX(), 6, 1); }
		if(Input.GetKeyDown(KeyCode.E)) { board1.TakeDamage(cursor1.getX(), 6, 2); }*/
		if(Input.GetKeyDown(KeyCode.P)) { board1.Debug_JustListFuckingEverything(); }
		if(Input.GetKeyDown(KeyCode.End)) { board2.BeDefeated(); }
		else if(Input.GetKey(KeyCode.PageDown)) { board1.BeDefeated(); }
		if(Input.GetKey(KeyCode.Backspace)) { PD.GoToMainMenu(); }
	}
}