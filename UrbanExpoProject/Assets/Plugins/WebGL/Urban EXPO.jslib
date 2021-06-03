var plugin = {
	OnInit: function() {
		// Run on init application event
		if (typeof ue_OnInit === "function")
			ue_OnInit();
	},

	OnSceneChange: function(sceneName) {
		// On scene change event
		if (typeof ue_OnSceneChange === "function")
			ue_OnSceneChange(Pointer_stringify(sceneName));
	},

	OnGamePause: function() {
		// On pause game event
		if (typeof ue_OnGamePause === "function")
			ue_OnGamePause();
	},

	OnGameResume: function() {
		// On resume game event
		if (typeof ue_OnGameResume === "function")
			ue_OnGameResume();
	},

	OnPlayerPositionChange: function(x, y, z) {
		// On player position change
		if (typeof ue_OnPlayerPositionChange === "function")
			ue_OnPlayerPositionChange(x, y, z);
	},

	OnPlayerAreaChange: function(areaName, playerName) {
		// On player area changed
		if (typeof ue_OnPlayerAreaChange === "function")
			ue_OnPlayerAreaChange(Pointer_stringify(areaName), Pointer_stringify(playerName));
	}
};

mergeInto(LibraryManager.library, plugin);