# Ahm
Ahm (ah-m) is a Discord Bot for a Destiny 2 clan that primarily gives roles to users who have unlocked titles in-game.

## Title System
When a user enters the command [TitleAdd](https://github.com/TuneFlat/Ahm.DiscordBot/blob/main/Modules/RoleModules/TitleRoleModule.cs) The [Destiny Service](https://github.com/TuneFlat/Ahm.DiscordBot/blob/main/Services/DestinyService.cs) is then used.

First, the user's Destiny information is grabbed from an sqlite3 database by using the user's Discord Id. 

Then, the [GetProfile](https://bungie-net.github.io/multi/operation_get_Destiny2-GetProfile.html) endpoint from Destiny's API is then used to get a Dictionary of player records, which contain progress on triumphs and titles. 

Upon instantiation of the Destiny Service, all title Ids, or hashes, are gained from Destiny 2's manifest also referred to as the mobileworldcontent. There are some titles that have the same name with different records associated with them. The data structure chosen to handle this was a Dictionary<string, List<string>>, where the key is the title name and the value is a list of Ids/hashes associated with the title.
 
Each Id/hash associated with the title is then enumerated through to search the user records for the completion progress of the title.

If the user has the title then a list of roles from the Discord server will be queryed to find a role with the name matching the title and the role will then be given to the player.

