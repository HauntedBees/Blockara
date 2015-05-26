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
public class BoardCursorTutorial:BoardCursorWar {
	private TutorialHelper tc;
	public override bool shiftLeft() { return tc.IsActionAllowed(0, x, y) && base.shiftLeft(); }
	public override bool shiftRight() { return tc.IsActionAllowed(0, x, y) && base.shiftRight(); }
	public override bool shiftAllLeft() {return tc.IsActionAllowed(1, x, y) && base.shiftAllLeft(); }
	public override bool shiftAllRight() { return tc.IsActionAllowed(1, x, y) && base.shiftAllRight(); }
	public override bool launch() { return tc.IsActionAllowed(2, x, y) && base.launch(); }
	public void setTC(TutorialHelper t) { tc = t; }
}