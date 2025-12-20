# Phase 5: v1.2 UI 개선

> **목표**: UI 디자인 개선 및 테마 기능 수정

**상태**: 🚧 진행 중

---

## 발견된 문제점

### 1. Dark/Light 테마 설정이 적용 안 됨 ⭐
- **증상**: 설정에서 테마를 변경해도 UI가 변하지 않음
- **원인**: MainWindow.xaml에서 모든 색상이 **하드코딩**됨
  - `Background="#FAFAFA"` 대신 `{DynamicResource BackgroundBrush}` 사용해야 함
  - 테마 리소스 키가 정의되어 있지만 실제로 사용되지 않음
- **해결**: 모든 하드코딩된 색상을 DynamicResource로 변경
- **우선순위**: 🔴 High

### 2. 카테고리 선택 시 하이라이트 안 보임
- **증상**: 카테고리 탭에서 키보드로 이동해도 어디가 선택됐는지 안 보임
- **원인**: CategoryButtonStyle에 `IsFocused` 트리거 없음
- **해결**: 포커스 상태에 대한 시각적 스타일 추가
- **우선순위**: 🔴 High

### 3. 전반적인 UI 디자인 개선 필요
- **현재**: 기본적인 WPF 스타일
- **목표**: 모던하고 깔끔한 디자인
- **우선순위**: 🟡 Medium

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

### 인기 UI 라이브러리

| 라이브러리 | 설명 | 적합성 |
|-----------|------|--------|
| **WPF UI** | Windows 11 스타일 Fluent Design | ⭐ 추천 |
| **ModernWpf** | 모던 스타일 + Light/Dark 테마 | ⭐ 추천 |
| **Adonis UI** | 깔끔한 모던 스타일 | 고려 |
| **MahApps.Metro** | Metro 스타일 (약간 구식) | 패스 |

### .NET 9 새 기능
- `ThemeMode` 속성으로 Fluent 스타일 간편 적용
- 내장 Light/Dark 모드 지원
- 시스템 액센트 색상 지원

### 참고 자료
- [WPF UI - Fluent Design](https://wpfui.lepo.co/)
- [ModernWpf GitHub](https://github.com/Kinnara/ModernWpf)
- [WPF Best Practices 2024](https://developer.mescius.com/blogs/wpf-development-best-practices-for-2024)
- [10 WPF Best Practices 2024](https://blog.postsharp.net/wpf-best-practices-2024)

---

## 작업 목록

### 5.1 테마 기능 수정 (필수)
- [ ] MainWindow.xaml의 하드코딩된 색상을 DynamicResource로 변경
  - [ ] Window Background
  - [ ] Border Background/BorderBrush
  - [ ] TextBlock Foreground
  - [ ] Button 스타일
- [ ] SettingsWindow.xaml도 동일하게 수정
- [ ] 테마 변경 테스트

**변경 예시**:
```xml
<!-- Before -->
<Window Background="#FAFAFA">
<Border Background="White" BorderBrush="#E0E0E0">

<!-- After -->
<Window Background="{DynamicResource BackgroundBrush}">
<Border Background="{DynamicResource SurfaceBrush}" BorderBrush="{DynamicResource BorderBrush}">
```

### 5.2 카테고리 하이라이트 (필수)
- [ ] CategoryButtonStyle에 IsFocused 트리거 추가
- [ ] 현재 선택된 카테고리 시각적 구분 강화

**변경 예시**:
```xml
<Trigger Property="IsFocused" Value="True">
    <Setter TargetName="border" Property="BorderBrush" Value="{DynamicResource AccentBrush}"/>
    <Setter TargetName="border" Property="BorderThickness" Value="0,0,0,3"/>
</Trigger>
```

### 5.3 Seamless Title Bar (필수)
- [ ] WindowChrome으로 기본 타이틀 바 제거
- [ ] 커스텀 타이틀 바 구현 (드래그, 닫기 버튼)
- [ ] 창 콘텐츠와 자연스럽게 연결되는 디자인

**WindowChrome 설정**:
```xml
<WindowChrome.WindowChrome>
    <WindowChrome GlassFrameThickness="0"
                  CornerRadius="8"
                  CaptionHeight="32"
                  ResizeBorderThickness="4"/>
</WindowChrome.WindowChrome>
```

**참고 자료**:
- [WindowChrome 가이드 - CodeProject](https://www.codeproject.com/Articles/5255192/Use-WindowChrome-to-Customize-the-Title-Bar-in-WPF)
- [Custom Title Bar 구현 - David Rickard](https://engy.us/blog/2020/01/01/implementing-a-custom-window-title-bar-in-wpf/)
- [BorderlessWPFWindow - GitHub](https://github.com/ali-harkous/BorderlessWPFWindow)

### 5.4 UI 디자인 개선 (선택)
- [ ] UI 라이브러리 도입 검토 (WPF UI 또는 ModernWpf)
- [ ] 그림자, 둥근 모서리 개선
- [ ] 폰트 및 간격 조정
- [ ] 애니메이션 추가 (선택)

---

## 색상 리소스 매핑

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

---

## 완료 기준 체크리스트

### 필수
- [ ] Dark 테마 적용 시 UI가 어두운 색상으로 변경됨
- [ ] Light 테마 적용 시 UI가 밝은 색상으로 변경됨
- [ ] 카테고리 탭에서 키보드 이동 시 현재 위치 표시됨
- [ ] Seamless 타이틀 바로 모던한 외관

### 선택
- [ ] 전반적인 UI가 더 모던해 보임
- [ ] 애니메이션/트랜지션 적용

---

## 작업 순서

1. **5.1 테마 기능 수정** - 하드코딩된 색상 → DynamicResource
2. **5.2 카테고리 하이라이트** - IsFocused 트리거 추가
3. **5.3 Seamless Title Bar** - WindowChrome으로 커스텀 타이틀 바
4. **테스트** - Dark/Light 테마 전환 확인
5. **5.4 UI 개선** - 추가 개선 (선택)
