# Contributing to Code Searcher

Code Searcher에 기여해 주셔서 감사합니다!

## 기여 방법

### 버그 리포트
- [Issues](https://github.com/yunchan8804/code-searcher/issues)에서 버그를 신고해 주세요
- 재현 방법, 예상 동작, 실제 동작을 상세히 기술해 주세요
- 가능하면 스크린샷을 첨부해 주세요

### 기능 제안
- 새로운 기능 아이디어가 있으시면 Issue를 열어주세요
- 왜 이 기능이 필요한지 설명해 주세요

### Pull Request
1. 이 저장소를 Fork합니다
2. 새 브랜치를 생성합니다 (`git checkout -b feature/amazing-feature`)
3. 변경사항을 커밋합니다 (`git commit -m 'Add amazing feature'`)
4. 브랜치에 Push합니다 (`git push origin feature/amazing-feature`)
5. Pull Request를 생성합니다

## 개발 환경 설정

### 요구사항
- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 또는 VS Code

### 빌드
```bash
git clone https://github.com/yunchan8804/code-searcher.git
cd code-searcher
dotnet restore
dotnet build
```

### 테스트
```bash
dotnet test
```

### 실행
```bash
dotnet run --project src/UnicodeSearcher
```

## 코드 스타일

- C# 코딩 컨벤션을 따릅니다
- MVVM 패턴을 유지합니다
- 새로운 기능에는 테스트를 추가해 주세요

## 문자 데이터 추가

새로운 문자나 카테고리를 추가하려면:

1. `src/UnicodeSearcher/Data/UnicodeData.cs` 수정
2. 적절한 카테고리에 문자 추가
3. 한글/영어 키워드 포함
4. 테스트 실행하여 검증

## 질문이 있으신가요?

Issue를 열어 질문해 주세요!
