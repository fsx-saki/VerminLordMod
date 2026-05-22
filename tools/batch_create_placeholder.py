#!/usr/bin/env python3
"""
批量创建缺失蛊虫和仙蛊屋的占位实现文件。
使用 pypinyin 将中文名转为拼音类名（首字母大写驼峰式）。
自动跳过已在 Weapons/ 目录有完整实现或在 Special/ 目录已存在的文件。
"""

import sqlite3
import os
import re
import json
from pathlib import Path
from pypinyin import pinyin, Style

BASE_DIR = Path("/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod")

RANK_RARITY = {
    '九转': 'ItemRarityID.Purple',
    '八转': 'ItemRarityID.Cyan',
    '七转': 'ItemRarityID.Lime',
    '六转': 'ItemRarityID.LightPurple',
    '五转': 'ItemRarityID.Pink',
    '四转': 'ItemRarityID.LightRed',
    '三转': 'ItemRarityID.Orange',
    '二转': 'ItemRarityID.Green',
    '一转': 'ItemRarityID.Blue',
    '零转': 'ItemRarityID.White',
    '仙蛊(未知转)': 'ItemRarityID.LightPurple',
    '传说/概念级': 'ItemRarityID.Red',
    '未知': 'ItemRarityID.White',
}

RANK_VALUE = {
    '九转': 5000000, '八转': 1000000, '七转': 500000,
    '六转': 100000, '五转': 50000, '四转': 20000,
    '三转': 10000, '二转': 5000, '一转': 1000,
    '零转': 500, '仙蛊(未知转)': 100000,
    '传说/概念级': 10000000, '未知': 1000,
}

# 手动修正映射：某些中文名转拼音后需要特殊处理
MANUAL_OVERRIDES = {
    '行': 'Xing', '重': 'Chong', '长': 'Chang', '乐': 'Le',
    '了': 'Le', '藏': 'Zang', '朝': 'Chao', '传': 'Chuan',
    '斗': 'Dou', '都': 'Du', '度': 'Du', '发': 'Fa',
    '分': 'Fen', '干': 'Gan', '更': 'Geng', '还': 'Huan',
    '间': 'Jian', '将': 'Jiang', '觉': 'Jue', '空': 'Kong',
    '奇': 'Qi', '强': 'Qiang', '曲': 'Qu', '数': 'Shu',
    '为': 'Wei', '相': 'Xiang', '兴': 'Xing', '应': 'Ying',
    '与': 'Yu', '着': 'Zhe', '正': 'Zheng', '只': 'Zhi',
    '中': 'Zhong', '种': 'Zhong', '转': 'Zhuan', '量': 'Liang',
    '率': 'Lv', '落': 'Luo', '调': 'Diao', '弹': 'Dan',
    '石': 'Shi', '薄': 'Bo', '恶': 'E', '解': 'Jie',
    '系': 'Xi', '校': 'Xiao', '说': 'Shuo', '宿': 'Su',
    '血': 'Xue', '叶': 'Ye', '遗': 'Yi', '殷': 'Yin',
    '员': 'Yuan', '曾': 'Zeng', '占': 'Zhan', '涨': 'Zhang',
    '折': 'Zhe', '挣': 'Zheng', '殖': 'Zhi', '著': 'Zhu',
    '钻': 'Zuan', '作': 'Zuo',
}


def chinese_to_pascal_case(name):
    """将中文名转为首字母大写驼峰式拼音类名"""
    py_list = pinyin(name, style=Style.NORMAL)
    parts = []
    for char, py in zip(name, py_list):
        p = py[0].capitalize()
        if char in MANUAL_OVERRIDES:
            p = MANUAL_OVERRIDES[char]
        parts.append(p)
    return ''.join(parts)


def classify_rank(rank_str):
    rank_str = rank_str.strip()
    if not rank_str or rank_str in ('未知', '未明确'):
        return '未知'
    if '九转' in rank_str: return '九转'
    if '八转' in rank_str: return '八转'
    if '七转' in rank_str: return '七转'
    if '六转' in rank_str: return '六转'
    if '五转' in rank_str: return '五转'
    if '四转' in rank_str: return '四转'
    if '三转' in rank_str: return '三转'
    if '二转' in rank_str: return '二转'
    if '一转' in rank_str: return '一转'
    if '零转' in rank_str or '凡蛊' in rank_str or '凡级' in rank_str: return '零转'
    if '仙蛊' in rank_str or '仙级' in rank_str: return '仙蛊(未知转)'
    if '传说' in rank_str or '概念' in rank_str: return '传说/概念级'
    return '未知'


def is_gu_house(name):
    keywords = ['仙蛊屋', '屋', '宫', '殿', '阁', '楼', '台', '亭', '坛', '城', '堡', '庙', '馆', '园', '池', '塔']
    for kw in keywords:
        if kw in name:
            return True
    return False


def get_weapon_classes():
    """收集 Weapons 目录下所有已实现的蛊虫类名"""
    weapons_dir = BASE_DIR / "Content" / "Items" / "Weapons"
    classes = set()
    if weapons_dir.exists():
        for rank_dir in sorted(weapons_dir.iterdir()):
            if rank_dir.is_dir() and rank_dir.name not in ('Daos',):
                for f in rank_dir.glob('*.cs'):
                    classes.add(f.stem)
    return classes


def get_special_classes():
    """收集 Special 目录下所有已存在的类名"""
    special_dir = BASE_DIR / "Content" / "Items" / "Special"
    classes = set()
    if special_dir.exists():
        for f in special_dir.glob('*.cs'):
            classes.add(f.stem)
    return classes


def create_placeholder_file(chinese_name, rank_str, output_dir):
    """创建占位实现文件，使用拼音类名"""
    rank = classify_rank(rank_str)
    rarity = RANK_RARITY.get(rank, 'ItemRarityID.White')
    value = RANK_VALUE.get(rank, 1000)
    class_name = chinese_to_pascal_case(chinese_name)
    namespace = "VerminLordMod.Content.Items.Special"

    content = f'''using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace {namespace}
{{
    /// <summary>
    /// 特殊物品 — {chinese_name}
    /// {rank_str}
    /// </summary>
    public class {class_name} : ModItem
    {{
        public override void SetDefaults()
        {{
            Item.width = 24;
            Item.height = 24;
            Item.rare = {rarity};
            Item.maxStack = 1;
            Item.value = {value};
        }}
    }}
}}
'''
    filepath = output_dir / f"{class_name}.cs"
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    return filepath, class_name


def main():
    # 读取分析结果
    json_path = BASE_DIR / "tools" / "missing_entities_v3.json"
    with open(str(json_path), 'r', encoding='utf-8') as f:
        data = json.load(f)

    output_dir = BASE_DIR / "Content" / "Items" / "Special"
    output_dir.mkdir(parents=True, exist_ok=True)

    # 收集已存在的实现
    weapon_classes = get_weapon_classes()
    special_classes = get_special_classes()

    total_created = 0
    skipped_weapons = 0
    skipped_special = 0
    name_mapping = {}

    # 处理蛊虫
    for cat, items in data["unmatched_gu_worms"].items():
        for chinese_name, rank_str in items:
            class_name = chinese_to_pascal_case(chinese_name)

            # 检查是否已在 Weapons 中有完整实现
            if class_name in weapon_classes:
                skipped_weapons += 1
                continue

            # 检查是否已在 Special 中存在
            if class_name in special_classes:
                skipped_special += 1
                continue

            create_placeholder_file(chinese_name, rank_str, output_dir)
            name_mapping[chinese_name] = class_name
            total_created += 1
            if total_created <= 5 or total_created % 50 == 0:
                print(f"  [+] {chinese_name} -> {class_name} ({rank_str})")

    # 处理仙蛊屋
    for chinese_name, rank_str in data["unmatched_gu_houses"]:
        class_name = chinese_to_pascal_case(chinese_name)

        if class_name in weapon_classes:
            skipped_weapons += 1
            continue

        if class_name in special_classes:
            skipped_special += 1
            continue

        create_placeholder_file(chinese_name, rank_str, output_dir)
        name_mapping[chinese_name] = class_name
        total_created += 1
        print(f"  [+] {chinese_name} -> {class_name} ({rank_str}) [仙蛊屋]")

    # 保存名称映射
    mapping_path = BASE_DIR / "tools" / "placeholder_name_mapping.json"
    with open(str(mapping_path), 'w', encoding='utf-8') as f:
        json.dump(name_mapping, f, ensure_ascii=False, indent=2)

    print(f"\n完成！")
    print(f"  创建: {total_created}")
    print(f"  跳过(已在Weapons中): {skipped_weapons}")
    print(f"  跳过(已在Special中): {skipped_special}")
    print(f"  目录: {output_dir}")
    print(f"  名称映射已保存到: {mapping_path}")


if __name__ == "__main__":
    main()
