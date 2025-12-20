# Unicode Searcher (유니코드 검색기)

[![Version](https://img.shields.io/badge/version-1.2-blue.svg)](https://github.com/yunchan8804/code-searcher/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build](https://github.com/yunchan8804/code-searcher/actions/workflows/build.yml/badge.svg)](https://github.com/yunchan8804/code-searcher/actions)

Windows용 유니코드 특수문자 검색 및 복사 도구입니다.

**한글/영어** 자연어로 **1,312개** 특수문자를 검색하고, **글로벌 핫키**로 어디서든 빠르게 호출할 수 있습니다.

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

### 3. 카테고리 브라우징 (32개 카테고리)
| 카테고리 | 예시 |
|---------|------|
| ⭐ 별 | ★ ☆ ✦ ✧ ⭐ 🌟 |
| 🔷 도형 | ● ○ ■ □ ▲ △ ◆ ◇ |
| ➡️ 화살표 | → ← ↑ ↓ ⇒ ⇐ ➔ ↗ |
| ✅ 체크 | ✓ ✔ ☑ ✗ ✘ ☐ |
| ❤️ 하트 | ♥ ♡ ❤ 💕 💗 💖 |
| 🎵 음악 | ♪ ♫ ♬ 🎵 🎶 🎼 |
| ➕ 수학 | ± × ÷ ≠ ≤ ≥ ∞ √ ∑ |
| 💰 통화 | ₩ $ € ¥ £ ₿ ฿ |
| 📝 괄호 | 「」『』【】〈〉《》 |
| 😀 얼굴 | 😊 😂 🥰 😎 🤔 😭 |
| 🐱 동물 | 🐶 🐱 🦁 🐼 🐰 🦊 |
| 🍎 음식 | 🍕 🍔 🍜 🍣 🍰 ☕ |
| 🔤 그리스 | α β γ δ Ω Σ Π |
| 📦 물건 | 📱 💻 ⌨️ 🖥️ 📷 🔧 |
| 🚩 플래그 | 🇰🇷 🇺🇸 🇯🇵 🇬🇧 🏳️ |
| ... | 그 외 17개 카테고리 |

### 4. 즐겨찾기 & 최근 사용
- 자주 쓰는 문자를 즐겨찾기에 추가
- 최근 사용한 20개 문자 자동 저장

### 5. 시스템 트레이
- 백그라운드 실행
- 트레이 아이콘 더블클릭으로 호출
- Windows 시작 시 자동 실행 (설정 가능)

## 단축키

### 글로벌 (어디서든)
| 단축키 | 동작 |
|--------|------|
| `Ctrl+Alt+Space` | 앱 호출/토글 |

### 앱 내
| 단축키 | 동작 |
|--------|------|
| `Enter` | 복사 + 이전 앱에 붙여넣기 + 창 닫기 |
| `Ctrl+C` / `Space` | 복사 (창 유지) |
| `ESC` | 창 닫기 |
| `↑` `↓` `←` `→` | 문자/영역 이동 |
| `Tab` / `Shift+Tab` | 영역 간 이동 |
| `Ctrl+D` | 즐겨찾기 토글 |
| `Ctrl+F` | 검색창으로 이동 |
| `Ctrl+,` | 설정 창 열기 |
| `Ctrl+←` `Ctrl+→` | 카테고리 전환 |
| `1`-`9` | 최근 사용 N번째 문자 |
| `Ctrl+1`-`9` | 검색 결과 N번째 문자 |

## 설치

### 요구 사항
- Windows 10/11
- .NET 8.0 Runtime

### 다운로드
[Releases](../../releases) 페이지에서 최신 버전을 다운로드하세요.

### 빌드하기
```bash
# .NET 8 SDK 필요
git clone https://github.com/yunchan8804/code-searcher.git
cd code-searcher
dotnet build
dotnet run --project src/UnicodeSearcher
```

### 배포용 빌드
```bash
dotnet publish src/UnicodeSearcher -c Release -r win-x64 --self-contained false
```

## 주요 특징 (v1.2)

- 🎨 **Material Design 다크 테마** - 눈에 편한 어두운 UI
- 🪟 **Windows 11 스타일** - 네이티브 둥근 모서리
- ⌨️ **키보드 중심 UX** - 마우스 없이 모든 기능 사용 가능
- ⚡ **빠른 입력** - Enter로 바로 이전 앱에 붙여넣기
- 🔍 **강력한 검색** - 한글/영어/유니코드 코드포인트 검색

## 설정

설정 창(`Ctrl+,`)에서 다음을 변경할 수 있습니다:

- **테마**: 시스템 설정 / 라이트 / 다크
- **동작**: 선택 후 창 닫기, 검색어 초기화
- **핫키**: 글로벌 단축키 변경 (실시간 피드백)
- **시작**: Windows 시작 시 자동 실행

## 기술 스택

- WPF (.NET 8)
- C# 12
- CommunityToolkit.Mvvm (MVVM 패턴)
- H.Hooks (글로벌 핫키)
- Hardcodet.NotifyIcon.Wpf (시스템 트레이)

## 프로젝트 통계

| 항목 | 수치 |
|------|------|
| 특수문자 | 1,312개 |
| 카테고리 | 32개 |
| 단위 테스트 | 76개 |
| 지원 검색어 | 한글, 영어, 유니코드 코드포인트 |

## 라이선스

MIT License

## 기여

버그 리포트, 기능 제안, PR 환영합니다!

## 변경 이력

### v1.2 (2024-12-21)
- Material Design 다크 테마
- Windows 11 네이티브 둥근 모서리
- 컴팩트 UI (상단 영역 최적화)
- 설정 접근성 개선 (`Ctrl+,`)
- 키보드 네비게이션 전면 개선
- 앱 아이콘 추가

### v1.1 (2024-12-20)
- Enter 동작 변경 (복사 → 복사+붙여넣기)
- 클립보드 안정성 개선
- 카테고리 키보드 네비게이션
- 핫키 설정 실시간 피드백

### v1.0 (2024-12-20)
- 최초 릴리즈
- 1,070개 문자, 28개 카테고리
- 글로벌 핫키, 시스템 트레이
- 즐겨찾기, 최근 사용
