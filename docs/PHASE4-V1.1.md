# Phase 4: v1.1 UX 개선

> **목표**: 키보드 중심 UX 완성 및 버그 수정

**상태**: ✅ 완료 (2024-12-20)

---

## 발견된 문제점

### 1. 복사 성공인데 "복사 실패!" 메시지 표시
- **증상**: Enter로 문자 복사 시 실제로는 복사되지만 "복사 실패!" 토스트가 나옴
- **원인**: ClipboardService 재시도 로직에서 Thread.Sleep 사용
- **해결**: ClipboardService 간소화
- **우선순위**: 🔴 High ✅

### 2. 키보드 네비게이션 시 하이라이트 안 보임
- **증상**: 화살표 키로 아이템 이동 시 어떤 아이템이 선택됐는지 시각적 표시 없음
- **원인**: ItemsControl 사용 (선택 상태 지원 안 함)
- **해결**: ListBox로 변경 + IsSelected 트리거 스타일 적용
- **우선순위**: 🔴 High ✅

### 3. 카테고리 키보드 네비게이션 없음
- **증상**: 검색창/최상단에서 ↑↓ 키로 카테고리 선택 불가
- **해결**: HandleCategoryKeyDown 메서드 추가, 포커스 흐름 구현
- **우선순위**: 🟡 Medium ✅

### 4. 핫키로 창 호출 시 검색창 포커스 안 됨
- **증상**: Ctrl+Alt+Space로 창 띄울 때 검색창에 자동 포커스 안 됨
- **해결**: ShowWindow에서 Dispatcher.BeginInvoke로 포커스 설정
- **우선순위**: 🔴 High ✅

### 5. Enter 동작 변경 요청
- **현재**: Enter → 클립보드 복사 + 창 닫기
- **요청**: Enter → 이전 활성 창에 붙여넣기 + 창 닫기
- **해결**: PasteAndClose 커맨드 + SendKeys.SendWait("^v") 사용
- **우선순위**: 🟡 Medium ✅

### 6. 창이 최하단에 열림 + 작업표시줄 클릭 시 바로 꺼짐 (추가 발견)
- **증상**: 단축키로 창 열면 다른 프로그램 뒤에 열림, 작업표시줄 클릭 시 바로 꺼짐
- **원인**: Window.Deactivated 이벤트가 창 표시 직후에도 발생
- **해결**: `_lastShowTime` 추가, 500ms 동안 Deactivated 무시, Topmost 유지
- **우선순위**: 🔴 High ✅

### 7. 설정 저장 후 핫키가 적용 안 됨 (추가 발견)
- **증상**: 앱 시작 시 저장된 핫키가 작동 안 함, 설정 창 열어야 작동
- **원인**: App.OnStartup에서 설정 로드 전에 핫키 등록
- **해결**: `await _settingsService.LoadAsync()` 를 핫키 등록 전에 호출
- **우선순위**: 🔴 High ✅

### 8. Enter 누르면 앱이 1-2초 멈춤 (추가 발견) ⭐ 핵심 버그
- **증상**: 문자 선택 후 Enter 누르면 앱이 1-2초 멈춤
- **원인**: `System.Windows.Clipboard.SetText()`가 클립보드 잠금 시 내부적으로 1초 동안 재시도하며 블로킹
- **에러**: `CLIPBRD_E_CANT_OPEN (0x800401D0)` - 다른 프로그램이 클립보드 점유
- **해결**: WPF Clipboard 대신 `System.Windows.Forms.Clipboard.SetText()` 사용
- **우선순위**: 🔴 Critical ✅

---

## 디버깅 과정 (교훈)

### 문제 해결 방법론

❌ **잘못된 접근**: 추측으로 코드 수정 → 시간 낭비 + 코드 망가짐

✅ **올바른 접근**:
1. **디버깅 로그 추가** - 각 단계별 타임스탬프 기록
2. **로그 분석** - 어느 단계에서 시간이 오래 걸리는지 확인
3. **원인 검색** - 인터넷에서 에러 메시지로 검색
4. **해결책 적용** - 검증된 해결책 적용

### 로그 분석 예시
```
[20:34:01.096] Before Clipboard.SetText
[20:34:02.108] Clipboard ERROR: OpenClipboard 실패입니다. (0x800401D0)
```
→ `Clipboard.SetText`에서 1초 블로킹 발견!

### 참고 자료
- [Clipboard.SetText fails with CLIPBRD_E_CANT_OPEN - GitHub Issue](https://github.com/dotnet/wpf/issues/9901)
- [OpenClipboard failed - Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/1695747/occasional-system-exception-openclipboard-failed-)

---

## 변경된 파일 목록

### 핵심 수정
| 파일 | 변경 내용 |
|------|-----------|
| `App.xaml.cs` | OnStartup을 async로 변경, 설정 먼저 로드 |
| `MainWindow.xaml` | ItemsControl → ListBox, 선택 스타일 추가 |
| `MainWindow.xaml.cs` | 카테고리 네비게이션, 포커스 관리, Deactivated 타이밍 제어 |
| `ViewModels/MainViewModel.cs` | PasteAndClose 커맨드, WinForms Clipboard 사용 |
| `Helpers/WindowHelper.cs` | PasteToActiveWindow (SendKeys.SendWait) |
| `Services/ClipboardService.cs` | 재시도 로직 제거, 간소화 |

### 추가된 파일
| 파일 | 용도 |
|------|------|
| `GlobalUsings.cs` | WPF/WinForms 타입 충돌 해결 |
| `Helpers/DebugLogger.cs` | 디버깅용 로그 파일 기록 |

### 설정 변경
| 파일 | 변경 내용 |
|------|-----------|
| `UnicodeSearcher.csproj` | `<UseWindowsForms>true</UseWindowsForms>` 추가 |
| `SettingsWindow.xaml` | 창 높이 520→580, SaveButton에 x:Name 추가 |
| `SettingsWindow.xaml.cs` | 핫키 입력 후 녹색 피드백 표시 |

---

## 완료 기준 체크리스트

### 버그 수정
- [x] 복사 성공 시 "복사 완료" 메시지 표시
- [x] 핫키로 창 호출 시 검색창 자동 포커스
- [x] 창이 최상단에 표시됨
- [x] 작업표시줄 클릭 시 창이 유지됨
- [x] 앱 시작 시 저장된 핫키 작동
- [x] Enter 시 앱 멈춤 현상 해결 (WinForms Clipboard 사용)

### UX 개선
- [x] 화살표 키로 아이템 이동 시 하이라이트 표시
- [x] 카테고리 탭 키보드 네비게이션 (↑↓←→ 지원)
- [x] Enter = 붙여넣기, Ctrl+C = 복사
- [x] 핫키 설정 UI 개선 (녹색 피드백)

### 테스트
- [x] 수동 테스트: 키보드만으로 전체 워크플로우 완료 가능

---

## 키보드 UX 전체 흐름

```
[다른 앱에서 작업 중]
       │
       ▼ Ctrl+Alt+Space (또는 사용자 설정 핫키)
┌─────────────────────────────────┐
│ 검색창 (자동 포커스, 전체 선택)  │
│         │                       │
│         ▼ ↓ 키                  │
│ [전체][별][화살표][체크]...     │ ← ← → 카테고리 전환
│         │                       │
│         ▼ ↓ 키 또는 Enter       │
│ ┌───┬───┬───┬───┐              │
│ │ ★ │ ☆ │ ✦ │...│              │ ← 하이라이트 표시
│ └───┴───┴───┴───┘              │   ← → ↑ ↓ 이동
│         │                       │
│         ▼ Enter                 │
└─────────────────────────────────┘
       │
       ▼ 이전 앱으로 복귀 + 붙여넣기
[선택한 문자가 입력됨]
```

---

## 동작 매트릭스

| 키 | 동작 | 창 상태 |
|----|------|---------|
| Enter | 클립보드 복사 + 이전 창에 붙여넣기 | 닫힘 |
| Ctrl+C | 클립보드 복사 | 유지 |
| Space | 클립보드 복사 | 유지 |
| Escape | 창 닫기 | 닫힘 |
| Ctrl+D | 즐겨찾기 토글 | 유지 |
