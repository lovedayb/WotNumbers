How to update the application after a new WoT patch/update

- Most times we'll have to deal changing the battle2json files. (Remember to update the parser[version] in wotbr2j.py to the one of the game) 	
- There could be something new that need to be added to the user data base, like when a new map is added.
Add these to DbVersion.cs, increase version number in the ExpectedNumber field.

- If new tanks are added to the game, we can use the Admin tool to download and update the Admin.db database.
In this case increase ExpectedNumber field in DbVersion.cs too, and set CopyAdminDB to true.

- If Dossier format changes, we'll need to change Dossier2json files. Change version number in the parser in wotdc2j.py (like we do in wotbr2j.py).
Most probably a new structures_xxx.json will be necessary, and we must ensure we deploy this file in the installer. 
We have to add a new entry into the Product.wxs file. Look for structures_108.json and add where necessary. We will need to create a Guid for every new entry.
Use Tools\Create GUID -> use 4th option (windows registry format)

- Increase application version number (we need this to deploy a new binary and let the users know there is a new updated version)
Go to assembly info and update AssemblyVersion / AssemblyFileVersion.
Go to InstallerWix3 project, open WotNumbersLicense.rtf and modify version number there.

- Remember to update log files to keep track of changes which is optional but good praxis.
- Rebuild the solution in Release.
- Copy InstallerWix3/bin/Release/WotNumbersSetup.msi to LatestRelease directory
- Rename WotNumbersSetup.msi to WotNumbersSetup_VERSION_DATE.msi
if VERSION is 1.0.4 and DATE is 20-Oct-2022 the name would become WotNumbersSetup_1.0.4_20-Oct_2022.msi

- You can create an MD5 from this file. It's not mandatory  but good praxis.
- Update date and version number in VersionSettings.json file. (Date is in YYYY-MM-DD format)
- Upload to GitHub.
