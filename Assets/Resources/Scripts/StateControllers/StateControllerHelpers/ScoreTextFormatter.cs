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
using System.Collections.Generic;
public class ScoreTextFormatter:WritingWriter {
	protected int scoreRowWidth;
	protected int timeRowWidth;
	public ScoreTextFormatter() { }
	public ScoreTextFormatter(PersistData p) { PD = p; }
	public void SetScoreRowAndTimeRowWidths(List<KeyValuePair<string, int>> scores, List<KeyValuePair<string, int>> times) {
		scoreRowWidth = ("10. TIT " + scores[0].Value.ToString()).Length;
		timeRowWidth = ("10. TIT " + ConvertSecondsToMinuteSecondFormat(times[PD.gameType == PersistData.GT.Campaign?9:0].Value)).Length; // 13 unless minutes > 100
	}
	public string GetRowText(string front, string back, bool time) {
		int paddingLength = (time?timeRowWidth:scoreRowWidth) - front.Length - back.Length;
		if(paddingLength == 0) { return front + back; }
		string filler = new string('0', paddingLength);
		return front + filler + back;
	}
	public string ConvertSecondsToMinuteSecondFormat(int time, bool forceHours = false) {
		int seconds = time % 60;
		int minutes = Mathf.FloorToInt(time / 60.0f);
		if(minutes < 60) { return (forceHours?"00:":"") + ((minutes<100&&timeRowWidth>13)?"0":"") +(minutes<10?"0":"") + minutes + ":" + (seconds<10?"0":"") + seconds; }
		int hours = Mathf.FloorToInt(minutes / 60.0f);
		minutes %= 60;
		return hours + ":" + ((minutes<100&&timeRowWidth>13)?"0":"") +(minutes<10?"0":"") + minutes + ":" + (seconds<10?"0":"") + seconds;
	}
}