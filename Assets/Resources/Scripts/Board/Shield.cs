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
public class Shield {
	public int health, pos, curframe;
	public Tile shieldTile;
	private Sprite[] sheet;
	public Shield(int p, Tile g, Sprite[] s) {
		pos = p;
		health = 6;
		shieldTile = g;
		sheet = s;
		curframe = 0;
	}
	public bool SetHealthAndReturnIfDestroyed(int h) {
		if(h > 0) { health = h; } else { health += h; }
		if(health <= 0) { return true; }
		curframe = 3 - Mathf.CeilToInt(health/2.0f);
		shieldTile.ChangeShieldTile(sheet[curframe]);
		return false;
	}
}