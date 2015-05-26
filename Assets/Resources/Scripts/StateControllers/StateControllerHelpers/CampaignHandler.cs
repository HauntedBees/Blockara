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
using System.Xml;
public class CampaignHandler {
	private PersistData PD;
	private BoardWarSpecial board1;
	private BoardWarCampaign board2;
	private BoardMirrorSpecial mirror2;
	private BoardCursorWar cursor1;
	private BoardCursorBot cursor2;
	private CampaignHUD hud;
	private int prevCursorX, prevCursorY;
	public bool inCampaignShop, playerDied;
	private XmlNode navDetails;
	public CampaignHandler(PersistData p, BoardWarSpecial b1, BoardWarCampaign b2, BoardMirrorSpecial m, BoardCursorWar c1, BoardCursorBot c2, CampaignHUD h, XmlNode top) {
		PD = p; hud = h; mirror2 = m;
		board1 = b1; board2 = b2;
		cursor1 = c1; cursor2 = c2;
		inCampaignShop = false; playerDied = false;
		navDetails = top;
	}
	private string GetXmlValue(string id) {
		XmlNode elem = navDetails.SelectSingleNode(id);
		return (elem == null ? "ERROR LOL" : elem.InnerText);
	}
	public void Update(bool isClickingTile, bool isClickingPause) {
		DebugShit();
		if(inCampaignShop) { HandleShop(isClickingTile, isClickingPause); return; }
		if(board1.IsDead()) { playerDied = true; }
		else if(board2.IsDead()) {
			board2.freeRefillsOnThursdays();
			board2.UnDieHaHaLikeUndiesCanIBorrowYoursIPoopedInMine();
			mirror2.DoUpdate();
			cursor2.LevelUpAI(1);
			inCampaignShop = true;
			PD.difficulty++;
			cursor1.hideWhite = true;
		}
	}
	private void HandleShop(bool isClickingTile, bool isClickingPause) {
		if(cursor1.pause() || isClickingPause) {
			inCampaignShop = false;
			PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
			hud.UpdateShopBox("");
			cursor1.hideWhite = false;
			return;
		}
		hud.DoUpdate(true, board1.GetScore(), board2.gold, true);
		cursor1.DoUpdate();
		int cx = cursor1.getX();
		int cy = cursor1.getY();
		Tile t = board1.GetValueAtXY(cx, cy);
		string newText = "";
		bool forceTextUpdate = false;
		if(cy < (board1.height - 1) && t.IsDead()) { // tiles
			newText = string.Format(GetXmlValue("replacetile"), "\r\n", GetTilePrice(cy));
			if(cursor1.launch() || isClickingTile) {
				forceTextUpdate = true;
				if(ReplaceTile(cx, cy)) {
					PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
					newText = string.Format(GetXmlValue("tilereplaced"), "\r\n");
				} else {
					PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny);
					newText = string.Format(GetXmlValue("cantafford"), "\r\n");
				}
			}
		} else if(cy == (board1.height - 1) && t.isShield) { // shields
			newText = string.Format(GetXmlValue("repairshield"), "\r\n", GetShieldPrice());
			if(cursor1.launch() || isClickingTile) {
				forceTextUpdate = true;
				if(RepairShield(cx, cy)) {
					PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm);
					newText = string.Format(GetXmlValue("shieldreplaced"), "\r\n");
				} else {
					PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny);
					newText = string.Format(GetXmlValue("cantafford"), "\r\n");
				}
			}
		} else { // empty shield tile or active regular tile
			newText = string.Format(GetXmlValue("presstocontinue"), "\r\n", PD.controller.GetFriendlyActionName(InputMethod.Action.pause));
		}
		if(forceTextUpdate || cx != prevCursorX || cy != prevCursorY) { hud.UpdateShopBox(newText); }
		prevCursorX = cx;
		prevCursorY = cy;
	}
	private bool ReplaceTile(int x, int y) {
		int price = GetTilePrice(y);
		if(price > board2.gold) { return false; }
		board2.gold -= price;
		board1.CreateTileAtLocation(x, y);
		return true;
	}
	private bool RepairShield(int x, int y) {
		int price = GetShieldPrice();
		if(price > board2.gold) { return false; }
		board2.gold -= price;
		board1.SetHPAndReturnIfDestroyed(board1.GetListPosFromXY(x, y), 6);
		return true;
	}
	private int GetTilePrice(int y) { return 100 * PD.difficulty * (y + 1); }
	private int GetShieldPrice() { return 100 * PD.difficulty; }
	
	private void DebugShit() {
		if(PD.controller.EnableCheat1()) { (board1 as BoardWarSpecial).AddShield(); }
		if(PD.controller.EnableCheat2()) { UnityEngine.Debug.Log("HAPEN"); board2.gold += 100; } 
	}
}