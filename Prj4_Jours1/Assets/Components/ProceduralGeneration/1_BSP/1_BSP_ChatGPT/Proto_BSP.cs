using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

//----------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------
//                      PROTO_BSP : BINARY SPACE PARTITIONING
//                                 WITH CHATGPT
//----------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------

// J'ai compris la théorie, les étapes qu'on doit faire.
// Mais impossible de le retranscrire en code C# fonctionnel, d'où l'aide de l'IA...
// A REVOIR

namespace Components.ProceduralGeneration.BSP
{
    /// <summary>
    /// Proto_BSP (version RandomService-only)
    /// - Découpe la grille en feuilles (leaves) en alternant Vertical/Horizontal par NIVEAU.
    /// - Le nombre de feuilles visé est une PUISSANCE DE 2 (2,4,8,16,...).
    /// - À la fin : 1 room par feuille + couloirs en remontant l’arbre.
    /// - Mode Debug : montre les découpes étape par étape (avec pause).
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP/BSP (Proto_ChatGPT)")]
    public class Proto_BSP : ProceduralGenerationMethod
    {
        // ─────────────────────────── PARAMÈTRES ───────────────────────────

        [Header("Target Leaves (power of two)")]
        [Tooltip("Nombre final de feuilles/rooms. Doit être une puissance de deux (2,4,8,16,32,...)")]
        [SerializeField] private int _targetRooms = 16;

        [Header("Leaf Constraints")]
        [Tooltip("Taille minimale autorisée pour un morceau après une coupe (garde-fou).")]
        [SerializeField] private Vector2Int _minLeafSize = new(8, 8);

        [Header("Split Ratio (random range)")]
        [Tooltip("Ratio aléatoire de coupe (en % de la largeur/hauteur), clampé pour respecter MinLeafSize.")]
        [Range(0.1f, 0.9f)][SerializeField] private float _splitRatioMin = 0.30f;
        [Range(0.1f, 0.9f)][SerializeField] private float _splitRatioMax = 0.70f;

        [Header("Rooms")]
        [SerializeField] private Vector2Int _roomMinSize = new(4, 4);
        [SerializeField] private Vector2Int _roomMaxSize = new(15, 10);
        [Tooltip("Bordure aléatoire (marge) appliquée à chaque feuille avant de choisir la room.")]
        [SerializeField] private Vector2Int _roomBorderMinMax = new(1, 2);

        [Header("Debug (step-by-step)")]
        [Tooltip("Affiche la découpe niveau par niveau, avec pause entre chaque niveau.")]
        [SerializeField] private bool _debugShowSplitsStepByStep = true;
        [Tooltip("Nombre de StepDelay à attendre entre deux niveaux de coupe.")]
        [SerializeField] private int _debugPauseSteps = 2;
        [Tooltip("Épaisseur du trait pour dessiner le contour des morceaux (tiles de type Corridor).")]
        [SerializeField] private int _debugOutlineThickness = 1;
        [Tooltip("Après le debug de découpe, repeindre toute la grille en Grass avant rooms.")]
        [SerializeField] private bool _debugClearAfterSplits = true;

        // ─────────────────────────── PIPELINE PRINCIPAL ───────────────────────────

        /// <summary>
        /// Point d’entrée de la génération (appelée par ProceduralGenerationMethod.Generate()).
        /// Étapes :
        ///  A) Calcule la profondeur cible à partir de TargetRooms (puissance de 2).
        ///  B) Découpe l’espace par NIVEAU (V/H alterné), avec ratios aléatoires via RandomService.
        ///  C) (Debug) Affiche les contours à chaque niveau, avec pause.
        ///  D) Place 1 room dans chaque feuille.
        ///  E) Connecte les rooms en remontant l’arbre avec des couloirs en “L”.
        ///  F) Tapis de sol (Grass) pour lisibilité.
        /// </summary>
        protected override async UniTask ApplyGeneration(CancellationToken ct)
        {
            // A) — Sécurisation + calcul profondeur cible
            // - On borne _targetRooms et on le convertit en puissance de 2 (ex: 10 → 16).
            // - targetDepth = log2(targetRooms) : nb de niveaux de coupe.
            int clamped = Mathf.Min(_targetRooms, 1 << 12); // borne haute 4096 (2^12), évite les valeurs énormes
            int target = Mathf.Max(2, NextPowerOfTwo(clamped)); // min 2 sinon pas de coupe
            int targetDepth = (int)Mathf.Log(target, 2);        // ex: 16 -> 4 niveaux
            bool startVertical = true;                          // niveau 0 = Vertical (comme ton schéma)

            // B) — Construire l’arbre par NIVEAU (BSP depth-controlled)
            RectInt rootRect = new RectInt(0, 0, Grid.Width, Grid.Lenght);
            var root = new BspNode(rootRect);

            // Listes de nœuds par niveau (level 0: racine)
            var nodesByDepth = new List<List<BspNode>>(targetDepth + 1);
            for (int i = 0; i <= targetDepth; i++) nodesByDepth.Add(new List<BspNode>());
            nodesByDepth[0].Add(root);

            // Boucle de niveaux : 0..targetDepth-1
            for (int depth = 0; depth < targetDepth; depth++)
            {
                // Alternance stricte par NIVEAU (vertical/horizontal)
                bool vertical = (startVertical ? depth % 2 == 0 : depth % 2 != 0);

                // On parcourt tous les nœuds de ce niveau et on tente de les couper
                foreach (var node in nodesByDepth[depth])
                {
                    if (TrySplitDepthControlled(node, vertical, out var left, out var right))
                    {
                        node.Left = left;
                        node.Right = right;
                        nodesByDepth[depth + 1].Add(left);
                        nodesByDepth[depth + 1].Add(right);

                        // DEBUG : dessiner le contour des deux enfants (visualise la coupe)
                        if (_debugShowSplitsStepByStep)
                        {
                            DrawRectOutline(left.Bounds, _debugOutlineThickness, CORRIDOR_TILE_NAME, true);
                            DrawRectOutline(right.Bounds, _debugOutlineThickness, CORRIDOR_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        // Coupe impossible (trop étroit/plat selon MinLeaf) → ce nœud restera feuille.
                        node.IsLeaf = true;

                        if (_debugShowSplitsStepByStep)
                            DrawRectOutline(node.Bounds, _debugOutlineThickness, CORRIDOR_TILE_NAME, true);
                    }
                }

                // Pause “pédagogique” entre deux niveaux (si debug ON)
                if (_debugShowSplitsStepByStep)
                {
                    int pauses = Mathf.Max(1, _debugPauseSteps);
                    for (int p = 0; p < pauses; p++)
                        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: ct);
                }
            }

            // Marquer toutes les feuilles finales (nœuds sans enfants)
            MarkLeaves(root);

            // Si on a utilisé le “dessin debug”, on nettoie avant de poser rooms (optionnel)
            if (_debugShowSplitsStepByStep && _debugClearAfterSplits)
            {
                ClearAllWithTile(GRASS_TILE_NAME);
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: ct);
            }

            // D) — Créer une room dans chaque feuille (avec bordure + tailles bornées)
            var leaves = CollectLeaves(root);
            foreach (var leaf in leaves)
            {
                ct.ThrowIfCancellationRequested();

                if (TryCreateRoomInLeaf(leaf))
                    PaintRoom(leaf.Room!.Value); // dessine les tuiles "Room"

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: ct);
            }

            // E) — Connecter les rooms : on remonte l’arbre et on relie gauche/droite
            ConnectRoomsInSubtree(root, ct);

            // F) — Tapis de sol (Grass) pour lisibilité globale
            BuildGround();
        }

        // ─────────────────────────── BSP : COUPE PAR NIVEAU ───────────────────────────

        /// <summary>
        /// Tente de couper un nœud au NIVEAU courant.
        /// - vertical == true  → coupe “verticale” (on sépare en GAUCHE/DROITE).
        /// - vertical == false → coupe “horizontale” (on sépare en BAS/HAUT).
        /// - Position de coupe basée sur un RATIO ALÉATOIRE [splitMin, splitMax], tiré via RandomService.
        /// - Le ratio est CLAMPÉ pour que chaque enfant respecte MinLeafSize.
        /// Retourne true si la coupe a réussi + out les 2 enfants.
        /// </summary>
        private bool TrySplitDepthControlled(BspNode node, bool vertical, out BspNode left, out BspNode right)
        {
            left = right = null;
            RectInt rect = node.Bounds;

            // Tirage du ratio avec le RandomService seedé (0.30..0.70 par défaut)
            float rawRatio = RandomService.Range(_splitRatioMin, _splitRatioMax);

            if (vertical)
            {
                // On coupe sur l'axe X (largeur)
                int minCut = rect.xMin + _minLeafSize.x; // position minimale de coupe
                int maxCut = rect.xMax - _minLeafSize.x; // position maximale de coupe
                if (maxCut - minCut <= 1) return false;  // pas assez de place pour couper proprement

                // position “idéale” selon le ratio
                int ideal = rect.xMin + Mathf.RoundToInt(rect.width * rawRatio);
                int splitX = Mathf.Clamp(ideal, minCut, maxCut);

                // tailles enfants
                int wA = splitX - rect.xMin;
                int wB = rect.xMax - splitX;
                if (wA < _minLeafSize.x || wB < _minLeafSize.x) return false;

                left = new BspNode(new RectInt(rect.xMin, rect.yMin, wA, rect.height));
                right = new BspNode(new RectInt(splitX, rect.yMin, wB, rect.height));
                return true;
            }
            else
            {
                // On coupe sur l'axe Y (hauteur)
                int minCut = rect.yMin + _minLeafSize.y;
                int maxCut = rect.yMax - _minLeafSize.y;
                if (maxCut - minCut <= 1) return false;

                int ideal = rect.yMin + Mathf.RoundToInt(rect.height * rawRatio);
                int splitY = Mathf.Clamp(ideal, minCut, maxCut);

                int hA = splitY - rect.yMin;
                int hB = rect.yMax - splitY;
                if (hA < _minLeafSize.y || hB < _minLeafSize.y) return false;

                left = new BspNode(new RectInt(rect.xMin, rect.yMin, rect.width, hA));
                right = new BspNode(new RectInt(rect.xMin, splitY, rect.width, hB));
                return true;
            }
        }

        /// <summary>
        /// Marque comme “leaf” (feuille) tous les nœuds qui n’ont pas d’enfants.
        /// </summary>
        private void MarkLeaves(BspNode node)
        {
            if (node == null) return;
            if (node.Left == null && node.Right == null) { node.IsLeaf = true; return; }
            MarkLeaves(node.Left);
            MarkLeaves(node.Right);
        }

        /// <summary>
        /// Retourne la liste des feuilles (parcours DFS).
        /// </summary>
        private List<BspNode> CollectLeaves(BspNode root)
        {
            var list = new List<BspNode>(64);
            void Dfs(BspNode n)
            {
                if (n == null) return;
                if (n.IsLeaf) { list.Add(n); return; }
                Dfs(n.Left); Dfs(n.Right);
            }
            Dfs(root);
            return list;
        }

        // ─────────────────────────── ROOMS ───────────────────────────

        /// <summary>
        /// Essaie de créer une room dans un leaf :
        ///  1) Tire une BORDURE aléatoire [min..max] avec RandomService.
        ///  2) Calcule l’espace intérieur (leaf - 2*bordure).
        ///  3) Si l’intérieur est trop petit pour RoomMinSize → pas de room.
        ///  4) Tire une TAILLE de room entre RoomMin/Max, CLAMPÉE à l’intérieur dispo.
        ///  5) Place la room à une POSITION aléatoire dans l’intérieur.
        /// </summary>
        private bool TryCreateRoomInLeaf(BspNode leaf)
        {
            int border = RandomService.Range(_roomBorderMinMax.x, _roomBorderMinMax.y + 1); // int [min, maxInclusif]

            int innerW = Mathf.Max(0, leaf.Bounds.width - border * 2);
            int innerH = Mathf.Max(0, leaf.Bounds.height - border * 2);
            if (innerW < _roomMinSize.x || innerH < _roomMinSize.y)
            {
                leaf.Room = null;
                return false;
            }

            int roomW = Mathf.Clamp(RandomService.Range(_roomMinSize.x, _roomMaxSize.x + 1), _roomMinSize.x, innerW);
            int roomH = Mathf.Clamp(RandomService.Range(_roomMinSize.y, _roomMaxSize.y + 1), _roomMinSize.y, innerH);

            // Position aléatoire de la room à l’intérieur
            int minX = leaf.Bounds.xMin + border;
            int maxX = leaf.Bounds.xMin + border + (innerW - roomW);
            int minY = leaf.Bounds.yMin + border;
            int maxY = leaf.Bounds.yMin + border + (innerH - roomH);

            int rx = (maxX >= minX) ? RandomService.Range(minX, maxX + 1) : minX;
            int ry = (maxY >= minY) ? RandomService.Range(minY, maxY + 1) : minY;

            leaf.Room = new RectInt(rx, ry, roomW, roomH);
            return true;
        }

        /// <summary>
        /// Peint toutes les cellules de la room avec le tile "Room" (override ON).
        /// </summary>
        private void PaintRoom(RectInt room)
        {
            for (int x = room.xMin; x < room.xMax; x++)
            {
                for (int y = room.yMin; y < room.yMax; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out var cell)) continue;
                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }

        // ─────────────────────────── CONNEXIONS ───────────────────────────

        /// <summary>
        /// Parcours post-ordre : connecte les sous-arbres gauche et droit
        /// dès qu’ils possèdent chacun au moins une room (centre).
        /// </summary>
        private void ConnectRoomsInSubtree(BspNode node, CancellationToken ct)
        {
            if (node == null) return;
            ct.ThrowIfCancellationRequested();

            if (node.Left != null) ConnectRoomsInSubtree(node.Left, ct);
            if (node.Right != null) ConnectRoomsInSubtree(node.Right, ct);

            if (node.Left != null && node.Right != null)
            {
                Vector2Int? a = GetAnyRoomCenter(node.Left);
                Vector2Int? b = GetAnyRoomCenter(node.Right);
                if (a.HasValue && b.HasValue)
                    CreateDogLegCorridor(a.Value, b.Value);
            }
        }

        /// <summary>
        /// Récupère un centre de room n’importe où dans ce sous-arbre (DFS).
        /// </summary>
        private Vector2Int? GetAnyRoomCenter(BspNode node)
        {
            if (node == null) return null;
            if (node.Room.HasValue) return node.Room.Value.GetCenter(); // extension déjà utilisée dans SRP

            var l = GetAnyRoomCenter(node.Left);
            if (l.HasValue) return l;
            var r = GetAnyRoomCenter(node.Right);
            if (r.HasValue) return r;

            return null;
        }

        /// <summary>
        /// Crée un couloir en “L” (dog-leg) entre deux points.
        /// On choisit aléatoirement l’ordre H-then-V ou V-then-H via RandomService.Chance().
        /// </summary>
        private void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
        {
            bool horizontalFirst = RandomService.Chance(0.5f);

            if (horizontalFirst)
            {
                CreateHorizontalCorridor(start.x, end.x, start.y);
                CreateVerticalCorridor(start.y, end.y, end.x);
            }
            else
            {
                CreateVerticalCorridor(start.y, end.y, start.x);
                CreateHorizontalCorridor(start.x, end.x, end.y);
            }
        }

        /// <summary> Trace un segment horizontal (x1..x2) à la ligne y. </summary>
        private void CreateHorizontalCorridor(int x1, int x2, int y)
        {
            int xMin = Mathf.Min(x1, x2);
            int xMax = Mathf.Max(x1, x2);

            for (int x = xMin; x <= xMax; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell)) continue;
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        /// <summary> Trace un segment vertical (y1..y2) à la colonne x. </summary>
        private void CreateVerticalCorridor(int y1, int y2, int x)
        {
            int yMin = Mathf.Min(y1, y2);
            int yMax = Mathf.Max(y1, y2);

            for (int y = yMin; y <= yMax; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell)) continue;
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        // ─────────────────────────── GROUND ───────────────────────────

        /// <summary>
        /// Repeint un “sol” (Grass) sur toute la carte (override = false,
        /// donc on ne remplace pas les rooms/corridors déjà posés).
        /// </summary>
        private void BuildGround()
        {
            var grass = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out var cell)) continue;
                    GridGenerator.AddGridObjectToCell(cell, grass, false);
                }
            }
        }

        // ─────────────────────────── DEBUG (Contours & Clear) ───────────────────────────

        /// <summary>
        /// Dessine le contour d’un rectangle (leaf) avec une certaine épaisseur, en utilisant le tile "Corridor".
        /// → Purement visuel pour comprendre la découpe. Utilisé en mode Debug step-by-step.
        /// </summary>
        private void DrawRectOutline(RectInt r, int thickness, string tile, bool overwrite)
        {
            thickness = Mathf.Max(1, thickness);

            // Lignes haut/bas
            for (int t = 0; t < thickness; t++)
            {
                int yTop = r.yMax - 1 - t;
                int yBot = r.yMin + t;
                for (int x = r.xMin; x < r.xMax; x++)
                {
                    PaintCell(x, yTop, tile, overwrite);
                    PaintCell(x, yBot, tile, overwrite);
                }
            }

            // Lignes gauche/droite
            for (int t = 0; t < thickness; t++)
            {
                int xL = r.xMin + t;
                int xR = r.xMax - 1 - t;
                for (int y = r.yMin; y < r.yMax; y++)
                {
                    PaintCell(xL, y, tile, overwrite);
                    PaintCell(xR, y, tile, overwrite);
                }
            }
        }

        /// <summary> Peint une cellule (utilitaire debug/paint). </summary>
        private void PaintCell(int x, int y, string tile, bool overwrite)
        {
            if (Grid.TryGetCellByCoordinates(x, y, out var cell))
                AddTileToCell(cell, tile, overwrite);
        }

        /// <summary>
        /// “Nettoie” l’écran en repeignant toutes les cases avec un tile donné (typiquement Grass).
        /// Utilisé pour effacer les contours de debug avant de placer les rooms.
        /// </summary>
        private void ClearAllWithTile(string tile)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out var cell)) continue;
                    AddTileToCell(cell, tile, true);
                }
            }
        }

        // ─────────────────────────── HELPERS ───────────────────────────

        /// <summary> Prochaine puissance de 2 supérieure ou égale à n. </summary>
        private static int NextPowerOfTwo(int n)
        {
            if (n < 1) return 1;
            int p = 1;
            while (p < n) p <<= 1; // 1<<k == 2^k
            return p;
        }

        // ─────────────────────────── TYPE NŒUD ───────────────────────────

        /// <summary>
        /// Nœud BSP : un rectangle (Bounds), éventuellement découpé en Left/Right.
        /// - IsLeaf = true s’il n’a pas d’enfants.
        /// - Room = la salle posée dans ce leaf (une fois la phase rooms lancée).
        /// </summary>
        private sealed class BspNode
        {
            public RectInt Bounds;
            public BspNode Left, Right;
            public bool IsLeaf;
            public RectInt? Room;

            public BspNode(RectInt b) { Bounds = b; }
        }
    }
}
