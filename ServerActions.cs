[System.Serializable]
public struct PlayerInitialization
{
    public string BoardData;
    public int playerOrder;

    public PlayerInitialization(int playerOrder, string BoardData)
    {
        this.BoardData = BoardData;
        this.playerOrder = playerOrder;
    }
}


[System.Serializable]
public struct HashCheck 
{
    public string Hash;
    
    public HashCheck(string hash)
    {
        Hash = hash;
    }
}


[System.Serializable]
public struct PlayerData {
    public string NickName;
}


