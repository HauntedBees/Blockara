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
public class TrainingAI:AICore {
	public TrainingAI(BoardWar mine, BoardWar theirs, BoardCursorActualCore cursor):base(mine, theirs, cursor) { state = 0; }
	override public AIAction TakeAction() {
		if(state == 1) {
			state = 0;
			return new AIAction(0, 0, -1);
		} else if(state == 2) {
			state = 0;
			return new AIAction(0, 0, 2, false, true);
		} else if(state == 3) {
			delay = Random.Range(delayLowerbound, delayUpperbound);
			return new AIAction(0, 0, 2, false, true);
		}
		return null;
	}
}