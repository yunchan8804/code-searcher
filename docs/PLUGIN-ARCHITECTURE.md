# Plugin Architecture Design

> UnicodeSearcher 플러그인 시스템 설계 문서
>
> 작성일: 2025-12-27
> 버전: 1.0

---

## 1. 개요

### 1.1 목적

UnicodeSearcher의 핵심 기능을 유지하면서 확장 가능한 플러그인 시스템을 구축한다.
플러그인을 통해 GIF 검색, 코드 스니펫, 이모지 팩 등 다양한 콘텐츠를 통합된 UI에서 제공한다.

### 1.2 설계 원칙

| 원칙 | 설명 |
|------|------|
| **단일 책임 (SRP)** | 각 플러그인은 하나의 콘텐츠 유형만 담당 |
| **개방-폐쇄 (OCP)** | 코어 수정 없이 새 플러그인 추가 가능 |
| **의존성 역전 (DIP)** | 플러그인은 인터페이스에만 의존 |
| **인터페이스 분리 (ISP)** | 필요한 기능만 구현하도록 인터페이스 세분화 |

### 1.3 아키텍처 다이어그램

```
┌──────────────────────────────────────────────────────────────┐
│                        UnicodeSearcher                        │
├──────────────────────────────────────────────────────────────┤
│  UI Layer                                                     │
│  ├── MainWindow.xaml (탭/모드 전환 UI)                        │
│  ├── PluginHostControl.xaml (플러그인 결과 표시 영역)          │
│  └── SettingsWindow.xaml (플러그인 설정 탭)                   │
├──────────────────────────────────────────────────────────────┤
│  Core Layer                                                   │
│  ├── IPlugin (기본 플러그인 인터페이스)                        │
│  ├── ISearchablePlugin (검색 기능 인터페이스)                  │
│  ├── IPluginManager (플러그인 로딩/관리)                       │
│  └── PluginSettings (활성화 상태 저장)                        │
├──────────────────────────────────────────────────────────────┤
│  Built-in Plugins                                             │
│  ├── UnicodePlugin (기본 유니코드 문자 - 기존 기능)            │
│  ├── GifPlugin (Tenor GIF 검색)                               │
│  └── SnippetPlugin (코드 스니펫) [향후]                       │
└──────────────────────────────────────────────────────────────┘
```

---

## 2. 핵심 인터페이스

### 2.1 IPlugin (기본 플러그인)

모든 플러그인이 구현해야 하는 최소 인터페이스.

```csharp
namespace UnicodeSearcher.Plugins;

/// <summary>
/// 플러그인 기본 인터페이스
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 플러그인 고유 ID (예: "unicode", "gif", "snippet")
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 표시 이름 (예: "유니코드", "GIF", "스니펫")
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 플러그인 설명
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 플러그인 버전
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 탭/버튼에 표시할 아이콘 (Segoe MDL2 Assets 문자 또는 이모지)
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// 플러그인 활성화 상태
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// 플러그인 초기화 (앱 시작 시 호출)
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 플러그인 정리 (앱 종료 시 호출)
    /// </summary>
    Task ShutdownAsync();
}
```

### 2.2 ISearchablePlugin (검색 가능한 플러그인)

검색 기능을 제공하는 플러그인이 추가로 구현하는 인터페이스.

```csharp
namespace UnicodeSearcher.Plugins;

/// <summary>
/// 검색 기능을 제공하는 플러그인
/// </summary>
public interface ISearchablePlugin : IPlugin
{
    /// <summary>
    /// 검색창 플레이스홀더 텍스트
    /// </summary>
    string SearchPlaceholder { get; }

    /// <summary>
    /// 검색 debounce 시간 (ms)
    /// </summary>
    int SearchDebounceMs { get; }

    /// <summary>
    /// 검색 수행
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>검색 결과 목록</returns>
    Task<IReadOnlyList<ISearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 카테고리 목록 (선택적)
    /// </summary>
    IReadOnlyList<PluginCategory>? Categories { get; }

    /// <summary>
    /// 카테고리별 검색 (선택적)
    /// </summary>
    Task<IReadOnlyList<ISearchResult>> SearchByCategoryAsync(
        string categoryId,
        string query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 플러그인 카테고리
/// </summary>
public record PluginCategory(string Id, string Name, string? Icon = null);
```

### 2.3 ISearchResult (검색 결과)

플러그인이 반환하는 검색 결과의 공통 인터페이스.

```csharp
namespace UnicodeSearcher.Plugins;

/// <summary>
/// 검색 결과 항목
/// </summary>
public interface ISearchResult
{
    /// <summary>
    /// 결과 고유 ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 표시할 제목/이름
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 부가 설명 (선택적)
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 결과 유형
    /// </summary>
    SearchResultType Type { get; }

    /// <summary>
    /// 클립보드에 복사할 콘텐츠
    /// </summary>
    Task<ClipboardContent> GetClipboardContentAsync();

    /// <summary>
    /// 미리보기 콘텐츠 (UI에 표시)
    /// </summary>
    object? Preview { get; }
}

/// <summary>
/// 검색 결과 유형
/// </summary>
public enum SearchResultType
{
    /// <summary>텍스트 (유니코드 문자, 스니펫)</summary>
    Text,

    /// <summary>이미지 (GIF, 스티커)</summary>
    Image,

    /// <summary>파일 경로</summary>
    FilePath
}

/// <summary>
/// 클립보드 콘텐츠
/// </summary>
public class ClipboardContent
{
    /// <summary>텍스트 콘텐츠</summary>
    public string? Text { get; init; }

    /// <summary>이미지 콘텐츠 (BitmapSource 또는 byte[])</summary>
    public object? Image { get; init; }

    /// <summary>HTML 콘텐츠 (리치 텍스트용)</summary>
    public string? Html { get; init; }

    /// <summary>파일 경로 목록</summary>
    public IReadOnlyList<string>? FilePaths { get; init; }
}
```

### 2.4 IPluginManager (플러그인 관리자)

플러그인 로딩, 활성화, 라이프사이클 관리를 담당.

```csharp
namespace UnicodeSearcher.Plugins;

/// <summary>
/// 플러그인 관리자 인터페이스
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// 등록된 모든 플러그인
    /// </summary>
    IReadOnlyList<IPlugin> Plugins { get; }

    /// <summary>
    /// 활성화된 플러그인만
    /// </summary>
    IReadOnlyList<IPlugin> EnabledPlugins { get; }

    /// <summary>
    /// 검색 가능한 플러그인만
    /// </summary>
    IReadOnlyList<ISearchablePlugin> SearchablePlugins { get; }

    /// <summary>
    /// 플러그인 등록
    /// </summary>
    void Register(IPlugin plugin);

    /// <summary>
    /// 플러그인 활성화/비활성화
    /// </summary>
    Task SetEnabledAsync(string pluginId, bool enabled);

    /// <summary>
    /// 플러그인 ID로 조회
    /// </summary>
    IPlugin? GetPlugin(string pluginId);

    /// <summary>
    /// 모든 플러그인 초기화
    /// </summary>
    Task InitializeAllAsync();

    /// <summary>
    /// 모든 플러그인 종료
    /// </summary>
    Task ShutdownAllAsync();

    /// <summary>
    /// 플러그인 상태 변경 이벤트
    /// </summary>
    event EventHandler<PluginStateChangedEventArgs>? PluginStateChanged;
}

public class PluginStateChangedEventArgs : EventArgs
{
    public required string PluginId { get; init; }
    public required bool IsEnabled { get; init; }
}
```

---

## 3. 플러그인 설정

### 3.1 UserSettings 확장

```csharp
// Models/UserSettings.cs에 추가

/// <summary>
/// 플러그인 설정
/// </summary>
[JsonPropertyName("plugins")]
public PluginSettings Plugins { get; set; } = new();

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
        ["gif"] = false,     // 기본 비활성화 (API 키 필요)
    };

    /// <summary>
    /// 플러그인별 개별 설정
    /// Key: 플러그인 ID, Value: JSON 직렬화된 설정
    /// </summary>
    [JsonPropertyName("config")]
    public Dictionary<string, JsonElement> Config { get; set; } = new();
}
```

### 3.2 플러그인별 설정 인터페이스 (선택적)

```csharp
/// <summary>
/// 개별 설정을 가지는 플러그인
/// </summary>
public interface IConfigurablePlugin : IPlugin
{
    /// <summary>
    /// 설정 타입
    /// </summary>
    Type ConfigType { get; }

    /// <summary>
    /// 현재 설정 가져오기
    /// </summary>
    object GetConfig();

    /// <summary>
    /// 설정 적용
    /// </summary>
    void ApplyConfig(object config);

    /// <summary>
    /// 설정 UI 컨트롤 생성 (선택적)
    /// </summary>
    FrameworkElement? CreateSettingsControl();
}
```

---

## 4. GIF 플러그인 상세 설계

### 4.1 Tenor API 연동

```csharp
namespace UnicodeSearcher.Plugins.Gif;

/// <summary>
/// GIF 플러그인 설정
/// </summary>
public class GifPluginConfig
{
    /// <summary>
    /// Tenor API 키 (Google Cloud Console에서 발급)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 검색 결과 수 (기본: 20, 최대: 50)
    /// </summary>
    public int ResultLimit { get; set; } = 20;

    /// <summary>
    /// 콘텐츠 필터 (off, low, medium, high)
    /// </summary>
    public string ContentFilter { get; set; } = "medium";

    /// <summary>
    /// 미리보기 크기 (tinygif, nanogif, gif)
    /// </summary>
    public string PreviewSize { get; set; } = "tinygif";

    /// <summary>
    /// 복사 시 사용할 형식 (gif, mp4, webm)
    /// </summary>
    public string CopyFormat { get; set; } = "gif";
}
```

### 4.2 GifSearchResult 구현

```csharp
public class GifSearchResult : ISearchResult
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public SearchResultType Type => SearchResultType.Image;

    /// <summary>미리보기 URL (tinygif)</summary>
    public required string PreviewUrl { get; init; }

    /// <summary>원본 GIF URL</summary>
    public required string FullUrl { get; init; }

    /// <summary>미리보기 이미지 (다운로드 후 캐싱)</summary>
    public object? Preview { get; set; }

    public async Task<ClipboardContent> GetClipboardContentAsync()
    {
        // GIF 다운로드 후 클립보드에 복사
        var gifData = await DownloadGifAsync(FullUrl);
        return new ClipboardContent
        {
            Image = gifData,
            // HTML 형식으로도 복사 (일부 앱 호환성)
            Html = $"<img src=\"{FullUrl}\" />"
        };
    }

    private async Task<byte[]> DownloadGifAsync(string url)
    {
        using var client = new HttpClient();
        return await client.GetByteArrayAsync(url);
    }
}
```

### 4.3 Tenor API 제약사항

| 항목 | 제한 | 대응 방안 |
|------|------|-----------|
| Rate Limit | 1 RPS | 검색 debounce 500ms 이상 |
| 캐시 유효기간 | 24시간 | 메모리 캐시 + 만료 처리 |
| Attribution | 필수 | UI에 "Powered by Tenor" 표시 |
| API Key | 필수 | 사용자가 직접 발급 또는 기본 키 제공 |

---

## 5. UI 통합 가이드

### 5.1 메인 UI 구조

```
┌─────────────────────────────────────────────────────┐
│ [🔍 검색창] [유니코드 ▼] [GIF ○] [스니펫 ○]         │  ← 플러그인 탭/토글
├─────────────────────────────────────────────────────┤
│ 최근 사용: [★] [→] [←] [♥]                         │
├─────────────────────────────────────────────────────┤
│ [전체] [화살표] [기호] [수학] ...                   │  ← 카테고리 (플러그인별)
├─────────────────────────────────────────────────────┤
│                                                     │
│   ┌───┐ ┌───┐ ┌───┐ ┌───┐ ┌───┐                    │
│   │ ★ │ │ → │ │ ← │ │ ♥ │ │ ◆ │  ...              │  ← 결과 그리드
│   └───┘ └───┘ └───┘ └───┘ └───┘                    │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### 5.2 플러그인 전환 방식

**옵션 A: 탭 방식**
- 상단에 탭으로 플러그인 전환
- 각 탭은 독립적인 검색 컨텍스트 유지

**옵션 B: 접두어 방식 (권장)**
- 검색창에 접두어로 플러그인 선택
- 예: `gif:cat`, `snip:for loop`
- 접두어 없으면 기본 유니코드 검색

**옵션 C: 하이브리드**
- 기본은 탭 방식
- 빠른 전환을 위한 접두어 지원

### 5.3 설정 UI

```
┌─────────────────────────────────────────────────────┐
│ 설정                                                │
├─────────────────────────────────────────────────────┤
│ [일반] [단축키] [외관] [플러그인]                    │
├─────────────────────────────────────────────────────┤
│                                                     │
│ 플러그인 관리                                        │
│                                                     │
│ ┌─────────────────────────────────────────────────┐ │
│ │ ☑ 유니코드 문자 (기본)              v1.0.0      │ │
│ │   유니코드 문자 및 기호 검색                     │ │
│ ├─────────────────────────────────────────────────┤ │
│ │ ☐ GIF 검색                          v1.0.0      │ │
│ │   Tenor를 통한 GIF 검색              [설정]     │ │
│ ├─────────────────────────────────────────────────┤ │
│ │ ☐ 코드 스니펫                        v1.0.0      │ │
│ │   자주 쓰는 코드 스니펫 관리         [설정]     │ │
│ └─────────────────────────────────────────────────┘ │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 6. 새 플러그인 개발 가이드

### 6.1 체크리스트

새 플러그인을 만들 때 아래 항목을 확인하세요:

- [ ] `IPlugin` 또는 `ISearchablePlugin` 구현
- [ ] 고유한 `Id` 설정 (영문 소문자, 하이픈 허용)
- [ ] `DisplayName`에 한글 이름 설정
- [ ] `Icon`에 적절한 아이콘 문자 설정
- [ ] `InitializeAsync`에서 리소스 초기화
- [ ] `ShutdownAsync`에서 리소스 정리
- [ ] DI 컨테이너에 등록 (`App.xaml.cs`)
- [ ] `PluginManager`에 등록
- [ ] 기본 설정값 추가 (`PluginSettings.Enabled`)

### 6.2 플러그인 구현 템플릿

```csharp
namespace UnicodeSearcher.Plugins.MyPlugin;

public class MyPlugin : ISearchablePlugin
{
    // === 메타데이터 ===
    public string Id => "my-plugin";
    public string DisplayName => "내 플러그인";
    public string Description => "플러그인 설명";
    public Version Version => new(1, 0, 0);
    public string Icon => "🔌";  // 또는 Segoe MDL2 문자

    // === 상태 ===
    public bool IsEnabled { get; set; }

    // === 검색 설정 ===
    public string SearchPlaceholder => "검색어 입력...";
    public int SearchDebounceMs => 200;
    public IReadOnlyList<PluginCategory>? Categories => null;

    // === 생명주기 ===
    public Task InitializeAsync()
    {
        // 초기화 로직
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        // 정리 로직
        return Task.CompletedTask;
    }

    // === 검색 ===
    public async Task<IReadOnlyList<ISearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<ISearchResult>();

        // 검색 로직 구현
        var results = new List<ISearchResult>();
        // ...
        return results;
    }

    public Task<IReadOnlyList<ISearchResult>> SearchByCategoryAsync(
        string categoryId,
        string query,
        CancellationToken cancellationToken = default)
    {
        // 카테고리별 검색 (필요시 구현)
        return SearchAsync(query, cancellationToken);
    }
}
```

### 6.3 DI 등록 예시

```csharp
// App.xaml.cs
private void ConfigureServices(IServiceCollection services)
{
    // 기존 서비스들...

    // 플러그인 시스템
    services.AddSingleton<IPluginManager, PluginManager>();

    // 플러그인 등록
    services.AddSingleton<IPlugin, UnicodePlugin>();
    services.AddSingleton<IPlugin, GifPlugin>();
    // services.AddSingleton<IPlugin, SnippetPlugin>();  // 향후 추가
}
```

---

## 7. 규칙 및 베스트 프랙티스

### 7.1 네이밍 규칙

| 항목 | 규칙 | 예시 |
|------|------|------|
| 플러그인 ID | 영문 소문자, 하이픈 | `gif`, `code-snippet` |
| 클래스명 | PascalCase + Plugin 접미사 | `GifPlugin`, `SnippetPlugin` |
| 네임스페이스 | `UnicodeSearcher.Plugins.{Name}` | `UnicodeSearcher.Plugins.Gif` |
| 설정 클래스 | `{Name}PluginConfig` | `GifPluginConfig` |
| 결과 클래스 | `{Name}SearchResult` | `GifSearchResult` |

### 7.2 성능 가이드라인

| 항목 | 권장값 | 비고 |
|------|--------|------|
| 검색 debounce | 150-500ms | API 호출 시 500ms 권장 |
| 결과 캐싱 | 5분 이상 | 메모리 캐시 사용 |
| 이미지 로딩 | 비동기 + 썸네일 | 원본은 필요시 로드 |
| 초기화 시간 | 100ms 이하 | 무거운 작업은 지연 로딩 |

### 7.3 에러 처리

```csharp
public async Task<IReadOnlyList<ISearchResult>> SearchAsync(
    string query,
    CancellationToken cancellationToken)
{
    try
    {
        // 검색 로직
    }
    catch (HttpRequestException ex)
    {
        // 네트워크 오류 → 빈 결과 + 로깅
        Debug.WriteLine($"[{Id}] Network error: {ex.Message}");
        return Array.Empty<ISearchResult>();
    }
    catch (OperationCanceledException)
    {
        // 취소됨 → 조용히 빈 결과
        return Array.Empty<ISearchResult>();
    }
    catch (Exception ex)
    {
        // 기타 오류 → 로깅 + 빈 결과
        Debug.WriteLine($"[{Id}] Unexpected error: {ex}");
        return Array.Empty<ISearchResult>();
    }
}
```

### 7.4 테스트 가이드

각 플러그인은 다음 테스트를 포함해야 합니다:

```csharp
[TestClass]
public class MyPluginTests
{
    [TestMethod]
    public async Task Initialize_ShouldSucceed()
    {
        var plugin = new MyPlugin();
        await plugin.InitializeAsync();
        // Assert 초기화 상태
    }

    [TestMethod]
    public async Task Search_WithValidQuery_ShouldReturnResults()
    {
        var plugin = new MyPlugin();
        await plugin.InitializeAsync();

        var results = await plugin.SearchAsync("test");

        Assert.IsTrue(results.Count > 0);
    }

    [TestMethod]
    public async Task Search_WithEmptyQuery_ShouldReturnEmpty()
    {
        var plugin = new MyPlugin();
        var results = await plugin.SearchAsync("");

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task Search_WhenCancelled_ShouldReturnEmpty()
    {
        var plugin = new MyPlugin();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var results = await plugin.SearchAsync("test", cts.Token);

        Assert.AreEqual(0, results.Count);
    }
}
```

---

## 8. 구현 로드맵

### Phase 1: 플러그인 코어 (필수)
- [ ] `IPlugin`, `ISearchablePlugin` 인터페이스 정의
- [ ] `ISearchResult`, `ClipboardContent` 모델 정의
- [ ] `PluginManager` 구현
- [ ] `PluginSettings` 추가 (UserSettings 확장)
- [ ] 기존 유니코드 기능을 `UnicodePlugin`으로 리팩토링

### Phase 2: 설정 UI
- [ ] SettingsWindow에 "플러그인" 탭 추가
- [ ] 플러그인 목록 표시 (활성화 토글)
- [ ] 플러그인별 설정 버튼

### Phase 3: GIF 플러그인
- [ ] Tenor API 클라이언트 구현
- [ ] `GifPlugin` 구현
- [ ] GIF 미리보기 UI
- [ ] 클립보드 복사 (이미지/URL)

### Phase 4: UI 통합
- [ ] 메인 UI에 플러그인 탭/전환 추가
- [ ] 검색 결과 영역 플러그인별 렌더링
- [ ] 키보드 단축키 (Ctrl+1, 2, 3으로 플러그인 전환)

---

## 9. 참고 자료

### API 문서
- [Tenor API Quickstart](https://developers.google.com/tenor/guides/quickstart)
- [Tenor Rate Limits](https://developers.google.com/tenor/guides/rate-limits-and-caching)

### 플러그인 아키텍처
- [WPF Plugin Architecture (Medium)](https://medium.com/c-sharp-programming/introduction-to-a-plug-in-architecture-using-the-example-of-a-wpf-application-7f2e225b647a)
- [MEF Documentation](https://learn.microsoft.com/en-us/dotnet/framework/mef/)

### 프로젝트 내 참고
- `ISearchService` - 검색 인터페이스 패턴
- `UserSettings` - 설정 저장 패턴
- `CharacterDataService` - 데이터 로딩 패턴
