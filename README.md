# Unicode Searcher (유니코드 검색기)

Windows용 유니코드 특수문자 검색 및 복사 도구입니다.

**한글/영어** 자연어로 특수문자를 검색하고, **글로벌 핫키**로 어디서든 빠르게 호출할 수 있습니다.

## 왜 만들었나요?

- `Win + .` 이모지 창의 특수문자 검색이 부실함 (예: "별" 검색해도 ★ 안 나옴)
- 한자키 없는 키보드에서 특수문자 입력이 불편함
- 기존 문자표(charmap)는 검색 기능이 없고 UX가 좋지 않음

## 주요 기능

### 1. 자연어 검색
- **한글**: "별", "화살표", "동그라미", "체크", "하트"
- **영어**: "star", "arrow", "circle", "check", "heart"
- **유니코드 이름**: "BLACK STAR", "RIGHTWARDS ARROW"
- **코드포인트**: "U+2605", "2605"

### 2. 글로벌 핫키
- 기본: `Ctrl + Alt + Space`
- 어떤 앱에서든 호출 가능
- 설정에서 변경 가능

### 3. 카테고리 브라우징
| 카테고리 | 예시 |
|---------|------|
| 별/스타 | ★ ☆ ✦ ✧ |
| 도형 | ● ○ ■ □ ▲ △ |
| 화살표 | → ← ↑ ↓ ⇒ |
| 체크 | ✓ ✔ ☑ ✗ |
| 하트 | ♥ ♡ ❤ |
| 음악 | ♪ ♫ ♬ |
| 수학 | ± × ÷ ≠ ≤ ≥ ∞ |
| 화폐 | ₩ $ € ¥ £ |

### 4. 즐겨찾기 & 최근 사용
- 자주 쓰는 문자를 즐겨찾기에 추가
- 최근 사용한 20개 문자 자동 저장

### 5. 시스템 트레이
- 백그라운드 실행
- 트레이 아이콘 더블클릭으로 호출
- Windows 시작 시 자동 실행 (설정 가능)

## 단축키

| 단축키 | 동작 |
|--------|------|
| `Ctrl+Alt+Space` | 앱 호출/토글 (글로벌) |
| `ESC` | 창 닫기 |
| `Enter` | 선택 문자 복사 + 창 닫기 |
| `↑` `↓` `←` `→` | 문자 선택 이동 |
| `F` | 즐겨찾기 토글 |
| `1`-`9` | 최근 사용 N번째 문자 선택 |

## 설치

### 요구 사항
- Windows 10/11
- .NET 8.0 Runtime

### 다운로드
[Releases](../../releases) 페이지에서 최신 버전을 다운로드하세요.

### 빌드하기
```bash
# .NET 8 SDK 필요
git clone https://github.com/username/UnicodeSearcher.git
cd UnicodeSearcher
dotnet build
dotnet run --project src/UnicodeSearcher
```

### 배포용 빌드
```bash
dotnet publish src/UnicodeSearcher -c Release -r win-x64 --self-contained false
```

## 설정

설정 창에서 다음을 변경할 수 있습니다:

- **테마**: 시스템 설정 / 라이트 / 다크
- **동작**: 선택 후 창 닫기, 검색어 초기화
- **핫키**: 글로벌 단축키 변경
- **시작**: Windows 시작 시 자동 실행

## 기술 스택

- WPF (.NET 8)
- C# 12
- CommunityToolkit.Mvvm (MVVM 패턴)
- H.Hooks (글로벌 핫키)
- Hardcodet.NotifyIcon.Wpf (시스템 트레이)

## 라이선스

MIT License

## 기여

버그 리포트, 기능 제안, PR 환영합니다!
