# Phase 5: v1.2 UI 개선

> **목표**: UI 디자인 개선 및 테마 기능 수정

**상태**: ✅ 완료 (테마 테스트만 남음)

---

## 발견된 문제점

### 1. Dark/Light 테마 설정이 적용 안 됨 ⭐ ✅ 해결
- **증상**: 설정에서 테마를 변경해도 UI가 변하지 않음
- **원인**: MainWindow.xaml에서 모든 색상이 **하드코딩**됨
  - `Background="#FAFAFA"` 대신 `{DynamicResource BackgroundBrush}` 사용해야 함
  - 테마 리소스 키가 정의되어 있지만 실제로 사용되지 않음
- **해결**: 모든 하드코딩된 색상을 DynamicResource로 변경
- **우선순위**: 🔴 High

### 2. 카테고리 선택 시 하이라이트 안 보임 ✅ 해결
- **증상**: 카테고리 탭에서 키보드로 이동해도 어디가 선택됐는지 안 보임
- **원인**:
  - CategoryButtonStyle에 `IsFocused` 트리거 없음
  - RadioButton.IsChecked가 ViewModel.SelectedCategory와 연결 안 됨
- **해결**:
  - IsFocused/IsKeyboardFocused 트리거 추가
  - CategorySelectedConverter로 선택 상태 바인딩
  - 스크롤 자동화 (BringIntoView)
- **우선순위**: 🔴 High

### 3. 전반적인 UI 디자인 개선 필요 ✅ 해결
- **현재**: 기본적인 WPF 스타일
- **목표**: 모던하고 깔끔한 디자인
- **해결**: Seamless Title Bar + 둥근 모서리 + 모던 스크롤바
- **우선순위**: 🟡 Medium

### 4. "전체" 카테고리 중복 표시 ✅ 해결
- **증상**: 카테고리 탭에 "전체"가 두 개 표시됨
- **원인**: ViewModel에서 수동 추가 + 데이터 파일에도 존재
- **해결**: ViewModel에서 수동 추가 코드 제거, 데이터 파일의 카테고리만 사용

---

## 리서치: WPF UI 개선 방법

### 핵심 원칙 (2024 Best Practices)

1. **Fluent Design System 적용**
   - Acrylic 효과 (반투명 배경)
   - Subtle 애니메이션
   - 적응형 레이아웃

2. **미니멀리즘**
   - 불필요한 그라데이션/그림자 제거
   - 깔끔한 선과 충분한 여백
   - Flat 디자인 원칙

3. **일관된 디자인 언어**
   - 중앙화된 스타일 딕셔너리
   - Application.Resources에서 스타일 관리

### 참고 자료
- [WPF UI - Fluent Design](https://wpfui.lepo.co/)
- [ModernWpf GitHub](https://github.com/Kinnara/ModernWpf)
- [WPFModernScrollbarStyle](https://github.com/punker76/WPFModernScrollbarStyle)
- [FluentWPF ScrollBar](https://github.com/sourcechord/FluentWPF)
- [WindowChrome 가이드](https://www.codeproject.com/Articles/5255192/Use-WindowChrome-to-Customize-the-Title-Bar-in-WPF)

---

## 작업 목록

### 5.1 테마 기능 수정 (필수) ✅
- [x] MainWindow.xaml의 하드코딩된 색상을 DynamicResource로 변경
  - [x] Window Background
  - [x] Border Background/BorderBrush
  - [x] TextBlock Foreground
  - [x] Button 스타일 (입력, 즐겨찾기, 최근, 즐겨찾기 문자)
- [x] 테마 파일에 추가 리소스 추가
  - [x] FavoriteBackgroundBrush, FavoriteBorderBrush, FavoriteHoverBrush
  - [x] RecentHoverBrush, RecentPressedBrush
  - [x] WarningBackgroundBrush, WarningForegroundBrush, WarningBorderBrush
  - [x] PrimaryButtonForegroundBrush, DisabledBrush, OverlayBrush
- [ ] SettingsWindow.xaml도 동일하게 수정 (선택)
- [ ] 테마 변경 테스트

### 5.2 카테고리 하이라이트 (필수) ✅
- [x] CategoryButtonStyle에 IsFocused 트리거 추가
- [x] IsKeyboardFocused 트리거도 추가
- [x] CategorySelectedConverter 추가 (SelectedCategory와 IsChecked 연동)
- [x] PropertyChanged 이벤트로 스크롤 자동화 (BringIntoView)
- [x] 중복 "전체" 카테고리 제거

### 5.3 Seamless Title Bar (필수) ✅
- [x] WindowChrome으로 기본 타이틀 바 제거
- [x] 커스텀 타이틀 바 구현 (최소화, 닫기 버튼)
- [x] 창 콘텐츠와 자연스럽게 연결되는 디자인
- [x] 둥근 모서리 (CornerRadius="8")
- [x] 테두리 스타일링

### 5.4 모던 스크롤바 스타일 ✅
- [x] 얇은 디자인 (6px → 호버 시 8px)
- [x] 둥근 모서리 (CornerRadius: 3-4px)
- [x] 화살표 버튼 없는 미니멀 디자인
- [x] 테마 색상 연동 (DynamicResource)
- [x] 드래그 시 AccentBrush로 강조

---

## 색상 리소스 매핑

### 기본 색상
| 하드코딩 값 | 리소스 키 | 용도 |
|------------|----------|------|
| `#FAFAFA` | `BackgroundBrush` | 창 배경 |
| `White` / `#FFFFFF` | `SurfaceBrush` | 카드/패널 배경 |
| `#1A1A1A` | `ForegroundBrush` | 기본 텍스트 |
| `#666666` / `Gray` | `SecondaryForegroundBrush` | 보조 텍스트 |
| `#2196F3` | `AccentBrush` | 강조 색상 |
| `#E0E0E0` | `BorderBrush` | 테두리 |
| `#E3F2FD` | `HoverBrush` | 호버 배경 |
| `#BBDEFB` | `SelectedBrush` | 선택 배경 |
| `#F5F5F5` | `StatusBarBrush` | 상태바 배경 |

### 추가된 색상 (v1.2)
| 리소스 키 | Light 테마 | Dark 테마 | 용도 |
|----------|-----------|-----------|------|
| `FavoriteBackgroundBrush` | `#E8F5E9` | `#1B3A1B` | 즐겨찾기 버튼 배경 |
| `FavoriteBorderBrush` | `#4CAF50` | `#4CAF50` | 즐겨찾기 버튼 테두리 |
| `WarningBackgroundBrush` | `#FFF8E1` | `#3A3020` | 경고/스타 버튼 배경 |
| `WarningForegroundBrush` | `#F57C00` | `#FFB74D` | 경고/스타 버튼 텍스트 |
| `OverlayBrush` | `#80FFFFFF` | `#80000000` | 로딩 오버레이 |

---

## 변경된 파일 목록

### 테마 파일
| 파일 | 변경 내용 |
|------|-----------|
| `Resources/Themes/LightTheme.xaml` | 추가 색상 리소스 (Favorite, Recent, Warning, Overlay 등) |
| `Resources/Themes/DarkTheme.xaml` | 추가 색상 리소스 (Favorite, Recent, Warning, Overlay 등) |

### 스타일 파일
| 파일 | 변경 내용 |
|------|-----------|
| `Resources/Styles/ScrollBarStyle.xaml` | **신규** - 모던 스크롤바 스타일 |

### 컨버터
| 파일 | 변경 내용 |
|------|-----------|
| `Converters/CategorySelectedConverter.cs` | **신규** - 카테고리 선택 상태 바인딩용 |

### 뷰
| 파일 | 변경 내용 |
|------|-----------|
| `MainWindow.xaml` | 하드코딩 색상 → DynamicResource, WindowChrome, 커스텀 타이틀 바, 카테고리 MultiBinding, **ItemsControl → ListBox** (최근/즐겨찾기), **ListBoxItem 스타일 추가** |
| `MainWindow.xaml.cs` | MinimizeButton_Click, CloseButton_Click, ViewModel_PropertyChanged, ScrollCategoryIntoView, **RecentList/FavoriteList KeyDown 핸들러**, **FocusNextArea/FocusPreviousArea**, **Ctrl+F 단축키** |

### ViewModel
| 파일 | 변경 내용 |
|------|-----------|
| `ViewModels/MainViewModel.cs` | 중복 "전체" 카테고리 추가 코드 제거, **FavoriteCharacters를 ObservableCollection으로 변경**, **UpdateFavoriteCharacters() 추가** |

### 앱 설정
| 파일 | 변경 내용 |
|------|-----------|
| `App.xaml` | ScrollBarStyle.xaml 추가, CategorySelectedConverter 등록 |

---

## 완료 기준 체크리스트

### 필수
- [ ] Dark 테마 적용 시 UI가 어두운 색상으로 변경됨 (테스트 필요)
- [ ] Light 테마 적용 시 UI가 밝은 색상으로 변경됨 (테스트 필요)
- [x] 카테고리 탭에서 키보드 이동 시 현재 위치 표시됨
- [x] 선택된 카테고리가 화면 밖에 있으면 자동 스크롤
- [x] Seamless 타이틀 바로 모던한 외관
- [x] 모던 스크롤바 스타일 적용
- [x] 즐겨찾기 등록/표시 정상 동작
- [x] 최근/즐겨찾기 키보드 네비게이션 동작

### 선택
- [x] 전반적인 UI가 더 모던해 보임
- [x] Ctrl+F로 검색창 바로 이동
- [ ] 애니메이션/트랜지션 적용

---

## 키보드 단축키

| 단축키 | 기능 | 영역 |
|--------|------|------|
| `Esc` | 창 닫기 | 전역 |
| `Ctrl+F` | 검색창으로 이동 | 전역 |
| `Ctrl+D` | 즐겨찾기 토글 | 전역 |
| `Ctrl+←/→` | 카테고리 이동 | 전역 |
| `↑/↓` | 영역 간 이동 | 최근/즐겨찾기/카테고리 |
| `←/→` | 아이템 선택 | 최근/즐겨찾기/카테고리/그리드 |
| `Enter` | 문자 입력 (복사+붙여넣기+닫기) | 최근/즐겨찾기/그리드 |
| `Space` | 문자 복사 (창 유지) | 그리드 |
| `Ctrl+C` | 문자 복사 (창 유지) | 그리드 |
| `Tab` | 다음 영역으로 이동 | 전역 |
| `Shift+Tab` | 이전 영역으로 이동 | 전역 |
| `1-9` | N번째 최근 문자 복사 | 검색창 (검색어 비었을 때) |
| `Ctrl+1-9` | N번째 검색 결과 입력 | 검색창 |
| `Home/End` | 첫/마지막 문자로 이동 | 그리드 |
| `PageUp/PageDown` | 한 페이지 이동 | 그리드 |

---

## 작업 순서

1. ✅ **5.1 테마 기능 수정** - 하드코딩된 색상 → DynamicResource
2. ✅ **5.2 카테고리 하이라이트** - 선택 바인딩 + 스크롤 자동화
3. ✅ **5.3 Seamless Title Bar** - WindowChrome으로 커스텀 타이틀 바
4. ✅ **5.4 모던 스크롤바** - 얇고 미니멀한 스크롤바
5. ✅ **5.5 즐겨찾기 버그 수정** - ObservableCollection으로 변경
6. ✅ **5.6 키보드 네비게이션 버그 수정** - ListBox + FocusNextArea/FocusPreviousArea
7. ✅ **5.7 UX 개선** - Ctrl+F 검색창 이동
8. 🚧 **테스트** - Dark/Light 테마 전환 확인

---

## 🐛 발견된 버그 (v1.2)

### 버그 5: 즐겨찾기 등록/표시 문제 ✅ 해결
- **증상**: 즐겨찾기를 등록해도 UI에 표시되지 않음 (등록 여부 확인 어려움)
- **원인 분석**:
  - `FavoriteCharacters`가 `IReadOnlySet<string>`으로 노출됨
  - `OnPropertyChanged(nameof(FavoriteCharacters))` 호출해도 **같은 객체 참조**이므로 WPF가 변경 감지 못함
  - `IReadOnlySet<T>`은 `INotifyCollectionChanged`를 구현하지 않아 컬렉션 변경 알림 불가
- **해결**:
  - `FavoriteCharacters`를 `ObservableCollection<string>`으로 변경
  - `UpdateFavoriteCharacters()` 메서드 추가하여 변경 시 새 컬렉션 생성
  - 초기화 및 FavoritesChanged 이벤트 시 호출
- **우선순위**: 🔴 High
- **영향 범위**: `MainViewModel.cs`

**수정된 코드**:
```csharp
// MainViewModel.cs - 수정 후
[ObservableProperty]
private ObservableCollection<string> _favoriteCharacters = [];

// 이벤트 핸들러
_favoriteService.FavoritesChanged += (_, _) => UpdateFavoriteCharacters();

private void UpdateFavoriteCharacters()
{
    FavoriteCharacters = new ObservableCollection<string>(_favoriteService.Favorites);
}
```

### 버그 6: 즐겨찾기/최근 아이템 키보드 네비게이션 불가 ✅ 해결
- **증상**: 카테고리는 키보드로 이동 가능하나, 즐겨찾기와 최근 아이템은 키보드 선택 불가
- **원인 분석**:
  - 즐겨찾기/최근 아이템이 `ItemsControl` + `Button`으로 구현됨
  - `ItemsControl`은 선택 기능 없음 (vs `ListBox`)
  - `Window_PreviewKeyDown`에서 최근/즐겨찾기 리스트 체크 없어서 `HandleGridKeyDown`이 잘못 호출됨
- **해결**:
  - `ItemsControl` → `ListBox`로 변경
  - `ListBoxItem` 스타일 추가 (`RecentCharacterItemStyle`, `FavoriteCharacterItemStyle`)
  - `Window_PreviewKeyDown`에서 `RecentList.IsKeyboardFocusWithin`, `FavoriteList.IsKeyboardFocusWithin` 체크 추가
  - `FocusNextArea(string)`, `FocusPreviousArea(string)` 함수로 영역 간 이동 로직 통합
- **우선순위**: 🔴 High
- **영향 범위**: `MainWindow.xaml`, `MainWindow.xaml.cs`

**키보드 네비게이션 순서**:
```
검색창 ↔ 최근 ↔ 즐겨찾기 ↔ 카테고리 ↔ 아이템 그리드
   ↓        ↓         ↓           ↓           ↓
  Down    Down      Down        Down        (그리드 내 이동)
   ↑        ↑         ↑           ↑           ↑
  (N/A)    Up        Up          Up          Up (첫 행에서)
```

---

## 버그 수정 작업 목록

### 5.5 즐겨찾기 표시 버그 수정 (필수) ✅
- [x] `FavoriteCharacters`를 `ObservableCollection<string>`으로 변경
- [x] `UpdateFavoriteCharacters()` 메서드 추가
- [x] FavoritesChanged 이벤트 시 컬렉션 재생성
- [x] 초기화 시 `UpdateFavoriteCharacters()` 호출
- [x] UI 표시 테스트

### 5.6 키보드 네비게이션 버그 수정 (필수) ✅
- [x] `ItemsControl` → `ListBox`로 변경 (최근/즐겨찾기)
- [x] `RecentCharacterItemStyle`, `FavoriteCharacterItemStyle` 추가
- [x] `Window_PreviewKeyDown`에서 최근/즐겨찾기 리스트 체크 추가
- [x] `FocusNextArea(string)`, `FocusPreviousArea(string)` 함수 구현
- [x] 포커스 상태 시각화 (IsSelected 트리거)
- [x] Tab 순서 정리 (검색창 → 최근 → 즐겨찾기 → 카테고리 → 그리드)

### 5.7 UX 개선 ✅
- [x] `Ctrl+F`: 어디서든 검색창으로 바로 이동
