using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellular Automata/Code_Elias")]
public class Test_E : ProceduralGenerationMethod
{
    [SerializeField] private int _noiseDensity = 50;
    [SerializeField, Range(0, 8)]
    private int _grassThreshold = 4;


    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        bool[,] currentState = new bool[Grid.Width, Grid.Lenght]; // true = grass, false = water
        var grassTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");

        InitialiseNoise(currentState, grassTemplate, waterTemplate);

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool[,] newState = CellularAutomaton(currentState);

            int changes = 0;
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (currentState[x, z] != newState[x, z])
                        changes++;
                }
            }

            currentState = newState;

            // Mise à jour de la grille Unity
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var currentCell))
                        continue;

                    if (currentState[x, z])
                        GridGenerator.AddGridObjectToCell(currentCell, grassTemplate, true);
                    else
                        GridGenerator.AddGridObjectToCell(currentCell, waterTemplate, true);
                }
            }

            Debug.Log($"Step {i}: changes = {changes}");

            // Si plus de changements, on arrête la boucle
            if (changes == 0)
            {
                Debug.Log("Cellular Automaton stabilized, stopping early.");
                break;
            }

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }


    private void InitialiseNoise(bool[,] currentState, GridObjectTemplate grassTemplate, GridObjectTemplate waterTemplate)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }

                int randomValue = RandomService.Range(0, 100);
                if (randomValue < _noiseDensity)
                {
                    currentState[x, z] = true; // Grass
                    GridGenerator.AddGridObjectToCell(chosenCell, grassTemplate, false);
                }
                else
                {
                    currentState[x, z] = false; // Water
                    GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, false);
                }
            }
        }
    }
    private bool[,] CellularAutomaton(bool[,] currentState)
    {
        bool[,] newState = new bool[Grid.Width, Grid.Lenght];
        int totalGrassBefore = 0;

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (currentState[x, z])
                    totalGrassBefore++;

                int grassNeighbors = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dz == 0) continue;

                        int neighborX = x + dx;
                        int neighborZ = z + dz;

                        if (neighborX >= 0 && neighborX < Grid.Width && neighborZ >= 0 && neighborZ < Grid.Lenght)
                        {
                            if (currentState[neighborX, neighborZ])
                                grassNeighbors++;
                        }
                    }
                }

                newState[x, z] = grassNeighbors >= _grassThreshold;
            }
        }

        int totalGrassAfter = 0;
        for (int x = 0; x < Grid.Width; x++)
            for (int z = 0; z < Grid.Lenght; z++)
                if (newState[x, z]) totalGrassAfter++;

        Debug.Log($"Cellular Automaton step: Grass before = {totalGrassBefore}, after = {totalGrassAfter}");

        return newState;
    }


}