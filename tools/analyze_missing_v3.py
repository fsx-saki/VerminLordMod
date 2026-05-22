 #!/usr/bin/env python3
"""
精确分析：小说 vs 项目 — 找出缺失的蛊虫和仙蛊屋。
使用多策略匹配：精确匹配、子串匹配、拼音匹配。
"""

import sqlite3
import os
import re
import json
from pathlib import Path

BASE_DIR = Path("/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod")

def load_novel_gu_worms():
    db_path = BASE_DIR / "novel_analyzer" / "novel.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT name, rank FROM entities WHERE type='gu_worm' ORDER BY name")
    rows = cursor.fetchall()
    conn.close()
    return {name: rank for name, rank in rows}

def load_novel_items():
    db_path = BASE_DIR / "novel_analyzer" / "novel.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT name, rank FROM entities WHERE type='item' ORDER BY name")
    rows = cursor.fetchall()
    conn.close()
    return {name: rank for name, rank in rows}

def load_finish_all():
    """从 finish.db 加载所有中文名"""
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    
    all_cn_names = set()
    
    # gu_weapons 表
    cursor.execute("SELECT name FROM gu_weapons")
    for row in cursor.fetchall():
        name = row[0]
        if name and any('\u4e00' <= c <= '\u9fff' for c in name):
            all_cn_names.add(name)
    
    # items 表
    cursor.execute("SELECT name FROM items")
    for row in cursor.fetchall():
        name = row[0]
        if name and any('\u4e00' <= c <= '\u9fff' for c in name):
            all_cn_names.add(name)
    
    conn.close()
    return all_cn_names

def get_project_class_names():
    """获取项目中所有 .cs 文件的类名"""
    classes = set()
    
    for subdir in ['Weapons', 'Special', 'GuHouses']:
        d = BASE_DIR / "Content" / "Items" / subdir
        if d.exists():
            for f in os.listdir(str(d)):
                if f.endswith(".cs"):
                    classes.add(f[:-3])
    
    return classes

def normalize(s):
    """标准化：去空格、去蛊/虫/仙蛊后缀"""
    s = s.strip()
    s = re.sub(r'[（(].*?[）)]', '', s)  # 去括号内容
    s = s.replace(' ', '')
    return s

def extract_core_name(name):
    """提取核心名称（去掉蛊/虫/仙蛊等后缀）"""
    name = normalize(name)
    for suffix in ['仙蛊', '蛊', '虫']:
        if name.endswith(suffix) and len(name) > len(suffix):
            return name[:-len(suffix)]
    return name

def match_novel_to_finish(novel_name, finish_cn_names):
    """尝试将小说中的蛊虫名匹配到 finish.db 中的中文名"""
    nn = normalize(novel_name)
    
    # 1. 精确匹配
    if nn in finish_cn_names:
        return True, "exact"
    
    # 2. 去掉后缀匹配
    core = extract_core_name(novel_name)
    for fn in finish_cn_names:
        fn_core = extract_core_name(fn)
        if core == fn_core:
            return True, f"core_match: {fn}"
    
    # 3. 子串匹配（一方包含另一方）
    for fn in finish_cn_names:
        if nn in fn or fn in nn:
            return True, f"substr: {fn}"
    
    # 4. 去掉"仙"字匹配
    nn_no_xian = nn.replace('仙', '')
    for fn in finish_cn_names:
        fn_no_xian = normalize(fn).replace('仙', '')
        if nn_no_xian == fn_no_xian:
            return True, f"no_xian: {fn}"
    
    # 5. 核心词匹配（取前2-3个字）
    if len(core) >= 2:
        for fn in finish_cn_names:
            fn_core = extract_core_name(fn)
            if len(fn_core) >= 2 and (core[:2] == fn_core[:2] or core[-2:] == fn_core[-2:]):
                if core in fn_core or fn_core in core:
                    return True, f"core_substr: {fn}"
    
    return False, ""

def main():
    print("=" * 80)
    print("精确分析 v3：小说 vs 项目 — 缺失蛊虫和仙蛊屋")
    print("=" * 80)
    
    novel_gu = load_novel_gu_worms()
    novel_items = load_novel_items()
    finish_cn = load_finish_all()
    project_classes = get_project_class_names()
    
    print(f"\n小说蛊虫: {len(novel_gu)}")
    print(f"小说物品: {len(novel_items)}")
    print(f"finish.db 中文名: {len(finish_cn)}")
    print(f"项目类名: {len(project_classes)}")
    
    # ============ 蛊虫匹配 ============
    print("\n" + "=" * 80)
    print("一、蛊虫匹配分析")
    print("=" * 80)
    
    matched = {}
    unmatched = {}
    
    for name, rank in novel_gu.items():
        found, source = match_novel_to_finish(name, finish_cn)
        if found:
            matched[name] = (rank, source)
        else:
            unmatched[name] = rank
    
    print(f"\n已匹配: {len(matched)}")
    print(f"未匹配: {len(unmatched)}")
    
    # 按等级分类未匹配
    rank_cats = {}
    for name, rank in sorted(unmatched.items()):
        cat = classify_rank(rank)
        if cat not in rank_cats:
            rank_cats[cat] = []
        rank_cats[cat].append((name, rank))
    
    print("\n未匹配蛊虫按等级:")
    for cat in ['九转', '八转', '七转', '六转', '五转', '四转', '三转', '二转', '一转', '零转', '仙蛊(未知转)', '传说/概念级', '未知']:
        if cat in rank_cats:
            items = rank_cats[cat]
            print(f"\n  [{cat}] ({len(items)}个):")
            for name, rank in items:
                print(f"    {name}|{rank}")
    
    # ============ 仙蛊屋匹配 ============
    print("\n" + "=" * 80)
    print("二、仙蛊屋/特殊物品匹配分析")
    print("=" * 80)
    
    gu_house_kw = ['仙蛊屋', '屋', '宫', '殿', '阁', '楼', '台', '亭', '坛', '城', '堡', '庙', '馆', '园', '池', '塔', '阵']
    
    novel_gu_houses = {}
    for name, rank in novel_items.items():
        for kw in gu_house_kw:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    for name, rank in novel_gu.items():
        for kw in gu_house_kw:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    print(f"\n小说中仙蛊屋/建筑类: {len(novel_gu_houses)}")
    
    # 检查 Special 目录
    special_files = set()
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    if special_dir.exists():
        for f in os.listdir(str(special_dir)):
            if f.endswith(".cs"):
                special_files.add(f[:-3])
    
    # 检查 GuHouses 目录
    guhouses_files = set()
    guhouses_dir = BASE_DIR / "Content" / "Items" / "GuHouses"
    if guhouses_dir.exists():
        for f in os.listdir(str(guhouses_dir)):
            if f.endswith(".cs"):
                guhouses_files.add(f[:-3])
    
    matched_houses = []
    unmatched_houses = []
    
    for name, rank in sorted(novel_gu_houses.items()):
        found = False
        
        # 检查 Special 目录（中文文件名）
        if name in special_files:
            matched_houses.append((name, rank, "Special"))
            found = True
        
        # 检查 finish.db
        if not found:
            fn, src = match_novel_to_finish(name, finish_cn)
            if fn:
                matched_houses.append((name, rank, src))
                found = True
        
        if not found:
            unmatched_houses.append((name, rank))
    
    print(f"已匹配仙蛊屋: {len(matched_houses)}")
    print(f"未匹配仙蛊屋: {len(unmatched_houses)}")
    
    if unmatched_houses:
        print("\n未匹配仙蛊屋列表:")
        for name, rank in unmatched_houses:
            print(f"  {name}|{rank if rank else '未知'}")
    
    # ============ 输出 ============
    print("\n" + "=" * 80)
    print("三、总结")
    print("=" * 80)
    print(f"\n需要创建的占位蛊虫: {len(unmatched)}")
    print(f"需要创建的占位仙蛊屋: {len(unmatched_houses)}")
    
    # 保存结果
    output = {
        "unmatched_gu_worms": {cat: items for cat, items in rank_cats.items()},
        "unmatched_gu_houses": [(n, r) for n, r in unmatched_houses],
        "total_unmatched_gu": len(unmatched),
        "total_unmatched_houses": len(unmatched_houses),
    }
    
    output_path = BASE_DIR / "tools" / "missing_entities_v3.json"
    with open(str(output_path), 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)
    print(f"\n报告已保存: {output_path}")

def classify_rank(rank_str):
    rank_str = rank_str.strip()
    if not rank_str or rank_str in ('未知', '未明确'):
        return '未知'
    if '九转' in rank_str:
        return '九转'
    if '八转' in rank_str:
        return '八转'
    if '七转' in rank_str:
        return '七转'
    if '六转' in rank_str:
        return '六转'
    if '五转' in rank_str:
        return '五转'
    if '四转' in rank_str:
        return '四转'
    if '三转' in rank_str:
        return '三转'
    if '二转' in rank_str:
        return '二转'
    if '一转' in rank_str:
        return '一转'
    if '零转' in rank_str or '凡蛊' in rank_str or '凡级' in rank_str:
        return '零转'
    if '仙蛊' in rank_str or '仙级' in rank_str:
        return '仙蛊(未知转)'
    if '传说' in rank_str or '概念' in rank_str:
        return '传说/概念级'
    return '未知'

if __name__ == "__main__":
    main()
