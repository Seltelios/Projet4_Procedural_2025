using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise/Noise_TestPerso")]
public class Test_Noise_Perso : ProceduralGenerationMethod
{
    [Header("Parameter")]
    [SerializeField, Tooltip("Frequency"), Range(0.01f, 0.1f)] private float _frequency = 0.01f;
    [SerializeField] private FastNoiseLite.NoiseType _noiseType;
    [SerializeField, Tooltip("Octaves"), Range(0f,10f)] private float _octave;
    [SerializeField, Tooltip("Lacunarity"), Range(0f, 10f)] private float _lacunarity;
    [SerializeField, Tooltip("Gain"), Range(0f, 0.1f)] private float _gain;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        // Create and configure FastNoise object
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // Gather noise data
        float[,] noiseData = new float[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                noiseData[x, y] = noise.GetNoise(x, y);
            }
        }


        // Do something with this data...
    }
}
