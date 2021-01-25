# DoomScrolls -> Retro 3D FPS (Turn-Based)

![DoomClone](Title.png)
![DoomClone](SalesPitchImage.png)

## Notes/Know Bugs:

> *Important* The serverClients do not currently exit, so the program must be closed from task manager if doing multiplayer, otherwise there is a memory leak *Important*
*Fixed as of 23/01/2021*

> A significant amount of input may result in some commands not being run

>No Turn Limit At The Moment
*Fixed as of 23/01/2021*


## Features(Planned In Brackets)

2+ Player Multiplayer (If you know what IP Address to connect to)
Animated Enemies
Turn-Based FPS combat
Animated Player Weapons
Randomly Generated Weapons
Random Map Generation
(Intractable log files and story)
(Multiple playable resolutions)
Multiple Editable Player Skins
(Save states)

## View of progress
**Displaying user view as 3d boxes**

Below is the process used to draw a '3d' world.

![DoomClone](CellLines.PNG)

**Displaying user view as 3d boxes, with random color between 'points' to make**
**it look like walls/floors**

![DoomClone](ColorCellLines.PNG)

**Displaying user view without boxlines, giving a 'real' impression**'

![DoomClone](ColorCells.PNG)

**Displaying user view with scaled (based on distance) sprites**

![DoomClone](ColorCellsEnemies.PNG)

**Displaying user view with 1:1 window size scaled 'player character view'**

![DoomClone](ColorCellEnemiesGun.PNG)


## Latest Build

*23/01/2021 - 0.1a- Gameplay*

![MapMakerImage](Gameplay.png)

## Latest Update Notes:
Animated Weapon Alpha

To Sum Up:
Remove Memory Leak bug for good practice 

Add new Enemy Unit type- SoldierGun for fun

Add new Animated Projectile type - target (For when SoldierGun targets player) for variety in combat

Add New Skin- Rambo for a request I got on TikTok

Add New Audio Track - Combat.Wav for player enjoyment

Add Animated player HUD to make game appear more professional 

Add Synced music playing - both background music and sound in-game for better immersion 

Add debug hitboxes for debug purposes

Add UnitType to keep consistent unit generation

Add DebugTimer for better analytics 

Add Corpse Object to allow player to move through bodies 

Add Loading screen for aesthetic 


```

20/01/2021{
	HUD.cs -> Add Hud class that builds and conains the logic and graphics for a Hud icon, similar to how the 'gun.cs' class works.
		This is so Huds can be tested independently and to provide a more 'game-y' excperience.
	HUD.cs -> DrawHud() -> Add method that is passed Graphics and draw a HUD to it to be able to call the Hud class
	HUD.cs -> Hud() -> Add handling in constructer that gets a subimage for each of a given player's 'idle' aniamtions.
		This subimage is a face, and is stored for each frame in an array. This lets an aniamted player face be easily drawn.
	Form1.cs -> BeginSession() -> Add a Hu instance 'playerHud' to test Hud
	Form1.cs -> UpdateForm() -> Modify so end of the draw function draws playerHud with each global anim frame to test Hud
	Globals.cs -> Add bool 'drawHUD' so that Hud can be toggled. This is to make good developer content

	TODO: Comment 'DrawHud()' , Create more weapon versions , test new enemy
}
22/01/2021{
Issues Encountered:
	Using System.Media.MediaPlayer did let the application run multiple synced soundfiles, however since the Mediapalyer class 
		also handles video, it re-set the size of the application, causing issues.
		This was resolved by using mci Strings instead.

	Form1.cs->TimerCall() -> Add a check on each 'tick' to see if the time tha had passed since music last played
		is the same ammount of time a repeatable song palys for. If it is, it calls 'PlayMusic' again. This is to have
		looping music in the background.
	Form1.cs->BeginSession() & Form1() -> Add the full URL to both calls of 'PlayMusic()', rather than local URL's from 
		application launch. This is so the new 'MCIStringSend' commands work correctly.
	Globals.cs -> PlayMusic() & StopMusic() -> Add PlayMusic and StopMusic commands that use Direct exernal MCI String calls
		to a system's audio player, based on a song file path. This is so Music can be played with in-game sound effects
}
23/01/2021{
Issues Encountered:
	When changing Entity's Image Draw functions from dynamically cropping and scaling an image to scaling a base array
		of root images, iamges with scaleRatios '1' would not display.
		This was due to the scale ratio drawing 'scaleRatio-2 * width' into a cell, and not scaleRatio/2.
		This meant scale 1 things were drawn 1 length to the left and up.
		This was resolved by changing it to  'scaleRation/2 * width.

	Raycast projectiles would spawn and run on the same turn, making SoldierGun enemies instantly hit the player.
		This was resolved by giving the projectile a 'isRaycast' and 'isRaycastJustMade' variable.
		This meant before a raycast projectile actioned, it coudl be checked to see if it had just been made.

	Globals.cs -> Add 'DrawOutlines' bool. This is to be able to test hit-box receptivness. Add 'PlayMusic' bool to 
		allow other classes to play or not paly music as required.

	Globals.cs -> WriteDebug() -> Add Debug Method Prin statement and 'writeDebug' bool so Debug outputs can be switched
		at to generify debug statements. 

	Globals.cs -> Add Enum 'UnitType' to differentiate enemy units. 

	CursorObject->Draw() -> Add a red rectangle being drawn around x,y co-ord if Globals.DrawOutline is true for Debug.

	Entity ->Draw() -> Add a red rectangle being drawn around true outline co-ord if Globals.DrawOutline is true for Debug.
		Modify Draw so it resizes one baseIamge file from an array isntead o croppign each Draw.
		This is to make the program more efficent. 
		Remove akward handling of static images comapred to aniamted.
		New system uses one Draw function. If static image, this is defined under the 'animated' variable.
		If not animated, Draw will only ever day index '0' of the new image array.
		This is to ahve cleaner code nad better run times

	Entity ->Entity() -> Add a Cosntructor that takes a base animation image. This constructor fills a new Bitmap array
		in Entity called 'baseImages'.
		This cosntuctor is repeated for all inherited classes.
	
	
	Unit.cs -> Added variable unitType to differentiate enemy units. 

	Unit.cs-> SetUpUnit() -> Modify Constructor to take UnitType Enum instead of 2 Bitmaps. Method will programatically get
		correct BitmapFiles for Idle and death anims. This is to refractor refrences to unit type.

	Unit.cs -> GetUnitType() -> Add GetUnitType Method to return Unit type. This is to so MapGen Screen can show enemies

	Unit.cs -> CreateProjectile() -> Remove the String of Projectile type. Create Projectiles based on instance's 
		'UnitType' instead. This is for good coding practice.

	Unit.cs -> Draw() -> Inherit the soundpalying parts of Draw() from Entity.cs. 
		This is so sound effect handling only happens with Units. 

	Unit.cs -> Kill() -> Modify so Unit set to not alive and cellGrid co-ords of dead unit removes that unit
		and creates a corpse with same unit type. This is so palyers can move through corpses and to not handle
		unit functions when unessecary as unit is dead.

	Cell.cs -> CreateUnit() -> Modify Unit Setup Call to match Unit constructor change above 

	MapFormGen.cs -> Move MapFormGen to 'Screens' Subfolder for consistency 
	
	MapGenForm.cs -> paintDrawing() - > Modify to rename to 'PaintDrawing'. Modify to draw red cell where enemy is.

	Resources -> Image -> Added new Idle, death and shooting animation images for a new Enemy type 'SoldierGun' for
		variety.

	Player.cs-> Add 'actionPoints' int to limit player actions per turn.

	Player.cs -> ChangeActionPoins() -> Add Method to modify Player actionPoints. It returns the remaining points so 
		program can handle player running out of action points;

	Hud.cs -> DrawHud() -> Modify so  background draws across Screen for aesthetic. Made Hud draw remaining ActionPoints for 
		player information.

	Form1.cs-> BeginSession() -> Modify the 'enemy placing' component of the method to randomly pick either Devil or 
		SoldierGun for variety. Modify 'Setup Unit' calls to match the new Unit constructor using a UniType enum for simplicity.

	Form1.cs() -> RunCommands () -> Modify so actioning players lose their ActionPoints by 1 for certain commands running.
		If the retuned value is less than 1 9Out of actions), playerEndTurnArray is modified to be true for them, 
		preventing further action. This is too add game mechanics and turn limitation.
		Modify "SEE" Command to use new Uni'SetUnit()' command using Globals.UnitType.
		Modify "ETS", End Server Turn command to reset local Action points and all palyer's action points as server 
			so mechanics work in multiplayer
		Modify "SHE", Shoot Enemy command so the index of the enemy hit can be -1. If this happens no Enemy is damaged
			but player still loses action points. This is to add game mechanics.
		Modify "SEP", Set Player on Client Side so that the last digit is the AP to give the player. This is for 
			consistency across server+client.
		Add Debug Time Test and average time total ant end of function. This is o test changes to effeciency.

	Form1.cs() - > Modify so that any created threads 'BackgroundThread' propert true.
		This means on Form close, all child threads are killed.
		This is to prevent memory leaks.

	Form1.cs -> CheckPlayerHasAP() -> Add Method to confirm player of set ID has ActionPoints left, if not adds command
		to end that palyer's turn. his is to autoamte turn timings.

	Form1.cs -> CursorHandler() -> Modify so it always sends a "SHE" command. This is due to the SHE  command
		now being able to remove action points but not cause damage. 

	Form1.cs->OnKeyUp() - . Add 'Globals.DrawHud' Toggle on '4' being pressed and 'Globals.DrawOutlines' 
		being toggled on '6' for Debug. Add '7' to Toggle Globals.PlayMusic since music can get annoying.
		'7' handler also calls 'Globals.StopMusic' if toggling to false.

	Projectile.cs -> RunProjectile() -> Add Variable 'Raycast'. This is so there can be different types of attacks.
		Modify so if projectile 'rayast', it will act liek a regualr projectile hitting a wall if it at is target co-ords.
		This is so, if it is made ON the target co-ords, it gives palyers one turn to dodge.

	Projectile.cs -> Projectile() -> Add Refrence to a 'Unit - sender' in Constructor.
		This is so projectile can be managed from the sending unit's 'proj' array.
	
	Corpse.cs -> Add 'Corpse' class that inherits from Entity so that player can move through dead units while still 
		displaying bodies.

	Cell.cs -> CreateCorpse() -> Add Function to produce movable-through entity, similar to projectile.
	Cell.cs -> Draw() -> Modify to call Entity.Draw of 'Corpse' child of Cell if it exsists. This is to render corpses.

	}
	25/01/2021{
	Form1.cs-> BeginSession() -> Modify so form state is set to  'pauseForInfo = true' While world initalizes.
		This is so there isn't a freeze while world is built.

		Form1.cs -> UpdateForm1() -> Modify to show new Loading Screen Image instead of "Connecting to server/ building world."
		When pauseForInfo is true. This is to have a more professional design.
		The image used has space for a loading bar between width/2,height-(height/15)

		Form1.cs-> Added "LoadPercent" variable so loadscreen can draw dynamic loading bar.

		Form1.cs -> Form1() && Whole application-> Add a PrivateFontCollection to load new custom font 'DoomFont' tts.
			This replaces all prior calls to other font families in the code

		Form1.cs -> AddLoadValue() -> Add Method that autoamtically updates 'LoadPercent' varialbe and refreshes screen
			to show dynamic loading.

		Cell.cs -> CreateUnit() - > Modify so method takes a backup PNG for Player Aniamtions, since there is no set Skin
			associated wih the 'Globals.UnitType.Player' enum. This works similar to Cell.CreateCorpse();
	
	}
```

## Latest Updates

*23/01/2021 - Alpha 'Gameplay' release* 

![PromoImage](Gameplay.png)

*20/01/2021 - Animated HUD Alpha* 

![PromoImage](HUDImage.png)

*13/01/2021 - Animated Weapon Alpha* 

![PromoImage](Demo.gif)

*09/12/2020 - Menu build*

![PromoImage](PromoImage.PNG)

*29/07/2020 - Map maker*

![MapMakerImage](MapMaker.PNG)

*15/05/2020 - Multiplayer*

![MultiplayerImage](Multiplayer.PNG)

## Next Build

End Of December -Content Build  **Delayed from September*

* Enemy Variety and attack patterns

* Rehaul of weapon System

* Main Menu

* Full documentation

* Animated Weapons

* Turn limitations (Only x moves per turn, turn ends on shoot ect.)

* Story logs held in computors


## Skill developing

I planned on this project improving my skills in the following:

>Understanding of 3-D representation

>Proof of skill development since ![Gun Run](https://github.com/StarshipladDev/GunRun)

>C# Code practice

>Correct SDLC practice

>Pixel Art and Animation

## Installing and Compiling:
At the moment, the program can be run by doing the following:

Unzip the "Executable" .ZIP folder.

Unzipping the 'Resources.zip' folder & placing it in the  the 'Executable'folder.

When you run 'DoomScrolls.exe' , a map generator will pop up.
Generate maps until you find one you liek then close the window.

Each turn you have 5 action points. Shooting, moving or turnign will use 1 action point.

Enemies will action after you end your turn or run out of AP. Your AP will then be restored.

Devils fire fireball that cause 10 damage and keep moving.

Gun Soldiers put a target on you. If you are in that cell the next enemy turn, you will take 5 damage.


The *bin/Debug/config.xml* file contains the option to select your server address to connect to and your clientName when you connect.

This file also contains the skin your palyer will use. It mus be a single digit.
Current skins are 

1) Default army skin

2) DanyelDev - [Twitter](https://twitter.com/DevDanyel)

3) CoolJosh3k - [Twitter](https://twitter.com/CoolJosh3k)

4) NerdsipPodcast - [Twitter](https://twitter.com/nerdssippodcast)

5)	Nibby - [Twitter](https://twitter.com/NibbyCodes)

6) Can't Even Bro Avatar - [Twitter](https://twitter.com/CantEvenBro)

7) Rambo - As requested by TikTok user @meth4kids


The program can be run by opening *Executable/DoomCloneV2.exe*

Any resource in *Executable/Resources* can be replaced with a matching file type if the name stays the same.

The IP Address of the server can be found by opening up a command prompt on the server's computor, and typing 'ipconfig'

The Program currently has the following commands:

*Space* - End your turn. You will be frozen until all connected players have ended their turn.

*W* Move your player Forward

*S* Move your player Back

*A* Move your player Left

*D* Move your player Right

*Q* Replace the gun currently equipped

*O* Start a server on your local ip address

*P* Start a client that connects with the address listed in config.xml

*K* If client created, send a hello message over the network stream

*L* Create a second local client. This client will not perform actions, but act as a faux connection

*V* If they exist, move players[1] to the right

*1* or *Spacebar* Display lines of each cell

*2* Toggle color of cells

*3* Toggle drawing player gun

*4* Toggle displaying debug text

*5* Toggle displaying playerHUD

*6* Toggle outlines of units

*7* Toggle background music playing

To Fire, click on an enemy who is alive. A cursor will appear,
and the next 3-4 clicks will shoot at the centre of the cursor.
You can right click to exit firing.


