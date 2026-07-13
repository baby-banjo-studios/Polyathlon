# Adding and Removing Characters

## Adding a New Character

### Part 1: Preparing your character for import into Unity

#### If using Adobe Fuse, we need to first extract the character's textures and rig the character so that it may be animated.
1. First, we must export our character from Adobe Fuse. We will begin by exporting the character's textures.

    1. In Adobe Fuse, go to File->Export->Export Textures.
    2. A new window will appear. Enter the desired Character Prefix (usually just the character's name), select your desired output directory, ensure that the "Export in new folder" option is checked, and set the "Configuration" to be "Unity 5." Then click "OK."
    3. This will create a new folder containing all of your character's textures within the selected output directory, the title of which will be the Character Prefix that you specified.
2. Next, we will send our character to [Mixamo](https://www.mixamo.com/#/) to be rigged, essentially allowing for the character to be animated.
    1. Within Adobe Fuse, export the character and upload to Mixamo for rigging.
    2. When the auto-rigging is complete, you will see your character standing in the window, looking around. Click "Finish."
    3. On the next page, Mixamo will prompt you to either download or animate your character. Since the animations we will be using are already in _Polyathlon_, we will click "Download."        
        1. When prompted, set the format to "FBX for Unity(.fbx)," and leave the pose as "T-pose." Then click "Download."
        <img align="right" width="441" height="136" src="Images/download.JPG">
    4. Put your downloaded FBX in the same directory that the folder of your character's textures is located within.
        1. _IMPORTANT_: Do not put the FBX in the folder _with_ the textures, rather, put the FBX in the folder that houses the folder with the textures.
3. You can also upload characters made in programs other than Adobe Fuse directly to Mixamo and have them rigged and animated just like they would be if they were from Fuse.

### Part 2: Add your character to _Polyathlon_
#### After rigging our character and obtaining its textures, it is time to import the character into our Unity project and give it all the properties that a playable character in the game has.
1. Open the _Polyathlon_ Unity project if you haven't already.
2. The project has been set up to use [this script](https://forum.unity.com/threads/script-for-importing-adobe-fuse-character-model-into-unity-fixes-materials.482093/) for properly importing characters that were created with Adobe Fuse. This script provides us with the "Mixamo" drop-down in the toolbar at the top of the window.
3. Click Mixamo->Import Character. A file explorer window will appear prompting you to select your character's FBX file.
    1. Assuming that your character's folder of textures is located within the same directory as the FBX and has the same filename, the folder full of textures will be imported with the character, and materials reflecting the character's appearance within Adobe Fuse will automatically be created.
4. Once the character has been imported, the character will appear within your scene as the child of a newly created GameObject called "Mixamo." You can delete the Mixamo GameObject and its child (your character) from the hierarchy, as we will not be using them. We will instead be using the prefab that was created.
5. Navigate to your new character's imported model folder at `Assets/Mixamo/<your_character's_name>/Model`. Click on the imported FBX file for your character. In the Inspector, go to the "Rig" tab and change the "Animation Type" to `Humanoid` and click "Apply".
6. In the "Polyathlon" drop-down in the toolbar at the top of the window, click "Character Creator". This will open the "Create character" set-up wizard.
    1. Add your character's prefab (located at `Assets/Mixamo/<your_character's_name>/<your_character's_name>.prefab`) to the "Character Model" field.
    2. In the "Character Name" field, type your character's name as you want it to appear in the game and filesystem.
    3. Leave the "Ragdoll Profile" as the default "Mixamo" ragdoll profile.
    4. Leave "Preview Only" unchecked, as we want to import the character as a playable character instead of just creating a preview prefab.
    5. Check "Add to Roster" so that the character will appear in the character roster.
7. Click "Create". This will create a "preview" prefab for your character in `Assets/Characters/Previews`, a player prefab for your character in `Assets/Characters/Players`, and a NPC/CPU prefab for your character in `Assets/Characters/NPCs` (both the player and NPC prefabs rely on the preview prefab). This will also create a character registry for your character in `Assets/Characters/Registries`, and it will add your characters registry to the list of character registries at `Assets/Characters/CharacterList.asset`.
8. If you play the game from the Main Menu scene, you should see that your character has been added to the character selection screen with a blank icon. If you select your character and then play a race, you should see your character appear in the game, fully animated and playable as expected.
9. To add an image to appear in your new character's icon in the character selection screen, click on `Assets/Characters/Registries/<your_character's_name>.asset` and add your desired sprite to the "Icon" field. You can also add a subtitle to appear in Polypedia in the "Subtitle" field if you wish.
10. If you play the game with your new character and equip a back sentry, glider, or jetpack, you might notice that those equipped accessories may not line up correctly with the shape of your character's back, or they might not even be touching your character's back. To fix this:
    1. Open your character's preview prefab at `Assets/Characters/Previews/<your_character's_name>Preview.prefab`.
    2. In the hierarchy, locate your character's instance of the BackpackMount prefab at `mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/BackpackMount`.
    3. Under BackpackMount, you should see inactive prefabs for BackSentry, glider, and jetpack. Selectively activate, reposition/realign, and then deactivate each one.
11. Your character is ready to race!


## Removing a Character

1. Delete the player prefab `Assets/Characters/Players/<your_character's_name>.prefab`.
2. Delete the NPC prefab `Assets/Characters/NPCs/<your_character's_name>NPC.prefab`.
3. Delete the Preview prefab `Assets/Characters/Previews/<your_character's_name>Preview.prefab`.
4. Delete the registry `Assets/Characters/Registries/<your_character's_name>.asset`.
5. Open `Assets/Characters/CharacterList.asset` and remove the empty element from the list (this is a broken reference to your character's registry).
6. Delete your character's folder in `Assets/Mixamo`.
7. Delete your character's sprite that was being used for the character selection icon.

