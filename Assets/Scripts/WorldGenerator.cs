using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public GameObject[] availableBlocks;

    private BlockType[,,] blocksData;
    // Start is called before the first frame update
    void Start()
    {
        if (seed == 0)
        {
            seed = Random.Range(100000, 999999);
            Debug.Log(seed);
        }
        blocksData = new BlockType[size, bedrockHeight + groundHeight + height, size];
        GenerateWorld();
        GenerateWater();
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
        Debug.Log(minH);
        Debug.Log(maxH);

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
        //for (var y = 0; y < bedrockHeight + groundHeight + height; ++y)
        //{
        //    for (var x = 0; x < size; ++x)
        //    {
        //        for (var z = 0; z < size; ++z)
        //        {
        //            if (blocksData[x, y, z] == BlockType.NONE)
        //            {

        //            }
        //        }
        //    }
        //}
    }

    private void CreateBlock(Vector3Int pos, BlockType type)
    {
        Debug.Log(pos);
        GameObject go = availableBlocks[(int)type - 1];
        var block = Instantiate(go, pos, Quaternion.identity);
        block.tag = type.ToString();
        block.transform.SetParent(GameObject.FindGameObjectWithTag("World").transform);
        blocksData[pos.x, pos.y, pos.z] = type;
    }
}
