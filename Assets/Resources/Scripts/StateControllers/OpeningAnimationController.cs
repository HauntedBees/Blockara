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
public class OpeningAnimationController:StateController {
	private const int TEXT_SPEED = 3;
	private string openingText;
	private bool atSpace;
	private TextMesh textObject;
	private int curFrame, maxFrame, timer, state, wordMark, wordMarkMax;
	private float startTime;
	private float[] wordMarkIndexes;
	private GameObject[] charTiles, twinkles;
	public void Start() {
		Screen.showCursor = false;
		StateControllerInit(false);
		startTime = Time.time;
		PD.sounds.SetVoiceAndPlay(SoundPaths.NarratorPath + "002", 0);
		openingText = string.Format(GetXMLHead().SelectSingleNode("opening").InnerText, "\n");
		FontData f = new FontData(TextAnchor.UpperLeft, TextAlignment.Left, 0.065f);
		f.color = Color.white;
		textObject = GetMeshText(new Vector3(-3.4f, 1.85f), "", f);
		timer = TEXT_SPEED;
		wordMark = (PD.culture == "en") ? 0 : -1;
		//						 every   12000   years     the  septm.  dragon   rises   from     her  slmbr.      to   grant       a    wish      to     the    best  alcmt.
		wordMarkIndexes = new float[] { 0.651f, 1.777f, 2.526f, 2.526f, 3.428f, 4.019f, 4.544f, 4.685f, 4.879f, 5.836f, 5.997f, 6.281f, 6.334f, 6.750f, 6.842f, 7.043f, 7.441f, 
										8.083f, 8.194f, 8.316f, 9.278f, 10.17f, 10.37f};
		//								    in     the   world   today     she  awkns.
		wordMarkMax = wordMarkIndexes.Length;
		curFrame = 0;
		state = 0;
		maxFrame = openingText.Length;
	}
	public void Update() {
		switch(state) {
			case 0: UpdateOpeningText(); break;
			case 1: UpdateAnimation(); break;
			case 2: FlickerEyes(); break;
			case 3: UpdateLeavingAnimation(); break;
		}
		if(PD.GetP1Controller() != null) { PD.GoToMainMenu(); }
	}
	private void UpdateOpeningText() {
		if(wordMark < 0) { atSpace = false; }
		if(atSpace && wordMark < wordMarkMax) {
			if((Time.time - startTime) < wordMarkIndexes[wordMark]) { return; }
			wordMark++;
			atSpace = false;
		}
		if(--timer <= 0 && curFrame < maxFrame) {
			if(openingText[curFrame] == ' ') { atSpace = true; }
			textObject.text = openingText.Substring(0, curFrame++);
			timer = TEXT_SPEED;
		}
		else if(curFrame >= maxFrame && !PD.sounds.IsVoicePlaying()) { InitAnimation(); state = 1; }
	}
	private void FlickerEyes() {
		for(int i = 0; i < 21; i++) {
			int d = curFrame++;
			float ns = 0.1f + 0.03f*(d<90?d:((180 - d)));
			twinkles[i].transform.localScale = new Vector3(ns, ns);
		}
		if(curFrame >= 180) {
			for(int i = 0; i < 21; i++) { Destroy(twinkles[i]); }
			twinkles = null;
			curFrame = 0;
			state = 3;
		}
	}
	private void UpdateLeavingAnimation() {
		if(curFrame++ == 30) {
			for(int i = 0; i < 10; i++) { Destroy(charTiles[i]); }
			charTiles = null;
			PD.GoToMainMenu();
		}
		if(curFrame >= 30) { return; }
		float ny = Mathf.Pow(curFrame * 0.065f, 2);
		for(int i = 0; i < 10; i++) {
			Vector3 p = charTiles[i].transform.position;
			p.y = (i % 2 != 0)?-ny:ny;
			charTiles[i].transform.position = p;
		}
	}
	private void UpdateAnimation() {
		float ny = 3.2f - Mathf.Sqrt(curFrame * 0.45f);
		if(ny < 0) { ny = 0; state = 2; InitFlickerEyes(); }
		for(int i = 0; i < 10; i++) {
			Vector3 p = charTiles[i].transform.position;
			p.y = (i % 2 != 0)?-ny:ny;
			charTiles[i].transform.position = p;
		}
		curFrame++;
	}
	private void InitAnimation() {
		curFrame = 1;
		charTiles = new GameObject[10];
		Sprite[] charSheet = Resources.LoadAll<Sprite>(SpritePaths.OpeningAnimTiles);
		for(int i = 0; i < 10; i++) {
			charTiles[i] = GetGameObject(new Vector3(-3.18f + i * 0.71f, (i%2==0?3.2f:-3.2f)), "charTile" + i, charSheet[i], false, "Pause HUD Cursor");
			charTiles[i].transform.localScale = new Vector3(1.84f, 1.84f);
		}
	}
	private void InitFlickerEyes() {
		Sprite twinkleSprite = Resources.Load<Sprite>(SpritePaths.EyeSparkle);
		twinkles = new GameObject[21];
		PD.sounds.SetSoundAndPlay(SoundPaths.S_Ding);
		curFrame = 0;
		twinkles[0] = GetTwinkle(twinkleSprite, new Vector3(-3.32f, 0.50f));
		twinkles[1] = GetTwinkle(twinkleSprite, new Vector3(-3.07f, 0.48f));

		twinkles[2] = GetTwinkle(twinkleSprite, new Vector3(-2.70f, 0.41f));
		twinkles[3] = GetTwinkle(twinkleSprite, new Vector3(-2.26f, 0.42f));

		twinkles[4] = GetTwinkle(twinkleSprite, new Vector3(-1.92f, 0.35f));

		twinkles[5] = GetTwinkle(twinkleSprite, new Vector3(-1.32f, 0.39f));
		twinkles[6] = GetTwinkle(twinkleSprite, new Vector3(-1.29f, 0.65f));
		twinkles[7] = GetTwinkle(twinkleSprite, new Vector3(-0.95f, 0.39f));
		twinkles[8] = GetTwinkle(twinkleSprite, new Vector3(-0.83f, 0.64f));

		twinkles[9] = GetTwinkle(twinkleSprite, new Vector3(-0.37f, 0.22f));

		twinkles[10] = GetTwinkle(twinkleSprite, new Vector3(0.09f, 0.47f));
		twinkles[11] = GetTwinkle(twinkleSprite, new Vector3(0.39f, 0.44f));

		twinkles[12] = GetTwinkle(twinkleSprite, new Vector3(1.00f, 0.42f));
		twinkles[13] = GetTwinkle(twinkleSprite, new Vector3(1.27f, 0.38f));

		twinkles[14] = GetTwinkle(twinkleSprite, new Vector3(1.60f, 0.41f));
		twinkles[15] = GetTwinkle(twinkleSprite, new Vector3(1.85f, 0.51f));
		twinkles[16] = GetTwinkle(twinkleSprite, new Vector3(1.95f, 0.36f));

		twinkles[17] = GetTwinkle(twinkleSprite, new Vector3(2.38f, 0.50f));
		twinkles[18] = GetTwinkle(twinkleSprite, new Vector3(2.59f, 0.50f));

		twinkles[19] = GetTwinkle(twinkleSprite, new Vector3(2.98f, 0.28f));
		twinkles[20] = GetTwinkle(twinkleSprite, new Vector3(3.30f, 0.28f));
	}
	private GameObject GetTwinkle(Sprite twinkle, Vector3 pos) {
		GameObject g = GetGameObject(pos, "eyeTwinkle", twinkle, false, "Pause HUD Cursor");
		g.transform.localScale = new Vector3(0.1f, 0.1f);
		return g;
	}
}