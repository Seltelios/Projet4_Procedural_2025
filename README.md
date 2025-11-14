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

--> Configuration: <br>
A l'ouverture du projet Unity, utiliser la scène `GridGenerator`.
Sur le GameObject `ProceduralGridGenerator`, vérifier que la variable GenerationMethod utilise le scriptableObject concerné.

<img width="30%" src="Prj4_Jours1/Documentation/ProceduralGridGenerator_ScriptableObject_SimpleRoom.png"></img> <br>

Si ce n'est pas le bon scriptableObject, pas de panique, pour le trouver: <br>
Assets > Components > ProceduralGeneration --> Dossier de la partie concerné. <br>
Simple glisser/déposer du ScriptableObejct dans l'inspector de ProceduralGridGenerator > GenerationMethod. <br>

- - -
## SimpleRoomPlacement
<br>

On utilise `ProceduralGridGenerator`, et on utilise le scriptableObject `SimpleRoomPlacement`. <br>

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
Résultat final, layer Room > Corridor pour le rendu final. <br>

<br>

- - -
## CellularAutomata
<br>

On utilise toujours `ProceduralGridGenerator`, mais cette fois on utilise le scriptableObject `New CellularAutomata_Correction`. <br>
Libre à vous d'utiliser et de voir le rendu des autres CellularAutomata présents dans le dossier concerné. <br>
On retrouve peu de paramètre pour cette partie:
- `MaxSteps`: toujours le nombre de répétition à faire.
- `GroundDesnity`: Pourcentage de "Sol" que de "Eau" (Grass / Water)
- `minGroundNeighbourCount`: Condition pour transformer une case en sol. Ex: minGroundCount = 5, au moins 5 cases doit être "Grass" pour devenir a son tour Grass.
<br> <img width="20%" src="Prj4_Jours1/Documentation/Cell_Auto/CellularAutomaton.png"></img> <br>
Ici, la case rouge deviendra "Grass" car on retrouve bien minimum 5cases adjacents en "Grass". <br>

<br>
Les étapes du CellularAutomata: <br>
1. Remplir une grille aléatoirement de Grass ou de Water, en fonction du paramètre de pourcentage de l'inspector. <br>
2. Pour chaque nouvelle itération, on créé un nouveau tableau qui représentera la nouvelle grille, qui reflètera les nouveaux "état" de chaque cellule (mise en place de la condition `minGroundNeighbourCount`). <br>
3. Analyse la grille de base avec la nouvelle, si une cellule à eut un changement, on change l'état de la cellule concerné (on obtient ainsi une nouvelle grille proprement). <br>
4. On réitère l'étape 2 et 4 jusqu'à atteindre le nombre MaxSteps. <br>

<br>

- - -
## Noise
<br>

On utilise toujours `ProceduralGridGenerator`, mais cette fois on utilise le scriptableObject `New Test_Noise_Perso`. <br>
Libre à vous d'utiliser et de voir le rendu des autres Noises présents dans le dossier concerné. <br>
En comparaison avec CellularAutomata, ici on est baigné dans les paramètres dans l'inspector.
- `noiseType` : Type de bruit. Change le rendu du terrain.
- `frequency` : Gère la fréquence du bruit, petit -> Grande frome, grand -> plus petite
- `amplitude` : Influence la “hauteur” gloable.
- `fractalType`, `octaves`, `lacunarity`, `persistence` : Paramètres pour faire plusieurs couches et ajouter du détail.
- `HeightMap`: <br>
On gère la coloration des tuiles en fonction de la hauteur des tuiles. <br>
On retrouve ici: <br>
`waterHeight`, `sandHeight`, `grassHeight`, `rockHeight`. <br>
Dans l'inspector, on peut paramètrer le seuils de hauteur pour décider si une case sera Water, Sand, Grass ou Rock.

Exemple de rendu: <br>
<br> <img width="20%" src="Prj4_Jours1/Documentation/Noise/Exemple_Noise1.png"></img> <br>
<br> <img width="50%" src="Prj4_Jours1/Documentation/Noise/ConfigNoise_Exemple1.png"></img> <br>

Les étapes du script: <br>
1. Avec les paramètres de l'inspector, initialise un bruit dans une grille. <br>
2. On récupère une cellule de la grille, puis note sa hauteur associée. <br>
3. On compare la hauteur de la cellule, avec les paramètres Water/Sand/Grass/Rock, pour savoir quel état lui mettre. <br>
4. On peint la cellule à l'état associés. <br>
5. On répète le processus 2 à 4 pour chaque cellule de la grille pour avoir un rendu. <br>
6. Change chaque variable dans l'inspector pour tenter d'avoir un résultat sympa (tips: Frequency, Amplitude et les paramètres de HeightSettings sont primordiaux). <br>


- - -
- - -

<br>
Remerciement: <br>
Merci GamingCampus pour le Cursus GTech. <br>
Et un GRAND Merci à RUTKOWSKI Yona pour son intervention durant ce projet. <br>

