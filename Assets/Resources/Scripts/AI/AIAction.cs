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
using System;
public class AIAction {
	public int dx, dy, newx, newy, shift;
	public bool launch, shiftall;
	public AIAction(int dx, int dy, int shift = 0, bool launch = false, bool shiftall = false, int newx = -1, int newy = -1) {
		this.dx = dx;
		this.dy = dy;
		this.shift = shift;
		this.launch = launch;
		this.newx = newx;
		this.newy = newy;
		this.shiftall = shiftall;
	}
}

