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
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
public class TweenHandler:ScriptableObject {
	private const float TWEEN_LENGTH = 0.1f;
	public void DoTween(GameObject o, Vector3 destination) { o.transform.DOMove(destination, TWEEN_LENGTH); }
	public void DoTileTween(Tile t, Vector3 destination, bool destroyWhenComplete = false, bool crop = false, bool initialVisible = false, int cropDir = 0, bool mirror = false) {
		if(!crop) {
			t.shape.transform.DOMove(destination, TWEEN_LENGTH).OnComplete(()=>FinishTileTween(t, destroyWhenComplete));
			t.block.transform.DOMove(destination, TWEEN_LENGTH);
			return;
		}
		if(initialVisible) {
			Vector3 p = t.block.transform.position;
			p.x += (mirror?-1:1) * cropDir * t.block.renderer.bounds.size.x / 2;
			t.block.transform.position = p;
			t.shape.transform.position = p;
			t.shape.transform.localScale = new Vector3(0.0f, 1.0f);
			t.block.transform.localScale = new Vector3(0.0f, 1.0f);
			t.shape.transform.DOScaleX(1.0f, TWEEN_LENGTH);
			t.block.transform.DOScaleX(1.0f, TWEEN_LENGTH);
			t.shape.transform.DOMove(destination, TWEEN_LENGTH).OnComplete(()=>FinishTileTween(t, destroyWhenComplete));
			t.block.transform.DOMove(destination, TWEEN_LENGTH);
		} else {
			Vector3 p = destination;
			p.x += (mirror?-1:1) * cropDir * t.block.renderer.bounds.size.x / 2;
			t.shape.transform.DOScaleX(0.0f, TWEEN_LENGTH);
			t.block.transform.DOScaleX(0.0f, TWEEN_LENGTH);
			t.shape.transform.DOMove(p, TWEEN_LENGTH).OnComplete(()=>FinishTileTween(t, destroyWhenComplete));
			t.block.transform.DOMove(p, TWEEN_LENGTH);
		}
	}
	private void FinishTileTween(Tile t, bool destroyWhenComplete) { if(destroyWhenComplete) { t.CleanGameObjects(); } }

}