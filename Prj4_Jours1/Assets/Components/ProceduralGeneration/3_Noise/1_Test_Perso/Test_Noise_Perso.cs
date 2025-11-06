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
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {

    }
}
