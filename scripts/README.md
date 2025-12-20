# Scripts

이 디렉토리에는 프로젝트 관리를 위한 스크립트들이 있습니다.

## analyze-characters.py

`characters.json` 파일을 분석하여 현재 상태와 빠진 문자를 파악하는 도구입니다.

### 요구사항

- Python 3.6+

### 사용법

```bash
# 전체 요약 (카테고리별 개수, 샘플, 코드포인트 범위)
python scripts/analyze-characters.py --summary

# 유니코드 블록별 커버리지 (어떤 블록이 몇 % 커버됐는지)
python scripts/analyze-characters.py --unicode-blocks

# 빠진 문자 범위 분석 (카테고리별로 뭐가 빠졌는지)
python scripts/analyze-characters.py --gaps

# 특정 카테고리 상세 (문자 목록 + 빠진거)
python scripts/analyze-characters.py --category emoji

# JSON 형식 출력 (다른 도구와 연동용)
python scripts/analyze-characters.py --json

# 커스텀 파일 경로 지정
python scripts/analyze-characters.py --file path/to/characters.json
```

### 옵션

| 옵션 | 설명 |
|------|------|
| `--summary` | 전체 요약 (기본값) |
| `--unicode-blocks` | 유니코드 블록별 커버리지 |
| `--gaps` | 빠진 유니코드 범위 분석 |
| `--category NAME` | 특정 카테고리 상세 분석 |
| `--json` | JSON 형식으로 출력 |
| `--file PATH` | characters.json 파일 경로 |

### 출력 예시

#### --summary
```
============================================================
📊 CHARACTER DATABASE SUMMARY
============================================================

총 문자 수: 1312개
카테고리 수: 30개

------------------------------------------------------------
카테고리         개수   샘플  코드포인트 범위
------------------------------------------------------------
emoji           285    😀   U+2328 - U+1F9F2
math             99    ×   U+00AC - U+2A06
...
```

#### --gaps
```
============================================================
🔍 GAP ANALYSIS (빠진 문자 범위)
============================================================

📁 arrow:
   U+2190-U+21FF: 76개 빠짐
   U+27F0-U+27FF: 10개 빠짐

📁 roman:
   U+2160-U+216F: 10개 빠짐
...
```

### 활용 방법

1. **작업 시작 전**: `--summary`로 현재 상태 파악
2. **추가할 문자 찾기**: `--gaps`로 빠진 범위 확인
3. **특정 카테고리 작업**: `--category <name>`으로 상세 분석
4. **자동화**: `--json`으로 다른 도구와 연동
