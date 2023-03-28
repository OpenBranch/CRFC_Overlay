
# CRFC Overlay

This program aids in the smoth process of a OBS overlay.
Working with OBS it creates a smoth experiance edditing text on screen.

## Support

For support, email stonegoblin9@gmail.com or join our discord server [discord](discord.seam-co.net.)


## Installation

[![Youtube Tutorial](https://img.youtube.com/vi/Lcuk0bTHBGY/0.jpg)](https://youtu.be/Lcuk0bTHBGY)

>>**Program**
1. Download the zip file called test `CRFC_Overlay.zip`
2. Extract the zip to computer
3. run the CRFC_Overlay.exe
&nbsp;
>>**OBS**
1. Open OBS
2. Click `Scene Collection`
3. Click `Import`
4. Click `Add`
5. Go back into the extracted `CRFC_Overlay.zip` folder
6. Select `CRFCOverlay.json`
7. Rename new import name in the OBS "Scene Collection Importer" sub-menu
7. Click `Import` with the collection Path checked
&nbsp;
>>**Using Overlay in OBS**
1. Click `Scene Collection` in OBS
2. Select newly created import
3. Under `Sources` double click each `Text` source and change file path
```bash
  These include:
  ETA
  Half
  LeftScore
  RightScore
  RightNAme
  LeftName
```
4. File Path example
```bash
  old- D:/Desktop/GitHub/CRFC_Overlay/src/TextFiles/ETA.txt

  new- .../TextFiles/ETA.txt
```
5. Make sure you link the correct file to EACH text source in OBS. like the `ETA` source needs to be linked with the `ETA.txt`
