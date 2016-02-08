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
public static class Consts {
	public const float YBOTTOM = -1.85f;
	public const float TILE_SIZE = 0.2222222222222222222222222222222222222222222222f;
	public const int DELAY_INT = 2;
}
public static class SoundPaths {
	private const string RootPath 			= "Sounds/";
	private const string SoundEffectsPath 	= RootPath + "sfx/";
	private const string MusicPath 			= RootPath + "music/";
	public const string VoicePath			= RootPath + "voice/"; 
	public const string NarratorPath		= VoicePath + "Narrator/"; 
	public const string CutscenePath 		= RootPath + "cutsceneBeeps/";
	public const string A_BEEEEEE 			= RootPath + "aBeeClean";
	public const string S_Menu_Confirm 		= SoundEffectsPath + "menuconfirm";
	public const string S_Menu_Deny 		= SoundEffectsPath + "menucancel";
	public const string S_Menu_Select 		= SoundEffectsPath + "menusel";
	public const string S_Menu_Pause 		= SoundEffectsPath + "pause";
	public const string S_Menu_Unpause 		= SoundEffectsPath + "unpause";
	public const string S_EyeClose 			= SoundEffectsPath + "closeEye";
	public const string S_EyeOpen 			= SoundEffectsPath + "openEye";
	public const string S_Freeze 			= SoundEffectsPath + "timestop";
	public const string S_Damage 			= SoundEffectsPath + "damage2";
	public const string S_Deflect 			= SoundEffectsPath + "deflect";
	public const string S_Explode 			= SoundEffectsPath + "explode";
	public const string S_Launch 			= SoundEffectsPath + "launch";
	public const string S_Money 			= SoundEffectsPath + "money";
	public const string S_Shield 			= SoundEffectsPath + "shield";
	public const string S_ShieldHit 		= SoundEffectsPath + "shieldHit";
	public const string S_ShiftRow 			= SoundEffectsPath + "shiftRow";
	public const string S_ShiftRows 		= SoundEffectsPath + "shiftRowMult";
	public const string S_Ding		 		= SoundEffectsPath + "ding";
	public const string S_Applause	 		= SoundEffectsPath + "applause";
	public const string M_Credits 			= MusicPath + "Credits";
	public const string M_Cutscene 			= MusicPath + "Cutscene";
	public const string M_InGame 			= MusicPath + "Main";
	public const string M_InGame_Intense 	= MusicPath + "Intense";
	public const string M_Menu 				= MusicPath + "Menu";
	public const string M_Title_Default 	= MusicPath + "Title";
	public const string M_Title_DerivPath	= MusicPath + "ThemeDerivs/";
	public const string M_WinMusic			= M_Title_DerivPath + "Group";
	public const string M_LoseMusic			= M_Title_DerivPath + "GroupSlow";
}
public static class SpritePaths {
	private const string RootPath				= "Spritesheets/";
	private const string HUDPath 				= RootPath + "HUD/";
	private const string MenuPath 				= RootPath + "Menus/";
	private const string TilePath 				= RootPath + "Tiles/";
	public const string BGPath	 				= RootPath + "Backgrounds/";
	public const string CharPath 				= RootPath + "CharSel/";
	public const string CreditsPath				= RootPath + "Credits/";

	public const string Borbs					= CreditsPath + "borbs";
	public const string CutsceneSkipBox 		= MenuPath + "cutsceneskip";
	public const string MouseCursor 			= MenuPath + "mouseCursor";
	public const string OptionsCollider 		= MenuPath + "optionsBox";
	public const string SoundTest	 			= MenuPath + "SoundTest";
	public const string SoundTestCollider		= MenuPath + "SoundTestCollider";
	public const string SoundTestControlButtons = MenuPath + "SoundTestControlButtons";

	public const string BackButtons				= MenuPath + "backButtons";
	public const string CancelButtons			= MenuPath + "cancelButtons";
	public const string ConfirmButtons			= MenuPath + "confirmButtons";
	public const string PauseMenus 				= MenuPath + "pauseMenus";
	public const string LeftButtons 			= MenuPath + "leftButtons";
	public const string ShortButtons 			= MenuPath + "shortButtons";
	public const string LongButtons 			= MenuPath + "longButtons";
	public const string RightArrows 			= MenuPath + "rightArrows";

	public const string HighScoreSlab 			= MenuPath + "HighScoreSlab";
	public const string HighScoreInput 			= MenuPath + "enterText";

	public const string BigOptionInfoBox 		= MenuPath + "persistDataInfo";
	public const string PersistDataBack			= MenuPath + "goBack";
	public const string PuzzleSmallBlocks 		= MenuPath + "puzzleblocks";
	public const string PuzzleSmallBG	 		= MenuPath + "puzzleblocksbg";
	public const string PuzzleInfoBox 			= MenuPath + "puzzleinfo";
	public const string PuzzleListBox 			= MenuPath + "puzzlemenu";
	public const string PuzzleCursor 			= MenuPath + "puzzlemenuselector";
	public const string ScoreCollider 			= MenuPath + "scoreBox";
	public const string VerticalArrows 			= MenuPath + "scorename";
	public const string InputIcons 				= MenuPath + "WheresTheAnyKey";
	public const string TutorialBox 			= HUDPath + "bigbox";
	public const string FullBlackCover 			= HUDPath + "Black";
	public const string DemoText 				= HUDPath + "DEMO";
	public const string DestroyTile 			= HUDPath + "destroyTile";
	public const string DialogBox 				= HUDPath + "dialogTile";
	public const string EyeSheet 				= HUDPath + "eyes";
	public const string EyeSheetScopo 			= HUDPath + "eyes_sp";
	public const string TransparentBlackCover 	= HUDPath + "finnCover";
	public const string GuideCircle 			= HUDPath + "guide";
	public const string InfoBox 				= HUDPath + "infoTile1P";
	public const string InfoBoxCampaign 		= HUDPath + "infoTileCampaign";
	public const string Launch_Particles		= HUDPath + "launchParticles";
	public const string RoundStateIcons 		= HUDPath + "roundIndicators";
	public const string RecoveryTile 			= HUDPath + "recoveryTile";
	public const string TileCursor 				= HUDPath + "tileCursor";
	public const string HiContastTileCursor 	= HUDPath + "tileCursor_HI";
	public const string LogoText 				= HUDPath + "titleText";
	public const string DetailsBox 				= HUDPath + "winBox";
	public const string NewUnlocks				= HUDPath + "hereComesANewChallenger";
	public const string RepairText 				= HUDPath + "repairsText";
	public const string WinnerTexts 			= HUDPath + "congratsText";
	public const string White					= HUDPath + "white";
	public const string WhiteSingle				= HUDPath + "whiteSingle";
	public const string Texts					= HUDPath + "text";
	public const string BGTileBack	 			= BGPath + "bgFull";
	public const string BGBlackFadeCharSel 		= BGPath + "blackFadeCharSel";
	public const string BGBlackFadeLeft 		= BGPath + "blackFadeLeft";
	public const string DefaultBG 				= BGPath + "Default";
	public const string OpeningAnimTiles		= CharPath + "OpeningRects";
	public const string EyeSparkle				= CharPath + "EyeSparkle";
	public const string CharSelCursors 			= CharPath + "charSelCursors";
	public const string CharSelCursorsWhite 	= CharPath + "charSelCursors2";
	public const string CharSelCursorsAll		= CharPath + "charSelCursors3";
	public const string CharSelSheet 			= CharPath + "charSelGrid";
	public const string CharSelSheetWhite		= CharPath + "charSelGrid2";
	public const string CharSelSheetAll			= CharPath + "charSelGrid3";
	public const string CharSelProfiles 		= CharPath + "CharSelPlayers1";
	public const string CharSelVictoryIcons 	= CharPath + "completionIcons";
	public const string CharFullShots	 		= CharPath + "CharFull";
	public const string CharGroupShot 			= CharPath + "GroupShot";
	public const string CharNames	 			= CharPath + "names";
	public const string LockedRow 				= TilePath + "LockedRow";
	public const string TileShape 				= TilePath + "AllTileLarge_Shapes";
	public const string TileBlock 				= TilePath + "AllTileLarge_Tiles";
	public const string TileOverlay 			= TilePath + "AllTileLarge_Overlays";
	public const string Glows	 				= TilePath + "glows";
	public const string Shield 					= TilePath + "shield";
	public const string TileCover				= TilePath + "tileCover";
	public const string Zapper 					= TilePath + "zappyboots";
	public const string ColorblindSuffix 		= "_CB";
}