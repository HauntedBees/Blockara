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
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
public class CreditsController:StateController {
	private List<GameObject> objects;
	private GameObject end;
	private Sprite[] borbs;
	private int finalTimeout;
	public void Update() {
		if(end.transform.position.y < 0.0f) {
			foreach(GameObject g in objects) {
				Vector3 p = g.transform.position;
				p.y += Time.deltaTime * 0.475f;
				g.transform.position = p;
			}
		} else if(finalTimeout-- < 0 && !PD.sounds.IsMusicPlaying()) {
			PD.DoWin(0, 0, true, false);
		}
		UpdateMouseInput();
		if(clicker.isDown() || PD.controller.Pause() || PD.controller.G_Launch() || PD.controller.M_Confirm()) { PD.DoWin(0, 0, true, false); }
	}
	public void Start() {
		StateControllerInit(false);
		objects = new List<GameObject>();
		borbs = Resources.LoadAll<Sprite>(SpritePaths.Borbs);
		XmlNodeList credits = GetXMLHead("/Credits", "credits").SelectNodes("credit");
		finalTimeout = 200;

		AddHeadingCredit(credits[0], 0.0f, -2.0f, 0.1f); // BLOCKARA

		AddHeadingCredit(credits[1], 0.0f, -3.0f); // CORE TEAM
		AddFullCredit(credits[2], 0.0f, -4.0f); // SEAN
		AddFullCredit(credits[3], 0.0f, -5.1f); // BRANDT
		AddFullCredit(credits[4], 0.0f, -6.2f); // MICHIO
		AddFullCredit(credits[5], 0.0f, -7.3f); // JENETTE
		
		AddHeadingCredit(credits[6], 0.0f, -8.5f); // VOICES
		AddVoiceCredit(credits[7], -2.0f, -9.5f); // ABNER
		AddVoiceCredit(credits[8], 2.0f, -9.5f); // CAL
		AddVoiceCredit(credits[9], 0.0f, -9.5f); // CAROLINA
		AddVoiceCredit(credits[10], -2.0f, -11f); // MATT
		AddVoiceCredit(credits[11], 2.0f, -11f); // DYLAN
		AddVoiceCredit(credits[12], 0.0f, -11f); // SEAN
		AddVoiceCredit(credits[13], 0.0f, -12.5f); // VERONICA

		AddHeadingCredit(credits[14], 0.0f, -14.0f); // HELPING HANDS
		AddAdditionalCredit(credits[15], -1.0f, -14.25f); // MARCIN
		AddAdditionalCredit(credits[16], 1.0f, -14.25f); // HARI
		AddAdditionalCredit(credits[17], -2.0f, -14.75f); // DIEGO
		AddAdditionalCredit(credits[18], 0.0f, -14.75f); // JOE
		AddAdditionalCredit(credits[19], 2.0f, -14.75f); // KEVIN
		AddAdditionalCredit(credits[20], -2.0f, -15.25f); // JACOB
		AddAdditionalCredit(credits[21], 0.0f, -15.25f); // LUKE
		AddAdditionalCredit(credits[22], 2.0f, -15.25f); // NOLAN
		AddAdditionalCredit(credits[23], -2.0f, -15.75f); // ASH
		AddAdditionalCredit(credits[24], 0.0f, -15.75f); // GEORGE
		AddAdditionalCredit(credits[25], 2.0f, -15.75f); // EVIN

		AddHeadingCredit(credits[26], 0.0f, -16.75f); // ADDITIONAL ASSETS
		AddAssetCredit(credits[27], -2.1f, -17.0f); // Mirko
		AddAssetCredit(credits[28], -0.7f, -17.0f); // milton
		AddAssetCredit(credits[29], 0.7f, -17.0f); // daenerys
		AddAssetCredit(credits[30], 2.1f, -17.0f); // sandyrb
		AddAssetCredit(credits[31], -2.1f, -17.5f); // maarten_wez
		AddAssetCredit(credits[32], -0.7f, -17.5f); // xtrgamr
		AddAssetCredit(credits[33], 0.7f, -17.5f); // theta4
		AddAssetCredit(credits[34], 2.1f, -17.5f); // qubodup
		AddAssetCredit(credits[35], -2.1f, -18.0f); // CTCollab
		AddAssetCredit(credits[36], -0.7f, -18.0f); // Cristiano
		AddAssetCredit(credits[37], 0.7f, -18.0f); // zerolagtime
		AddAssetCredit(credits[38], 2.1f, -18.0f); // Northern_Monkey
		AddAssetCredit(credits[39], -2.1f, -18.5f); // timgormly
		AddAssetCredit(credits[40], -0.7f, -18.5f); // soundnimja
		AddAssetCredit(credits[41], 0.7f, -18.5f); // Udderdude
		AddAssetCredit(credits[42], 2.1f, -18.5f); // Erdie
		AddAssetCredit(credits[43], -2.1f, -19.0f); // Tobiasz
		AddAssetCredit(credits[44], -0.7f, -19.0f); // NenadSimic
		AddAssetCredit(credits[45], 0.7f, -19.0f); // D
		AddAssetCredit(credits[46], 2.1f, -19.0f); // broumbroum
		AddAssetCredit(credits[47], -2.1f, -19.5f); // Kenney
		AddAssetCredit(credits[48], -0.7f, -19.5f); // Merritt
		AddAssetCredit(credits[49], 0.7f, -19.5f); // Cody
		AddAssetCredit(credits[50], 2.1f, -19.5f); // Jonathan
		AddAssetCredit(credits[51], 0.0f, -20.0f); // DOTween
		
		AddHeadingCredit(credits[52], 0.0f, -21.5f); // SPECIAL THANKS
		AddHeadingCredit(credits[53], 0.0f, -22.0f, 0.06f); // Andrew W.K.

		end = GetGameObject(new Vector3(0.0f, -24.5f), "thanks", Resources.Load<Sprite>(SpritePaths.CreditsPath + "11"));
		objects.Add(end);
	}
	private void AddFullCredit(XmlNode credit, float x, float y) {
		float gap = 0.43f;
		GameObject img = GetGameObject(new Vector3(x, y), "img:" + credit.SelectSingleNode("name").InnerText, Resources.Load<Sprite>(SpritePaths.CreditsPath + credit.SelectSingleNode("img").InnerText));
		FontData f = new FontData(TextAnchor.UpperLeft, TextAlignment.Left, 0.025f);
		f.color = Color.white;
		GameObject job = GetMeshText(new Vector3(x + gap, y + 0.05f), string.Format(credit.SelectSingleNode("job").InnerText, "\r\n"), f).gameObject;
		f.scale = 0.06f;
		GameObject name = GetMeshText(new Vector3(x + gap, y + 0.4f), credit.SelectSingleNode("name").InnerText, f).gameObject;
		float width = img.renderer.bounds.size.x + gap;
		float nameW = name.renderer.bounds.size.x;
		float jobW = job.renderer.bounds.size.x;
		if(nameW > jobW) { width += nameW; } else { width += jobW; }
		float shiftW = width / 2.0f - 0.55f;
		img.transform.position = GetVectorShiftedByX(img, -shiftW);
		name.transform.position = GetVectorShiftedByX(name, -shiftW);
		job.transform.position = GetVectorShiftedByX(job, -shiftW);
		objects.Add(img);
		objects.Add(name);
		objects.Add(job);
	}
	private void AddVoiceCredit(XmlNode credit, float x, float y) {
		objects.Add(GetGameObject(new Vector3(x, y), "img:" + credit.SelectSingleNode("name").InnerText, Resources.Load<Sprite>(SpritePaths.CreditsPath + credit.SelectSingleNode("img").InnerText)));
		string[] borbSplit = credit.SelectSingleNode("borbs").InnerText.Split(new char[1] {','});
		for(int i = 0; i < borbSplit.Length; i++) { objects.Add(GetGameObject(new Vector3(x + 0.35f - i * 0.2f, y - 0.35f), "borb", borbs[int.Parse(borbSplit[i])], false, "HUD")); }
		FontData f = new FontData(TextAnchor.UpperCenter, TextAlignment.Center, 0.025f);
		f.color = Color.white;
		objects.Add(GetMeshText(new Vector3(x, y - 0.7f), credit.SelectSingleNode("for").InnerText, f).gameObject);
		f.scale = 0.055f;
		objects.Add(GetMeshText(new Vector3(x, y - 0.4f), credit.SelectSingleNode("name").InnerText, f).gameObject);
	}
	private void AddAdditionalCredit(XmlNode credit, float x, float y) {
		FontData f = new FontData(TextAnchor.UpperCenter, TextAlignment.Center, 0.02f);
		f.color = Color.white;
		objects.Add(GetMeshText(new Vector3(x, y - 0.6f), credit.SelectSingleNode("for").InnerText, f).gameObject);
		f.scale = 0.04f;
		objects.Add(GetMeshText(new Vector3(x, y - 0.4f), credit.SelectSingleNode("name").InnerText, f).gameObject);
	}
	private void AddAssetCredit(XmlNode credit, float x, float y) {
		FontData f = new FontData(TextAnchor.UpperCenter, TextAlignment.Center, 0.015f);
		f.color = Color.white;
		objects.Add(GetMeshText(new Vector3(x, y - 0.7f), credit.SelectSingleNode("from").InnerText, f).gameObject);
		objects.Add(GetMeshText(new Vector3(x, y - 0.6f), credit.SelectSingleNode("for").InnerText, f).gameObject);
		f.scale = 0.03f;
		objects.Add(GetMeshText(new Vector3(x, y - 0.4f), credit.SelectSingleNode("name").InnerText, f).gameObject);
	}
	private void AddHeadingCredit(XmlNode credit, float x, float y, float fontSize = 0.07f) {
		FontData f = new FontData(TextAnchor.UpperCenter, TextAlignment.Center, fontSize);
		f.color = Color.white;
		objects.Add(GetMeshText(new Vector3(x, y), credit.SelectSingleNode("text").InnerText, f).gameObject);
	}
	private Vector3 GetVectorShiftedByX(GameObject g, float x) { Vector3 pos = g.transform.position; pos.x += x; return pos; }
}