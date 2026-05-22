#!/usr/bin/env python3
"""
分析小说数据库 vs 项目实现，找出缺失的蛊虫和仙蛊屋。
输出缺失列表，供创建占位实现使用。
"""

import sqlite3
import os
import re
import json
from pathlib import Path

BASE_DIR = Path("/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod")

def load_novel_gu_worms():
    """从 novel.db 加载所有蛊虫"""
    db_path = BASE_DIR / "novel_analyzer" / "novel.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT name, rank FROM entities WHERE type='gu_worm' ORDER BY name")
    rows = cursor.fetchall()
    conn.close()
    return {name: rank for name, rank in rows}

def load_novel_items():
    """从 novel.db 加载所有物品"""
    db_path = BASE_DIR / "novel_analyzer" / "novel.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT name, rank FROM entities WHERE type='item' ORDER BY name")
    rows = cursor.fetchall()
    conn.close()
    return {name: rank for name, rank in rows}

def load_finish_gu_weapons():
    """从 finish.db 加载已实现的蛊虫"""
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT class_name, name, rank, dao_type FROM gu_weapons ORDER BY class_name")
    rows = cursor.fetchall()
    conn.close()
    return {name: {"class_name": cn, "rank": r, "dao_type": d} for cn, name, r, d in rows}

def load_finish_items():
    """从 finish.db 加载已实现的物品"""
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT class_name, name, category, rank FROM items ORDER BY class_name")
    rows = cursor.fetchall()
    conn.close()
    return {name: {"class_name": cn, "category": cat, "rank": r} for cn, name, cat, r in rows}

def scan_project_cs_files():
    """扫描项目中所有 .cs 文件，提取类名"""
    weapons_dir = BASE_DIR / "Content" / "Items" / "Weapons"
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    guhouses_dir = BASE_DIR / "Content" / "Items" / "GuHouses"
    
    class_names = set()
    
    # 扫描 Weapons 目录下所有 .cs 文件
    for root, dirs, files in os.walk(str(weapons_dir)):
        for f in files:
            if f.endswith(".cs"):
                class_names.add(f[:-3])
    
    # 扫描 Special 目录
    if special_dir.exists():
        for f in os.listdir(str(special_dir)):
            if f.endswith(".cs"):
                class_names.add(f[:-3])
    
    # 扫描 GuHouses 目录
    if guhouses_dir.exists():
        for f in os.listdir(str(guhouses_dir)):
            if f.endswith(".cs"):
                class_names.add(f[:-3])
    
    return class_names

def extract_chinese_name_from_file(filepath):
    """从 .cs 文件中提取中文注释或类名中的中文信息"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        # 查找中文注释
        cn_names = re.findall(r'//\s*([\u4e00-\u9fff\w]+)', content)
        # 查找类名中的中文（如果有）
        class_match = re.search(r'class\s+(\w+)', content)
        class_name = class_match.group(1) if class_match else ""
        return cn_names, class_name
    except:
        return [], ""

def normalize_name(name):
    """标准化名称用于匹配"""
    # 移除蛊/虫/仙蛊等后缀
    name = re.sub(r'[蛊虫仙蛊]', '', name)
    # 移除空白和标点
    name = name.strip()
    return name

def find_matching_gu(novel_name, finish_gu_map, project_classes):
    """尝试在项目中找到匹配的蛊虫"""
    # 直接匹配 finish.db 中的 name
    if novel_name in finish_gu_map:
        return True, f"finish.db: {finish_gu_map[novel_name]['class_name']}"
    
    # 尝试拼音匹配 - 从 finish.db 的 class_name 反查
    for cn_name, info in finish_gu_map.items():
        if novel_name == cn_name:
            return True, f"finish.db: {info['class_name']}"
    
    # 检查项目文件中的中文注释
    weapons_dir = BASE_DIR / "Content" / "Items" / "Weapons"
    for root, dirs, files in os.walk(str(weapons_dir)):
        for f in files:
            if f.endswith(".cs"):
                filepath = os.path.join(root, f)
                cn_names, _ = extract_chinese_name_from_file(filepath)
                for cn in cn_names:
                    if novel_name.startswith(cn) or cn.startswith(novel_name):
                        return True, f"file: {f}"
    
    return False, ""

def classify_rank(rank_str):
    """对等级进行分类"""
    rank_str = rank_str.strip()
    if not rank_str or rank_str == '未知' or rank_str == '未明确':
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

def main():
    print("=" * 80)
    print("小说 vs 项目 — 缺失蛊虫和仙蛊屋分析")
    print("=" * 80)
    
    # 加载数据
    novel_gu = load_novel_gu_worms()
    novel_items = load_novel_items()
    finish_gu = load_finish_gu_weapons()
    finish_items = load_finish_items()
    project_classes = scan_project_cs_files()
    
    print(f"\n小说蛊虫总数: {len(novel_gu)}")
    print(f"小说物品总数: {len(novel_items)}")
    print(f"finish.db 蛊虫数: {len(finish_gu)}")
    print(f"finish.db 物品数: {len(finish_items)}")
    print(f"项目 .cs 文件类名数: {len(project_classes)}")
    
    # 分析缺失的蛊虫
    print("\n" + "=" * 80)
    print("一、缺失蛊虫分析")
    print("=" * 80)
    
    missing_gu = []
    matched_gu = []
    
    for name, rank in sorted(novel_gu.items()):
        found, source = find_matching_gu(name, finish_gu, project_classes)
        if not found:
            missing_gu.append((name, rank))
        else:
            matched_gu.append((name, rank, source))
    
    print(f"\n已匹配蛊虫: {len(matched_gu)}")
    print(f"缺失蛊虫: {len(missing_gu)}")
    
    # 按等级分类缺失蛊虫
    rank_categories = {}
    for name, rank in missing_gu:
        cat = classify_rank(rank)
        if cat not in rank_categories:
            rank_categories[cat] = []
        rank_categories[cat].append((name, rank))
    
    print("\n缺失蛊虫按等级分类:")
    for cat in ['九转', '八转', '七转', '六转', '五转', '四转', '三转', '二转', '一转', '零转', '仙蛊(未知转)', '传说/概念级', '未知']:
        if cat in rank_categories:
            items = rank_categories[cat]
            print(f"\n  [{cat}] ({len(items)}个):")
            for name, rank in items[:20]:
                print(f"    - {name} ({rank})")
            if len(items) > 20:
                print(f"    ... 还有 {len(items)-20} 个")
    
    # 分析仙蛊屋（从物品中筛选）
    print("\n" + "=" * 80)
    print("二、仙蛊屋分析")
    print("=" * 80)
    
    # 从 novel_items 中找出仙蛊屋相关
    gu_house_keywords = ['仙蛊屋', '屋', '宫', '殿', '阁', '楼', '台', '亭', '坛', '城', '堡', '庙', '馆', '园', '池', '塔', '阵']
    
    novel_gu_houses = {}
    for name, rank in novel_items.items():
        for kw in gu_house_keywords:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    # 也检查蛊虫中是否有仙蛊屋
    for name, rank in novel_gu.items():
        for kw in gu_house_keywords:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    print(f"\n小说中仙蛊屋/建筑类物品总数: {len(novel_gu_houses)}")
    
    # 检查哪些已经在 Special 或 GuHouses 中
    special_files = set()
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    if special_dir.exists():
        for f in os.listdir(str(special_dir)):
            if f.endswith(".cs"):
                special_files.add(f[:-3])
    
    guhouses_files = set()
    guhouses_dir = BASE_DIR / "Content" / "Items" / "GuHouses"
    if guhouses_dir.exists():
        for f in os.listdir(str(guhouses_dir)):
            if f.endswith(".cs"):
                guhouses_files.add(f[:-3])
    
    print(f"Special 目录文件数: {len(special_files)}")
    print(f"GuHouses 目录文件数: {len(guhouses_files)}")
    
    # 检查仙蛊屋是否在项目中
    missing_gu_houses = []
    existing_gu_houses = []
    
    for name, rank in sorted(novel_gu_houses.items()):
        # 检查 Special 目录（文件名是中文）
        if name in special_files:
            existing_gu_houses.append((name, rank, "Special"))
            continue
        # 检查 GuHouses 目录
        # 拼音映射检查
        found_in_guhouses = False
        for gf in guhouses_files:
            if name in gf or gf in name:
                existing_gu_houses.append((name, rank, f"GuHouses/{gf}"))
                found_in_guhouses = True
                break
        if found_in_guhouses:
            continue
        # 检查 finish.db items
        if name in finish_items:
            existing_gu_houses.append((name, rank, "finish.db items"))
            continue
        missing_gu_houses.append((name, rank))
    
    print(f"\n已存在的仙蛊屋: {len(existing_gu_houses)}")
    print(f"缺失的仙蛊屋: {len(missing_gu_houses)}")
    
    if missing_gu_houses:
        print("\n缺失仙蛊屋列表:")
        for name, rank in missing_gu_houses:
            print(f"  - {name} ({rank if rank else '未知'})")
    
    # 输出完整缺失列表到文件
    output = {
        "missing_gu_worms": [(name, rank) for name, rank in missing_gu],
        "missing_gu_worms_by_rank": {cat: items for cat, items in rank_categories.items()},
        "missing_gu_houses": [(name, rank) for name, rank in missing_gu_houses],
        "total_novel_gu": len(novel_gu),
        "total_matched_gu": len(matched_gu),
        "total_missing_gu": len(missing_gu),
        "total_novel_gu_houses": len(novel_gu_houses),
        "total_existing_gu_houses": len(existing_gu_houses),
        "total_missing_gu_houses": len(missing_gu_houses),
    }
    
    output_path = BASE_DIR / "tools" / "missing_entities_report.json"
    with open(str(output_path), 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)
    
    print(f"\n完整报告已保存到: {output_path}")
    
    # 输出缺失蛊虫的完整列表（用于创建占位）
    print("\n" + "=" * 80)
    print("三、缺失蛊虫完整列表（可用于创建占位实现）")
    print("=" * 80)
    
    for name, rank in missing_gu:
        print(f"{name}|{rank}")

if __name__ == "__main__":
    main()
