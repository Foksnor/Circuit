using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectDataCollection
{
    public List<CharacterData> characterDataList = new();
    public List<BiomeData> biomeDataList = new();
    public List<GridCubeData> gridCubeDataList = new();
    public List<CardData> cardDataList = new();
}

[System.Serializable]
public class CharacterData
{
    public string Name;
    public int Health;
    public _StatusType Status;
    public int[] Position; // Position is saved as array. Json files can have a hard time reading vectors

    public CharacterData(Character character)
    {
        Name = character.name;
        Vector3 pos = character.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
        Health = character.Health;
        Status = character.StatusType;
    }

    public string GetName()
    {
        return Name;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
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
    public int BiomeID;
    public int[] Position; // Position is saved as array. Json files can have a hard time reading vectors

    public BiomeData(BiomeChunk biomeChunk)
    {
        Name = biomeChunk.name;
        BiomeID = biomeChunk.biomeID;
        Vector3 pos = biomeChunk.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
    }

    public string GetName()
    {
        return Name;
    }

    public int GetBiomeID()
    {
        return BiomeID;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
    }
}

[System.Serializable]
public class GridCubeData
{
    public string Name;
    public _StatusType Status;
    public _SurfaceType Surface;
    public int[] Position; // Position is saved as array. Json files can have a hard time reading vectors

    public GridCubeData(GridCube cube)
    {
        Name = cube.name;
        Vector3 pos = cube.transform.position;
        Position = new int[] { (int)pos.x, (int)pos.y, (int)pos.z };
        Status = cube.StatusType;
        Surface = cube.SurfaceType;
    }

    public string GetName()
    {
        return Name;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(Position[0], Position[1], Position[2]);
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

public enum _CardPlacement { Hand, Deck, Discard };
[System.Serializable]
public class CardData
{
    public string Name;
    public _CardPlacement CardPlacement;

    public CardData(CardScriptableObject card, _CardPlacement cardPlacement)
    {
        Name = card.name;
        CardPlacement = cardPlacement;
    }
}