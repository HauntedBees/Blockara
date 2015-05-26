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
public class CampaignHUD:InGameHUD {
	private TextMesh moneyText, shopText;
	private int money;
	override protected void AdditionalSetup(Sprite tile, int players, Vector3 offset, System.Xml.XmlNode top, int additionalInfo) {
		money = 0;
		hudBox.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite> (SpritePaths.InfoBoxCampaign);
		float x = PD.IsLeftAlignedHUD() ? -1.2f : 0.6f;
		GetMeshText(new Vector3(x, 1.03f), GetXmlValue(top, "money"), new FontData(TextAnchor.MiddleLeft, TextAlignment.Left, 0.03f));
		moneyText = GetMeshText(new Vector3(x + 0.6f, 0.84f), money.ToString(), new FontData(TextAnchor.MiddleRight, TextAlignment.Right, 0.045f));
		shopText = GetMeshText(new Vector3(x + 0.3f, 0.5f), "", new FontData(TextAnchor.MiddleCenter, TextAlignment.Center, 0.023f));
	}
	override public void DoUpdate(bool paused, int p1val, int p2val, bool hiddenPause = false) {
		if(gameEnd) { return; }
		HandlePause(paused, hiddenPause);
		if(!hiddenPause && Time.time >= lastCheck + 1.0f) { UpdateTimer(); }
		if(p1Score_val != p1val) { p1Score_val = p1val; UpdateTextValueAndSize(p1ScoreText, p1Score_val); }
		if(money != p2val) { SetAndUpdateMoneyTextSize(p2val); }
	}
	private void SetAndUpdateMoneyTextSize(int moneyAmount) { money = moneyAmount; UpdateTextValueAndSize(moneyText, money); }
 	public void UpdateShopBox(string text) { shopText.text = text; }
}