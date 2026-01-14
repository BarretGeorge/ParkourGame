using UnityEngine;

/// <summary>
/// 标签和层级常量定义
/// </summary>
public static class GameTags
{
    // 游戏对象标签
    public const string Player = "Player";
    public const string Obstacle = "Obstacle";
    public const string Coin = "Coin";
    public const string Magnet = "Magnet";
    public const string Shield = "Shield";
    public const string SpeedBoost = "SpeedBoost";
    public const string Checkpoint = "Checkpoint";
    public const string KillPlane = "KillPlane";
}

public static class GameLayers
{
    // 物理层级
    public const int Ground = 0;
    public const int Obstacle = 8;
    public const int Player = 9;
    public const int Collectible = 10;
    public const int Environment = 11;
}
