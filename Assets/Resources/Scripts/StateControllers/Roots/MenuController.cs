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
public class MenuController:StateController {
	protected MenuCursor cursor;
	protected int selectedIdx;
	protected MenuCursor GetMenuCursor(int w, int h, string sprite, float px, float py, float dx, float dy, int initX, int initY, int p = 1, int frame = -1, float dtweenchange = -1, string sortLayer = "") {
		GameObject g = new GameObject("Menu CursorContainer " + p);
		MenuCursor c = g.AddComponent<MenuCursor>();
		c.SetPD(PD);
		c.player = p;
		c.SetController(p==1?PD.controller:PD.controller2, PD.GetKeyBindings(p - 1));
		c.setWidthAndHeight(w, h);
		c.Setup(th);
		c.SetupMenu(sprite, px, py, dx, dy, initX, initY, frame, dtweenchange);
		if(sortLayer != "") { c.cursor.renderer.sortingLayerName = sortLayer; }
		return c;
	}
	protected OptionsCursor GetOptionsCursor(int w, int h, string sprite, float px, float py, float dx, float dy, int initX, int initY, int p = 1, int frame = -1, float dtweenchange = -1) {
		GameObject g = Instantiate(gameObject, Vector3.zero, Quaternion.identity) as GameObject;
		OptionsCursor c = g.AddComponent<OptionsCursor>();
		Destroy(g);
		c.SetPD(PD);
		c.player = p;
		c.SetController(p==1?PD.controller:PD.controller2, PD.GetKeyBindings(p - 1));
		c.setWidthAndHeight(w, h);
		c.Setup(th);
		c.SetupMenu(sprite, px, py, dx, dy, initX, initY, frame, dtweenchange);
		return c;
	}
	protected void SignalFailure() { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Deny); }
	protected void SignalMovement() { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Select); }
	protected void SignalSuccess() { PD.sounds.SetSoundAndPlay(SoundPaths.S_Menu_Confirm); }
}