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
public class InputCore:ObjCore {
	#region "Setup"
	protected InputMethod controller;
	public void SetController(InputMethod m, Dictionary<int, string> bindings) { controller = m; controller.Initialize(bindings); }
	public InputMethod GetController() { return controller; }
	#endregion
	#region "Input Handling"
	virtual public bool shiftLeft() { if(controller == null) { return false; } return controller.G_ShiftLeft(); }
	virtual public bool shiftRight() { if(controller == null) { return false; } return controller.G_ShiftRight(); }
	virtual public bool shiftAllLeft() { if(controller == null) { return false; } return controller.G_ShiftAllLeft(); }
	virtual public bool shiftAllRight() { if(controller == null) { return false; } return controller.G_ShiftAllRight(); }
	virtual public bool launch() { if(controller == null) { return false; } return controller.G_Launch(); }
	virtual public bool pause() { if(controller == null) { return false; } return controller.Pause(); }
	virtual public bool back() { if(controller == null) { return false; } return controller.M_Cancel(); }
	public bool launchOrPause() { return this.launch() || this.pause(); }
	#endregion
}