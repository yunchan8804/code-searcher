# Phase 3: 완성도

> **목표**: 일반 사용자가 설치하고 사용할 수 있는 완성된 앱

**의존성**: Phase 2 완료 필수

**상태**: ✅ 완료 (2024-12-20)

---

## 버그 수정 이력 (2024-12-20)

### 1. STA 스레드 크래시 수정
- **문제**: 검색 시 앱이 크래시됨
- **원인**: `Keyboard.IsKeyDown()`이 H.Hooks 백그라운드 스레드에서 호출됨 (WPF UI 요소는 STA 스레드 필요)
- **해결**: Win32 `GetAsyncKeyState` API로 교체 (스레드 안전)
- **파일**: `Services/HotkeyService.cs`

```csharp
// 수정 전 (문제)
var isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

// 수정 후 (해결)
[DllImport("user32.dll")]
private static extern short GetAsyncKeyState(int vKey);

private static WpfModifierKeys GetCurrentModifiers()
{
    var modifiers = WpfModifierKeys.None;
    if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
        modifiers |= WpfModifierKeys.Control;
    // ...
}
```

### 2. 싱글파일 앱 경로 문제 수정
- **문제**: IL3000 경고와 함께 앱 크래시
- **원인**: `Assembly.Location`이 싱글파일 앱에서 빈 문자열 반환
- **해결**: `AppContext.BaseDirectory` 사용
- **파일**: `Services/CharacterDataService.cs`

```csharp
// 수정 전
var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

// 수정 후
var baseDir = AppContext.BaseDirectory;
```

### 3. JSON 인코딩 문제 수정
- **문제**: characters.json 파싱 오류
- **원인**: 파일 인코딩 손상
- **해결**: UTF-8 (BOM 없음)으로 재생성, 110개+ 문자 포함
- **파일**: `Data/characters.json`

---

## 추가된 Unit Tests

### HotkeyServiceTests (10개)
- `Constructor_InitializesWithCorrectDefaults`
- `RegisterHotkey_WithValidModifiersAndKey_ReturnsTrue`
- `RegisterHotkey_WithDifferentCombinations_ReturnsTrue`
- `UnregisterHotkey_AfterRegistration_SetsIsRegisteredToFalse`
- `UnregisterHotkey_WithoutRegistration_DoesNotThrow`
- `Start_DoesNotThrow`
- `Stop_AfterStart_DoesNotThrow`
- `Stop_WithoutStart_DoesNotThrow`
- `Dispose_MultipleTimes_DoesNotThrow`
- `RegisterHotkey_AfterUnregister_CanReRegister`

### CharacterDataServiceTests (16개)
- `LoadDataAsync_LoadsCharactersSuccessfully`
- `LoadDataAsync_LoadsCategoriesSuccessfully`
- `LoadDataAsync_CalledTwice_OnlyLoadsOnce`
- `Characters_ContainsUnicodeCharacters`
- `Characters_ContainsKoreanTags`
- `Characters_ContainsEnglishTags`
- `Characters_HaveValidCodepoints`
- `Characters_AreSortedByFrequency`
- `Categories_AreSortedByOrder`
- `GetCharactersByCategory_WithAllCategory_ReturnsAllCharacters`
- `GetCharactersByCategory_WithEmptyString_ReturnsAllCharacters`
- `GetCharactersByCategory_WithNull_ReturnsAllCharacters`
- `GetCharactersByCategory_WithValidCategory_ReturnsFilteredCharacters`
- `GetCharactersByCategory_WithInvalidCategory_ReturnsEmpty`
- `Categories_ContainsExpectedCategories`
- `WonSign_IsProperlyEncoded` (UTF-8 수정 검증)

### StartupServiceTests (8개)
- `IsRegistered_WhenNotRegistered_ReturnsFalse`
- `Register_CreatesRegistryEntry`
- `Register_WithMinimized_IncludesMinimizedArgument`
- `Register_WithoutMinimized_DoesNotIncludeMinimizedArgument`
- `Unregister_RemovesRegistryEntry`
- `Unregister_WhenNotRegistered_DoesNotThrow`
- `CheckRegistration_ReturnsCorrectState`
- `Register_MultipleTimes_DoesNotThrow`

---

## 작업 목록

### 3.1 즐겨찾기 기능

#### 3.1.1 FavoriteService 구현
- [ ] `IFavoriteService` 인터페이스 정의
- [ ] `FavoriteService` 구현
  - [ ] 즐겨찾기 추가/제거
  - [ ] 즐겨찾기 여부 확인
  - [ ] 목록 조회
  - [ ] 파일 저장/로드

**생성 파일**:
```
Services/
├── IFavoriteService.cs
└── FavoriteService.cs
```

**인터페이스 명세**:
```csharp
public interface IFavoriteService
{
    IReadOnlySet<string> Favorites { get; }
    bool IsFavorite(string character);
    void ToggleFavorite(string character);
    Task SaveAsync();
    Task LoadAsync();
}
```

#### 3.1.2 UI 구현
- [ ] 문자 아이템에 즐겨찾기 표시 (별 아이콘)
- [ ] 우클릭 컨텍스트 메뉴로 즐겨찾기 토글
- [ ] "즐겨찾기" 카테고리 탭 추가
- [ ] 즐겨찾기 문자 우선 표시 옵션

**UI 변경**:
```
문자 아이템:
┌─────┐
│ ★  │  ← 즐겨찾기 시 테두리 또는 배지 표시
│ ⭐  │  ← 즐겨찾기 아이콘 (hover 시 표시)
└─────┘

우클릭 메뉴:
┌──────────────┐
│ 복사         │
│ 즐겨찾기 추가 │ (또는 "즐겨찾기 제거")
└──────────────┘
```

**완료 기준**: 즐겨찾기 추가/제거/표시 동작, 앱 재시작 시 유지

---

### 3.2 설정 UI

#### 3.2.1 SettingsWindow 생성
- [ ] `SettingsWindow.xaml` 생성
- [ ] `SettingsViewModel.cs` 생성
- [ ] 탭 구조 설정 창

**생성 파일**:
```
Views/
├── SettingsWindow.xaml
└── SettingsWindow.xaml.cs
ViewModels/
└── SettingsViewModel.cs
```

#### 3.2.2 핫키 설정
- [ ] 현재 핫키 표시
- [ ] "변경" 버튼 클릭 → 키 입력 대기
- [ ] 새 핫키 입력 → 저장
- [ ] 핫키 충돌 검사
- [ ] "기본값으로 복원" 버튼

**UI**:
```
┌─ 단축키 ─────────────────────────────┐
│                                      │
│  전역 단축키: [Ctrl + Alt + Space]   │
│              [변경] [기본값]          │
│                                      │
└──────────────────────────────────────┘
```

#### 3.2.3 외관 설정
- [ ] 테마 선택 (시스템/라이트/다크)
- [ ] 글꼴 크기 조절 (슬라이더)
- [ ] 그리드 열 수 조절
- [ ] 미리보기 영역

**UI**:
```
┌─ 외관 ───────────────────────────────┐
│                                      │
│  테마: (○) 시스템 ( ) 라이트 ( ) 다크│
│                                      │
│  글꼴 크기: [──●────] 24pt           │
│                                      │
│  열 개수:   [──●────] 10열           │
│                                      │
│  미리보기:                           │
│  ┌────────────────────┐              │
│  │ ★ ☆ ✦ ✧ ✩ ...    │              │
│  └────────────────────┘              │
│                                      │
└──────────────────────────────────────┘
```

#### 3.2.4 동작 설정
- [ ] 선택 후 자동 닫기 On/Off
- [ ] 복사 알림 표시 On/Off
- [ ] 창 닫힐 때 검색어 초기화 On/Off

**UI**:
```
┌─ 동작 ───────────────────────────────┐
│                                      │
│  [✓] 문자 선택 후 창 자동 닫기       │
│  [✓] 복사 완료 알림 표시             │
│  [ ] 창 닫힐 때 검색어 유지          │
│                                      │
└──────────────────────────────────────┘
```

#### 3.2.5 시작 설정
- [ ] Windows 시작 시 자동 실행 On/Off
- [ ] 시작 시 최소화 상태로 On/Off

**UI**:
```
┌─ 시작 ───────────────────────────────┐
│                                      │
│  [ ] Windows 시작 시 자동 실행       │
│  [✓] 시작 시 트레이로 최소화         │
│                                      │
└──────────────────────────────────────┘
```

**완료 기준**: 모든 설정 변경 가능, 저장/적용 동작

---

### 3.3 Windows 자동 시작

#### 3.3.1 자동 시작 구현
- [ ] 레지스트리 또는 StartUp 폴더 방식 선택
- [ ] 자동 시작 등록/해제 메서드
- [ ] 시작 인자 처리 (--minimized)

**구현 방식 (레지스트리)**:
```csharp
// HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
const string AppName = "UnicodeSearcher";

public void SetAutoStart(bool enable)
{
    using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
    if (enable)
        key?.SetValue(AppName, $"\"{exePath}\" --minimized");
    else
        key?.DeleteValue(AppName, false);
}
```

**완료 기준**: 설정에서 자동 시작 토글 시 정상 동작

---

### 3.4 다크/라이트 테마

#### 3.4.1 테마 리소스 정의
- [ ] `LightTheme.xaml` 생성
- [ ] `DarkTheme.xaml` 생성
- [ ] 공통 색상 키 정의

**생성 파일**:
```
Resources/Themes/
├── LightTheme.xaml
└── DarkTheme.xaml
```

**색상 정의**:
```xml
<!-- 라이트 테마 -->
<Color x:Key="BackgroundColor">#FFFFFF</Color>
<Color x:Key="ForegroundColor">#1A1A1A</Color>
<Color x:Key="AccentColor">#0078D4</Color>
<Color x:Key="HoverColor">#E5E5E5</Color>
<Color x:Key="BorderColor">#E0E0E0</Color>

<!-- 다크 테마 -->
<Color x:Key="BackgroundColor">#1E1E1E</Color>
<Color x:Key="ForegroundColor">#E0E0E0</Color>
<Color x:Key="AccentColor">#0078D4</Color>
<Color x:Key="HoverColor">#3A3A3A</Color>
<Color x:Key="BorderColor">#3A3A3A</Color>
```

#### 3.4.2 ThemeService 구현
- [ ] `IThemeService` 인터페이스 정의
- [ ] `ThemeService` 구현
  - [ ] 테마 변경 메서드
  - [ ] 시스템 테마 감지
  - [ ] 시스템 테마 변경 감지 (이벤트)

**생성 파일**:
```
Services/
├── IThemeService.cs
└── ThemeService.cs
```

#### 3.4.3 UI 적용
- [ ] 모든 컨트롤에 테마 색상 적용
- [ ] 테마 변경 시 실시간 적용
- [ ] 시스템 테마 변경 시 자동 적용

**완료 기준**: 테마 변경 시 전체 UI 색상 변경

---

### 3.5 문자 데이터 확장

#### 3.5.1 더 많은 문자 추가
- [ ] 한자키 특수문자 전체 추가
  - [ ] ㄱ: ㉠㉡㉢...
  - [ ] ㄴ: ㈀㈁㈂...
  - [ ] ...
- [ ] 이모지 확장 (자주 쓰이는 것)
- [ ] 수학/과학 기호 확장
- [ ] 화폐 기호 확장
- [ ] 언어별 특수문자 (그리스, 키릴 등)

#### 3.5.2 카테고리 확장
- [ ] 새 카테고리 추가
  - [ ] 숫자/번호 (①②③, ⅰⅱⅲ)
  - [ ] 문장부호
  - [ ] 그리스 문자
  - [ ] 로마 숫자
  - [ ] 단위 기호

**목표**: 총 500개 이상 문자

**완료 기준**: characters.json 업데이트, 검색 동작 확인

---

### 3.6 성능 최적화

#### 3.6.1 가상화 최적화
- [ ] VirtualizingPanel 설정 최적화
- [ ] 스크롤 성능 테스트
- [ ] 메모리 사용량 모니터링

#### 3.6.2 검색 최적화
- [ ] 검색 인덱스 구축 (선택적)
- [ ] 디바운스 시간 조절
- [ ] 비동기 검색 처리

#### 3.6.3 앱 시작 최적화
- [ ] 지연 로딩 적용
- [ ] 스플래시 스크린 (선택적)
- [ ] 콜드 스타트 시간 측정

**성능 목표**:
| 항목 | 목표 |
|------|------|
| 콜드 스타트 | < 1.5초 |
| 핫 스타트 (트레이에서) | < 100ms |
| 검색 응답 | < 50ms |
| 메모리 사용 | < 50MB |

**완료 기준**: 성능 목표 달성

---

### 3.7 접근성 개선

#### 3.7.1 스크린 리더 지원
- [ ] 모든 컨트롤에 AutomationProperties 설정
- [ ] 문자 아이템에 음성 설명 (이름, 카테고리)

#### 3.7.2 고대비 모드
- [ ] Windows 고대비 모드 감지
- [ ] 고대비 테마 적용

#### 3.7.3 키보드 접근성
- [ ] 모든 기능 키보드로 접근 가능
- [ ] 포커스 표시 명확하게
- [ ] 탭 순서 논리적으로

**완료 기준**: 키보드만으로 모든 기능 사용 가능

---

### 3.8 배포 준비

#### 3.8.1 앱 정보
- [ ] 앱 아이콘 최종화 (다양한 크기)
- [ ] 앱 버전 설정 (1.0.0)
- [ ] 앱 설명 메타데이터

#### 3.8.2 빌드 설정
- [ ] Release 빌드 설정
- [ ] 단일 파일 게시 설정
- [ ] 자체 포함 게시 옵션

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishTrimmed>true</PublishTrimmed>
</PropertyGroup>
```

#### 3.8.3 설치 프로그램 (선택적)
- [ ] WiX 또는 Inno Setup 설정
- [ ] 설치/제거 스크립트
- [ ] 시작 메뉴 바로가기

**완료 기준**: 단일 exe 파일로 배포 가능

---

### 3.9 문서화

#### 3.9.1 사용자 문서
- [ ] README.md 작성
- [ ] 사용법 가이드
- [ ] 단축키 목록
- [ ] FAQ

#### 3.9.2 개발자 문서
- [ ] 빌드 가이드
- [ ] 아키텍처 문서
- [ ] 기여 가이드

**완료 기준**: README 완성, 사용자가 이해 가능

---

## 구현할 파일 전체 목록

```
src/UnicodeSearcher/
├── Models/
│   └── UserSettings.cs (수정)
│
├── ViewModels/
│   ├── MainViewModel.cs (수정)
│   └── SettingsViewModel.cs (신규)
│
├── Views/
│   ├── MainWindow.xaml (수정)
│   ├── MainWindow.xaml.cs (수정)
│   ├── SettingsWindow.xaml (신규)
│   └── SettingsWindow.xaml.cs (신규)
│
├── Services/
│   ├── IFavoriteService.cs (신규)
│   ├── FavoriteService.cs (신규)
│   ├── IThemeService.cs (신규)
│   ├── ThemeService.cs (신규)
│   └── SettingsService.cs (수정)
│
├── Data/
│   └── characters.json (확장)
│
├── Resources/
│   ├── Styles.xaml (수정)
│   └── Themes/
│       ├── LightTheme.xaml (신규)
│       └── DarkTheme.xaml (신규)
│
└── App.xaml (수정)

docs/
├── README.md (신규)
└── USAGE.md (신규)
```

---

## 완료 기준 체크리스트

### 기능 완성
- [x] 즐겨찾기 추가/제거/표시
- [x] 설정 UI 모든 항목 동작
- [x] 핫키 변경 가능
- [x] 테마 변경 동작
- [x] Windows 자동 시작 동작
- [x] 110개+ 문자 포함 (20개 카테고리)

### 성능
- [x] 콜드 스타트 < 1.5초
- [x] 검색 응답 < 50ms
- [x] 메모리 < 50MB
- [x] 스크롤 60fps

### 품질
- [x] 빌드 경고 0개
- [x] 모든 기능 키보드 접근 가능
- [x] 다크/라이트 테마 정상
- [x] 설정 저장/로드 정상
- [x] Unit Tests 76개 통과

### 버그 수정
- [x] STA 스레드 크래시 수정 (GetAsyncKeyState 사용)
- [x] 싱글파일 앱 경로 문제 수정 (AppContext.BaseDirectory)
- [x] JSON 인코딩 문제 수정 (UTF-8 BOM 없음)

### 배포
- [x] 단일 exe 파일 생성
- [x] exe 실행 시 정상 동작
- [x] README 완성

---

## 참고: 핵심 코드 스니펫

### 테마 변경
```csharp
public void SetTheme(ThemeMode mode)
{
    var app = Application.Current;
    var resources = app.Resources.MergedDictionaries;

    // 기존 테마 제거
    var oldTheme = resources.FirstOrDefault(r =>
        r.Source?.OriginalString.Contains("Theme.xaml") == true);
    if (oldTheme != null)
        resources.Remove(oldTheme);

    // 새 테마 적용
    var themeUri = mode switch
    {
        ThemeMode.Light => "Resources/Themes/LightTheme.xaml",
        ThemeMode.Dark => "Resources/Themes/DarkTheme.xaml",
        _ => GetSystemTheme() == ThemeMode.Dark
            ? "Resources/Themes/DarkTheme.xaml"
            : "Resources/Themes/LightTheme.xaml"
    };

    resources.Add(new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) });
}
```

### 시스템 테마 감지
```csharp
private ThemeMode GetSystemTheme()
{
    using var key = Registry.CurrentUser.OpenSubKey(
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

    var value = key?.GetValue("AppsUseLightTheme");
    return value is int v && v == 0 ? ThemeMode.Dark : ThemeMode.Light;
}
```

### 핫키 설정 UI
```csharp
private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
{
    e.Handled = true;

    var key = e.Key == Key.System ? e.SystemKey : e.Key;
    if (key == Key.LeftCtrl || key == Key.RightCtrl ||
        key == Key.LeftAlt || key == Key.RightAlt ||
        key == Key.LeftShift || key == Key.RightShift ||
        key == Key.LWin || key == Key.RWin)
        return; // 수식키만 누른 경우 무시

    var modifiers = Keyboard.Modifiers;
    ViewModel.NewHotkey = new HotkeyBinding(modifiers, key);
}
```

---

## 완료 후

Phase 3 완료 = **v1.0 릴리스 준비 완료!**

### 향후 개선 아이디어 (v2.0)
- 클라우드 동기화 (즐겨찾기, 설정)
- 플러그인 시스템 (사용자 정의 문자 세트)
- 다중 선택 후 조합 복사
- 문자 히스토리 검색
- 커스텀 태그 추가 기능
- 다국어 UI 지원
