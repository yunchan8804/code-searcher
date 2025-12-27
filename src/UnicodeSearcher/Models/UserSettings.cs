using System.Text.Json.Serialization;
using System.Windows.Input;

namespace UnicodeSearcher.Models;

/// <summary>
/// 사용자 설정 모델
/// </summary>
public class UserSettings
{
    /// <summary>
    /// 핫키 설정
    /// </summary>
    [JsonPropertyName("hotkey")]
    public HotkeySettings Hotkey { get; set; } = new();

    /// <summary>
    /// 동작 설정
    /// </summary>
    [JsonPropertyName("behavior")]
    public BehaviorSettings Behavior { get; set; } = new();

    /// <summary>
    /// 외관 설정
    /// </summary>
    [JsonPropertyName("appearance")]
    public AppearanceSettings Appearance { get; set; } = new();

    /// <summary>
    /// 시작 설정
    /// </summary>
    [JsonPropertyName("startup")]
    public StartupSettings Startup { get; set; } = new();

    /// <summary>
    /// 플러그인 설정
    /// </summary>
    [JsonPropertyName("plugins")]
    public PluginSettings Plugins { get; set; } = new();
}

/// <summary>
/// 핫키 설정
/// </summary>
public class HotkeySettings
{
    [JsonPropertyName("modifiers")]
    public string Modifiers { get; set; } = "Ctrl+Alt";

    [JsonPropertyName("key")]
    public string Key { get; set; } = "Space";

    [JsonIgnore]
    public ModifierKeys ModifierKeys
    {
        get
        {
            var result = ModifierKeys.None;
            if (Modifiers.Contains("Ctrl")) result |= ModifierKeys.Control;
            if (Modifiers.Contains("Alt")) result |= ModifierKeys.Alt;
            if (Modifiers.Contains("Shift")) result |= ModifierKeys.Shift;
            if (Modifiers.Contains("Win")) result |= ModifierKeys.Windows;
            return result;
        }
        set
        {
            var parts = new List<string>();
            if (value.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (value.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (value.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (value.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            Modifiers = string.Join("+", parts);
        }
    }

    [JsonIgnore]
    public Key KeyValue
    {
        get => Enum.TryParse<Key>(Key, out var key) ? key : System.Windows.Input.Key.Space;
        set => Key = value.ToString();
    }
}

/// <summary>
/// 동작 설정
/// </summary>
public class BehaviorSettings
{
    /// <summary>
    /// 선택 후 창 닫기
    /// </summary>
    [JsonPropertyName("closeOnSelect")]
    public bool CloseOnSelect { get; set; } = true;

    /// <summary>
    /// 복사 시 알림 표시
    /// </summary>
    [JsonPropertyName("showNotification")]
    public bool ShowNotification { get; set; } = true;

    /// <summary>
    /// 창 닫을 때 검색어 초기화
    /// </summary>
    [JsonPropertyName("clearSearchOnClose")]
    public bool ClearSearchOnClose { get; set; } = true;

    /// <summary>
    /// 창 시작 위치: "center" 또는 "cursor"
    /// </summary>
    [JsonPropertyName("windowPosition")]
    public string WindowPosition { get; set; } = "cursor";

    /// <summary>
    /// 최근 사용 문자 최대 개수
    /// </summary>
    [JsonPropertyName("maxRecentCharacters")]
    public int MaxRecentCharacters { get; set; } = 20;

    /// <summary>
    /// 작업 표시줄에 표시
    /// </summary>
    [JsonPropertyName("showInTaskbar")]
    public bool ShowInTaskbar { get; set; } = false;
}

/// <summary>
/// 외관 설정
/// </summary>
public class AppearanceSettings
{
    /// <summary>
    /// 문자 크기 (기본: 24)
    /// </summary>
    [JsonPropertyName("fontSize")]
    public int FontSize { get; set; } = 24;

    /// <summary>
    /// 그리드 아이템 크기 (기본: 48)
    /// </summary>
    [JsonPropertyName("itemSize")]
    public int ItemSize { get; set; } = 48;

    /// <summary>
    /// 테마: "light" 또는 "dark"
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "light";
}

/// <summary>
/// 시작 설정
/// </summary>
public class StartupSettings
{
    /// <summary>
    /// Windows 시작 시 자동 실행
    /// </summary>
    [JsonPropertyName("runAtStartup")]
    public bool RunAtStartup { get; set; } = false;

    /// <summary>
    /// 시작 시 최소화 (트레이로)
    /// </summary>
    [JsonPropertyName("startMinimized")]
    public bool StartMinimized { get; set; } = true;
}

/// <summary>
/// 플러그인 설정
/// </summary>
public class PluginSettings
{
    /// <summary>
    /// 플러그인별 활성화 상태
    /// Key: 플러그인 ID, Value: 활성화 여부
    /// </summary>
    [JsonPropertyName("enabled")]
    public Dictionary<string, bool> Enabled { get; set; } = new()
    {
        ["unicode"] = true,  // 기본 활성화
    };

    /// <summary>
    /// 마지막 활성 플러그인 ID
    /// </summary>
    [JsonPropertyName("lastActivePlugin")]
    public string LastActivePlugin { get; set; } = "unicode";
}
