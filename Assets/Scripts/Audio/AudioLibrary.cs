using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 音频库 - 存储和管理所有音频资源
/// </summary>
public class AudioLibrary : MonoBehaviour
{
    [Header("背景音乐")]
    [SerializeField] private List<AudioClip> menuMusicTracks = new List<AudioClip>();
    [SerializeField] private List<AudioClip> gameMusicTracks = new List<AudioClip>();
    [SerializeField] private AudioClip pauseMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("玩家音效")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip slideSound;
    [SerializeField] private AudioClip wallRunSound;
    [SerializeField] private AudioClip climbSound;

    [Header("金币收集音效")]
    [SerializeField] private AudioClip collectBronzeSound;
    [SerializeField] private AudioClip collectSilverSound;
    [SerializeField] private AudioClip collectGoldSound;
    [SerializeField] private AudioClip collectPlatinumSound;
    [SerializeField] private AudioClip collectDiamondSound;

    [Header("道具音效")]
    [SerializeField] private AudioClip collectMagnetSound;
    [SerializeField] private AudioClip collectShieldSound;
    [SerializeField] private AudioClip collectSpeedBoostSound;
    [SerializeField] private AudioClip collectInvulnerabilitySound;
    [SerializeField] private AudioClip magnetActiveSound;
    [SerializeField] private AudioClip shieldHitSound;
    [SerializeField] private AudioClip shieldBreakSound;
    [SerializeField] private AudioClip speedBoostSound;

    [Header("碰撞和死亡音效")]
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip respawnSound;

    [Header("UI音效")]
    [SerializeField] private AudioClip uiButtonSound;
    [SerializeField] private AudioClip uiCancelSound;
    [SerializeField] private AudioClip uiConfirmSound;
    [SerializeField] private AudioClip uiHoverSound;
    [SerializeField] private AudioClip newHighScoreSound;
    [SerializeField] private AudioClip achievementSound;

    [Header("环境音效")]
    [SerializeField] private AudioClip rainAmbience;
    [SerializeField] private AudioClip windAmbience;
    [SerializeField] private AudioClip forestAmbience;
    [SerializeField] private AudioClip cityAmbience;

    // 音乐索引
    private int currentMenuMusicIndex = 0;
    private int currentGameMusicIndex = 0;

    #region 背景音乐

    public AudioClip GetMenuMusic()
    {
        if (menuMusicTracks.Count == 0) return null;

        AudioClip clip = menuMusicTracks[currentMenuMusicIndex];
        currentMenuMusicIndex = (currentMenuMusicIndex + 1) % menuMusicTracks.Count;
        return clip;
    }

    public AudioClip GetGameMusic()
    {
        if (gameMusicTracks.Count == 0) return null;

        AudioClip clip = gameMusicTracks[currentGameMusicIndex];
        currentGameMusicIndex = (currentGameMusicIndex + 1) % gameMusicTracks.Count;
        return clip;
    }

    public AudioClip GetMusic(string musicName)
    {
        switch (musicName)
        {
            case "Menu":
                return GetMenuMusic();
            case "Game":
                return GetGameMusic();
            case "Pause":
                return pauseMusic;
            case "GameOver":
                return gameOverMusic;
            default:
                return null;
        }
    }

    #endregion

    #region 音效获取

    public AudioClip GetSFX(string sfxName)
    {
        switch (sfxName)
        {
            // 玩家音效
            case "Footstep":
                return footstepSound;
            case "Jump":
                return jumpSound;
            case "DoubleJump":
                return doubleJumpSound;
            case "Land":
                return landSound;
            case "Slide":
                return slideSound;
            case "WallRun":
                return wallRunSound;
            case "Climb":
                return climbSound;

            // 金币收集
            case "CollectBronze":
                return collectBronzeSound;
            case "CollectSilver":
                return collectSilverSound;
            case "CollectGold":
                return collectGoldSound;
            case "CollectPlatinum":
                return collectPlatinumSound;
            case "CollectDiamond":
                return collectDiamondSound;

            // 道具
            case "CollectMagnet":
                return collectMagnetSound;
            case "CollectShield":
                return collectShieldSound;
            case "CollectSpeedBoost":
                return collectSpeedBoostSound;
            case "CollectInvulnerability":
                return collectInvulnerabilitySound;
            case "MagnetActive":
                return magnetActiveSound;
            case "ShieldHit":
                return shieldHitSound;
            case "ShieldBreak":
                return shieldBreakSound;
            case "SpeedBoostActive":
                return speedBoostSound;

            // 碰撞和死亡
            case "Collision":
                return collisionSound;
            case "Death":
                return deathSound;
            case "Respawn":
                return respawnSound;

            // UI音效
            case "UIButton":
                return uiButtonSound;
            case "UICancel":
                return uiCancelSound;
            case "UIConfirm":
                return uiConfirmSound;
            case "UIHover":
                return uiHoverSound;
            case "NewHighScore":
                return newHighScoreSound;
            case "Achievement":
                return achievementSound;

            // 环境音效
            case "Rain":
                return rainAmbience;
            case "Wind":
                return windAmbience;
            case "Forest":
                return forestAmbience;
            case "City":
                return cityAmbience;

            default:
                Debug.LogWarning($"Sound effect '{sfxName}' not found in library.");
                return null;
        }
    }

    #endregion

    #region 音效分类

    public List<AudioClip> GetAllMenuMusic()
    {
        return new List<AudioClip>(menuMusicTracks);
    }

    public List<AudioClip> GetAllGameMusic()
    {
        return new List<AudioClip>(gameMusicTracks);
    }

    public void AddMenuMusic(AudioClip clip)
    {
        if (clip != null && !menuMusicTracks.Contains(clip))
        {
            menuMusicTracks.Add(clip);
        }
    }

    public void AddGameMusic(AudioClip clip)
    {
        if (clip != null && !gameMusicTracks.Contains(clip))
        {
            gameMusicTracks.Add(clip);
        }
    }

    #endregion

    #region 工具方法

    public bool HasMusic(string musicName)
    {
        return GetMusic(musicName) != null;
    }

    public bool HasSFX(string sfxName)
    {
        return GetSFX(sfxName) != null;
    }

    public int GetMenuMusicCount()
    {
        return menuMusicTracks.Count;
    }

    public int GetGameMusicCount()
    {
        return gameMusicTracks.Count;
    }

    #endregion
}
