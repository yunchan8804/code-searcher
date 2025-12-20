# Phase 2: 핵심 기능

> **목표**: 다른 앱에서 핫키로 호출 → 검색 → 복사 → 자동 닫힘이 동작하는 상태

**의존성**: Phase 1 완료 필수

---

## 작업 목록

### 2.1 글로벌 핫키 구현

#### 2.1.1 NuGet 패키지 추가
- [ ] `H.Hooks` 패키지 설치

#### 2.1.2 HotkeyService 구현
- [ ] `IHotkeyService` 인터페이스 정의
- [ ] `HotkeyService` 구현
  - [ ] 핫키 등록 (기본: Ctrl + Alt + Space)
  - [ ] 핫키 해제
  - [ ] 핫키 이벤트 발생 시 콜백
  - [ ] 핫키 충돌 감지 및 처리

**생성 파일**:
```
Services/
├── IHotkeyService.cs
└── HotkeyService.cs
```

**인터페이스 명세**:
```csharp
public interface IHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;
    bool RegisterHotkey(ModifierKeys modifiers, Key key);
    void UnregisterHotkey();
    bool IsRegistered { get; }
}
```

**완료 기준**: 다른 앱에서 Ctrl+Alt+Space 누르면 이벤트 발생

---

### 2.2 창 표시/숨김 로직

#### 2.2.1 WindowHelper 구현
- [ ] 창 표시 위치 계산 (마우스 커서 근처 또는 화면 중앙)
- [ ] 활성 창 정보 저장 (복사 후 포커스 복원용)
- [ ] 창 표시/숨김 애니메이션 (선택적)

**생성 파일**:
```
Helpers/
└── WindowHelper.cs
```

#### 2.2.2 MainWindow 동작 수정
- [ ] 핫키로 창 표시 시 위치 설정
- [ ] 창 표시 시 검색창 포커스
- [ ] 검색창 텍스트 전체 선택
- [ ] ESC 키로 창 닫기
- [ ] 포커스 잃으면 창 숨기기 (Deactivated 이벤트)
- [ ] 창 숨김 시 검색어 초기화 (설정 가능)

**수정 파일**:
```
Views/
├── MainWindow.xaml
└── MainWindow.xaml.cs
```

**완료 기준**: 핫키 → 창 표시 → ESC → 창 숨김 동작

---

### 2.3 시스템 트레이 구현

#### 2.3.1 NuGet 패키지 추가
- [ ] `Hardcodet.NotifyIcon.Wpf` 패키지 설치

#### 2.3.2 트레이 아이콘 구현
- [ ] 트레이 아이콘 리소스 추가 (.ico 파일)
- [ ] App.xaml에 NotifyIcon 정의
- [ ] 컨텍스트 메뉴 구현
  - [ ] "열기" - 창 표시
  - [ ] "설정" - 설정 창 (Phase 3에서 구현)
  - [ ] 구분선
  - [ ] "종료" - 앱 종료
- [ ] 트레이 아이콘 더블클릭 → 창 표시
- [ ] 툴팁 텍스트 설정

**생성/수정 파일**:
```
App.xaml (수정)
App.xaml.cs (수정)
Resources/Icons/
└── app.ico
```

**컨텍스트 메뉴 구조**:
```
┌─────────────────┐
│ 유니코드 검색기  │ (비활성 텍스트)
├─────────────────┤
│ 열기 (Ctrl+Alt+Space)
│ 설정...
├─────────────────┤
│ 종료
└─────────────────┘
```

**완료 기준**: 트레이 아이콘 표시, 메뉴 동작

---

### 2.4 최근 사용 문자

#### 2.4.1 RecentCharactersService 구현
- [ ] `IRecentCharactersService` 인터페이스 정의
- [ ] `RecentCharactersService` 구현
  - [ ] 문자 추가 (최대 20개, 중복 시 맨 앞으로)
  - [ ] 목록 조회
  - [ ] 목록 초기화
  - [ ] 파일 저장/로드

**생성 파일**:
```
Services/
├── IRecentCharactersService.cs
└── RecentCharactersService.cs
```

**인터페이스 명세**:
```csharp
public interface IRecentCharactersService
{
    IReadOnlyList<string> RecentCharacters { get; }
    void AddCharacter(string character);
    void Clear();
    Task SaveAsync();
    Task LoadAsync();
}
```

#### 2.4.2 UI에 최근 사용 섹션 추가
- [ ] MainWindow에 최근 사용 문자 영역 추가
- [ ] 최근 사용 문자 클릭 시 복사
- [ ] 접기/펼치기 기능 (선택적)

**UI 레이아웃**:
```
┌─────────────────────────────────────┐
│ [🔍 검색어 입력...]                  │
├─────────────────────────────────────┤
│ ⏱ 최근: ★  ☆  →  ●  ✓  ♥          │  ← 추가
├─────────────────────────────────────┤
│ [전체] [⭐별] [◆도형] ...           │
├─────────────────────────────────────┤
│  문자 그리드...                      │
└─────────────────────────────────────┘
```

**완료 기준**: 문자 복사 시 최근 사용에 추가, 앱 재시작 시 유지

---

### 2.5 카테고리 탭 네비게이션

#### 2.5.1 카테고리 탭 UI
- [ ] 카테고리 탭 바 추가
- [ ] "전체" 탭 (모든 문자 표시)
- [ ] 각 카테고리 탭 (아이콘 + 이름)
- [ ] 탭 선택 시 해당 카테고리 문자만 표시
- [ ] 스크롤 가능한 탭 바 (카테고리 많을 경우)

#### 2.5.2 ViewModel 수정
- [ ] `SelectedCategory` 속성 추가
- [ ] 카테고리 필터링 로직 추가
- [ ] 검색 + 카테고리 필터 조합

**수정 파일**:
```
ViewModels/
└── MainViewModel.cs (수정)
Views/
├── MainWindow.xaml (수정)
└── MainWindow.xaml.cs (수정)
```

**완료 기준**: 카테고리 탭 클릭 시 필터링 동작

---

### 2.6 설정 저장/로드

#### 2.6.1 UserSettings 모델
- [ ] `UserSettings.cs` 모델 클래스 생성
- [ ] 핫키 설정 (modifiers, key)
- [ ] 동작 설정 (closeOnSelect, showNotification)
- [ ] 외관 설정 (fontSize, columnsCount)

**생성 파일**:
```
Models/
└── UserSettings.cs
```

#### 2.6.2 SettingsService 구현
- [ ] `ISettingsService` 인터페이스 정의
- [ ] `SettingsService` 구현
  - [ ] 설정 파일 경로 (%APPDATA%/UnicodeSearcher/settings.json)
  - [ ] 설정 로드 (없으면 기본값)
  - [ ] 설정 저장
  - [ ] 설정 변경 이벤트

**생성 파일**:
```
Services/
├── ISettingsService.cs
└── SettingsService.cs
```

**설정 파일 위치**:
```
%APPDATA%/UnicodeSearcher/
├── settings.json
└── recent.json
```

**완료 기준**: 앱 종료 후 재시작해도 설정 유지

---

### 2.7 복사 후 자동 닫힘

#### 2.7.1 동작 개선
- [ ] 문자 클릭 시 복사
- [ ] 더블클릭/Enter 시 복사 + 창 닫기
- [ ] 설정에 따른 자동 닫힘 제어
- [ ] 복사 완료 알림 (옵션)

#### 2.7.2 알림 토스트 (선택적)
- [ ] 복사 완료 시 작은 토스트 메시지
- [ ] 1초 후 자동 사라짐

**완료 기준**: 더블클릭으로 복사 + 자동 닫힘

---

### 2.8 키보드 중심 UX (핵심!)

> **원칙**: 마우스 없이 키보드만으로 모든 기능을 빠르게 사용할 수 있어야 함
> 이 섹션은 앱의 핵심 UX이므로 가장 신경 써서 구현해야 함

#### 2.8.1 KeyboardNavigationService 구현
- [ ] `IKeyboardNavigationService` 인터페이스 정의
- [ ] `KeyboardNavigationService` 구현
  - [ ] 현재 포커스 영역 추적 (Search, Recent, Category, Grid)
  - [ ] 영역 간 이동 로직
  - [ ] 그리드 내 2D 탐색 로직 (행/열 계산)

**생성 파일**:
```
Services/
├── IKeyboardNavigationService.cs
└── KeyboardNavigationService.cs
```

**인터페이스 명세**:
```csharp
public enum FocusArea { Search, Recent, Category, Grid }

public interface IKeyboardNavigationService
{
    FocusArea CurrentArea { get; }
    int SelectedIndex { get; }

    void MoveTo(FocusArea area);
    void MoveInGrid(Direction direction);  // Up, Down, Left, Right
    void MoveToFirst();
    void MoveToLast();
    void PageUp();
    void PageDown();

    event EventHandler<FocusArea> AreaChanged;
    event EventHandler<int> SelectionChanged;
}
```

#### 2.8.2 검색창 키보드 동작
- [ ] 앱 열릴 때 검색창 자동 포커스
- [ ] 기존 텍스트 전체 선택 상태
- [ ] `↓` 키: 그리드 첫 번째 문자로 이동
- [ ] `Enter` 키 (결과 있을 때): 첫 번째 결과 복사 + 닫기
- [ ] `Enter` 키 (결과 없을 때): 아무 동작 없음 (비프음 X)
- [ ] `Ctrl+A`: 전체 선택
- [ ] `ESC`: 창 닫기
- [ ] 검색 결과 첫 번째 문자 자동 선택 표시 (미리보기)

#### 2.8.3 최근 사용 영역 키보드 동작
- [ ] `1`-`9` 숫자키로 즉시 선택 및 복사
  - 검색창에 포커스 있어도 동작 (검색어 없을 때)
  - 숫자 입력 시 해당 문자 복사 + 창 닫기
- [ ] `Tab`: 카테고리 탭으로 이동
- [ ] `Shift+Tab`: 검색창으로 이동

#### 2.8.4 카테고리 탭 키보드 동작
- [ ] `←` `→` 화살표: 카테고리 전환
- [ ] `Enter`: 해당 카테고리 선택 + 그리드로 포커스
- [ ] `Tab`: 그리드로 이동
- [ ] `Shift+Tab`: 최근 사용으로 이동
- [ ] `Ctrl+←` `Ctrl+→`: 어디서든 카테고리 전환 (글로벌 단축키)

#### 2.8.5 문자 그리드 키보드 동작
- [ ] `↑` `↓` `←` `→`: 2D 탐색
  - 행 끝에서 오른쪽 → 다음 행 첫 번째
  - 행 시작에서 왼쪽 → 이전 행 마지막
  - 첫 행에서 위 → 검색창으로 이동
- [ ] `Enter`: 선택 문자 복사 + 창 닫기
- [ ] `Space`: 선택 문자 복사 (창 유지) - 여러 개 복사 시 유용
- [ ] `Ctrl+Enter`: 복사 + 이전 앱에 붙여넣기
- [ ] `Home`: 첫 번째 문자로 이동
- [ ] `End`: 마지막 문자로 이동
- [ ] `Page Up`: 한 페이지(화면) 위로
- [ ] `Page Down`: 한 페이지(화면) 아래로
- [ ] `Ctrl+1`-`9`: N번째 검색 결과 바로 선택
- [ ] `F` 또는 `Ctrl+F`: 즐겨찾기 토글 (Phase 3)
- [ ] 알파벳/한글 입력: 검색창으로 자동 포커스 + 입력 시작

#### 2.8.6 포커스 시각적 피드백
- [ ] **검색창**: 포커스 시 테두리 accent color (2px)
- [ ] **카테고리 탭**: 선택된 탭 배경색 + 밑줄
- [ ] **문자 아이템 (호버)**: 연한 배경색
- [ ] **문자 아이템 (포커스)**: 진한 테두리 (2px) + 배경색 + scale(1.1)
- [ ] **문자 아이템 (선택됨)**: 다른 색상 테두리 + 체크 배지

**스타일 예시**:
```xml
<!-- 포커스된 문자 아이템 -->
<Style x:Key="FocusedCharacterItem">
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="BorderBrush" Value="{DynamicResource AccentColor}"/>
    <Setter Property="Background" Value="{DynamicResource HoverColor}"/>
    <Setter Property="RenderTransform">
        <Setter.Value>
            <ScaleTransform ScaleX="1.1" ScaleY="1.1"/>
        </Setter.Value>
    </Setter>
</Style>
```

#### 2.8.7 글로벌 단축키 (어디서든 동작)
- [ ] `ESC`: 창 닫기
- [ ] `Ctrl+←` `Ctrl+→`: 카테고리 탭 이동
- [ ] `1`-`9`: 최근 사용 문자 (검색어 비었을 때)
- [ ] `Ctrl+1`-`9`: N번째 검색 결과

#### 2.8.8 PreviewKeyDown 이벤트 핸들러
- [ ] MainWindow에 PreviewKeyDown 핸들러 추가
- [ ] 키 입력을 중앙에서 처리
- [ ] 포커스 영역에 따른 분기 처리

**핵심 코드 구조**:
```csharp
private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
{
    // 글로벌 단축키 먼저 처리
    if (e.Key == Key.Escape)
    {
        Hide();
        e.Handled = true;
        return;
    }

    // Ctrl + 방향키: 카테고리 이동
    if (Keyboard.Modifiers == ModifierKeys.Control)
    {
        if (e.Key == Key.Left) { PreviousCategory(); e.Handled = true; }
        if (e.Key == Key.Right) { NextCategory(); e.Handled = true; }
    }

    // 숫자키: 최근 사용 (검색어 비었을 때)
    if (string.IsNullOrEmpty(SearchQuery) && e.Key >= Key.D1 && e.Key <= Key.D9)
    {
        int index = e.Key - Key.D1;
        SelectRecentCharacter(index);
        e.Handled = true;
        return;
    }

    // 포커스 영역별 처리
    switch (_navigationService.CurrentArea)
    {
        case FocusArea.Search:
            HandleSearchKeyDown(e);
            break;
        case FocusArea.Grid:
            HandleGridKeyDown(e);
            break;
        // ...
    }
}
```

**생성/수정 파일**:
```
Views/
├── MainWindow.xaml (수정 - 포커스 스타일)
└── MainWindow.xaml.cs (수정 - PreviewKeyDown)

ViewModels/
└── MainViewModel.cs (수정 - 네비게이션 커맨드)

Resources/
└── Styles.xaml (수정 - 포커스 스타일)
```

**완료 기준 체크리스트**:
- [ ] 앱 열기 → 타이핑 → Enter = 복사 + 닫기 (마우스 0회)
- [ ] 최근 사용에서 숫자키로 즉시 선택
- [ ] 화살표로 그리드 탐색 자연스러움
- [ ] Tab/Shift+Tab으로 영역 이동
- [ ] 모든 포커스 상태가 시각적으로 명확함
- [ ] 그리드에서 알파벳 입력 시 검색창 자동 포커스
- [ ] ESC로 언제든 닫기

---

## 구현할 파일 전체 목록

```
src/UnicodeSearcher/
├── Models/
│   └── UserSettings.cs (신규)
│
├── ViewModels/
│   └── MainViewModel.cs (수정)
│
├── Views/
│   ├── MainWindow.xaml (수정)
│   └── MainWindow.xaml.cs (수정)
│
├── Services/
│   ├── IHotkeyService.cs (신규)
│   ├── HotkeyService.cs (신규)
│   ├── IRecentCharactersService.cs (신규)
│   ├── RecentCharactersService.cs (신규)
│   ├── ISettingsService.cs (신규)
│   ├── SettingsService.cs (신규)
│   ├── IKeyboardNavigationService.cs (신규)
│   └── KeyboardNavigationService.cs (신규)
│
├── Helpers/
│   └── WindowHelper.cs (신규)
│
├── Resources/Icons/
│   └── app.ico (신규)
│
├── App.xaml (수정)
└── App.xaml.cs (수정)
```

---

## 완료 기준 체크리스트

### 핫키 기능
- [ ] 다른 앱 사용 중 Ctrl+Alt+Space → 창 표시
- [ ] 창이 마우스 커서 근처에 표시
- [ ] ESC 키로 창 닫힘
- [ ] 창 외부 클릭 시 창 닫힘
- [ ] 창 닫힌 후 이전 앱에 포커스 복원

### 시스템 트레이
- [ ] 시스템 트레이에 아이콘 표시
- [ ] 트레이 아이콘 더블클릭 → 창 표시
- [ ] 우클릭 메뉴 동작
- [ ] 종료 메뉴로 완전 종료

### 최근 사용
- [ ] 문자 복사 시 최근 사용에 추가
- [ ] 최근 사용 문자 클릭 시 복사
- [ ] 앱 재시작 시 최근 사용 유지 (최대 20개)

### 카테고리
- [ ] 카테고리 탭 표시
- [ ] 탭 클릭 시 해당 카테고리 문자만 표시
- [ ] 검색 + 카테고리 필터 조합 동작

### 설정
- [ ] 설정 파일 저장/로드
- [ ] 앱 재시작 시 설정 유지

### 키보드
- [ ] 화살표 키로 문자 선택 이동
- [ ] Enter로 복사 + 닫기
- [ ] 전체 흐름이 키보드만으로 가능

---

## 참고: 핵심 코드 스니펫

### HotkeyService (H.Hooks 사용)
```csharp
public class HotkeyService : IHotkeyService
{
    private readonly LowLevelKeyboardHook _hook;
    private ModifierKeys _modifiers;
    private Key _key;

    public event EventHandler? HotkeyPressed;

    public HotkeyService()
    {
        _hook = new LowLevelKeyboardHook();
        _hook.Down += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyboardEventArgs e)
    {
        var currentModifiers = Keyboard.Modifiers;
        var currentKey = KeyInterop.KeyFromVirtualKey(e.VirtualKeyCode);

        if (currentModifiers == _modifiers && currentKey == _key)
        {
            e.IsHandled = true;
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

### 창 표시 위치 계산
```csharp
public static class WindowHelper
{
    public static void PositionNearCursor(Window window)
    {
        var cursor = System.Windows.Forms.Cursor.Position;
        var screen = System.Windows.Forms.Screen.FromPoint(cursor);

        double left = cursor.X - window.Width / 2;
        double top = cursor.Y - window.Height / 2;

        // 화면 경계 체크
        left = Math.Max(screen.WorkingArea.Left,
               Math.Min(left, screen.WorkingArea.Right - window.Width));
        top = Math.Max(screen.WorkingArea.Top,
              Math.Min(top, screen.WorkingArea.Bottom - window.Height));

        window.Left = left;
        window.Top = top;
    }
}
```

### 시스템 트레이 XAML
```xml
<tb:TaskbarIcon x:Key="TrayIcon"
                IconSource="/Resources/Icons/app.ico"
                ToolTipText="유니코드 검색기 (Ctrl+Alt+Space)"
                DoubleClickCommand="{Binding ShowWindowCommand}">
    <tb:TaskbarIcon.ContextMenu>
        <ContextMenu>
            <MenuItem Header="열기" Command="{Binding ShowWindowCommand}"/>
            <MenuItem Header="설정..." Command="{Binding OpenSettingsCommand}"/>
            <Separator/>
            <MenuItem Header="종료" Command="{Binding ExitCommand}"/>
        </ContextMenu>
    </tb:TaskbarIcon.ContextMenu>
</tb:TaskbarIcon>
```

---

## 다음 단계

Phase 2 완료 후 → [Phase 3: 완성도](./PHASE3-POLISH.md)
