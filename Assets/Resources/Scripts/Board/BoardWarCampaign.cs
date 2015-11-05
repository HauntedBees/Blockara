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
public class BoardWarCampaign:BoardWarSpecialFull {
	public void UnDieHaHaLikeUndiesCanIBorrowYoursIPoopedInMine() { dead = false; }
	override public int GetSpecial() {
		if(Random.value < 0.7f) { return 0; }
		return Random.Range(3, 7);
	}
	public void freeRefillsOnThursdays(bool mirror = false) {
		for(int x = 0; x < width; x++) {
			int topy = height - topoffset, length = topy - GetHighestYAtX(x);
			LaunchTiles(length, topy, x, mirror);
			changes.Add(new MirrorChangeLaunch(length, topy, x));
		}
		UpdateHighestRowWithTiles();
	}
}