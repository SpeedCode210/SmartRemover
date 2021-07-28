# SmartRemover

Un désinstalleur complémentaire de SmartInstaller réalisé sous WPF en C# moderne épuré avec une interface style Windows 11 et simple d'utilisation.
Il n'y a pas de configuration à faire dessus, il utilise les fichiers de configuration générés par SmartInstaller.

![Capture d'écran](https://raw.githubusercontent.com/eclipium/SmartRemover/master/screen.png)

## Configuration requise pour la compilation
-.Net Framework 4.8 ou supérieur

-Visual studio 2017 ou supérieur

## Configuration requise pour l'utilisateur
-.Net Framework 4.8 ou supérieur

-Windows 7 ou plus


## Paquet d'installation

Doit être un fichier zip contenant: un dossier bin contenant l'application et un fichier package.json sous cette forme:

```json
{
  "Name": "{NomDeApplication}",
  "MainExe": "{NomDeExecutable(exemple: app.exe)}",
  "VersionName": "{NomDeVersion}",
  "VersionCode": {NumeroDeVersion}, 
  "Date": "{DateDePublication}"
}
```

##Exemple :

```json
{
  "Name": "Hieroctive",
  "MainExe": "Hieroctive.exe",
  "VersionName": "1.2",
  "VersionCode": 3, 
  "Date": "02/07/2021"
}
```
