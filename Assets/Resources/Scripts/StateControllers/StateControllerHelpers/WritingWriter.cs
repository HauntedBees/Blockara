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
public class WritingWriter {
	protected PersistData PD;
	public string GetWrappedString(TextMesh mesh, string text, Vector2 boundingBox) {
		string[] words = text.Split(new char[]{' '});
		string result = "";
		mesh.text = "";
		for(int i = 0; i < words.Length; i++) {
			var word = words[i].Trim();
			if (i > 0) {
				result += " " + word;
			} else { result = words[0]; mesh.text = result; }
			mesh.text = result;
			if (mesh.gameObject.renderer.bounds.size.x > boundingBox.x) {
				result = result.Substring(0, result.Length - word.Length);
				result += "\n" + word;
			}
		}
		mesh.text = result;
		if(mesh.gameObject.renderer.bounds.size.y > boundingBox.y) {
			Vector3 ls = mesh.gameObject.transform.localScale;
			mesh.gameObject.transform.localScale = new Vector3(ls.x - 0.001f, ls.y - 0.001f);
			return GetWrappedString(mesh, text, boundingBox);
		}
		return result;
	}
}