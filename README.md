<img width="80%" src="Prj4_Jours1/Documentation/Projet4_JeuProcedural.png"></img>
- - -
**BOURDON Julien**
> Gaming Campus - GTech3 <br>
> Groupe GameBoy - 2025 <br>
> Semaine Théorique sur Unity - `Jeu Procédurale` <br>
- - -


### Sommaires
<br>

- [Initialisation](#Initialisation)
- [SimpleRoomPlacement](#SimpleRoomPlacement)
- [BSP](#BSP)
- [CellularAutomata](#CellularAutomata)
- [Noise](#Noise)


- - -
### Initialisation
<br>

Comment initialiser ce genre de projet ? <br>
Utilisation de **`UniTask`**: <br>
--> Guide d'installation ([**Lien UniTask OpenUPM**](https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation)) <br>
<br>
**1ère Etape** <br>
Sur Unity, dans un projet 3D: <br>
--> Onglet >Edit <br>
--> Project Setting <br>
--> Package Manager <br>
<br>
|---- Name  : `package.openupm.com` <br>
|---- URL   : `https://package/openupm.com` <br>
|---- Scope : `com.cysharp.unitask` <br>
<br>
<img width="50%" src="Prj4_Jours1/Documentation/SetupUniTask_Unity_FullOnglet.png"></img> <br>
<br>
 **2ème Etapes** <br>
 Une fois validé, on peut fermer la fenêtre puis: <br>
 --> Onglet: Window  <br>
 --> Package Manager <br>
 --> [+] <br>
 --> Name: `com.cysharp.unitask` | version: `2.5.10` <br>
<br>
 <img width="25%" src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name.png"></img> <br>
<img width="25%" src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name_InputField.png"></img> <br>
<br>
**3ème Etapes:** (Facultatif, seulement si tu souhaite recommencer avec une base basique) <br>
Une fois UniTask correctement installé, on peut télécharger le package découverte de l'intervenant. <br>  
[**LienDriveCampus**](https://drive.google.com/drive/folders/1QxmWzBSGsTq-miRODwUX_zA8UEcFaUDW) <br>
Nom du package: `ArchitectureProceduralGeneration.unitypackage` <br>
Une fois téléchargé, simplement glisser le package dans la Hierarchy Unity, puis import le tout. <br>
<br>

- - -

**FIN INITIALISATION**
<br>
Ici, le projet contient plus d'élément que le simple package de l'étape 3. <br>
On retrouve les exemples de: <br>
- SimpleRoomPlacement <br>
- BSP <br>
- Cellular Automata <br>
- Noise <br>

<br>

- - -
## SimpleRoomPlacement
<br>

A l'ouverture du projet Unity, utiliser la scène `GridGenerator`.
Sur le GameObject `ProceduralGridGenerator`, vérifier que la variable GenerationMethod utilise le scriptableObject `Simple Room Placement`.

<img width="30%" src="Prj4_Jours1/Documentation/ProceduralGridGenerator_ScriptableObject_SimpleRoom.png"></img> <br>

Si ce n'est pas le bon scriptableObject, pas de panique, pour le trouver: <br>
Assets > Components > ProceduralGeneration > 0_SimpleRoomPlacement > `SimpleRoomPlacement` <br>
Simple glisser/déposer dans l'inspector de ProceduralGridGenerator > GenerationMethod. <br>

Ici, on décortique justement le script du ScriptableObject `Simple Room Placement.cs` <br>


1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.

- - -
## BSP
<br>

Partie explication BSP


1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.

- - -
## CellularAutomata
<br>

Partie explication Cellular Automata

1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.

- - -
## Noise
<br>

Partie explication Noise

1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
1.
