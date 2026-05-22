#!/usr/bin/env python3
"""
精确分析小说数据库 vs 项目实现，找出缺失的蛊虫和仙蛊屋。
使用 finish.db 中的中文名映射 + 文件中文注释进行匹配。
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

def load_finish_gu_weapons():
    """从 finish.db 加载已实现的蛊虫"""
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT class_name, name, rank, dao_type FROM gu_weapons ORDER BY class_name")
    rows = cursor.fetchall()
    conn.close()
    result = {}
    for cn, name, rank, dao in rows:
        result[name] = {"class_name": cn, "rank": rank, "dao_type": dao}
    return result

def load_finish_items():
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT class_name, name, category, rank FROM items ORDER BY class_name")
    rows = cursor.fetchall()
    conn.close()
    result = {}
    for cn, name, cat, rank in rows:
        result[name] = {"class_name": cn, "category": cat, "rank": rank}
    return result

def scan_all_cs_files_for_cn_names():
    """扫描所有 .cs 文件，提取中文名映射"""
    result = {}  # cn_name -> filepath
    weapons_dir = BASE_DIR / "Content" / "Items" / "Weapons"
    
    for root, dirs, files in os.walk(str(weapons_dir)):
        for f in files:
            if not f.endswith(".cs"):
                continue
            filepath = os.path.join(root, f)
            try:
                with open(filepath, 'r', encoding='utf-8') as fp:
                    content = fp.read()
                # 查找中文注释行
                for line in content.split('\n'):
                    line = line.strip()
                    # 匹配 // 中文注释
                    m = re.search(r'//\s*([\u4e00-\u9fff\w]+)', line)
                    if m:
                        cn = m.group(1)
                        # 只保留看起来像蛊虫名的（2-6个中文字符）
                        cn_clean = re.sub(r'[\d\w]', '', cn)
                        if 2 <= len(cn_clean) <= 8:
                            result[cn] = filepath
            except:
                pass
    
    return result

def build_cn_to_classname_map():
    """构建中文名到类名的映射"""
    mapping = {}
    
    # 从 finish.db gu_weapons
    db_path = BASE_DIR / "helps" / "finish.db"
    conn = sqlite3.connect(str(db_path))
    cursor = conn.cursor()
    cursor.execute("SELECT class_name, name FROM gu_weapons")
    for cn, name in cursor.fetchall():
        mapping[name] = cn
    
    # 从 finish.db items
    cursor.execute("SELECT class_name, name FROM items")
    for cn, name in cursor.fetchall():
        mapping[name] = cn
    
    conn.close()
    return mapping

def get_all_project_class_names():
    """获取项目中所有 .cs 文件的类名"""
    classes = set()
    
    # Weapons 目录
    weapons_dir = BASE_DIR / "Content" / "Items" / "Weapons"
    for root, dirs, files in os.walk(str(weapons_dir)):
        for f in files:
            if f.endswith(".cs"):
                classes.add(f[:-3])
    
    # Special 目录
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    if special_dir.exists():
        for f in os.listdir(str(special_dir)):
            if f.endswith(".cs"):
                classes.add(f[:-3])
    
    # GuHouses 目录
    guhouses_dir = BASE_DIR / "Content" / "Items" / "GuHouses"
    if guhouses_dir.exists():
        for f in os.listdir(str(guhouses_dir)):
            if f.endswith(".cs"):
                classes.add(f[:-3])
    
    return classes

def main():
    print("=" * 80)
    print("精确分析：小说 vs 项目 — 缺失蛊虫和仙蛊屋")
    print("=" * 80)
    
    novel_gu = load_novel_gu_worms()
    novel_items = load_novel_items()
    finish_gu = load_finish_gu_weapons()
    finish_items = load_finish_items()
    cn_map = build_cn_to_classname_map()
    project_classes = get_all_project_class_names()
    
    print(f"\n小说蛊虫: {len(novel_gu)}")
    print(f"小说物品: {len(novel_items)}")
    print(f"finish.db 蛊虫: {len(finish_gu)}")
    print(f"finish.db 物品: {len(finish_items)}")
    print(f"中文名→类名映射: {len(cn_map)}")
    print(f"项目 .cs 类名: {len(project_classes)}")
    
    # ============ 蛊虫匹配 ============
    print("\n" + "=" * 80)
    print("一、蛊虫匹配分析")
    print("=" * 80)
    
    matched = set()
    unmatched = {}
    
    for name, rank in novel_gu.items():
        found = False
        
        # 1. 直接匹配 finish.db 的 name 字段
        if name in finish_gu:
            matched.add(name)
            found = True
        
        # 2. 通过 cn_map 匹配
        if not found and name in cn_map:
            class_name = cn_map[name]
            if class_name in project_classes:
                matched.add(name)
                found = True
        
        # 3. 尝试部分匹配（去掉"仙蛊"后缀等）
        if not found:
            # 尝试去掉"仙蛊"后缀
            for suffix in ['仙蛊', '蛊', '虫']:
                if name.endswith(suffix):
                    base = name[:-len(suffix)]
                    if base in cn_map:
                        matched.add(name)
                        found = True
                        break
        
        # 4. 尝试在 finish.db 中找包含关系
        if not found:
            for fn_name in finish_gu:
                if name in fn_name or fn_name in name:
                    matched.add(name)
                    found = True
                    break
        
        if not found:
            unmatched[name] = rank
    
    print(f"\n已匹配蛊虫: {len(matched)}")
    print(f"未匹配蛊虫: {len(unmatched)}")
    
    # 按等级分类
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
    
    # 从物品中筛选仙蛊屋
    gu_house_kw = ['仙蛊屋', '屋', '宫', '殿', '阁', '楼', '台', '亭', '坛', '城', '堡', '庙', '馆', '园', '池', '塔', '阵']
    
    novel_gu_houses = {}
    for name, rank in novel_items.items():
        for kw in gu_house_kw:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    # 也检查蛊虫
    for name, rank in novel_gu.items():
        for kw in gu_house_kw:
            if kw in name:
                novel_gu_houses[name] = rank
                break
    
    print(f"\n小说中仙蛊屋/建筑类: {len(novel_gu_houses)}")
    
    # 检查 Special 目录中的文件
    special_files = {}
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    if special_dir.exists():
        for f in os.listdir(str(special_dir)):
            if f.endswith(".cs"):
                special_files[f[:-3]] = str(special_dir / f)
    
    # 检查 GuHouses 目录
    guhouses_files = {}
    guhouses_dir = BASE_DIR / "Content" / "Items" / "GuHouses"
    if guhouses_dir.exists():
        for f in os.listdir(str(guhouses_dir)):
            if f.endswith(".cs"):
                guhouses_files[f[:-3]] = str(guhouses_dir / f)
    
    matched_houses = []
    unmatched_houses = []
    
    for name, rank in sorted(novel_gu_houses.items()):
        found = False
        
        # 检查 Special 目录（中文文件名）
        if name in special_files:
            matched_houses.append((name, rank, "Special"))
            found = True
        
        # 检查 finish.db items
        if not found and name in finish_items:
            matched_houses.append((name, rank, "finish.db items"))
            found = True
        
        # 检查 cn_map
        if not found and name in cn_map:
            cn = cn_map[name]
            if cn in project_classes:
                matched_houses.append((name, rank, f"class:{cn}"))
                found = True
        
        if not found:
            unmatched_houses.append((name, rank))
    
    print(f"已匹配仙蛊屋: {len(matched_houses)}")
    print(f"未匹配仙蛊屋: {len(unmatched_houses)}")
    
    if unmatched_houses:
        print("\n未匹配仙蛊屋列表:")
        for name, rank in unmatched_houses:
            print(f"  {name}|{rank if rank else '未知'}")
    
    # ============ 输出总结 ============
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
    
    output_path = BASE_DIR / "tools" / "missing_entities_v2.json"
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
