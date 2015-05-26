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
public class MouseCore {
	protected Vector2 prevPos;
	virtual public bool hasMoved() { return false; }
	virtual public bool isDown() { return false; }
	virtual public bool isHeld() { return false; }
	virtual public Vector2 getPosition(bool local = false) { return Vector2.zero; }
	virtual public Vector3 getPositionInGameObject(GameObject o) { return Vector3.zero; }
}

