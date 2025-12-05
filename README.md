# Nacho's Achievements

This mod adds 30 achievements to Lethal Company, with more to come in the future.

All achievements and their progress can be accesed via the pause menu when hosting or joining a save file.

All achievements are tracked independently of which save file you're using and are saved to disk inside `steamapps\common\Lethal Company\BepInEx\config\NachosAchievements`.

If you wish to reset your achievements, either delete `achievements.json` in the folder specified above or enable the debug option. (Make sure to turn this option off if you wish to keep your achievements afterwards!) (This does not include save-dependent achievements, such as the "single run" type of achievement.)

## Why?

I love going for achievements in this type of games (PEAK is a great example), and I always felt Lethal Company should have a system for achievements. So, I made my own.

As of update 1.1.0, I added a way for any mod developer to add their own achievements without code, because I really liked the idea. For information on how to add achievements, refer to the developer section of this README.

## Contact

If you find any issues and you wish to report them, you can reach out to me in this mod's thread on the Lethal Company Modding discord. Thank you!

# Developer - Adding Achievements (WIP)

## Making the JSON File

To begin creating your own achievements, first, go to your mod manager's plugins folder. This is the folder that stores every mod you have in your profile.

This mod checks for every folder inside the currently used profile, and checks for a file named exactly `achievements.json`.

Which means, to get started, create (or go to) your mod's root folder, and in this folder create a file named `achievements.json` (make sure .json is the file's extension!)

Inside this file you will store your achievements. Here's a basic example of how this file could look like:

```
{

    "My Achievement": { // Achievement name

        "progress": "0", // Achievement starting progress. Usually should stay at "0".

        "completed": "False", // Wether the achievement starts completed or not. Usually should stay at "False".

        "count": "1", // How many times the achievement has to be triggered in order to gain the achievement.
        
        "callback": "On Scrap Collected", // When the achievement should be triggered.

        "scrap": "Any", // In this specific callback ("On Scrap Collected"), you can choose which type of scrap triggers the achievement using the "scrap" variable.
        // Leave at "Any" (or skip this argument altogether) so it triggers for all scrap.
        // Different callbacks have different variables.

        "local": "True", // Wether the achievement should trigger only for the user that accomplished the goal, or the entire team of people in the current lobby.

        "description": "Collect a piece of Scrap" // Achievement description. Will appear when hovering over your achievement in the achievement list.

    }
}
```
## Callbacks

There's 3 variables that exist for nearly every single callback: <br>
`"moon"`: If specified, the achievement will only be added if the current moon ID is equal to the ID specified in this parameter. <br>
`"local"`: If set to "True", the achievement will only be added for the user that accomplished the goal. <br>
`"challenge"`: If set to "True", the achievement will only be added if the user is in a challenge moon file. <br>

I will call these "default variables". Assume every callback has these unless specified otherwise.

There are also a few special variables that you can set for specific behaviour regarding achievement progress: <br>
`"single run"`: If set to "True", the achievement will be save-file dependent, and progress will not stay between files. *(Note that completion status is permanent)* <br>
`"single day"`: If set to "True", the achievement will reset its progress after the day ends or if you disconnect during the day. <br>

There is also a `"count callback"` variable. This will replace your current achievement count with whichever you specify: <br>
`"Unique Scrap Total"`: `"count`" will equal the amount of unique scrap items in the game, including modded. <br>
`"All Scrap Today"`: `"count"` will equal the amount of scrap that day, both collected and uncollected. Will update whenever any callback is triggered, if the `"moon"` variable is equal to the current level ID or "Any". <br>

Some variables support the `"Unique"` keyword. If a variable is set to this keyword, e.g `"scrap"`, you will only progress on the achievement if the element passed on by the callback for this variable hasn't been passed before. In this case, it will only update if the scrap acquired hasn't been gotten before. If paired with the `"single run"` variable, the progress for this achievement will be saved on the host save file, otherwise, it will be saved to your local general save file.

Here's a list of every callback, when they trigger, and other variables they output:

| Callback                  | Other Variables                            | Description                                                        |
|---------------------------|--------------------------------------------|--------------------------------------------------------------------|
| `"On Player Join"`        | `"players"`                                | `"On Player Join"`: Triggers when a player joins the lobby. **(WIP, use at own risk)** <br> `"players"`: Exact amount of players that have to be in the lobby to trigger. <br> **DOES NOT CONTAIN** **DEFAULT VARIABLES.**
| `"On Scrap Collected"`    | `"scrap"`, `"amount in ship"`              | `"On Scrap Collected"`: Triggers when scrap is brought to the ship. <br> `"scrap"`: Which type of scrap has to be brought to the ship to trigger. <br> `"amount in ship"`: Progress will equal the amount of scrap equal to the specified "scrap" variable if set to "True", all scrap if set to "False".
| `"On Item Collected"`    | `"scrap"`, `"amount in ship"`              | `"On Item Collected"`: Triggers when an item (non-scrap item) is brought to the ship. <br> `"scrap"`: Which type of item has to be brought to the ship to trigger. <br> `"amount in ship"`: Progress will equal the amount of scrap equal to the specified "item" variable if set to "True", all items if set to "False".
| `"On Kill Enemy"`        | `"weapon"`, `"enemy"`                       | `"enemy"`: The name of the enemy to be killed to trigger. (Has to be equal to the internal name, a.k.a EnemyType.enemyName, can be seen through Imperium) <br> `"weapon"`: The weapon the enemy has to be killed with to trigger. Accepts `"Shovel"`, `"Knife"`, `"Worm"`, `"Shotgun"`, `"Lightning"`, `"Landmine"` and `"Easter egg"`.
| `"On Enemy Sound Heard"`   | `"sound"`, `"soundMinDistance"`             | `"On Enemy Sound Heard"`: Triggers when an enemy hears or is hearing any sound. <br> `"sound"`: Triggers if the sound ID heard is equal to this variable. <br> `"soundMinDistance"`: How close the enemy has to be at least while hearing this sound for it to trigger.
| `"Enemy Stared At"`      | `"enemy"`                                | `"Enemy Stared At"`: Triggers when you have constant line of sight to the enemy for at least 5 seconds. <br> `"enemy"`: The name of the enemy to stare at to trigger.
| `"On Level Finished Loading"` | None. `"local"` variable is excluded. | `"On Level Finished Loading"`: Triggers when a level finishes loading. Mostly used for internal purposes.
| `"On Tree Destroyed"` | None                                        | `"On Tree Destroyed"`: Triggers when a tree is destroyed.
| `"On Log Collected"` | None.  `"local"` variable is excluded.       | `"On Log Collected"`: Triggers when a log is collected.


