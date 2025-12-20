# 유니코드 특수문자 검색기 (Unicode Character Finder)

## 개요

Windows 기본 `Win + .` 이모지 창의 특수문자 검색 기능이 부실한 문제를 해결하기 위한 데스크톱 앱.
한글 자음 + 한자키 조합의 특수문자표를 대체하면서, 더 강력한 검색 기능을 제공한다.

---

## 현재 진행 상태

| Phase | 상태 | 완료일 |
|-------|------|--------|
| Phase 1: MVP | ✅ 완료 | 2024-12-19 |
| Phase 2: 핵심 기능 | ✅ 완료 | 2024-12-20 |
| Phase 3: 완성도 (v1.0) | ✅ 완료 | 2024-12-20 |
| Phase 4: UX 개선 (v1.1) | ✅ 완료 | 2024-12-20 |
| Phase 5: UI 개선 (v1.2) | ✅ 완료 | 2024-12-21 |

### Phase 5 완료 항목 (v1.2)

| 기능 | 설명 |
|------|------|
| Material Design 다크 테마 | 세련된 색상 팔레트, 섹션별 포커스 하이라이트 |
| 설정 창 다크 테마 | 모든 컨트롤 커스텀 템플릿 적용 |
| Windows 11 네이티브 둥근 모서리 | DWM API 활용 |
| 컴팩트 UI | 상단 영역 최적화, 최근+즐겨찾기 통합, 검색 결과 영역 최대화 |
| 검색 아이콘 Path 변환 | WPF 이모지 렌더링 이슈 해결 (테마 호환) |
| 설정 접근성 개선 | 타이틀바 설정 버튼 (⚙️), Ctrl+, 단축키 |
| 키보드 네비게이션 개선 | 최근↔즐겨찾기 좌우 이동, 위/아래로 검색/카테고리 이동 |
| 앱 아이콘 추가 | ★+🔍 컨셉, 다중 해상도 ICO (16~256px) |
| 가상화 적용 | VirtualizingWrapPanel로 1300개 문자 렌더링 최적화 |
| 핫키 포커스 수정 | SetForegroundWindow API로 창 활성화 문제 해결 |

### 최근 버그 수정 (2024-12-20)

| 문제 | 원인 | 해결 방법 |
|------|------|-----------|
| 검색 시 앱 크래시 (STA 스레드 오류) | `Keyboard.IsKeyDown()` 이 H.Hooks 백그라운드 스레드에서 호출됨 | Win32 `GetAsyncKeyState` API로 교체 (스레드 안전) |
| 싱글파일 앱에서 경로 오류 (IL3000 경고) | `Assembly.Location`이 싱글파일 앱에서 빈 문자열 반환 | `AppContext.BaseDirectory` 사용 |
| JSON 파싱 오류 | characters.json 인코딩 손상 | UTF-8 (BOM 없음)으로 재생성 |

### 추가된 테스트 (76개)

- **HotkeyServiceTests** (10개): 핫키 등록/해제, Start/Stop, Dispose 안전성
- **CharacterDataServiceTests** (16개): JSON 로딩, UTF-8 인코딩, 카테고리 필터링
- **StartupServiceTests** (8개): Windows 자동 시작 레지스트리 관리
- **SearchServiceTests** (11개): 검색 로직
- **FavoriteServiceTests**: 즐겨찾기 기능
- **RecentCharactersServiceTests**: 최근 사용 문자
- **UserSettingsTests**: 설정 모델

## 배경 (왜 만드는가?)

### 현재 문제점
- `Win + .` / `Win + ;` 특수문자 검색이 형편없음 (예: "별" 검색해도 ☆★ 안 나옴)
- 한자키 없는 키보드에서 특수문자 입력이 불편함
- 기존 문자표(charmap)는 검색이 안 되고 UX가 구림
- 자주 쓰는 특수문자를 빠르게 찾기 어려움

### 해결 목표
- 한글/영어 자연어로 특수문자 검색 가능
- 글로벌 핫키로 어디서든 빠르게 호출
- 최근 사용/즐겨찾기로 자주 쓰는 문자 빠른 접근
- 가볍고 빠른 응답 속도

---

## 핵심 기능 상세

### 1. 글로벌 핫키로 빠른 호출
- **기본 단축키**: `Ctrl + Alt + Space` (사용자 변경 가능)
- **호출 방식**:
  - 현재 활성 창 위에 오버레이로 표시
  - 마우스 커서 근처 또는 화면 중앙에 팝업
- **닫기 방식**:
  - ESC 키
  - 포커스 잃으면 자동 닫힘
  - 문자 선택 후 자동 닫힘 (설정 가능)

### 2. 자연어 검색
- **한글 검색**: "별", "화살표", "동그라미", "체크", "하트" 등
- **영어 검색**: "star", "arrow", "circle", "check", "heart" 등
- **유니코드 이름 검색**: "BLACK STAR", "RIGHTWARDS ARROW" 등
- **코드포인트 검색**: "U+2605", "2605", "0x2605" 등
- **실시간 검색**: 타이핑하면서 즉시 결과 표시
- **퍼지 매칭**: 오타 허용, 부분 일치 지원

### 3. 카테고리 브라우징
| 카테고리 | 포함 문자 예시 |
|---------|---------------|
| 별/스타 | ★ ☆ ✦ ✧ ⭐ 🌟 ✡ |
| 도형 | ● ○ ◆ ◇ ■ □ ▲ △ |
| 화살표 | → ← ↑ ↓ ⇒ ⇐ ➔ ➜ |
| 체크/선택 | ✓ ✔ ☑ ✗ ✘ ☐ |
| 하트 | ♥ ♡ ❤ 💕 💗 |
| 음악 | ♪ ♫ ♬ 🎵 🎶 |
| 수학 | ± × ÷ ≠ ≤ ≥ ∞ √ |
| 화폐 | ₩ $ € ¥ £ ₿ |
| 괄호/인용 | 「」『』【】〈〉《》 |
| 한자키 특수문자 | ㉠㉡㉢ ㈀㈁㈂ ⅰⅱⅲ |

### 4. 문자 선택 동작
- **클릭 1회**: 클립보드에 복사 (토스트 알림)
- **더블클릭 / Enter**: 복사 + 창 닫기
- **Ctrl + Enter**: 복사 + 이전 활성 창에 붙여넣기 (직접 입력)
- **여러 문자 선택**: Shift + 클릭으로 여러 개 선택 후 일괄 복사

### 5. 부가 기능
- **최근 사용**: 마지막 사용한 20개 문자 저장
- **즐겨찾기**: 우클릭으로 즐겨찾기 등록/해제
- **문자 정보**: 선택 시 유니코드 이름, 코드포인트, 카테고리 표시
- **시스템 트레이 상주**: 백그라운드 실행, 트레이 아이콘 메뉴
- **자동 시작**: Windows 시작 시 자동 실행 옵션

### 6. 키보드 중심 UX (핵심!)

> **원칙**: 마우스 없이 키보드만으로 모든 기능을 빠르게 사용할 수 있어야 함

#### 전체 흐름
```
[다른 앱에서 작업 중]
       │
       ▼ Ctrl+Alt+Space
┌─────────────────────────────────┐
│ 검색창 (자동 포커스)             │ ← 바로 타이핑 시작
│         │                       │
│         ▼ 타이핑                │
│ 실시간 검색 결과                 │ ← 첫 번째 결과 자동 선택
│         │                       │
│         ▼ Enter                 │
│ 복사 + 창 닫기                   │
│         │                       │
└─────────────────────────────────┘
       │
       ▼ 이전 앱으로 자동 복귀
[Ctrl+V로 붙여넣기]
```

#### 단축키 전체 목록
| 단축키 | 동작 | 컨텍스트 |
|--------|------|----------|
| `Ctrl+Alt+Space` | 앱 호출/토글 | 글로벌 |
| `ESC` | 창 닫기 | 전체 |
| `Enter` | 선택 문자 복사 + 창 닫기 | 전체 |
| `Ctrl+Enter` | 복사 + 이전 앱에 붙여넣기 | 전체 |
| `Tab` | 다음 영역으로 이동 | 전체 |
| `Shift+Tab` | 이전 영역으로 이동 | 전체 |
| `↑` `↓` `←` `→` | 문자 선택 이동 | 그리드 |
| `↓` | 검색창 → 그리드 첫 문자 | 검색창 |
| `1`-`9` | 최근 사용 N번째 문자 선택 | 전체 |
| `Ctrl+1`-`9` | N번째 검색 결과 선택 | 검색 중 |
| `Home` | 첫 번째 문자로 이동 | 그리드 |
| `End` | 마지막 문자로 이동 | 그리드 |
| `Page Up/Down` | 한 페이지씩 이동 | 그리드 |
| `Ctrl+A` | 검색어 전체 선택 | 검색창 |
| `Ctrl+←` `Ctrl+→` | 카테고리 탭 이동 | 전체 |
| `Ctrl+,` | 설정 창 열기 | 전체 |
| `Space` | 선택 문자 복사 (창 유지) | 그리드 |
| `F` 또는 `Ctrl+F` | 즐겨찾기 토글 | 그리드 |

#### 영역별 키보드 동작

**검색창 (시작 포커스)**
- 앱 열리면 자동 포커스, 기존 텍스트 전체 선택
- 타이핑 즉시 검색 시작
- `↓` 또는 `Enter` (결과 있을 때): 그리드로 포커스 이동
- 첫 번째 검색 결과가 자동 선택됨

**최근 사용 영역**
- `1`-`9` 숫자키로 즉시 선택
- `Tab`으로 카테고리 탭 이동

**카테고리 탭**
- `←` `→`로 카테고리 전환
- `Enter`로 해당 카테고리 선택 및 그리드로 이동
- `Ctrl+←` `Ctrl+→`는 어디서든 카테고리 전환

**문자 그리드**
- 화살표로 2D 탐색 (행/열 기반)
- `Enter`: 복사 + 닫기
- `Space`: 복사 (창 유지, 여러 개 복사 시)
- 알파벳 입력 시 검색창으로 자동 포커스 이동

#### 포커스 시각적 피드백
- **검색창**: 테두리 강조 (accent color)
- **카테고리 탭**: 밑줄 또는 배경색
- **문자 아이템**: 두꺼운 테두리 + 배경색 + 약간 확대
- **선택된 문자**: 체크마크 또는 다른 색상 테두리

#### 빠른 사용 시나리오

**시나리오 1: 별 기호 입력 (최단 경로)**
```
Ctrl+Alt+Space → "별" 입력 → Enter
(3단계, 약 2초)
```

**시나리오 2: 최근 사용 문자**
```
Ctrl+Alt+Space → 1 (첫 번째 최근 문자)
(2단계, 약 1초)
```

**시나리오 3: 카테고리 탐색**
```
Ctrl+Alt+Space → Ctrl+→ (화살표 카테고리) → ↓↓→ → Enter
(여러 단계지만 마우스 불필요)
```

---

## 기술 스택

### 프레임워크 & 언어
- **프레임워크**: WPF (.NET 8)
- **언어**: C# 12
- **UI 패턴**: MVVM (CommunityToolkit.Mvvm 사용)

### 필수 NuGet 패키지
| 패키지 | 용도 | 버전 |
|--------|------|------|
| `CommunityToolkit.Mvvm` | MVVM 패턴 지원 | 8.x |
| `H.Hooks` | 글로벌 핫키 | 1.x |
| `Hardcodet.NotifyIcon.Wpf` | 시스템 트레이 | 1.x |
| `System.Text.Json` | JSON 처리 (기본 포함) | - |

### 선택적 패키지
| 패키지 | 용도 |
|--------|------|
| `Microsoft.Extensions.DependencyInjection` | DI 컨테이너 |
| `Serilog` | 로깅 |

---

## 아키텍처 설계

### 프로젝트 구조
```
UnicodeSearcher/
├── UnicodeSearcher.sln
├── src/
│   └── UnicodeSearcher/
│       ├── App.xaml                    # 앱 진입점
│       ├── App.xaml.cs
│       ├── AssemblyInfo.cs
│       │
│       ├── Models/                     # 데이터 모델
│       │   ├── UnicodeCharacter.cs     # 문자 정보 모델
│       │   ├── Category.cs             # 카테고리 모델
│       │   └── UserSettings.cs         # 사용자 설정 모델
│       │
│       ├── ViewModels/                 # MVVM ViewModels
│       │   ├── MainViewModel.cs        # 메인 창 VM
│       │   ├── SearchViewModel.cs      # 검색 로직 VM
│       │   └── SettingsViewModel.cs    # 설정 창 VM
│       │
│       ├── Views/                      # XAML Views
│       │   ├── MainWindow.xaml         # 메인 검색 창
│       │   ├── MainWindow.xaml.cs
│       │   ├── SettingsWindow.xaml     # 설정 창
│       │   └── SettingsWindow.xaml.cs
│       │
│       ├── Services/                   # 비즈니스 로직
│       │   ├── IHotkeyService.cs       # 핫키 인터페이스
│       │   ├── HotkeyService.cs        # 글로벌 핫키 관리
│       │   ├── IClipboardService.cs    # 클립보드 인터페이스
│       │   ├── ClipboardService.cs     # 클립보드 처리
│       │   ├── ISearchService.cs       # 검색 인터페이스
│       │   ├── SearchService.cs        # 검색 로직
│       │   ├── ICharacterDataService.cs
│       │   ├── CharacterDataService.cs # 문자 데이터 로드
│       │   ├── ISettingsService.cs
│       │   └── SettingsService.cs      # 설정 저장/로드
│       │
│       ├── Controls/                   # 커스텀 컨트롤
│       │   ├── CharacterGrid.xaml      # 문자 그리드
│       │   ├── CharacterGrid.xaml.cs
│       │   ├── CharacterItem.xaml      # 개별 문자 표시
│       │   └── CharacterItem.xaml.cs
│       │
│       ├── Converters/                 # 값 변환기
│       │   └── BoolToVisibilityConverter.cs
│       │
│       ├── Data/                       # 정적 데이터
│       │   └── characters.json         # 문자 데이터베이스
│       │
│       ├── Resources/                  # 리소스
│       │   ├── Styles.xaml             # 공통 스타일
│       │   ├── Icons/                  # 아이콘 파일
│       │   └── Themes/                 # 테마 (다크/라이트)
│       │
│       └── Helpers/                    # 유틸리티
│           ├── WindowHelper.cs         # 창 위치/크기
│           └── StringHelper.cs         # 문자열 처리
│
├── tests/
│   └── UnicodeSearcher.Tests/
│       ├── Services/
│       │   └── SearchServiceTests.cs
│       └── ViewModels/
│           └── MainViewModelTests.cs
│
└── docs/
    ├── PHASE1-MVP.md
    ├── PHASE2-CORE.md
    └── PHASE3-POLISH.md
```

### MVVM 아키텍처
```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│    View     │────▶│  ViewModel   │────▶│   Model     │
│  (XAML)     │◀────│  (C#)        │◀────│   (C#)      │
└─────────────┘     └──────────────┘     └─────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │  Services   │
                    │ (Interface) │
                    └─────────────┘
```

---

## 데이터 구조

### characters.json 스키마
```json
{
  "version": "1.0",
  "lastUpdated": "2024-01-01",
  "characters": [
    {
      "char": "★",
      "codepoint": "U+2605",
      "name": "BLACK STAR",
      "block": "Miscellaneous Symbols",
      "tags_ko": ["별", "검은별", "꽉찬별", "채운별"],
      "tags_en": ["star", "black star", "filled star", "solid star"],
      "category": "star",
      "subcategory": "solid",
      "keywords": ["rating", "favorite", "decoration"],
      "frequency": 95
    }
  ],
  "categories": [
    {
      "id": "star",
      "name_ko": "별",
      "name_en": "Stars",
      "icon": "⭐",
      "order": 1
    }
  ]
}
```

### UserSettings 구조
```json
{
  "hotkey": {
    "modifiers": ["Ctrl", "Alt"],
    "key": "Space"
  },
  "behavior": {
    "closeOnSelect": true,
    "pasteAfterCopy": false,
    "showToastNotification": true
  },
  "appearance": {
    "theme": "system",
    "fontSize": 24,
    "columnsCount": 10
  },
  "data": {
    "recentCharacters": ["★", "→", "✓"],
    "favoriteCharacters": ["★", "☆", "●"],
    "maxRecentCount": 20
  },
  "startup": {
    "runAtStartup": false,
    "startMinimized": true
  }
}
```

---

## UI 설계

### 메인 창 레이아웃
```
┌─────────────────────────────────────────────────────┐
│ 🔍 [검색어 입력...                           ] [×] │  ← 검색바 + 닫기
├─────────────────────────────────────────────────────┤
│ ⏱ 최근: ★  ☆  →  ●  ✓  ♥  ♪  ✔              │  ← 최근 사용 (접기 가능)
├─────────────────────────────────────────────────────┤
│ [전체] [⭐별] [◆도형] [→화살표] [✓체크] [더보기▼]│  ← 카테고리 탭
├─────────────────────────────────────────────────────┤
│                                                     │
│  ★  ☆  ✦  ✧  ✩  ✪  ✫  ✬  ✭  ✮                │
│  ✯  ⭐ 🌟 ⋆  ✡  ✴  ✵  ✶  ✷  ✸                │  ← 문자 그리드
│  ✹  ✺  ⁂  ⁎  ⁑  ⁕  ☪  ✨  💫 🔯                │     (스크롤 가능)
│                                                     │
├─────────────────────────────────────────────────────┤
│ ★ BLACK STAR                              [복사]   │  ← 선택 문자 정보
│ U+2605 · Miscellaneous Symbols · 별              │
└─────────────────────────────────────────────────────┘
```

### 테마
- **라이트 모드**: 밝은 배경, 어두운 문자
- **다크 모드**: 어두운 배경, 밝은 문자
- **시스템 연동**: Windows 테마 자동 감지

### 반응형 크기
- 기본: 500 x 400 px
- 최소: 400 x 300 px
- 최대: 800 x 600 px
- 크기 조절 가능, 마지막 크기 기억

---

## 개발 단계 개요

### Phase 1: MVP (최소 기능 제품)
> 기본적인 검색과 복사 기능이 동작하는 상태

- WPF 프로젝트 셋업
- 기본 UI (검색창 + 문자 그리드)
- 문자 데이터 JSON 로드
- 한글/영어 검색 구현
- 클립보드 복사 기능

**완료 기준**: 앱 실행 → 검색 → 문자 클릭 → 클립보드 복사 동작

### Phase 2: 핵심 기능
> 실제로 일상에서 쓸 수 있는 수준

- 글로벌 핫키 등록 및 창 호출
- 시스템 트레이 상주
- 최근 사용 문자 저장/표시
- 카테고리 탭 네비게이션
- 설정 저장/로드

**완료 기준**: 다른 앱에서 핫키로 호출 → 검색 → 복사 → 자동 닫힘

### Phase 3: 완성도
> 배포 가능한 완성된 앱

- 즐겨찾기 기능
- 설정 UI (핫키 변경, 테마 등)
- Windows 자동 시작 옵션
- 다크/라이트 테마
- 더 많은 문자 데이터 추가
- 성능 최적화

**완료 기준**: 일반 사용자가 설치하고 사용할 수 있는 수준

---

## 참고 자료

### 유니코드 데이터
- 공식 유니코드 데이터: https://unicode.org/Public/UNIDATA/
- UnicodeData.txt: 모든 문자 정보
- Blocks.txt: 블록 정보

### 한자키 특수문자
- ㄱ~ㅎ 한자키 조합 문자 목록 수집 필요
- 자주 쓰이는 특수문자 우선 포함

### 비슷한 앱 참고
- Windows 문자표 (charmap.exe)
- Mac 문자 뷰어
- WinCompose

---

## 시작하기

```bash
# .NET 8 SDK 필요
dotnet new sln -n UnicodeSearcher
mkdir src\UnicodeSearcher
cd src\UnicodeSearcher
dotnet new wpf
cd ..\..
dotnet sln add src\UnicodeSearcher\UnicodeSearcher.csproj
dotnet run --project src\UnicodeSearcher
```

---

## Future Plans (미래 계획)

### 이모지 검색 기능 추가

#### 배경
- 현재 앱은 특수문자/기호 위주로 구성
- 이모지도 유니코드의 일부 (U+1F300~ 범위)
- 사용자 요구에 따라 이모지 검색 기능 추가 고려

#### WPF 컬러 이모지 렌더링 이슈
WPF는 **네이티브로 컬러 이모지를 지원하지 않음** (흑백 렌더링)

| 프레임워크 | 컬러 이모지 | 비고 |
|-----------|------------|------|
| WPF | ❌ | DirectWrite 레벨 미지원 |
| UWP | ✅ | `IsColorFontEnabled` 속성 |
| WinUI 3 | ✅ | UWP 렌더링 엔진 계승 |

**관련 이슈**: [dotnet/wpf#91](https://github.com/dotnet/wpf/issues/91)

#### 해결 방안: Emoji.Wpf 라이브러리

```bash
dotnet add package Emoji.Wpf
```

**특징**:
- Segoe UI Emoji 폰트의 벡터 + COLR/CPAL 컬러 정보 파싱
- `TextBlock` → `EmojiTextBlock` 드롭인 교체
- 시스템 폰트 사용 (별도 폰트 임베드 불필요)
- .NET Framework 4.0+ 지원

**참고 라이브러리**:
- [samhocevar/emoji.wpf](https://github.com/samhocevar/emoji.wpf)
- [iNKORE-NET/UI.WPF.Emojis](https://github.com/iNKORE-NET/UI.WPF.Emojis)

#### 예상 작업량
- [ ] Emoji.Wpf NuGet 패키지 추가
- [ ] 이모지 데이터 수집 (약 3,600개)
- [ ] characters.json에 이모지 카테고리 추가
- [ ] 그리드 DataTemplate에서 EmojiTextBlock 적용
- [ ] 이모지 전용 카테고리 UI 구성

#### 성능 고려사항
- Windows 이모지 피커(Win+;)도 수천 개 이모지 + GIF 처리
- 현대 PC에서 이모지 수천 개 렌더링은 문제 없음
- 필요시 가상화 리스트 적용으로 최적화 가능

### 배포 및 릴리즈

#### 현재 방식
- **zip 배포**: single-file self-contained exe를 zip으로 묶어 GitHub Releases에 업로드
- **git tag**: `v1.0.0` 형식의 semantic versioning

#### 향후 개선 계획
- [ ] **GitHub Actions 자동화**: tag push 시 자동 빌드 → Release 생성 → zip 업로드
- [ ] **MSIX 패키징**: Windows 10+ 네이티브 설치 경험
  - 자체 서명 (self-signed certificate)으로 사이드로드 배포
  - 자동 업데이트 지원
  - 깔끔한 설치/제거 (샌드박스)
- [ ] **Microsoft Store 배포** (선택): 개발자 계정 필요 ($19 일회성)

#### 배포 방식 비교
| 방식 | 장점 | 단점 |
|------|------|------|
| zip + exe | 심플, 설치 불필요 | 자동 업데이트 없음 |
| MSIX 사이드로드 | 자동 업데이트, 깔끔한 설치 | 설정 복잡 |
| Microsoft Store | 검색 노출, 신뢰성 | 개발자 계정 비용 |

### 기타 미래 기능
- [ ] 클라우드 동기화 (즐겨찾기, 설정)
- [ ] 플러그인 시스템 (사용자 정의 문자셋)
- [ ] 다국어 검색 확장 (일본어, 중국어 태그)
