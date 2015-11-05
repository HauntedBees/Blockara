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
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class BlockNexter:ScriptableObject {
	private float TWEENLENGTH = 0.1f, sizeMult = 0.75f, sizeMultSmall = 0.45f, TILE_SIZE;
	private float xStart, yStart;
	private int player, width, height, dir; // height should always be at least the board height plus two or three just so it doesn't fucking act demickey
	private bool isShown, isSmall;
	private BlockHandler bh;
	private List<int> tileVals;
	private List<GameObject> tileSprites;
	private GameObject universalPrefabReference;
	private Sprite[] tileSheet;
	private Sprite addSprite, overlaySprite;
	private int nextSelectLength;
	private List<GameObject> nextSelectedTiles;
	public void SetupUniversalPrefabAndSheet(GameObject p, Sprite[] s, Sprite o) { universalPrefabReference = p; tileSheet = s; overlaySprite = o; }
	public void Initialize(BlockHandler b, float x, float y, int p, bool s, bool small, int d) {
		bh = b;
		player = p; dir = d;
		if(small) { width = 2; height = 12; }
		else { width = 3; height = 8; }
		xStart = x; yStart = y;
		isShown = s; isSmall = small;
		TILE_SIZE = Consts.TILE_SIZE * (isSmall?sizeMultSmall:sizeMult);
		nextSelectLength = 0;
		nextSelectedTiles = new List<GameObject>();
		addSprite = Resources.Load<Sprite>(SpritePaths.RecoveryTile);
		InitTiles();
	}
	public int GetTileForPlayer() {
		if(tileVals != null) {
			int res = tileVals[0];
			tileVals.RemoveAt(0);
			tileVals.Add(bh.GetPlayerColor(player));
			return res;
		} else { return bh.GetPlayerColor(player); }
	}
	private void InitTiles() {
		tileVals = new List<int>();
		int dims = width * height;
		while(dims-- > 0) { tileVals.Add(bh.GetPlayerColor(player)); }
	}
	public void SetupTileGraphics() {
		if(!isShown) { return; }
		tileSprites = new List<GameObject>();
		int tp = 0;
		for(int x = 0; x < width; x++) { 
			for(int y = 0; y < height; y++) {
				tileSprites.Add(CreateTile(x, y, GetListPosFromXYCoords(x, y)));
				tileSprites[tp].name = "bntile" + tp + " (" + x + ", " + y + ")";
				tp++;
			}
		}
	}
	public void HighlightNextTiles(int len) {
		if(!isShown || len == nextSelectLength) { return; }
		for(int i = 0; i < nextSelectLength; i++) { Destroy(nextSelectedTiles[i]); }
		nextSelectedTiles.Clear();
		if(len < 2) { nextSelectLength = 0; return; }
		nextSelectLength = len;
		for(int i = 0; i < nextSelectLength; i++) {
			GameObject o = CreateTile(0, i, i);
			o.GetComponent<SpriteRenderer>().sprite = addSprite;
			nextSelectedTiles.Add(o);
		}
	}
	public void ShiftTileGraphics(int len) {
		if(!isShown) { return; }
		HandleColumn(0, len, true);
		for(int x = 1; x < (width - 1); x++) { HandleColumn(x, len); }
		HandleColumn(width - 1, len, false, true);
		tileSprites.RemoveRange(0, len);
	}
	private void KillIt(GameObject o) { Destroy(o); }
	private void HandleColumn(int col, int len, bool firstCol = false, bool lastCol = false) {
		float FRACTION = Mathf.Max(Mathf.Ceil(TWEENLENGTH / len * 100)/100, 0.03f); // no idea why, but when launching 3 or 6 tiles, this fucks up unless I do all this shit.
		if(firstCol) {
			for(int y = 0; y < len; y++) {
				GameObject o = tileSprites[GetListPosFromXYCoords(col, y)];
				if(y > 0) {
					Sequence s = DOTween.Sequence();
					if(y - 1 == 0) {
						s.Append(o.transform.DOMove(GetDisplayPos(col, 0), FRACTION));
					} else {
						for(int i = 1; (y - i) >= 0; i++) { s.Append(o.transform.DOMove(GetDisplayPos(col, y - i), FRACTION)); }
					}
					s.Append(o.transform.DOMove(GetDisplayPos(col, -1), FRACTION).OnComplete(()=>KillIt(o)));
				} else { o.transform.DOMove(GetDisplayPos(col, -1), TWEENLENGTH).OnComplete(()=>KillIt(o)); }
			}
		} else {
			for(int y = 0; y < len; y++) {
				GameObject o = tileSprites[GetListPosFromXYCoords(col, y)]; 
				Vector3 scale = o.transform.localScale;
				Vector3 position = GetDisplayPos(col - 1, height - 1);
				o.transform.position = new Vector3(position.x, position.y - o.renderer.bounds.size.y / 2);
				o.transform.localScale = new Vector3(scale.x, 0);
				Sequence s = DOTween.Sequence();
				s.Append(o.transform.DOMove(position, FRACTION));
				s.Append(o.transform.DOMove(GetDisplayPos(col - 1, height - len + y), FRACTION * (len - y - 1)));
				s.Insert(0, o.transform.DOScaleY(scale.y, FRACTION));
				s.PrependInterval(FRACTION * y);
				GameObject comeFromTop = CreateTile(col, y, GetListPosFromXYCoords(col, y));
				Sequence s2 = DOTween.Sequence();
				for(int i = 1; i <= y; i++) { s2.Append(comeFromTop.transform.DOMove(GetDisplayPos(col, y - i), FRACTION)); }
				s2.Append(comeFromTop.transform.DOMove(GetDisplayPos(col, -1), TWEENLENGTH).OnComplete(()=>KillIt(comeFromTop)));
				s2.Insert(FRACTION * y, comeFromTop.transform.DOScaleY(0.0f, FRACTION));
			}
		}
		for(int y = len; y < height; y++) {
			GameObject o = tileSprites[GetListPosFromXYCoords(col, y)]; 
			Sequence s = DOTween.Sequence();
			for(int i = 1; i <= len; i++) { s.Append( o.transform.DOMove(GetDisplayPos(col, y - i), FRACTION)); }
		}
		if(!lastCol) { return; }
		int count = tileVals.Count - len;
		for(int y = 0; y < len; y++) {
			GameObject newFromBottom = CreateTile(col, height -1, count + y);
			tileSprites.Add(newFromBottom);
			Vector3 scale = newFromBottom.transform.localScale;
			Vector3 position = newFromBottom.transform.position;
			newFromBottom.transform.position = new Vector3(position.x, position.y - newFromBottom.renderer.bounds.size.y / 2);
			newFromBottom.transform.localScale = new Vector3(scale.x, 0);
			Sequence s = DOTween.Sequence();
			s.Append(newFromBottom.transform.DOMove(position, FRACTION));
			s.Append(newFromBottom.transform.DOMove(GetDisplayPos(col, height - len + y), FRACTION * (len - y - 1)));
			s.Insert(0, newFromBottom.transform.DOScaleY(scale.y, FRACTION));
			s.PrependInterval(FRACTION * y);
		}
	}
	private GameObject CreateTile(int x, int y, int idx) {
		GameObject g = Instantiate(universalPrefabReference, GetDisplayPos(x, y), Quaternion.identity) as GameObject;
		g.renderer.sortingLayerName = "HUDText";
		g.GetComponent<SpriteRenderer>().sprite = tileSheet[tileVals[idx]];
		GameObject g_minor = Instantiate(universalPrefabReference, GetDisplayPos(x, y), Quaternion.identity) as GameObject;
		g_minor.renderer.sortingLayerName = "HUDTextPlusOne";
		g_minor.GetComponent<SpriteRenderer>().sprite = overlaySprite;
		g_minor.transform.parent = g.transform;
		g.renderer.transform.localScale = isSmall?new Vector2(sizeMultSmall, sizeMultSmall):new Vector2(sizeMult, sizeMult);
		return g;
	}
	private Vector3 GetDisplayPos(int x, int y) { return new Vector3(xStart + dir * (TILE_SIZE * x), yStart - TILE_SIZE * y); }
	private int GetListPosFromXYCoords(int x, int y) { return (x * height) + y; }
}