using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellular Automata/Perso_Julien")]
public class Test : ProceduralGenerationMethod
{
    [SerializeField] private int _noiseDensity = 50;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var grassTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
        // Generate random grid with noise

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                        GridGenerator.AddGridObjectToCell(chosenCell, grassTemplate, false);
                    }
                    else
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, false);
                    }
                }
            }

            // Step i de l'algo
            if (Grid.TryGetCellByCoordinates(10, 10, out Cell cell))
            {
                if (cell.GridObject.Template.Name == GRASS_TILE_NAME)
                {
                    // C'est de l'herbe en 10 10 :) 
                }
            }

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }







}