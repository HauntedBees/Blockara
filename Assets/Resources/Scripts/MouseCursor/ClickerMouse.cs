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
public class ClickerMouse:MouseCore {
	public override bool isDown() { return Input.GetMouseButtonDown(0); }
	public override bool isHeld() { return Input.GetMouseButton(0); }
	public override bool hasMoved() {
		bool res = (prevPos.x != Input.mousePosition.x || prevPos.y != Input.mousePosition.y);
		if(prevPos == Vector2.zero) { res = false; }
		prevPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		return res; 
	}
	public override Vector2 getPosition(bool local = false) {
		prevPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D h = Physics2D.GetRayIntersection(r, Mathf.Infinity);
		if(local) {
			Vector2 p = h.point;
			GameObject o = h.collider.gameObject;
			float y = p.y - o.transform.localPosition.y;
			float x = p.x - o.transform.localPosition.x;
			return new Vector2(x, y);
		}
		return h.point;
	}
	public override Vector3 getPositionInGameObject(GameObject o) {
		RaycastHit2D[] hh = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		for(int i = 0; i < hh.Length; i++) {
			RaycastHit2D h = hh[i];
			Vector2 p = h.point;
			if(h.collider == null) { continue; }
			GameObject o2 = h.collider.gameObject;
			if(o != o2) { continue; }
			float y = p.y - o.transform.localPosition.y;
			float x = p.x - o.transform.localPosition.x;
			return new Vector3(x, y, 1.0f);
		}
		return Vector3.zero;
	}
}