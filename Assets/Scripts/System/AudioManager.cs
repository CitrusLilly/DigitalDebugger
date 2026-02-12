using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
/// <summary>
/// BGMとSEを管理するシングルトン
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ========================定数==========================
    // ===ファイル===
    private const string SETTINGS_FILE_NAME = "settings.json";
    // ===ミキサー===
    private const string MIXER_PARAM_BGM_VOLUME = "BGMVolume";
    private const string MIXER_PARAM_SE_VOLUME = "SEVolume";
    // ======================================================

    public static AudioManager Instance { get; private set; }

    private string settingsFilePath; // 設定ファイルパス

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer; // 音量調整用のミキサー

    [Header("DefaultVolume")]
    [SerializeField] private float defaultBGMVolume = 0.5f;
    [SerializeField] private float defaultSEVolume = 0.5f;

    [Header("SettingUI")] // 音量調整用のスライダーオブジェクトの参照
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioClip> bgmClips;  // インスペクターからBGMをセット

    [Header("SE")]
    [SerializeField] private AudioSource seSource;
    [SerializeField] private List<AudioClip> seClips;   // インスペクターからSEをセット

    // 名前からクリップを取得できるように辞書化
    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> seDict = new Dictionary<string, AudioClip>();

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 音量設定の読み込み
        settingsFilePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

        InitializeAudioClips();
    }

    private void Start() {
        // 起動時に音量設定を読み込む
        LoadSetting();
    }

    /// <summary>
    /// 登録済みのAudioClipを辞書にキャッシュ
    /// </summary>
    private void InitializeAudioClips() {
        foreach (var clip in bgmClips) {
            bgmDict[clip.name] = clip;
        }
        foreach (var clip in seClips) {
            seDict[clip.name] = clip;
        }
    }

    /// <summary>
    /// BGMの再生
    /// </summary>
    /// <param name="name"> BGM名 </param>
    /// <param name="loop"> ループ </param>
    public void PlayBGM(string name, bool loop = true) {
        if (bgmDict.ContainsKey(name)) {
            bgmSource.clip = bgmDict[name];
            bgmSource.loop = loop;
            bgmSource.Play();
        } else {
            Debug.LogError($"{name}が見つかりません。");
        }
    }

    /// <summary>
    /// 現在再生しているBGMを停止する
    /// </summary>
    public void StopBGM() => bgmSource.Stop();

    /// <summary>
    /// SEの再生。
    /// </summary>
    /// <param name="name"> SE名 </param>
    public void PlaySE(string name) {
        if (seDict.ContainsKey(name)) {
            seSource.PlayOneShot(seDict[name]);
        } else {
            Debug.LogError($"{name}が見つかりません。");
        }
    }


    /// <summary>
    /// 音量設定の反映
    /// </summary>
    private void LoadSetting() {
        if (File.Exists(settingsFilePath)) {
            // 設定ファイルが存在すれば読み込んで設定を適用
            string json = File.ReadAllText(settingsFilePath);
            GameSetting setting = JsonUtility.FromJson<GameSetting>(json);
            // オーディオミキサーにボリュームを設定
            SetVolume(MIXER_PARAM_BGM_VOLUME, setting.bgmVolume);
            SetVolume(MIXER_PARAM_SE_VOLUME, setting.seVolume);

            // スライダーに反映
            bgmSlider.value = setting.bgmVolume;
            seSlider.value = setting.seVolume;
        } else {
            // ファイルがなければミキサーにデフォルト値をセット
            SetVolume(MIXER_PARAM_BGM_VOLUME, defaultBGMVolume);
            SetVolume(MIXER_PARAM_SE_VOLUME, defaultSEVolume);
            // スライダーに反映
            bgmSlider.value = defaultBGMVolume;
            seSlider.value = defaultSEVolume;

            // デフォルト値で保存
            SaveSetting();
        }
    }

    /// <summary>
    /// 現在の音量を設定ファイルに保存する
    /// </summary>
    public void SaveSetting() {
        GameSetting setting = new GameSetting();

        // 現在の値を取得
        setting.bgmVolume = bgmSlider.value;
        setting.seVolume = seSlider.value;

        // JSONで保存
        string json = JsonUtility.ToJson(setting);
        File.WriteAllText(settingsFilePath,json);
    }

    /// <summary>
    /// スライダーイベントから呼び出す
    /// BGMを変更して保存
    /// </summary>
    public void OnBGMVolumeChanged(float volume) {
        SetVolume(MIXER_PARAM_BGM_VOLUME, volume);
        SaveSetting();
    }

    /// <summary>
    /// スライダーイベントから呼び出す
    /// SEを変更して保存
    /// </summary>
    public void OnSEVolumeChanged(float volume) {
        SetVolume(MIXER_PARAM_SE_VOLUME, volume);
        SaveSetting();
    }

    /// <summary>
    /// ボリュームをオーディオミキサーに設定
    /// </summary>
    public void SetVolume(string parameterName, float volume) {
        if (volume == 0) {
            audioMixer.SetFloat(parameterName, -80f); // ミュート
        } else {
            audioMixer.SetFloat(parameterName, Mathf.Log10(volume) * 20);
        }
    }
}
