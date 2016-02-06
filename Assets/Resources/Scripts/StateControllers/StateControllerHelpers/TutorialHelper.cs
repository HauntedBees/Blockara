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
using System.Xml;
public class TutorialHelper:StateController {
	private TextMesh tutorialText;
	private BoardWar bo1, bo2;
	private XmlNodeList instructions;
	private int tutState, negativeonetimer;
	private GameObject highlight;
	private WritingWriter writer;
	private Vector2 bounds;
	public void Init(TextMesh t, GameObject container) {
		GetPersistData();
		tutorialText = t;
		writer = new WritingWriter();
		bounds = new Vector2(1.75f, 2.0f);
		highlight = GetGameObject(Vector3.zero, "Tutorial Highlight", Resources.Load<Sprite>(SpritePaths.WhiteSingle), false, "HUDText");
		instructions = (GetXMLHead("/tutorial", "instructions")).SelectNodes("instruction");
		tutState = -1;
		negativeonetimer = 80;
	}
	public void SetBoards(BoardWar b1, BoardWar b2) { bo1 = b1; bo2 = b2; }
	public void MoveHighlightToPosition(Vector3 pos) {
		if(highlight == null) { return; }
		highlight.transform.position = pos;
	}
	public bool IsActionAllowed(int type, int x, int y) {
		switch(tutState) {
			case 1: return type == 0 && y == 5;
			case 2: return type == 0 && y == 4;
			case 3: return type == 2 && x == 4;
			case 4: return (type == 0 && y == 4) || (type == 2 && x == 4);
			case 5: return (type == 0 && y >= 4) || (type == 2 && x == 4 && AreAllTilesOfType(4, 2, 2));
			case 6: return type == 0 && y >= 4;
			case 7: return type == 0 && y == 3;
			case 8: return type == 0 && (y == 2 || y == 1);
			case 9: return type == 0 && y == 0;
			case 10: return type == 2 && x == 4;
			case 11: return type == 0 && y >= 3;
			case 12: return (type == 0 && y >= 3) || (type == 2 && x == 4 && AreAllTilesOfType(4, 3, 0));
			case 13: return type != 2;
			case 14: return type == 2 && x == 3;
			case 16: return type == 2;
			default: return false;
		}
	}
	private bool AreAllTilesOfType(int x, int depth, int type) {
		for(int y = 5; y > 5 - depth; y--) { if(bo1.GetValueAtXY(x, y).GetColorVal() != type) { return false; } }
		return true;
	}
	public void DoUpdate(BoardCursorActualCore c, BoardCursorActualCore c2) {
		switch(tutState) {
			case -1:
				if(--negativeonetimer == 0) { newState(); }
				break;
			case 0:
				if(c.getX() == 4 && c.getY() == 5) { Destroy(highlight); newState(); }
				break;
			case 1:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1) { newState(); }
				break;
			case 2:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1 && bo1.GetValueAtXY(4, 4).GetColorVal() == 1) { newState(); }
				break;
			case 3:
				if(bo1.launchInfo.launching) { newState(); }
				break;
			case 4:
				if(bo2.GetHighestYAtX(3) == 3) { newState(); }
				break;
			case 5:
				if(bo2.GetHighestYAtX(3) == 2) { newState(); }
				break;
			case 6:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1 && bo1.GetValueAtXY(4, 4).GetColorVal() == 1) { newState(); }
				break;
			case 7:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1 && bo1.GetValueAtXY(4, 4).GetColorVal() == 1 && bo1.GetValueAtXY(4, 3).GetColorVal() == 1) { newState(); }
				break;
			case 8:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1 && bo1.GetValueAtXY(4, 4).GetColorVal() == 1 && bo1.GetValueAtXY(4, 3).GetColorVal() == 1 && bo1.GetValueAtXY(4, 2).GetColorVal() == 1 && bo1.GetValueAtXY(4, 1).GetColorVal() == 1) { newState(); }
				break;
			case 9:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == 1 && bo1.GetValueAtXY(4, 4).GetColorVal() == 1 && bo1.GetValueAtXY(4, 3).GetColorVal() == 1 && bo1.GetValueAtXY(4, 2).GetColorVal() == 1 && bo1.GetValueAtXY(4, 1).GetColorVal() == 1 && bo1.GetValueAtXY(4, 0).GetColorVal() == 1) { newState(); }
				break;
			case 10:
				if(bo2.GetHighestYAtX(3) < 0) { newState(); }
				break;
			case 11:
				if(bo1.GetValueAtXY(4, 5).GetColorVal() == bo1.GetValueAtXY(4, 4).GetColorVal() && bo1.GetValueAtXY(4, 3).GetColorVal() == bo1.GetValueAtXY(4, 4).GetColorVal()) { ForceCursor2ToShift(c2, 1); newState(); }
				break;
			case 12:
				if(bo2.GetHighestYAtX(3) < 0) { ForceCursor2ToShift(c2, 2); newState(); }
				break;
			case 13:
				if(bo1.GetValueAtXY(3, 5).GetColorVal() == bo1.GetValueAtXY(3, 4).GetColorVal() && bo1.GetValueAtXY(3, 3).GetColorVal() == bo1.GetValueAtXY(3, 4).GetColorVal()) { newState(); }
				break;
			case 14:
				if(bo1.launchInfo.launching) { newState(); }
				break;
			case 15: 
				bo2.BeDefeated(); tutState++;
				break;
		}
	}
	private void newState() {
		writer.GetWrappedString(tutorialText, FormatInstructionText(instructions[++tutState].InnerText), bounds);
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath  + (3 + tutState).ToString("d3"), 0);
	}
	private void ForceCursor2ToShift(BoardCursorActualCore c, int i) { (c as BoardCursorBot).forceAIState(i); }
	private string FormatInstructionText(string s) {
		return s.Replace("{move}", PD.controller.GetFriendlyActionName(InputMethod.Action.move))
				.Replace("{lnch}", PD.controller.GetFriendlyActionName(InputMethod.Action.launch))
				.Replace("{shlf}", PD.controller.GetFriendlyActionName(InputMethod.Action.shiftL))
				.Replace("{shrt}", PD.controller.GetFriendlyActionName(InputMethod.Action.shiftR))
				.Replace("{shal}", PD.controller.GetFriendlyActionName(InputMethod.Action.shiftAL))
				.Replace("{shar}", PD.controller.GetFriendlyActionName(InputMethod.Action.shiftAR));
	}
}