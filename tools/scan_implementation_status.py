#!/usr/bin/env python3
"""
蛊界 Mod 实现状态扫描器
========================
扫描 Content/ 目录下所有 .cs 文件，分析每个类的实现状态，
生成：
1. 初始的 [ImplStatus] 属性标记（可选择性写入文件）
2. 实现状态报告 Markdown 文档
3. 待办清单 Markdown 文档

用法:
    python tools/scan_implementation_status.py [--write-attributes] [--output-dir helps]

选项:
    --write-attributes   将推断的 [ImplStatus] 属性写入 .cs 文件
    --output-dir DIR     输出目录（默认 helps/）
"""

import os
import re
import sys
import argparse
from pathlib import Path
from datetime import datetime
from collections import defaultdict

# 项目根目录
PROJECT_ROOT = Path(__file__).resolve().parent.parent
CONTENT_DIR = PROJECT_ROOT / "Content"
HELPS_DIR = PROJECT_ROOT / "helps"

# ============================================================
# 启发式规则：判断一个类是否是"占位实现"
# ============================================================

def is_placeholder(content: str, filepath: str) -> tuple:
    """
    分析 .cs 文件内容，判断是否为占位实现。
    返回 (is_placeholder: bool, reason: str)
    """
    filename = filepath.name

    # 规则 1: 只有贴图没有 .cs 文件（这种情况不会到这里）
    
    # 规则 2: 类体非常短（< 20 行有效代码）
    lines = [l for l in content.split('\n') if l.strip() and not l.strip().startswith('//') and not l.strip().startswith('*')]
    code_lines = [l for l in lines if not l.strip().startswith('using ') and not l.strip().startswith('namespace ')]
    
    # 规则 3: 只有 SetDefaults() 没有其他方法
    has_only_setdefaults = bool(re.search(r'override void SetDefaults', content)) and not bool(re.search(r'override bool\s+\w+', content)) and not bool(re.search(r'override void \w+(?<!SetDefaults)', content))
    
    # 规则 4: UseItem 只有简单的 ConsumeQi + QuickSpawnItem 模式
    simple_useitem = False
    useitem_match = re.search(r'override bool\?\s+UseItem.*?\{.*?\}', content, re.DOTALL)
    if useitem_match:
        useitem_body = useitem_match.group(0)
        # 检查是否只有 ConsumeQi + QuickSpawnItem/AddBuff 等简单操作
        has_complex_logic = any(kw in useitem_body for kw in [
            'for ', 'while ', 'foreach ', 'switch ', 'if.*else.*{.*}',
            'Main.NewText', 'Projectile.NewProjectile', 'NPC.NewNPC',
            'CombatText', 'Dust.NewDust', 'Gore.NewGore'
        ])
        simple_useitem = not has_complex_logic and len(useitem_body.split('\n')) < 15

    # 规则 5: 类名包含 Base、Test、Debug 等
    is_base_or_test = any(kw in filename for kw in ['Base', 'Test', 'Debug', 'DaggerTest'])

    # 规则 6: 文件大小非常小
    file_size = os.path.getsize(filepath)
    is_tiny = file_size < 800

    reasons = []
    score = 0

    if is_base_or_test:
        reasons.append("基类/测试类")
        # 基类不算占位，它们是基础设施
        return False, "基类/测试类"

    if len(code_lines) < 15:
        score += 2
        reasons.append(f"代码行数少({len(code_lines)}行)")

    if has_only_setdefaults:
        score += 3
        reasons.append("仅有SetDefaults")

    if simple_useitem:
        score += 2
        reasons.append("UseItem逻辑简单")

    if is_tiny:
        score += 1
        reasons.append(f"文件小({file_size}B)")

    # 判断是否为占位
    is_placeholder = score >= 3
    reason = "; ".join(reasons) if reasons else "正常实现"
    
    return is_placeholder, reason


def infer_category(filepath: Path) -> str:
    """从文件路径推断分类"""
    rel_path = filepath.relative_to(CONTENT_DIR)
    parts = rel_path.parts
    
    if len(parts) == 0:
        return "Unknown"
    
    # Content/Items/Weapons/One/XXX.cs -> Items.Weapons.One
    # Content/NPCs/Commoners/XXX.cs -> NPCs.Commoners
    # Content/Buffs/AddToSelf/Pobuff/XXX.cs -> Buffs.AddToSelf.Pobuff
    # Content/Projectiles/XXX.cs -> Projectiles
    # Content/Tiles/Cave/XXX.cs -> Tiles.Cave
    
    # 去掉文件名
    dir_parts = list(parts[:-1])
    
    # 特殊处理：如果目录名是数字（转数），保留
    turn_names = {
        'Zero': '零转', 'One': '一转', 'Two': '二转', 'Three': '三转',
        'Four': '四转', 'Five': '五转', 'Six': '六转', 'Test': '测试'
    }
    
    category_parts = []
    for p in dir_parts:
        if p in turn_names:
            category_parts.append(turn_names[p])
        else:
            category_parts.append(p)
    
    return '.'.join(category_parts)


def infer_turn(filepath: Path) -> str:
    """从文件路径推断转数"""
    turn_map = {
        'Zero': '零转', 'One': '一转', 'Two': '二转', 'Three': '三转',
        'Four': '四转', 'Five': '五转', 'Six': '六转', 'Test': '测试'
    }
    for part in filepath.parts:
        if part in turn_map:
            return turn_map[part]
    return ""


def infer_dao_type(content: str) -> str:
    """从类继承的基类推断 Dao 类型"""
    dao_map = {
        'WaterWeapon': '水', 'FireWeapon': '火', 'WoodWeapon': '木',
        'StarWeapon': '星', 'MoonWeapon': '月', 'DarkWeapon': '暗',
        'LightWeapon': '光', 'IceSnowWeapon': '冰雪', 'WindWeapon': '风',
        'GoldWeapon': '金', 'BoneWeapon': '骨', 'BloodWeapon': '血',
        'SoulWeapon': '魂', 'DreamWeapon': '梦', 'CloudWeapon': '云',
        'PoisonWeapon': '毒', 'LightningWeapon': '雷', 'MudWeapon': '土',
        'EatingWeapon': '食', 'QiWeapon': '气', 'SpaceWeapon': '空间',
        'TimeWeapon': '时间', 'WisdomWeapon': '智', 'RuleWeapon': '律',
        'LoveWeapon': '爱', 'LuckWeapon': '运', 'CharmWeapon': '媚',
        'ShadowWeapon': '影', 'SwordWeapon': '剑', 'KnifeWeapon': '刀',
        'WarWeapon': '战', 'KillingWeapon': '杀', 'PowerWeapon': '力',
        'InfoWeapon': '信息', 'VoiceWeapon': '音', 'DrawWeapon': '画',
        'SkyWeapon': '天', 'SlaveWeapon': '奴', 'StealingWeapon': '偷',
        'TacticalWeapon': '战术', 'UnrealWeapon': '虚幻', 'VariationWeapon': '变化',
        'VoidWeapon': '虚空', 'YinYangWeapon': '阴阳', 'LifeDeathWeapon': '生死',
        'PractiseWeapon': '修炼', 'PersonWeapon': '人', 'PelletWeapon': '丸',
        'BanWeapon': 'ban', 'FlyingWeapon': '飞',
    }
    
    for base_class, dao in dao_map.items():
        if base_class in content:
            return dao
    return ""


def get_class_name(content: str) -> str:
    """从文件内容提取类名"""
    match = re.search(r'(class|struct)\s+(\w+)', content)
    return match.group(2) if match else ""


def get_base_class(content: str) -> str:
    """从文件内容提取基类"""
    match = re.search(r'class\s+\w+\s*:\s*(\w+)', content)
    return match.group(1) if match else ""


def scan_directory(directory: Path) -> list:
    """扫描目录下所有 .cs 文件"""
    results = []
    
    for filepath in sorted(directory.rglob("*.cs")):
        # 跳过 bin/ obj/ 目录
        if any(p in filepath.parts for p in ['bin', 'obj', 'Properties']):
            continue
        
        rel_path = filepath.relative_to(PROJECT_ROOT)
        
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception as e:
            print(f"  无法读取 {filepath}: {e}")
            continue
        
        class_name = get_class_name(content)
        if not class_name:
            continue
        
        # 跳过接口和枚举
        if 'interface ' in content or 'enum ' in content:
            continue
        
        base_class = get_base_class(content)
        category = infer_category(filepath)
        turn = infer_turn(filepath)
        dao_type = infer_dao_type(content)
        
        is_ph, reason = is_placeholder(content, filepath)
        
        # 检查是否有贴图文件
        png_path = filepath.with_suffix('.png')
        has_texture = png_path.exists()
        
        # 检查是否有 _Head.png, _Party.png, _Wings.png 等变体
        texture_variants = []
        if has_texture:
            texture_variants.append(str(png_path.name))
        # 检查其他常见贴图变体
        stem = filepath.stem
        for variant in ['_Head.png', '_Party.png', '_Wings.png']:
            variant_path = filepath.parent / f"{stem}{variant}"
            if variant_path.exists():
                texture_variants.append(variant_path.name)
        
        results.append({
            'filepath': str(rel_path),
            'class_name': class_name,
            'base_class': base_class,
            'category': category,
            'turn': turn,
            'dao_type': dao_type,
            'is_placeholder': is_ph,
            'reason': reason,
            'has_texture': has_texture,
            'texture_variants': texture_variants,
            'content': content,
        })
    
    return results


def generate_impl_status_attribute(entry: dict) -> str:
    """生成 [ImplStatus] 属性标记文本"""
    status = "ImplStatus.Placeholder" if entry['is_placeholder'] else "ImplStatus.Implemented"
    note = entry['reason'].replace('"', "'")
    turn = entry['turn']
    dao = entry['dao_type']
    
    parts = [f"ImplStatusAttribute({status}"]
    if note:
        parts.append(f'note = "{note}"')
    if turn:
        parts.append(f'plannedTurn = "{turn}"')
    if dao:
        parts.append(f'daoType = "{dao}"')
    
    return f"[{', '.join(parts)})]"


def write_attributes_to_files(results: list):
    """将 [ImplStatus] 属性写入 .cs 文件"""
    written = 0
    skipped = 0
    
    for entry in results:
        filepath = PROJECT_ROOT / entry['filepath']
        content = entry['content']
        
        # 检查是否已有 [ImplStatus] 属性
        if '[ImplStatus' in content or '[ImplStatusAttribute' in content:
            skipped += 1
            continue
        
        attr_text = generate_impl_status_attribute(entry)
        
        # 在 namespace 声明之前插入属性
        # 找到 class 声明行
        lines = content.split('\n')
        insert_idx = -1
        for i, line in enumerate(lines):
            if re.match(r'\s*(class|struct)\s+', line):
                insert_idx = i
                break
        
        if insert_idx >= 0:
            # 在 class 行之前插入属性
            indent = '    '  # 4 spaces
            lines.insert(insert_idx, f"{indent}{attr_text}")
            new_content = '\n'.join(lines)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)
            written += 1
            print(f"  ✓ 已写入 {filepath.name}")
    
    return written, skipped


def generate_report(results: list) -> str:
    """生成实现状态报告 Markdown"""
    lines = []
    lines.append("# 蛊界 Mod 实现状态报告")
    lines.append("")
    lines.append(f"> 自动生成时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    lines.append("")
    lines.append("## 总体统计")
    lines.append("")
    
    total = len(results)
    placeholders = [r for r in results if r['is_placeholder']]
    implemented = [r for r in results if not r['is_placeholder']]
    
    lines.append(f"| 指标 | 数值 |")
    lines.append(f"|------|------|")
    lines.append(f"| 总扫描对象 | {total} |")
    lines.append(f"| ✅ 完整/正常实现 | {len(implemented)} |")
    lines.append(f"| 🚧 占位实现 | {len(placeholders)} |")
    lines.append(f"| 📊 完成率 | {len(implemented)/total*100:.1f}% |" if total > 0 else "| 📊 完成率 | N/A |")
    lines.append("")
    
    # 按分类统计
    lines.append("## 按分类统计")
    lines.append("")
    lines.append("| 分类 | 总数 | ✅ 完成 | 🚧 占位 | 完成率 |")
    lines.append("|------|------|--------|---------|--------|")
    
    by_category = defaultdict(list)
    for r in results:
        by_category[r['category']].append(r)
    
    for cat in sorted(by_category.keys()):
        entries = by_category[cat]
        cat_total = len(entries)
        cat_done = len([e for e in entries if not e['is_placeholder']])
        cat_ph = len([e for e in entries if e['is_placeholder']])
        rate = cat_done / cat_total * 100 if cat_total > 0 else 0
        lines.append(f"| {cat} | {cat_total} | {cat_done} | {cat_ph} | {rate:.1f}% |")
    
    lines.append("")
    
    # 按转数统计
    lines.append("## 按转数统计（蛊虫）")
    lines.append("")
    lines.append("| 转数 | 总数 | ✅ 完成 | 🚧 占位 | 完成率 |")
    lines.append("|------|------|--------|---------|--------|")
    
    by_turn = defaultdict(list)
    for r in results:
        if r['turn']:
            by_turn[r['turn']].append(r)
    
    for turn in sorted(by_turn.keys()):
        entries = by_turn[turn]
        t_total = len(entries)
        t_done = len([e for e in entries if not e['is_placeholder']])
        t_ph = len([e for e in entries if e['is_placeholder']])
        rate = t_done / t_total * 100 if t_total > 0 else 0
        lines.append(f"| {turn} | {t_total} | {t_done} | {t_ph} | {rate:.1f}% |")
    
    lines.append("")
    
    # 占位实现清单
    lines.append("## 🚧 占位实现清单（待优化）")
    lines.append("")
    lines.append("| 类名 | 文件路径 | 分类 | 转数 | Dao | 基类 | 原因 | 贴图 |")
    lines.append("|------|----------|------|------|-----|------|------|------|")
    
    for r in sorted(placeholders, key=lambda x: (x['category'], x['class_name'])):
        texture_mark = "✅" if r['has_texture'] else "❌"
        lines.append(f"| {r['class_name']} | `{r['filepath']}` | {r['category']} | {r['turn']} | {r['dao_type']} | {r['base_class']} | {r['reason']} | {texture_mark} |")
    
    lines.append("")
    
    # 完整实现清单
    lines.append("## ✅ 完整/正常实现清单")
    lines.append("")
    lines.append("| 类名 | 文件路径 | 分类 | 转数 | Dao | 基类 | 贴图 |")
    lines.append("|------|----------|------|------|-----|------|------|")
    
    for r in sorted(implemented, key=lambda x: (x['category'], x['class_name'])):
        texture_mark = "✅" if r['has_texture'] else "❌"
        lines.append(f"| {r['class_name']} | `{r['filepath']}` | {r['category']} | {r['turn']} | {r['dao_type']} | {r['base_class']} | {texture_mark} |")
    
    return '\n'.join(lines)


def generate_todo_list(results: list) -> str:
    """生成待办清单 Markdown"""
    lines = []
    lines.append("# 蛊界 Mod 实现待办清单")
    lines.append("")
    lines.append(f"> 更新日期: {datetime.now().strftime('%Y-%m-%d')}")
    lines.append("")
    
    total = len(results)
    placeholders = [r for r in results if r['is_placeholder']]
    implemented = [r for r in results if not r['is_placeholder']]
    rate = len(implemented) / total * 100 if total > 0 else 0
    
    lines.append(f"**总进度**: {len(implemented)}/{total} ({rate:.1f}%)")
    lines.append("")
    lines.append(f"**待优化**: {len(placeholders)} 个占位实现")
    lines.append("")
    
    # 按分类列出待办
    by_category = defaultdict(list)
    for r in placeholders:
        by_category[r['category']].append(r)
    
    for cat in sorted(by_category.keys()):
        entries = by_category[cat]
        lines.append(f"### {cat}（{len(entries)} 个）")
        lines.append("")
        
        for r in sorted(entries, key=lambda x: x['class_name']):
            turn_info = f" [{r['turn']}]" if r['turn'] else ""
            dao_info = f" ({r['dao_type']})" if r['dao_type'] else ""
            note_info = f" — {r['reason']}" if r['reason'] else ""
            lines.append(f"- [ ] {r['class_name']}{turn_info}{dao_info}{note_info}")
        
        lines.append("")
    
    return '\n'.join(lines)


def main():
    parser = argparse.ArgumentParser(description="扫描 Content 目录下的实现状态")
    parser.add_argument('--write-attributes', action='store_true',
                        help='将推断的 [ImplStatus] 属性写入 .cs 文件')
    parser.add_argument('--output-dir', type=str, default='helps',
                        help='输出目录（默认 helps/）')
    args = parser.parse_args()
    
    output_dir = PROJECT_ROOT / args.output_dir
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print("=" * 60)
    print("  蛊界 Mod 实现状态扫描器")
    print("=" * 60)
    print()
    
    print(f"扫描目录: {CONTENT_DIR}")
    print()
    
    results = scan_directory(CONTENT_DIR)
    
    total = len(results)
    placeholders = [r for r in results if r['is_placeholder']]
    implemented = [r for r in results if not r['is_placeholder']]
    
    print(f"扫描完成: 共发现 {total} 个类")
    print(f"  ✅ 完整/正常实现: {len(implemented)}")
    print(f"  🚧 占位实现: {len(placeholders)}")
    print(f"  📊 完成率: {len(implemented)/total*100:.1f}%" if total > 0 else "")
    print()
    
    # 生成报告
    report = generate_report(results)
    report_path = output_dir / "implementation_status.md"
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report)
    print(f"📄 报告已生成: {report_path}")
    
    # 生成待办清单
    todo = generate_todo_list(results)
    todo_path = output_dir / "implementation_todo.md"
    with open(todo_path, 'w', encoding='utf-8') as f:
        f.write(todo)
    print(f"📋 待办清单已生成: {todo_path}")
    
    # 可选：写入属性
    if args.write_attributes:
        print()
        print("正在写入 [ImplStatus] 属性...")
        written, skipped = write_attributes_to_files(results)
        print(f"  已写入: {written} 个文件")
        print(f"  已跳过（已有属性）: {skipped} 个文件")
    
    print()
    print("完成!")


if __name__ == '__main__':
    main()
