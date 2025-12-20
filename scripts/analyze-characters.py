#!/usr/bin/env python3
"""
Character Database Analyzer
===========================
characters.json 파일을 분석하여 현재 상태와 빠진 문자를 파악하는 도구

사용법:
    python analyze-characters.py [--summary] [--category <name>] [--gaps] [--unicode-blocks]

옵션:
    --summary        전체 요약 (기본값)
    --category NAME  특정 카테고리 상세 분석
    --gaps           빠진 유니코드 범위 분석
    --unicode-blocks 유니코드 블록별 커버리지
    --json           JSON 형식으로 출력
"""

import json
import sys
import argparse
import io

# Windows 터미널 UTF-8 출력 설정
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
from pathlib import Path
from collections import defaultdict

# 유니코드 블록 정의 (주요 블록만)
UNICODE_BLOCKS = {
    "Basic Latin": (0x0000, 0x007F),
    "Latin-1 Supplement": (0x0080, 0x00FF),
    "General Punctuation": (0x2000, 0x206F),
    "Superscripts and Subscripts": (0x2070, 0x209F),
    "Currency Symbols": (0x20A0, 0x20CF),
    "Letterlike Symbols": (0x2100, 0x214F),
    "Number Forms": (0x2150, 0x218F),
    "Arrows": (0x2190, 0x21FF),
    "Mathematical Operators": (0x2200, 0x22FF),
    "Miscellaneous Technical": (0x2300, 0x23FF),
    "Enclosed Alphanumerics": (0x2460, 0x24FF),
    "Box Drawing": (0x2500, 0x257F),
    "Block Elements": (0x2580, 0x259F),
    "Geometric Shapes": (0x25A0, 0x25FF),
    "Miscellaneous Symbols": (0x2600, 0x26FF),
    "Dingbats": (0x2700, 0x27BF),
    "Supplemental Arrows-A": (0x27F0, 0x27FF),
    "Supplemental Arrows-B": (0x2900, 0x297F),
    "Miscellaneous Symbols and Arrows": (0x2B00, 0x2BFF),
    "Greek and Coptic": (0x0370, 0x03FF),
    "Emoticons": (0x1F600, 0x1F64F),
    "Miscellaneous Symbols and Pictographs": (0x1F300, 0x1F5FF),
    "Transport and Map Symbols": (0x1F680, 0x1F6FF),
    "Supplemental Symbols and Pictographs": (0x1F900, 0x1F9FF),
    "Symbols and Pictographs Extended-A": (0x1FA00, 0x1FA6F),
    "Regional Indicator Symbols": (0x1F1E0, 0x1F1FF),
}

# 카테고리별 예상 유니코드 범위
CATEGORY_UNICODE_RANGES = {
    "arrow": [(0x2190, 0x21FF), (0x27F0, 0x27FF), (0x2900, 0x297F)],
    "math": [(0x2200, 0x22FF), (0x2A00, 0x2AFF)],
    "greek": [(0x0370, 0x03FF)],
    "line": [(0x2500, 0x257F), (0x2580, 0x259F)],
    "geometric": [(0x25A0, 0x25FF), (0x2B00, 0x2B4F)],
    "circled": [(0x2460, 0x24FF), (0x3200, 0x32FF)],
    "superscript": [(0x2070, 0x209F)],
    "currency": [(0x20A0, 0x20CF)],
    "punctuation": [(0x2000, 0x206F)],
    "music": [(0x2669, 0x266F), (0x1F3B5, 0x1F3BC)],
    "emoji": [(0x1F600, 0x1F64F), (0x1F300, 0x1F5FF), (0x1F680, 0x1F6FF)],
    "animal": [(0x1F400, 0x1F43F), (0x1F980, 0x1F9AF)],
    "food": [(0x1F345, 0x1F37F), (0x1F950, 0x1F96F)],
    "flag": [(0x1F1E0, 0x1F1FF), (0x1F3C1, 0x1F3F4)],
    "star": [(0x2721, 0x2739), (0x2605, 0x2606)],
    "heart": [(0x2661, 0x2665), (0x2763, 0x2765), (0x1F493, 0x1F49F)],
    "weather": [(0x2600, 0x2602), (0x2614, 0x2614), (0x26C4, 0x26C8), (0x1F300, 0x1F32D)],
    "check": [(0x2610, 0x2612), (0x2713, 0x2718), (0x2705, 0x2705)],
    "zodiac": [(0x2648, 0x2653)],
    "roman": [(0x2160, 0x216F), (0x2170, 0x217F)],
    "hand": [(0x261A, 0x261F), (0x1F446, 0x1F450), (0x1F91A, 0x1F91F)],
    "face": [(0x1F600, 0x1F64F)],
    "object": [(0x1F451, 0x1F4FF), (0x1F6A0, 0x1F6FF)],
    "game": [(0x2654, 0x265F), (0x2660, 0x2667), (0x1F0A0, 0x1F0FF)],
    "bracket": [(0x0028, 0x0029), (0x005B, 0x005D), (0x007B, 0x007D), (0x2768, 0x2775), (0x27E6, 0x27EF)],
}


def load_characters(filepath):
    """characters.json 파일 로드"""
    with open(filepath, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data.get('characters', data) if isinstance(data, dict) else data


def get_codepoint_value(codepoint_str):
    """유니코드 코드포인트 문자열을 정수로 변환"""
    # "U+1F600" 또는 "U+1F1F0 U+1F1F7" 형태
    parts = codepoint_str.split()
    if parts:
        return int(parts[0].replace("U+", ""), 16)
    return 0


def analyze_categories(characters):
    """카테고리별 분석"""
    categories = defaultdict(list)
    for char in characters:
        cat = char.get('category', 'unknown')
        categories[cat].append(char)
    return categories


def get_category_summary(categories):
    """카테고리 요약 생성"""
    summary = []
    for cat, chars in sorted(categories.items(), key=lambda x: -len(x[1])):
        codepoints = [get_codepoint_value(c.get('codepoint', '')) for c in chars]
        min_cp = min(codepoints) if codepoints else 0
        max_cp = max(codepoints) if codepoints else 0
        summary.append({
            'category': cat,
            'count': len(chars),
            'codepoint_range': f"U+{min_cp:04X} - U+{max_cp:04X}",
            'sample': chars[0]['char'] if chars else ''
        })
    return summary


def analyze_unicode_coverage(characters):
    """유니코드 블록별 커버리지 분석"""
    codepoints = set()
    for char in characters:
        cp = get_codepoint_value(char.get('codepoint', ''))
        if cp:
            codepoints.add(cp)

    coverage = []
    for block_name, (start, end) in UNICODE_BLOCKS.items():
        block_size = end - start + 1
        covered = len([cp for cp in codepoints if start <= cp <= end])
        if covered > 0:
            coverage.append({
                'block': block_name,
                'range': f"U+{start:04X}-U+{end:04X}",
                'total': block_size,
                'covered': covered,
                'percentage': round(covered / block_size * 100, 1)
            })

    return sorted(coverage, key=lambda x: -x['covered'])


def find_gaps(characters, category=None):
    """빠진 유니코드 범위 찾기"""
    if category and category in CATEGORY_UNICODE_RANGES:
        ranges = CATEGORY_UNICODE_RANGES[category]
    else:
        ranges = list(UNICODE_BLOCKS.values())

    # 현재 있는 코드포인트
    existing = set()
    for char in characters:
        if category is None or char.get('category') == category:
            cp = get_codepoint_value(char.get('codepoint', ''))
            if cp:
                existing.add(cp)

    gaps = []
    for start, end in ranges:
        missing = []
        for cp in range(start, end + 1):
            if cp not in existing:
                # 유효한 유니코드 문자인지 확인 (간단히)
                try:
                    char = chr(cp)
                    if char.isprintable() or cp >= 0x1F300:
                        missing.append(cp)
                except:
                    pass

        if missing and len(missing) < (end - start + 1):  # 완전히 비어있지 않은 경우만
            gaps.append({
                'range': f"U+{start:04X}-U+{end:04X}",
                'missing_count': len(missing),
                'sample_missing': [f"U+{cp:04X}" for cp in missing[:5]]
            })

    return gaps


def print_summary(characters):
    """전체 요약 출력"""
    categories = analyze_categories(characters)
    summary = get_category_summary(categories)

    print("=" * 60)
    print("📊 CHARACTER DATABASE SUMMARY")
    print("=" * 60)
    print(f"\n총 문자 수: {len(characters)}개")
    print(f"카테고리 수: {len(categories)}개")
    print()
    print("-" * 60)
    print(f"{'카테고리':<15} {'개수':>6} {'샘플':>6} {'코드포인트 범위':<25}")
    print("-" * 60)

    for item in summary:
        print(f"{item['category']:<15} {item['count']:>6} {item['sample']:>6} {item['codepoint_range']:<25}")

    print("-" * 60)
    print(f"{'합계':<15} {len(characters):>6}")
    print()


def print_unicode_coverage(characters):
    """유니코드 블록 커버리지 출력"""
    coverage = analyze_unicode_coverage(characters)

    print("=" * 60)
    print("📦 UNICODE BLOCK COVERAGE")
    print("=" * 60)
    print()
    print(f"{'블록명':<35} {'커버':>6} {'전체':>6} {'%':>6}")
    print("-" * 60)

    for item in coverage:
        bar = "█" * int(item['percentage'] / 10)
        print(f"{item['block']:<35} {item['covered']:>6} {item['total']:>6} {item['percentage']:>5}% {bar}")

    print()


def print_category_detail(characters, category):
    """특정 카테고리 상세 출력"""
    cats = analyze_categories(characters)
    if category not in cats:
        print(f"카테고리 '{category}'를 찾을 수 없습니다.")
        print(f"사용 가능한 카테고리: {', '.join(sorted(cats.keys()))}")
        return

    chars = cats[category]
    print("=" * 60)
    print(f"📂 CATEGORY: {category}")
    print("=" * 60)
    print(f"\n총 {len(chars)}개 문자\n")

    # 코드포인트 순 정렬
    chars_sorted = sorted(chars, key=lambda x: get_codepoint_value(x.get('codepoint', '')))

    print(f"{'문자':>4} {'코드포인트':<12} {'이름':<30} {'태그(한글)':<20}")
    print("-" * 70)

    for char in chars_sorted[:50]:  # 처음 50개만
        tags = ', '.join(char.get('tags_ko', [])[:2])
        print(f"{char['char']:>4} {char.get('codepoint', ''):<12} {char.get('name', '')[:28]:<30} {tags:<20}")

    if len(chars) > 50:
        print(f"\n... 외 {len(chars) - 50}개 더 있음")

    # 갭 분석
    print("\n" + "-" * 60)
    print("빠진 문자 분석:")
    gaps = find_gaps(characters, category)
    if gaps:
        for gap in gaps[:3]:
            print(f"  - {gap['range']}: {gap['missing_count']}개 빠짐")
            print(f"    예시: {', '.join(gap['sample_missing'])}")
    else:
        print("  분석할 범위가 정의되지 않았습니다.")
    print()


def print_gaps(characters):
    """빠진 범위 출력"""
    print("=" * 60)
    print("🔍 GAP ANALYSIS (빠진 문자 범위)")
    print("=" * 60)
    print()

    for category in sorted(CATEGORY_UNICODE_RANGES.keys()):
        gaps = find_gaps(characters, category)
        if gaps:
            print(f"📁 {category}:")
            for gap in gaps:
                print(f"   {gap['range']}: {gap['missing_count']}개 빠짐")
            print()


def main():
    parser = argparse.ArgumentParser(description='Character Database Analyzer')
    parser.add_argument('--summary', action='store_true', help='전체 요약')
    parser.add_argument('--category', type=str, help='특정 카테고리 상세')
    parser.add_argument('--gaps', action='store_true', help='빠진 범위 분석')
    parser.add_argument('--unicode-blocks', action='store_true', help='유니코드 블록 커버리지')
    parser.add_argument('--json', action='store_true', help='JSON 출력')
    parser.add_argument('--file', type=str, help='characters.json 경로')

    args = parser.parse_args()

    # 파일 경로 찾기
    if args.file:
        filepath = Path(args.file)
    else:
        # 스크립트 위치 기준으로 찾기
        script_dir = Path(__file__).parent
        filepath = script_dir.parent / 'src' / 'UnicodeSearcher' / 'Data' / 'characters.json'

    if not filepath.exists():
        print(f"파일을 찾을 수 없습니다: {filepath}")
        sys.exit(1)

    characters = load_characters(filepath)

    # 기본값: summary
    if not any([args.summary, args.category, args.gaps, args.unicode_blocks]):
        args.summary = True

    if args.json:
        result = {
            'total': len(characters),
            'categories': get_category_summary(analyze_categories(characters)),
            'unicode_coverage': analyze_unicode_coverage(characters)
        }
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        if args.summary:
            print_summary(characters)
        if args.unicode_blocks:
            print_unicode_coverage(characters)
        if args.category:
            print_category_detail(characters, args.category)
        if args.gaps:
            print_gaps(characters)


if __name__ == '__main__':
    main()
