# 3D FPS
This is a simple PlaceHolder README while I File dump my current progress.
I've been working on this project for about 10 days, and am now trying to better document my
progress and follow better SDLC practice.
## Features(Planned In Brackets)

* Add features here :P

## View of progress
**Displaying user view as 3d boxes**
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

21/04/2020 - 0.01a- Initial Build

## Latest Update

16/05/2020 - Synced worlds over networks, synced damage models

## Next Build

Week ending 30/05/2020

## Skill developing

I planned on this project improving my skills in the following:

>Understanding of 3-D representation

>Proof of skill development since ![Gun Run](https://github.com/StarshipladDev/GunRun)

>C# Code practice

>Correct SDLC practice

## Installing and Compiling:
At the moment, the program can be run by unzipping the 'Resources.zip' folder & replacing the bin/Debug 'Resources' folder with it
The program can be run by opening *bin/DebugDoomCloneV2.exe*
The *bin/Debug/config.xml* file contains the option to select your server address to connect to and your clientName when you connect
The Program currently has the following commands:
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

To Fire, click on an enemy who is alive. A cursor wll appear,
and the next 3 clicks will shoot at the centre of the cursor.

