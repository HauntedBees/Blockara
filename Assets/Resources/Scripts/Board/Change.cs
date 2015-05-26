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
public class MirrorChange { public changeType t; public enum changeType { remove, shift, launch, wipe, shield } }
public class MirrorChangeRemoveTile:MirrorChange {
	public int x, y;
	public MirrorChangeRemoveTile(int x, int y) { t = changeType.remove; this.x = x; this.y = y; }
}
public class MirrorChangeShift:MirrorChange {
	public int y, shiftDir;
	public MirrorChangeShift(int y, int shiftDir) { t = changeType.shift; this.y = y; this.shiftDir = shiftDir; }
}
public class MirrorChangeLaunch:MirrorChange {
	public int length, colNum, topY;
	public MirrorChangeLaunch(int length, int topY, int colNum) { t = changeType.launch; this.length = length; this.colNum = colNum; this.topY = topY; }
}
public class MirrorChangeWipeRow:MirrorChange {
	public int y;
	public MirrorChangeWipeRow(int y) { t = changeType.wipe; this.y = y; }
}
public class MirrorChangeShield:MirrorChange {
	public int x, frame;
	public bool kill;
	public MirrorChangeShield(int x, int frame, bool kill = false) { t = changeType.shield; this.x = x; this.frame = frame; this.kill = kill; }
}