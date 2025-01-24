# AUI_Project_2024
A repository for our Advanced User Interfaces Project PoliMi


## Installation
First of all you will need to install __Unity__. Once downloaded the installer from the [__Unity webpage__](https://unity.com/download), you will need to log in with your account (or create one). Then install Unity, paying attention to <ins>__install the version 6000.0.25f1__</ins>. You can install, if you do not have it,  also __VisualStudio__ directly from here. No dependencies other than ones proposed in the installer are needed for now. 
<p align="center">
<img src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSG1Grii7CclN2rQzJ_IRGOb8zx-GrlvU-gHA&s" alt="drawing" height="100"/>  <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/2/2c/Visual_Studio_Icon_2022.svg/2048px-Visual_Studio_Icon_2022.svg.png" alt="drawing" height="100"/>
</p>

Then clone the repository from this GitHub page. Once downloaded it is time to install the dependencies.

### Python Dependencies
Of course the starting point is [__install Python 3.12__](https://www.python.org/downloads/) and  __install pip__ through the terminal command `py -m ensurepip --upgrade`.
You'll need then to install _Flask_ and _openai_ through _pip_. Run the command `$ pip install Flask, openai` into the terminal.

### Unity
In the upper bar go to _PMG/1-Scene Setup_ and accept in the pop-up window. Then click on _PMG/2-Addressable Setup_ and accept again. Once done you will need to open the scene __Assets/Scenes/SampleScene__ from the _Project_ tab. Go again on the _PMG_ tab and click _Prepare Build_. Inside it select _"Standalone Windows 64"_ from the dropdown menu and prepare the build. Finally go to _Window/Asset Management/Addressables/Groups_, under _"Play Mode Script"_ select _Use Asset Database_ and under _"Build"_ select _New Build/Default Build Script_. Ignore errors that may appear in the console.

#### Multiplayer
To allow for multiplayer, the two PCs must be under the same LAN. Each one of the players (at least the _client_) should have the IP address of the other player and insert it into __Network Manager's Unity Transport__ component, under the field _Address_ 

You should now be ready to run the game by clicking the Play button.


