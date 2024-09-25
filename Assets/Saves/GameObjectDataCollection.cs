using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectDataCollection
{
    public List<CharacterData> characterDataList = new();
    public List<BiomeData> biomeDataList = new();
    public List<GridCubeData> gridCubeDataList = new();
}

[System.Serializable]
public class CharacterData
{
    public string Name;
    public int[] Position; // Position is saved as array. Json files can have a hard time reading vectors
    public int Health;
    public _StatusType Status;

    public CharacterData(Character character)
    {
        Name = character.name;
        Vector3 pos = character.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
        Health = character.Health;
        Status = character.StatusType;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
    }

    public string GetName()
    {
        return Name;
    }

    public int GetHealth()
    {
        return Health;
    }

    public _StatusType GetStatus()
    {
        return Status;
    }
}

[System.Serializable]
public class BiomeData
{
    public string Name;
    public int[] Position;

    public BiomeData(BiomeChunk biomeChunk)
    {
        Name = biomeChunk.name;
        Vector3 pos = biomeChunk.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
    }

    public string GetName()
    {
        return Name;
    }
}


[System.Serializable]
public class GridCubeData
{
    public string Name;
    public int[] Position;
    public _StatusType Status;
    public _SurfaceType Surface;

    public GridCubeData(GridCube cube)
    {
        Name = cube.name;
        Vector3 pos = cube.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
        Status = cube.StatusType;
        Surface = cube.SurfaceType;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
    }

    public string GetName()
    {
        return Name;
    }

    public _StatusType GetStatus()
    {
        return Status;
    }

    public _SurfaceType GetSurface()
    {
        return Surface;
    }
}
