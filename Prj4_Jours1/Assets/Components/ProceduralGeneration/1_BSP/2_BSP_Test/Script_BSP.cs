using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VTools.RandomService;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP/ New_BSP")]
public class Script_BSP : ProceduralGenerationMethod
{
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        Debug.Log("Test algo");
        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        var root = new TestNode(allGrid, RandomService);
    }
}

public class TestNode
{
    private readonly RectInt _bounds;
    private readonly RandomService _randomService;
    private TestNode _child1, _child2;

    private Vector2Int _roomMinSize = new(5, 5);

    public TestNode(RectInt bounds, RandomService randomService)
    {
        _bounds = bounds;
        _randomService = randomService;

        RectInt splitBoundsLeft = new RectInt(_bounds.xMin, _bounds.yMin, _bounds.width / 2, _bounds.height);
        RectInt splitBoundsRight = new RectInt(_bounds.xMin + _bounds.width / 2, _bounds.yMax, _bounds.width / 2, _bounds.height);

        if (splitBoundsLeft.width < _roomMinSize.x || splitBoundsLeft.height < _roomMinSize.y)
        {
            // It's a Leaf !
            //Place....

            return;
        }

        _child1 = new TestNode(splitBoundsLeft, _randomService);
        _child2 = new TestNode(splitBoundsRight, _randomService);

        Debug.Log("Test Node --> Create OK");
    }
}

public class Node
{
    private Node Child1, Child2;

    private readonly RectInt _room;
    private readonly RandomService _randomService;
    private readonly VTools.Grid.Grid _grid;

    public Node(RandomService randomService, VTools.Grid.Grid grid, RectInt room)
    {
        _randomService = randomService;
        _grid = grid;
        _room = room;

        Split();
    }

    private void Split()
    {
        bool horizontal = _randomService.Chance(0.5f);

        if(horizontal)
        {
            int widthSplit = _room.height / 2;

            RectInt splitBoundsLeft = new RectInt(_room.xMin, _room.yMin, _room.width, widthSplit);
            RectInt splitBoundsRight = new RectInt(_room.xMin, _room.yMin + widthSplit, _room.width, _room.height);

            Child1 = new Node(_randomService, _grid, splitBoundsLeft);
            Child2 = new Node(_randomService, _grid, splitBoundsRight);
        }

        else
        {
            int heightSplit = _room.width / 2;

            RectInt splitBoundsUp = new RectInt(_room.xMin, _room.yMin, heightSplit , _room.height);
            RectInt splitBoundsDown = new RectInt(_room.xMin + heightSplit, _room.yMin, _room.width, _room.height);

            Child1 = new Node(_randomService, _grid, splitBoundsUp);
            Child2 = new Node(_randomService, _grid, splitBoundsDown);
        }
    }
}
