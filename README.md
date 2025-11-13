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

**Informations Utiles**
<br>

--> SEED: <br>
- Ici, on utilise RandomService() avec la Seed pour gérer l'aléatoire. <br>
- En programmation, l'utilisation d'une Seed permet d'avoir du Pseudo-Aléatoire. <br>
- En changeant la Seed, on change le résultat, si on réutilise cette même Seed, on retrouvera le même résultat. <br>
- Utiliser la même façon de gérer l'aléatoire (en utilisant RandomService, permet de retrouver les mêmes décors, mêmes générations en utilisant la même Seed. <br>

- - -
## SimpleRoomPlacement
<br>

A l'ouverture du projet Unity, utiliser la scène `GridGenerator`.
Sur le GameObject `ProceduralGridGenerator`, vérifier que la variable GenerationMethod utilise le scriptableObject `Simple Room Placement`.

<img width="30%" src="Prj4_Jours1/Documentation/ProceduralGridGenerator_ScriptableObject_SimpleRoom.png"></img> <br>

Si ce n'est pas le bon scriptableObject, pas de panique, pour le trouver: <br>
Assets > Components > ProceduralGeneration > 0_SimpleRoomPlacement > `SimpleRoomPlacement` <br>
Simple glisser/déposer dans l'inspector de ProceduralGridGenerator > GenerationMethod. <br>

Voici les étapes du ScriptableObject `Simple Room Placement.cs` <br>
1. Créer une `Room` de taille aléatoire compris entre `minSizeX / minSizeY` et `maxSizeX / maxSizeY` indiqué dans l'inspetor.
2. Positionne la `Room` aléatoirement dans la grille.
3. Vérifie si la nouvelle `Room` ne chevauche pas une room déjà en place.
4. Réitère l'étape 1 à 3 jusqu'à atteindre `MaxRooms` ou `MaxSteps` inscrit dans l'inspector.

5. Une fois l'étape 4 finis, on relie ensuite les rooms entres-elles. <br>
En passant par le centre des rooms, on créé des couloirs en forme de "L" en suivant l'ordre d'instanciation: <br>
Room1 --> Room2 --> Room3 --> etc. <br>
<br>


- - -
## BSP
<br>

On utilise toujours `ProceduralGridGenerator`, mais cette fois on utilise le scriptableObject `New BSP_Correction`. <br>
Libre à vous d'utiliser et de voir le rendu des autres BSP présents dans le dossier BSP. <br>
Rappel sur `Binary Tree`: <br>
<img width="30%" src="Prj4_Jours1/Documentation/Screen_BSP/BinaryTree.png"></img> <br>
<br>
Voici les différentes étapes du BSP imagé par un exemple possible: <br>

1. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_0Split.png"></img> <br>
Création de la grille mère appelée `Root`. <br>
2. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_1Split.png"></img> <br>
Création des `Sisters`. <br>
Avec une coupe aléatoire entre coupe verticale ou horizontale.
3. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_2Split.png"></img> <br>
Continue la création d'autres Sisters dans les Sisters. <br>
4. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_3Split.png"></img> <br>
Arrête la découpe s'il est impossible de créer d'autreq Sisters tout en respectant les paramètres de tailles minim les. <br>
S'arrête également si on a atteints les steps maximum possible. <br>
--> Résultat: Atteinte des Leafs utilisables. <br>
5. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildRoom.png"></img> <br>
Création des Rooms dans chacune des Leafs, respectant les paramètres inspector (taille et offSet entre le leaf).
6. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildCorridor.png"></img> <br>
Création des Corridors, reliant chaque Sisters entre elles (on remonte le BinaryTree). <br>
Couloir en forme de "L", passant par le centre des Rooms (pas des Leafs). <br>
Couloir "L" alterne entre vertical ou horizontal également.
7. <br> <img width="20%" src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildFinal.png"></img> <br>
Résultat final, layer Room > Corridor pour le rendu final.


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
