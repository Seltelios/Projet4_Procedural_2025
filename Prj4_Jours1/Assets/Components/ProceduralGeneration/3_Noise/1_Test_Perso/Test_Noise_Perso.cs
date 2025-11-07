using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise/Noise_TestPerso")]
public class Noise_Test : ProceduralGenerationMethod
{
    [Header("Noise Settings")]
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    [Range(0.01f, 0.1f)] public float frequency = 1f;
    [Range(0.5f, 1.5f)] public float amplitude = 1f;

    [Header("Fractal Settings")]
    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
    [Range(1, 5)] public int octaves = 3;
    [Range(1f, 3f)] public float lacunarity = 2f;
    [Range(0.5f, 1f)] public float persistence = 0.5f;

    [Header("Terrain Height Settings")]
    [Range(-1f, 1f)] public float waterHeight = 0.2f;
    [Range(-1f, 1f)] public float sandHeight = 0.3f;
    [Range(-1f, 1f)] public float grassHeight = 0.6f;
    [Range(-1f, 1f)] public float rockHeight = 1f;

    private enum TerrainType
    {
        Water = 0,
        Sand = 1,
        Grass = 2,
        Rock = 3
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        FastNoiseLite noise = InitNoise();

        int width = Grid.Width;
        int lenght = Grid.Lenght;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < lenght; z++)
            {
                float heightValue = SampleHeight(noise, x, z);
                var type = ClassifyHeightInTerrain(heightValue);
                PaintCell(x, z, type);
            }
        }
    }

    public FastNoiseLite InitNoise()
    {
        var noise = new FastNoiseLite();

        noise.SetNoiseType(noiseType);
        noise.SetFrequency(frequency);
        noise.SetFractalType(fractalType);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistence);

        return noise;
    }

    private float SampleHeight(FastNoiseLite noise, int x, int z)
    {
        float elevation = noise.GetNoise(x, z);
        elevation *= amplitude;
        return Mathf.Clamp(elevation, -1f, 1f);
    }

    private TerrainType ClassifyHeightInTerrain(float h)
    {
        if (h < waterHeight) return TerrainType.Water;
        if (h < sandHeight) return TerrainType.Sand;
        if (h < grassHeight) return TerrainType.Grass;
        return TerrainType.Rock;
    }

    private void PaintCell(int x, int z, TerrainType type)
    {
        if (!Grid.TryGetCellByCoordinates(x, z, out var cell))
            return;

        string templateName;

        switch (type)
        {
            case TerrainType.Water:
                templateName = "Water";
                break;

            case TerrainType.Sand:
                templateName = "Sand";
                break;

            case TerrainType.Grass:
                templateName = "Grass";
                break;

            case TerrainType.Rock:
                templateName = "Rock";
                break;

            default:
                templateName = "Grass";
                break;
        }

        var template = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(templateName);
        if (template != null)
            GridGenerator.AddGridObjectToCell(cell, template, true);
    }
}