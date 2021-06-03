var plugin = {
	OnInit: function() {
		// Run on init application event
		try {
			if (typeof ReactUnityWebGL.OnInit === "function")
				ReactUnityWebGL.OnInit();
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	},

	OnSceneChange: function(sceneName) {
		// On scene change event
		try {
			if (typeof ReactUnityWebGL.OnSceneChange === "function")
				ReactUnityWebGL.OnSceneChange(Pointer_stringify(sceneName));
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	},

	OnGamePause: function() {
		// On pause game event
		try {
			if (typeof ReactUnityWebGL.OnGamePause === "function")
				ReactUnityWebGL.OnGamePause();
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	},

	OnGameResume: function() {
		// On resume game event
		try {
			if (typeof ReactUnityWebGL.OnGameResume === "function")
				ReactUnityWebGL.OnGameResume();
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	},

	OnPlayerPositionChange: function(x, y, z) {
		// On player position change
		try {
			if (typeof ReactUnityWebGL.OnPlayerPositionChange === "function")
				ReactUnityWebGL.OnPlayerPositionChange(x, y, z);
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	},

	OnPlayerAreaChange: function(areaName, playerName) {
		// On player area changed
		try {
			if (typeof ReactUnityWebGL.OnPlayerAreaChange === "function")
				ReactUnityWebGL.OnPlayerAreaChange(Pointer_stringify(areaName), Pointer_stringify(playerName));
		} catch(e) {
			// No React Module found, ignore this if you are not using React Framework
		}
	}
};

mergeInto(LibraryManager.library, plugin);