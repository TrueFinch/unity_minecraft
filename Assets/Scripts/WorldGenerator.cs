using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public enum BlockType
{
    NONE = 0,
    BEDROCK = 1,
    STONE = 2,
    GRASS = 3,
    WATER = 4
}

public class WorldGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject creeper;

    public uint size;
    public uint height;
    public uint groundHeight;
    public uint bedrockHeight;
    public int seed;
    public float octave1;
    public float octave2;
    public float octave3;
    public float freq1;
    public float freq2;
    public float freq3;
    public float exponent;

    public int creeperCount = 1;
    public float creeperDistanceFromPlayer = 10F;

    public GameObject[] availableBlocks;

    private BlockType[,,] blocksData;
    private Dictionary<Vector3Int, GameObject> blocksRef;
    // Start is called before the first frame update
    void Start()
    {
        if (seed == 0)
        {
            seed = Random.Range(100000, 999999);
            Debug.Log(seed);
        }
        blocksData = new BlockType[size, bedrockHeight + groundHeight + height, size];
        blocksRef = new Dictionary<Vector3Int, GameObject>();

        GenerateWorld();
        GenerateWater();
        SpawnPlayer();
        for (var i = 0; i < creeperCount; ++i)
        {
            SpawnCreeper();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GenerateWorld()
    {
        // calculate heights map
        long[,] heightsMap = new long[size, size];
        long minH = (int)(height + 1), maxH = 0;
        for (var x = 0; x < size; ++x)
        {
            for (var z = 0; z < size; ++z)
            {
                float x_norm = (float)(x + seed) / size, z_norm = (float)(z + seed) / size;
                float e = octave1 * Mathf.PerlinNoise(freq1 * x_norm, freq1 * z_norm)
                        + octave2 * Mathf.PerlinNoise(freq2 * x_norm, freq2 * z_norm)
                        + octave3 * Mathf.PerlinNoise(freq3 * x_norm, freq3 * z_norm);
                e /= (octave1 + octave2 + octave3);
                e = Mathf.Pow(e, exponent);
                heightsMap[x, z] = Mathf.FloorToInt(e * height);
                minH = Mathf.FloorToInt(Mathf.Min(minH, heightsMap[x, z]));
                maxH = Mathf.FloorToInt(Mathf.Max(maxH, heightsMap[x, z]));
            }
        }
        //Debug.Log(minH);
        //Debug.Log(maxH);

        for (var x = 0; x < size; ++x)
        {
            for (var z = 0; z < size; ++z)
            {
                uint last_y = 0;
                // generate bedrock layers
                for (int y = (int)last_y; y < bedrockHeight - 1; ++y)
                {
                    CreateBlock(new Vector3Int(x, y, z), BlockType.BEDROCK);
                }
                CreateBlock(new Vector3Int(x, (int)(bedrockHeight - 1), z),
                    Random.value > 0.3 ? BlockType.BEDROCK : BlockType.STONE);

                last_y = bedrockHeight;
                // generate stone layers
                for (int y = 0; y < groundHeight; ++y)
                {
                    CreateBlock(new Vector3Int(x, (int)(last_y + y), z), BlockType.STONE);
                }

                last_y += groundHeight;
                // generate landscape
                for (int y = 0; y < heightsMap[x, z]; ++y)
                {
                    CreateBlock(new Vector3Int(x, (int)(last_y + y), z), BlockType.STONE);
                }
                CreateBlock(new Vector3Int(x, (int)(last_y + heightsMap[x, z]), z),
                    Random.value > 0.9 ? BlockType.STONE : BlockType.GRASS);
            }
        }
    }

    private void GenerateWater()
    {
        for (var y = 0; y < bedrockHeight + groundHeight + height; ++y)
        {
            if (CanCreateRiver(y))
            {
                for (var x = 0; x < size; ++x)
                {
                    for (var z = 0; z < size; ++z)
                    {
                        if (blocksData[x, y, z] == BlockType.NONE)
                        {
                            CreateBlock(new Vector3Int(x, y, z), BlockType.WATER);
                        }
                    }
                }
                break;
            }
        }
    }

    private bool IsPosValid(Vector3Int pos)
    {
        return 0 <= pos.x && pos.x < size
            && 0 <= pos.y && pos.y < bedrockHeight + groundHeight + height
            && 0 <= pos.z && pos.z < size;
    }

    private bool IsPosValid(Vector2Int pos)
    {
        return 0 <= pos.x && pos.x < size
            && 0 <= pos.y && pos.y < size;
    }

    private BlockType GetBlockData(Vector3Int pos)
    {
        return blocksData[pos.x, pos.y, pos.z];
    }

    private bool GetUncoloredPos(int[,] colors, out Vector2Int res)
    {
        for (var x = 0; x < size; ++x)
        {
            for (var z = 0; z < size; ++z)
            {
                if (colors[x, z] == 0)
                {
                    res = new Vector2Int(x, z);
                    return true;
                }
            }
        }
        res = Vector2Int.zero;
        return false;
    }
    private HashSet<int> GetBorderColors(int[,] colors, int sx, int fx, int sz, int fz)
    {
        HashSet<int> set = new HashSet<int>();
        for (int x = sx; x <= fx; ++x)
        {
            for (int z = sz; z <= fz; ++z)
            {
                if (colors[x, z] != -1)
                {
                    set.Add(colors[x, z]);
                }
            }
        }
        return set;
    }

    private bool CheckRiverIsBig(int[,] colors)
    {
        var downSet = GetBorderColors(colors, 0, (int)(size - 1), 0, 0);
        var leftSet = GetBorderColors(colors, 0, 0, 0, (int)(size - 1));
        var upSet = GetBorderColors(colors, 0, (int)(size - 1), (int)(size - 1), (int)(size - 1));
        var rightSet = GetBorderColors(colors, (int)(size - 1), (int)(size - 1), 0, (int)(size - 1));
        return downSet.Intersect(leftSet).Count() != 0
            || downSet.Intersect(upSet).Count() != 0
            || downSet.Intersect(rightSet).Count() != 0
            || leftSet.Intersect(upSet).Count() != 0
            || leftSet.Intersect(rightSet).Count() != 0
            || upSet.Intersect(rightSet).Count() != 0;
    }

    private bool CanCreateRiver(int y)
    {
        int[,] colors = new int[size, size];
        for (var x = 0; x < size; ++x)
        {
            for (var z = 0; z < size; ++z)
            {
                colors[x, z] = blocksData[x, y, z] != BlockType.NONE ? -1 : 0;
            }
        }
        int curColor = 0;
        bool emptyExist = true;
        Queue<Vector2Int> points = new Queue<Vector2Int>();
        while (emptyExist)
        {
            if (points.Count == 0)
            {
                ++curColor;
                Vector2Int pos;
                if (GetUncoloredPos(colors, out pos))
                {
                    colors[pos.x, pos.y] = curColor;
                    points.Enqueue(pos);
                }
                else
                {
                    emptyExist = false;
                    break;
                }
            }
            Vector2Int current = points.Dequeue();
            Vector2Int[] neighbours = new Vector2Int[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };
            foreach (var n in neighbours)
            {
                var nPos = new Vector2Int(current.x + n.x, current.y + n.y);
                if (IsPosValid(nPos) && colors[nPos.x, nPos.y] == 0)
                {
                    colors[current.x + n.x, current.y + n.y] = curColor;
                    points.Enqueue(current + n);
                }
            }
        }

        return CheckRiverIsBig(colors);
    }

    private void SpawnPlayer()
    {
        //spawn player at thee highest point
        for (var y = bedrockHeight + groundHeight + height - 1; y >= 0; --y)
        {
            for (var x = 0; x < size; ++x)
            {
                for (var z = 0; z < size; ++z)
                {
                    if (blocksData[x, y, z] != BlockType.NONE)
                    {
                        Instantiate(player, new Vector3(x, y + 1, z), Quaternion.identity);
                        return;
                    }
                }
            }
        }
    }

    private void SpawnCreeper()
    {
        var player_transform = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3 pos = Vector3.zero;
        while (true)
        {
            pos.x = Mathf.FloorToInt(Random.Range(0, size - 1));
            pos.z = Mathf.FloorToInt(Random.Range(0, size - 1));
            if (Vector3.Distance(player_transform.position, pos) >= creeperDistanceFromPlayer)
            {
                break;
            }
        }
        int x = (int)pos.x, z = (int)pos.z;
        for (var y = 0; y < bedrockHeight + groundHeight + height; ++y)
        {
            if (blocksData[x, y, z] == BlockType.NONE)
            {
                Instantiate(creeper, new Vector3(x, y + 1, z), Quaternion.identity);
                break;
            }
        }
    }

    public void CreateBlock(Vector3Int pos, BlockType type)
    {
        if (GetBlockData(pos) != BlockType.NONE || type == BlockType.NONE) {
            return;
        }
        GameObject go = availableBlocks[(int)type - 1];
        var block = Instantiate(go, pos, Quaternion.identity);
        blocksRef.Add(pos, block);
        block.tag = type.ToString();
        block.transform.SetParent(GameObject.FindGameObjectWithTag("World").transform);
        blocksData[pos.x, pos.y, pos.z] = type;

        if (type == BlockType.WATER)
        {
            Vector3Int[] neighbours = new Vector3Int[] {
                Vector3Int.forward, Vector3Int.back,
                Vector3Int.left, Vector3Int.right
            };
            foreach (var n in neighbours)
            {
                var n1Pos = new Vector3Int(pos.x + n.x * 2, pos.y, pos.z + n.z * 2);
                var n2Pos = new Vector3Int(pos.x + n.x, pos.y, pos.z + n.z);
                if (IsPosValid(n1Pos) && IsPosValid(n2Pos)
                    && GetBlockData(n1Pos) == BlockType.WATER
                        && GetBlockData(n2Pos) == BlockType.NONE)
                {
                    // TODO: move to coroutine
                    StartCoroutine(CreateBlockCoroutine(n2Pos, BlockType.WATER));
                }
            }
            var downPos = new Vector3Int(pos.x, pos.y - 1, pos.z);

            if (IsPosValid(downPos) && GetBlockData(downPos) == BlockType.NONE)
            {
                // TODO: move to coroutine
                StartCoroutine(CreateBlockCoroutine(downPos, BlockType.WATER));
            }
        }
    }

    public void DestroyBlock(Vector3Int pos)
    {
        if (blocksRef.ContainsKey(pos))
        {
            Destroy(blocksRef[pos]);
            blocksRef.Remove(pos);
            blocksData[pos.x, pos.y, pos.z] = BlockType.NONE;

            var upPos = pos + Vector3Int.up;
            if (IsPosValid(upPos) && GetBlockData(upPos) == BlockType.WATER)
            {
                // TODO: move to coroutine
                StartCoroutine(CreateBlockCoroutine(pos, BlockType.WATER));
            }
            else
            {
                var neighbors = new Tuple<Vector3Int, Vector3Int>[]
                {
                    new Tuple<Vector3Int, Vector3Int>(pos + Vector3Int.left, pos + Vector3Int.right),
                    new Tuple<Vector3Int, Vector3Int>(pos + Vector3Int.forward, pos + Vector3Int.back),
                };
                foreach (var n in neighbors)
                {
                    if (IsPosValid(n.Item1) && IsPosValid(n.Item2)
                        && GetBlockData(n.Item1) == BlockType.WATER
                        && GetBlockData(n.Item2) == BlockType.WATER)
                    {
                        // TODO: move to coroutine
                        StartCoroutine(CreateBlockCoroutine(pos, BlockType.WATER));
                        break;
                    }
                }
            }
        }
    }

    private List<Vector3Int> GetSpherePoints(int y, int radius)
    {
        HashSet<Vector3Int> res = new HashSet<Vector3Int>() { new Vector3Int(0, y, 0) };
        for (var x = 1; x < radius; ++x)
        {
            res.Add(new Vector3Int(x, y, 0));
            res.Add(new Vector3Int(-x, y, 0));
            res.Add(new Vector3Int(0, y, x));
            res.Add(new Vector3Int(0, y, -x));
            for (var z = 1; z < radius - x; ++z)
            {
                res.Add(new Vector3Int(x, y, z));
                res.Add(new Vector3Int(-x, y, z));
                res.Add(new Vector3Int(x, y, -z));
                res.Add(new Vector3Int(-x, y, -z));
            }
        }
        return res.ToList();
    }

    public void DestroySphere(Vector3Int center, int radius)
    {
        HashSet<Vector3Int> shifts = new HashSet<Vector3Int>();
        for (var y = -radius; y <= radius; ++y)
        {
            shifts.UnionWith(GetSpherePoints(y, radius - Mathf.Abs(y)));
        }

        foreach(var shift in shifts)
        {
            DestroyBlock(center + shift);
        }
    }

    IEnumerator CreateBlockCoroutine(Vector3Int pos, BlockType type)
    {
        yield return new WaitForSeconds(1F);
        CreateBlock(pos, type);
    }
}
