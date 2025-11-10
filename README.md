<img width="80%" src="Prj4_Jours1/Documentation/Projet4_JeuProcedural.png">

**BOURDON Julien** - GTech3 - Group GameBoy- 2025
Semaine Théorique sur Unity - `Jeu Procédurale`

<detail>
<summary> Table Of Contents </summary>
- - -
- [SimpleRoomPlacement](#simpleroom)
- [BSP](#bsp)
- [CellularAutomata](#cellautomata)
</detail>


## Table of Contents

- [Getting started](#getting-started)
- [Basics of UniTask and AsyncOperation](#basics-of-unitask-and-asyncoperation)
- [Cancellation and Exception handling](#cancellation-and-exception-handling)

  Getting started
---
Install via [UPM package](#upm-package) with git reference or asset package(`UniTask.*.*.*.unitypackage`) available in [UniTask/releases](https://github.com/Cysharp/UniTask/releases).

```csharp
// extension awaiter/methods can be used by this namespace
using Cysharp.Threading.Tasks;

// You can return type as struct UniTask<T>(or UniTask), it is unity specialized lightweight alternative of Task<T>
// zero allocation and fast excution for zero overhead async/await integrate with Unity
async UniTask<string> DemoAsync()
{
    // You can await Unity's AsyncObject
    var asset = await Resources.LoadAsync<TextAsset>("foo");
    var txt = (await UnityWebRequest.Get("https://...").SendWebRequest()).downloadHandler.text;
    await SceneManager.LoadSceneAsync("scene2");

    // .WithCancellation enables Cancel, GetCancellationTokenOnDestroy synchornizes with lifetime of GameObject
    // after Unity 2022.2, you can use `destroyCancellationToken` in MonoBehaviour
    var asset2 = await Resources.LoadAsync<TextAsset>("bar").WithCancellation(this.GetCancellationTokenOnDestroy());

    // .ToUniTask accepts progress callback(and all options), Progress.Create is a lightweight alternative of IProgress<T>
    var asset3 = await Resources.LoadAsync<TextAsset>("baz").ToUniTask(Progress.Create<float>(x => Debug.Log(x)));

    // await frame-based operation like a coroutine
    await UniTask.DelayFrame(100); 

    // replacement of yield return new WaitForSeconds/WaitForSecondsRealtime
    await UniTask.Delay(TimeSpan.FromSeconds(10), ignoreTimeScale: false);
    
    // yield any playerloop timing(PreUpdate, Update, LateUpdate, etc...)
    await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);

    // replacement of yield return null
    await UniTask.Yield();
    await UniTask.NextFrame();

    // replacement of WaitForEndOfFrame
#if UNITY_2023_1_OR_NEWER
    await UniTask.WaitForEndOfFrame();
#else
    // requires MonoBehaviour(CoroutineRunner))
    await UniTask.WaitForEndOfFrame(this); // this is MonoBehaviour
#endif

    // replacement of yield return new WaitForFixedUpdate(same as UniTask.Yield(PlayerLoopTiming.FixedUpdate))
    await UniTask.WaitForFixedUpdate();
    
    // replacement of yield return WaitUntil
    await UniTask.WaitUntil(() => isActive == false);

    // special helper of WaitUntil
    await UniTask.WaitUntilValueChanged(this, x => x.isActive);

    // You can await IEnumerator coroutines
    await FooCoroutineEnumerator();

    // You can await a standard task
    await Task.Run(() => 100);

    // Multithreading, run on ThreadPool under this code
    await UniTask.SwitchToThreadPool();

    /* work on ThreadPool */

    // return to MainThread(same as `ObserveOnMainThread` in UniRx)
    await UniTask.SwitchToMainThread();

    // get async webrequest
    async UniTask<string> GetTextAsync(UnityWebRequest req)
    {
        var op = await req.SendWebRequest();
        return op.downloadHandler.text;
    }

    var task1 = GetTextAsync(UnityWebRequest.Get("http://google.com"));
    var task2 = GetTextAsync(UnityWebRequest.Get("http://bing.com"));
    var task3 = GetTextAsync(UnityWebRequest.Get("http://yahoo.com"));

    // concurrent async-wait and get results easily by tuple syntax
    var (google, bing, yahoo) = await UniTask.WhenAll(task1, task2, task3);

    // shorthand of WhenAll, tuple can await directly
    var (google2, bing2, yahoo2) = await (task1, task2, task3);

    // return async-value.(or you can use `UniTask`(no result), `UniTaskVoid`(fire and forget)).
    return (asset as TextAsset)?.text ?? throw new InvalidOperationException("Asset not found");
}
```

Basics of UniTask and AsyncOperation
---
UniTask features rely on C# 7.0([task-like custom async method builder feature](https://github.com/dotnet/roslyn/blob/master/docs/features/task-types.md)) so the required Unity version is after `Unity 2018.3`, the official lowest version supported is `Unity 2018.4.13f1`.

Why is UniTask(custom task-like object) required? Because Task is too heavy and not matched to Unity threading (single-thread). UniTask does not use threads and SynchronizationContext/ExecutionContext because Unity's asynchronous object is automaticaly dispatched by Unity's engine layer. It achieves faster and lower allocation, and is completely integrated with Unity.

You can await `AsyncOperation`, `ResourceRequest`, `AssetBundleRequest`, `AssetBundleCreateRequest`, `UnityWebRequestAsyncOperation`, `AsyncGPUReadbackRequest`, `IEnumerator` and others when `using Cysharp.Threading.Tasks;`.

UniTask provides three pattern of extension methods.

```csharp
* await asyncOperation;
* .WithCancellation(CancellationToken);
* .ToUniTask(IProgress, PlayerLoopTiming, CancellationToken);
```

`WithCancellation` is a simple version of `ToUniTask`, both return `UniTask`. For details of cancellation, see: [Cancellation and Exception handling](#cancellation-and-exception-handling) section.

> Note: await directly is returned from native timing of PlayerLoop but WithCancellation and ToUniTask are returned from specified PlayerLoopTiming. For details of timing, see: [PlayerLoop](#playerloop) section.

> Note: AssetBundleRequest has `asset` and `allAssets`, default await returns `asset`. If you want to get `allAssets`, you can use `AwaitForAllAssets()` method.

The type of `UniTask` can use utilities like `UniTask.WhenAll`, `UniTask.WhenAny`, `UniTask.WhenEach`. They are like `Task.WhenAll`/`Task.WhenAny` but the return type is more useful. They return value tuples so you can deconstruct each result and pass multiple types.

```csharp
public async UniTaskVoid LoadManyAsync()
{
    // parallel load.
    var (a, b, c) = await UniTask.WhenAll(
        LoadAsSprite("foo"),
        LoadAsSprite("bar"),
        LoadAsSprite("baz"));
}

async UniTask<Sprite> LoadAsSprite(string path)
{
    var resource = await Resources.LoadAsync<Sprite>(path);
    return (resource as Sprite);
}
```

If you want to convert a callback to UniTask, you can use `UniTaskCompletionSource<T>` which is a lightweight edition of `TaskCompletionSource<T>`. 

```csharp
public UniTask<int> WrapByUniTaskCompletionSource()
{
    var utcs = new UniTaskCompletionSource<int>();

    // when complete, call utcs.TrySetResult();
    // when failed, call utcs.TrySetException();
    // when cancel, call utcs.TrySetCanceled();

    return utcs.Task; //return UniTask<int>
}
```

You can convert Task -> UniTask: `AsUniTask`, `UniTask` -> `UniTask<AsyncUnit>`: `AsAsyncUnitUniTask`, `UniTask<T>` -> `UniTask`: `AsUniTask`. `UniTask<T>` -> `UniTask`'s conversion cost is free.

If you want to convert async to coroutine, you can use `.ToCoroutine()`, this is useful if you want to only allow using the coroutine system.

UniTask can not await twice. This is a similar constraint to the [ValueTask/IValueTaskSource](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1?view=netcore-3.1) introduced in .NET Standard 2.1.

> The following operations should never be performed on a ValueTask<TResult> instance:
>
> * Awaiting the instance multiple times.
> * Calling AsTask multiple times.
> * Using .Result or .GetAwaiter().GetResult() when the operation hasn't yet completed, or using them multiple times.
> * Using more than one of these techniques to consume the instance.
>
> If you do any of the above, the results are undefined.

```csharp
var task = UniTask.DelayFrame(10);
await task;
await task; // NG, throws Exception
```

Store to the class field, you can use `UniTask.Lazy` that supports calling multiple times. `.Preserve()` allows for multiple calls (internally cached results). This is useful when there are multiple calls in a function scope.

Also `UniTaskCompletionSource` can await multiple times and await from many callers.

Cancellation and Exception handling
---
Some UniTask factory methods have a `CancellationToken cancellationToken = default` parameter. Also some async operations for Unity have `WithCancellation(CancellationToken)` and `ToUniTask(..., CancellationToken cancellation = default)` extension methods. 

You can pass `CancellationToken` to parameter by standard [`CancellationTokenSource`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource).

```csharp
var cts = new CancellationTokenSource();

cancelButton.onClick.AddListener(() =>
{
    cts.Cancel();
});

await UnityWebRequest.Get("http://google.co.jp").SendWebRequest().WithCancellation(cts.Token);

await UniTask.DelayFrame(1000, cancellationToken: cts.Token);
```

CancellationToken can be created by `CancellationTokenSource` or MonoBehaviour's extension method `GetCancellationTokenOnDestroy`.

```csharp
// this CancellationToken lifecycle is same as GameObject.
await UniTask.DelayFrame(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
```

### SimpleRoom
Partie explication SimpleRoomPlacement






















### BSP
Partie explication BSP





























 ### CellAutomata
 Partie explication Cellular Automata
