# Phase 1: MVP (최소 기능 제품)

> **목표**: 앱 실행 → 검색 → 문자 클릭 → 클립보드 복사가 동작하는 상태

---

## 작업 목록

### 1.1 프로젝트 초기 설정
- [ ] Solution 및 WPF 프로젝트 생성
- [ ] 폴더 구조 생성 (Models, Views, ViewModels, Services, Data)
- [ ] 필수 NuGet 패키지 설치
  - [ ] `CommunityToolkit.Mvvm` (MVVM 지원)
- [ ] .editorconfig 설정 (코딩 스타일)
- [ ] .gitignore 설정

**생성 파일**:
```
src/UnicodeSearcher/
├── UnicodeSearcher.csproj
├── App.xaml
├── App.xaml.cs
├── Models/
├── Views/
├── ViewModels/
├── Services/
└── Data/
```

**완료 기준**: `dotnet build` 성공, 빈 창 실행 가능

---

### 1.2 데이터 모델 정의
- [ ] `UnicodeCharacter.cs` 모델 클래스 생성
  - 속성: Char, Codepoint, Name, TagsKo, TagsEn, Category
- [ ] `Category.cs` 모델 클래스 생성
  - 속성: Id, NameKo, NameEn, Icon, Order
- [ ] JSON 직렬화/역직렬화 설정

**생성 파일**:
```
Models/
├── UnicodeCharacter.cs
└── Category.cs
```

**완료 기준**: 모델 클래스 컴파일 성공

---

### 1.3 문자 데이터 준비
- [ ] `characters.json` 초기 데이터 생성
- [ ] 기본 카테고리 정의 (별, 도형, 화살표, 체크, 하트)
- [ ] 카테고리별 대표 문자 20~30개씩 추가
- [ ] 한글/영어 태그 추가

**생성 파일**:
```
Data/
└── characters.json
```

**포함할 문자 (최소)**:
| 카테고리 | 문자 예시 | 수량 |
|---------|----------|------|
| 별 | ★ ☆ ✦ ✧ ⭐ 🌟 | 15+ |
| 도형 | ● ○ ◆ ◇ ■ □ ▲ △ | 20+ |
| 화살표 | → ← ↑ ↓ ⇒ ⇐ ➔ | 25+ |
| 체크 | ✓ ✔ ☑ ✗ ✘ ☐ | 10+ |
| 하트 | ♥ ♡ ❤ 💕 💗 | 10+ |

**완료 기준**: JSON 파일 유효, 총 80개 이상 문자 포함

---

### 1.4 서비스 레이어 구현

#### 1.4.1 CharacterDataService
- [ ] `ICharacterDataService` 인터페이스 정의
- [ ] `CharacterDataService` 구현
  - [ ] JSON 파일 로드 메서드
  - [ ] 전체 문자 목록 반환
  - [ ] 카테고리 목록 반환

**생성 파일**:
```
Services/
├── ICharacterDataService.cs
└── CharacterDataService.cs
```

**인터페이스 명세**:
```csharp
public interface ICharacterDataService
{
    Task<IReadOnlyList<UnicodeCharacter>> GetAllCharactersAsync();
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    Task LoadDataAsync();
}
```

#### 1.4.2 SearchService
- [ ] `ISearchService` 인터페이스 정의
- [ ] `SearchService` 구현
  - [ ] 한글 태그 검색
  - [ ] 영어 태그 검색
  - [ ] 유니코드 이름 검색
  - [ ] 코드포인트 검색 (U+XXXX)
  - [ ] 부분 일치 검색

**생성 파일**:
```
Services/
├── ISearchService.cs
└── SearchService.cs
```

**인터페이스 명세**:
```csharp
public interface ISearchService
{
    IReadOnlyList<UnicodeCharacter> Search(string query, IEnumerable<UnicodeCharacter> characters);
}
```

#### 1.4.3 ClipboardService
- [ ] `IClipboardService` 인터페이스 정의
- [ ] `ClipboardService` 구현
  - [ ] 문자 클립보드 복사

**생성 파일**:
```
Services/
├── IClipboardService.cs
└── ClipboardService.cs
```

**완료 기준**: 각 서비스 단위 테스트 통과

---

### 1.5 ViewModel 구현

#### MainViewModel
- [ ] `MainViewModel` 클래스 생성
- [ ] ObservableProperty 속성 정의
  - [ ] `SearchQuery` (검색어)
  - [ ] `Characters` (표시할 문자 목록)
  - [ ] `SelectedCharacter` (선택된 문자)
  - [ ] `IsLoading` (로딩 상태)
- [ ] RelayCommand 정의
  - [ ] `CopyCommand` (복사)
  - [ ] `SearchCommand` (검색 실행)
- [ ] 검색어 변경 시 자동 검색 (debounce)

**생성 파일**:
```
ViewModels/
└── MainViewModel.cs
```

**완료 기준**: ViewModel 바인딩 동작 확인

---

### 1.6 메인 UI 구현

#### MainWindow.xaml
- [ ] 검색 입력창 (TextBox)
  - [ ] 포커스 시 전체 선택
  - [ ] 플레이스홀더 텍스트
- [ ] 문자 그리드 (ItemsControl + WrapPanel)
  - [ ] 가상화 적용 (VirtualizingStackPanel)
  - [ ] 문자 아이템 템플릿
- [ ] 선택된 문자 정보 패널
  - [ ] 문자 (큰 글씨)
  - [ ] 유니코드 이름
  - [ ] 코드포인트
- [ ] 복사 버튼

**생성 파일**:
```
Views/
├── MainWindow.xaml
└── MainWindow.xaml.cs
```

**UI 레이아웃**:
```
┌──────────────────────────────────┐
│ [🔍 검색어 입력...]              │
├──────────────────────────────────┤
│                                  │
│  ★  ☆  ✦  ✧  ✩  ✪  ✫  ✬     │
│  ✭  ✮  ✯  ⭐ 🌟 ⋆  ✡  ✴     │
│  ...                             │
│                                  │
├──────────────────────────────────┤
│ ★ BLACK STAR (U+2605)    [복사] │
└──────────────────────────────────┘
```

**완료 기준**: UI 렌더링 정상, 스크롤 동작

---

### 1.7 기능 연결 및 통합
- [ ] App.xaml.cs에서 서비스 초기화
- [ ] MainWindow와 MainViewModel 연결
- [ ] 데이터 바인딩 동작 확인
- [ ] 검색 → 결과 표시 동작 확인
- [ ] 문자 클릭 → 클립보드 복사 동작 확인

**완료 기준**: 전체 흐름 동작

---

### 1.8 기본 스타일 적용
- [ ] `Styles.xaml` 리소스 딕셔너리 생성
- [ ] 기본 색상 정의 (배경, 전경, 강조)
- [ ] 문자 아이템 hover 효과
- [ ] 버튼 스타일

---

### 1.9 키보드 기본 동작 (MVP 필수!)

> Phase 2에서 전체 키보드 네비게이션을 구현하지만, MVP에서도 기본적인 키보드 사용이 가능해야 함

#### 1.9.1 검색창 키보드
- [ ] 앱 시작 시 검색창 자동 포커스
- [ ] 검색어 입력 → 실시간 필터링
- [ ] `Enter` 키: 첫 번째 결과 복사 (결과 있을 때)
- [ ] `ESC` 키: 창 닫기 (또는 앱 종료)

#### 1.9.2 문자 그리드 기본 키보드
- [ ] `Tab` 키: 검색창 → 그리드 포커스 이동
- [ ] `↑` `↓` `←` `→`: 문자 선택 이동
- [ ] `Enter` 키: 선택된 문자 복사
- [ ] 그리드에서 문자 입력 시 검색창으로 자동 포커스

#### 1.9.3 포커스 표시
- [ ] 검색창 포커스 시 테두리 강조
- [ ] 그리드 아이템 포커스 시 시각적 표시 (테두리 또는 배경색)

**핵심 코드**:
```csharp
// MainWindow.xaml.cs
private void Window_Loaded(object sender, RoutedEventArgs e)
{
    SearchTextBox.Focus();
    SearchTextBox.SelectAll();
}

private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    {
        Close(); // 또는 Hide()
        e.Handled = true;
    }

    if (e.Key == Key.Enter && ViewModel.FilteredCharacters.Any())
    {
        ViewModel.CopyFirstResult();
        e.Handled = true;
    }
}
```

**완료 기준**:
- [ ] 앱 열면 바로 타이핑 가능
- [ ] 검색 → Enter → 복사 (마우스 없이)
- [ ] ESC로 창 닫기
- [ ] 방향키로 그리드 탐색 가능

**생성 파일**:
```
Resources/
└── Styles.xaml
```

---

## 구현할 파일 전체 목록

```
src/UnicodeSearcher/
├── UnicodeSearcher.csproj
├── App.xaml
├── App.xaml.cs
│
├── Models/
│   ├── UnicodeCharacter.cs
│   └── Category.cs
│
├── ViewModels/
│   └── MainViewModel.cs
│
├── Views/
│   ├── MainWindow.xaml
│   └── MainWindow.xaml.cs
│
├── Services/
│   ├── ICharacterDataService.cs
│   ├── CharacterDataService.cs
│   ├── ISearchService.cs
│   ├── SearchService.cs
│   ├── IClipboardService.cs
│   └── ClipboardService.cs
│
├── Data/
│   └── characters.json
│
└── Resources/
    └── Styles.xaml
```

---

## 완료 기준 체크리스트

### 기능 테스트
- [ ] 앱 실행 시 문자 그리드에 모든 문자 표시
- [ ] 검색어 입력 시 실시간 필터링
- [ ] 한글 검색 동작 (예: "별" → ★☆ 표시)
- [ ] 영어 검색 동작 (예: "star" → ★☆ 표시)
- [ ] 코드포인트 검색 동작 (예: "2605" → ★ 표시)
- [ ] 문자 클릭 시 선택 상태 표시
- [ ] 선택된 문자 정보 패널에 정보 표시
- [ ] 복사 버튼 클릭 시 클립보드에 복사
- [ ] 문자 더블클릭 시 복사

### 키보드 테스트 (핵심!)
- [ ] 앱 열면 검색창에 자동 포커스
- [ ] 바로 타이핑 → 검색 동작
- [ ] Enter 키 → 첫 번째 결과 복사
- [ ] ESC 키 → 창 닫기
- [ ] Tab → 그리드로 포커스 이동
- [ ] 화살표 키 → 그리드 탐색
- [ ] 그리드에서 Enter → 선택 문자 복사
- [ ] 그리드에서 문자 입력 → 검색창으로 포커스

### 성능 테스트
- [ ] 앱 시작 시간 < 2초
- [ ] 검색 응답 시간 < 100ms
- [ ] 스크롤 부드러움 (60fps)

### 코드 품질
- [ ] 빌드 경고 0개
- [ ] 모든 public 멤버 XML 문서화

---

## 참고: 핵심 코드 스니펫

### UnicodeCharacter.cs
```csharp
public record UnicodeCharacter
{
    public required string Char { get; init; }
    public required string Codepoint { get; init; }
    public required string Name { get; init; }
    public string[] TagsKo { get; init; } = [];
    public string[] TagsEn { get; init; } = [];
    public string Category { get; init; } = string.Empty;
}
```

### SearchService (핵심 로직)
```csharp
public IReadOnlyList<UnicodeCharacter> Search(string query, IEnumerable<UnicodeCharacter> characters)
{
    if (string.IsNullOrWhiteSpace(query))
        return characters.ToList();

    var q = query.Trim().ToLowerInvariant();

    return characters.Where(c =>
        c.TagsKo.Any(t => t.Contains(q)) ||
        c.TagsEn.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
        c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
        c.Codepoint.Contains(q, StringComparison.OrdinalIgnoreCase)
    ).ToList();
}
```

### MainViewModel (핵심 부분)
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FilteredCharacters))]
private string _searchQuery = string.Empty;

public IEnumerable<UnicodeCharacter> FilteredCharacters =>
    _searchService.Search(SearchQuery, _allCharacters);

[RelayCommand]
private void CopyCharacter(UnicodeCharacter character)
{
    _clipboardService.Copy(character.Char);
}
```

---

## 다음 단계

Phase 1 완료 후 → [Phase 2: 핵심 기능](./PHASE2-CORE.md)
