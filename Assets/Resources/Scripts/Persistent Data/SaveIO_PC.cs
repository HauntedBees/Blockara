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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
[System.Serializable]
public class SaveIO_PC:SaveIOCore {
	public override void Save(SaveData s, string path) {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + path);
		bf.Serialize(file, s);
		file.Close();
	}
	public override SaveData Load(string path) {
		SaveData res = null;
		if(File.Exists(Application.persistentDataPath + path)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + path, FileMode.Open);
			try {
				res = (SaveData)bf.Deserialize(file);
			} catch (System.Exception e) {
				throw e;
			} finally {
				file.Close();
			}
		}
		return res;
	}
}