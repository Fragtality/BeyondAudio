Starts BeyondATC and sets Output Device:

1. Download [BeyondAudio.exe](https://raw.githubusercontent.com/Fragtality/BeyondAudio/master/BeyondAudio.exe)
2. Run
3. Select the Device you want to use for BeyondATC & and hit *Save!*
4. Use the `BeyondATC (BeyondAudio)` Link on your Desktop to Start BeyondATC
5. You can delete the downloaded Binary (BeyondAudio.exe) after that, if you like<br/>

Some Notes:
- BeyondAudio only queries active & connected Devices
- The Installation-Location is `%appdata%\BeyondAudio`
- To Remove: Just delete the `BeyondAudio` Folder in `%appdata%`
- To reset/change the Configuration: Run the Application outside of the AppData Folder (i.e. the downloaded Binary) -OR- delete the File `%appdata%\BeyondAudio\Device.json`<br/>

Technical Background:<br/>
All BeyondAudio does is really just starting BeyondATC (it reads the Installation Path from the Registry) and after that switching its Output-Device in the Windows Sound-Mixer (to the configured Device). So it does automatically what the User can already do manually (every Time BeyondATC is started).<br/>
The Device Switch is done via an COM Interface of Windows which was discovered/reverse-engineered by the Creators of EarTrumpet. The CoreAudio Package also implemented that COM Interface - so after all the Abstraction by that Package, the Device Switch is really just a simple Function-Call.

<br/><br/>
Credits:<br/>
Using Code from [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) <br/>
Using [CoreAudio](https://github.com/morphx666/CoreAudio) Package<br/>
