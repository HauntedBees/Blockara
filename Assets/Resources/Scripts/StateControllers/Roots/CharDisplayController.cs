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
public class CharDisplayController:StateController {
	protected CutsceneChar CreateActor(string playerPath, Vector3 pos, bool flip = false, bool higherLayer = false, bool inGame = false, bool isDuplicate = false) {
		Sprite[] sheet = Resources.LoadAll<Sprite>(SpritePaths.CharPath + playerPath);
		GameObject obj = GetGameObject(pos, "Character " + playerPath, null, false, higherLayer?"Cover HUD Actor":"BG1");
		if(flip) { Vector3 t = obj.transform.localScale; t.x *= -1.0f; obj.transform.localScale = t; }
		if(playerPath == "September") { Vector3 t = obj.transform.position; t.x *= 1.1f; obj.transform.position = t; }
		GetPersistData();
		if(isDuplicate) { obj.GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.2f, 0.25f, 1.0f); }
		if(PD.voicePitch > 1.0f && playerPath == "MasterAlchemist") { 
			Vector3 t = obj.transform.localScale;
			t.x *= 0.8f; t.y *= 0.8f;
			obj.transform.localScale = t;
			Vector3 p = obj.transform.position;
			p.y += 0.33f;
			obj.transform.position = p;
		}
		return new CutsceneChar(playerPath, obj, sheet, flip?1:0, PD, inGame);
	}
}