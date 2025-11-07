using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

//RIP: SEED --> 0 IMPACT --> RandomService.Range --> Seed pas d'impact si on ne l'utilise pas

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise/NoiseGeneration_Elias")]
public class NoiseGenerator : ProceduralGenerationMethod
{
    [Header("Parameter")]
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    [Range(0.01f, 0.1f)] public float frequency = 1f;
    [Range(0.5f, 1.5f)] public float amplitude = 1f;

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
    [Range(1, 5)] public int octaves = 3;
    [Range(1f, 3f)] public float lacunarity = 2f;
    [Range(0.5f, 1f)] public float persistence = 0.5f;

    [Header("Parameter Pourcentage ground")]
    [Range(-1f, 1f)] public float waterHeight = 0.2f;
    [Range(-1f, 1f)] public float sandHeight = 0.3f;
    [Range(-1f, 1f)] public float grassHeight = 0.6f;
    [Range(-1f, 1f)] public float rockHeight = 1f;

    private enum TerrainType
    {
        Water,
        Sand,
        Grass,
        Rock
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var noise = new FastNoiseLite();

        noise.SetNoiseType(noiseType);
        noise.SetFrequency(frequency);
        noise.SetFractalType(fractalType);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistence);

        //--------------------------------------
        // ????
        float value = noise.GetNoise(10f, 20f);
        Debug.Log(value);
        //--------------------------------------

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                TerrainType type = GenerateTerrainTypeFromNoise((float)(noise.GetNoise(x * frequency, z * frequency) * amplitude));

                PlaceTerrainObject(x, z, type);
            }
        }
    }

    private TerrainType GenerateTerrainTypeFromNoise(float _noiseValue)
    {
        if (_noiseValue < waterHeight)
            return TerrainType.Water;
        else if (_noiseValue < sandHeight)
            return TerrainType.Sand;
        else if (_noiseValue < grassHeight)
            return TerrainType.Grass;
        else
            return TerrainType.Rock;
    }

    private void PlaceTerrainObject(int x, int z, TerrainType type)
    {
        Cell cell;
        if (!Grid.TryGetCellByCoordinates(x, z, out cell))
            return;
        GridObjectTemplate template = type switch
        {
            TerrainType.Water => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water"),
            TerrainType.Sand => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Sand"),
            TerrainType.Grass => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass"),
            TerrainType.Rock => ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Rock"),
            _ => null
        };
        if (template != null)
        {
            GridGenerator.AddGridObjectToCell(cell, template, true);
        }
    }

    

}