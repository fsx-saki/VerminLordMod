#!/usr/bin/env python3
"""
蛊真人 · 小说知识库 Web 展示 (v3 — 故事线)
启动: python3 helps/novel_viewer.py  →  http://localhost:8080
"""
import json, os, yaml, http.server, urllib.parse, functools, re, sqlite3
from collections import defaultdict, OrderedDict

BASE = "/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/helps/novel_db"
DB = "/home/fsx/Documents/novel_analyzer/novel.db"
PORT = 8080

# ─── YAML 加载 ─────────────────────────────────────────────
yaml.add_representer(type(None), lambda d, v: d.represent_scalar('tag:yaml.org,2002:null', ''))

@functools.lru_cache(maxsize=1)
def load_arcs():
    arcs = []
    for f in sorted(os.listdir(os.path.join(BASE, "arcs"))):
        if f.endswith(".yaml"):
            with open(os.path.join(BASE, "arcs", f), encoding="utf-8") as fh:
                arcs.append(yaml.safe_load(fh))
    return arcs

@functools.lru_cache(maxsize=1)
def load_chapters():
    chapters = []
    for f in sorted(os.listdir(os.path.join(BASE, "chapters"))):
        if f.endswith(".yaml"):
            with open(os.path.join(BASE, "chapters", f), encoding="utf-8") as fh:
                chapters.append(yaml.safe_load(fh))
    return chapters

@functools.lru_cache(maxsize=1)
def load_entities(etype):
    fn = {"gu_worm": "gu_index.yaml", "person": "char_index.yaml", "location": "location_index.yaml"}
    f = fn.get(etype)
    if not f:
        return []
    with open(os.path.join(BASE, "entities", f), encoding="utf-8") as fh:
        data = yaml.safe_load(fh)
        key = {"gu_worm": "gu", "person": "characters", "location": "locations"}[etype]
        return data.get(key, [])

@functools.lru_cache(maxsize=1)
def load_map(mtype):
    fn = {"gu": "gu_to_chapters.yaml", "char": "char_to_chapters.yaml"}
    f = fn.get(mtype)
    if not f:
        return {}
    with open(os.path.join(BASE, "maps", f), encoding="utf-8") as fh:
        data = yaml.safe_load(fh)
        key = {"gu": "gu", "char": "characters"}[mtype]
        return data.get(key, {})

@functools.lru_cache(maxsize=1)
def load_meta():
    with open(os.path.join(BASE, "meta.yaml"), encoding="utf-8") as fh:
        return yaml.safe_load(fh)

# ─── 故事线引擎 ────────────────────────────────────────
# 事件类型分类关键词
EVENT_PATTERNS = [
    (r'(?:战斗|杀[害死]?|对[战峙]|交战|攻[击打]|轰|爆[炸裂]|破[坏灭])', '⚔️ 战斗'),
    (r'(?:炼[蛊制]|[炼培]化|炼成|合炼?|升炼)', '🔧 炼蛊'),
    (r'(?:修[炼行]?|突破|晋升|晋级|[提突]破|修为|境界)', '📈 修炼'),
    (r'(?:发[现觉]|找[到]?|获[得取]|得[到知]|寻[找到])', '🔍 发现'),
    (r'(?:交[易换]|买|卖|贸[易]?|购[买]?|换[取]?)', '🤝 交易'),
    (r'(?:说[道]?|问[道]?|[询疑]问|谈[话论]|对[话语]|告诉|[回解]答)', '💬 对话'),
    (r'(?:计划|谋划|[策阴]谋|[规筹]划|算计|[布部]局)', '🧠 谋划'),
    (r'(?:逃[跑脱走]|追[杀捕逐]|[奔飞]逃|流[窜亡])', '🏃 追逐'),
    (r'(?:死[亡去]?|[陨殒]落|[身战]死|[被击]杀|灭[亡杀]?)', '💀 死亡'),
    (r'(?:重生|转世|回[到归]|穿越|前[世生])', '🔄 重生'),
    (r'(?:开[启]?窍|资质|[点打]开.*窍)', '🔑 开窍'),
    (r'(?:谈[判]?|[协妥]商|[谈]?判|盟[约]?|联[盟姻]?)', '🤝 结盟'),
    (r'(?:秘[密]?|[隐遮]藏|潜[入伏]?|[埋隐]伏|[秘隐]密)', '🌙 隐秘'),
    (r'(?:破[解]?|解[开决]?|[化消]解|[攻突]破)', '🔓 破解'),
    (r'(?:救[援]?|[拯挽]救|[解]?救|帮[助]?|[援支]助)', '🆘 救援'),
    (r'(?:召[唤集]?|聚[集合]?|[号]?召|集[结合]?)', '📯 召集'),
    (r'(?:献[祭]?|祭[祀炼]?|[奉]?献|[牺]?牲)', '🕯️ 献祭'),
    (r'(?:叛[变逆]?|[背]?叛|[投]?敌|背[叛弃])', '⚡ 背叛'),
    (r'(?:控[制]?|[驾操]?纵|[掌]?控|奴[役]?)', '🔗 控制'),
    (r'(?:防[御]?|[守]?卫|[保]?护|[抵]?挡)', '🛡️ 防御'),
]

# 提取所有角色名（用于内容匹配）
@functools.lru_cache(maxsize=1)
def get_all_person_names():
    return sorted(set(e["name"] for e in load_entities("person") if e.get("name")))

@functools.lru_cache(maxsize=1)
def get_all_gu_names():
    return sorted(set(e["name"] for e in load_entities("gu_worm") if e.get("name")))

def extract_event_type(text):
    """Classify event by keyword patterns."""
    for pattern, label in EVENT_PATTERNS:
        if re.search(pattern, text):
            return label
    return '📖 叙述'

def extract_participants(text, person_names, gu_names):
    """Find entity names mentioned in text."""
    found_persons = []
    found_gu = []
    for name in person_names:
        if name and name in text:
            found_persons.append(name)
    for name in gu_names:
        if name and len(name) > 1 and name in text:
            found_gu.append(name)
    return found_persons[:8], found_gu[:8]

def summarize_text(text, max_len=100):
    """Extract a readable summary from text."""
    # Take the first meaningful sentence
    text = text.strip()
    # Remove leading spaces/indentation
    text = re.sub(r'^\s*', '', text)
    # Find first sentence end
    for sep in ['。', '！', '？', '……', '!\n', '?\n']:
        idx = text.find(sep)
        if idx > 5:
            return text[:idx+1][:max_len]
    return text[:max_len]

@functools.lru_cache(maxsize=1)
def build_storyline():
    """Build structured storyline from all data."""
    conn = sqlite3.connect(DB)
    conn.row_factory = sqlite3.Row
    
    # Load arcs
    arcs = load_arcs()
    arc_by_ch = {}
    for a in arcs:
        for c in range(a["chapter_range"][0], a["chapter_range"][1] + 1):
            arc_by_ch[c] = a
    
    # Get all sections ordered chronologically
    sections = conn.execute(
        "SELECT id, section_num, title, content FROM sections ORDER BY id"
    ).fetchall()
    
    # Pre-load entity data per chunk (all chunks)
    chunk_entities = {}
    for row in conn.execute(
        "SELECT d.chunk_id, e.name, e.type, d.source_text, d.content "
        "FROM entity_details d JOIN entities e ON e.id = d.entity_id "
        "WHERE d.chunk_id > 0"
    ).fetchall():
        cid = row["chunk_id"]
        if cid not in chunk_entities:
            chunk_entities[cid] = {"gu": set(), "persons": set(), "events": set()}
        if row["type"] == "gu_worm":
            chunk_entities[cid]["gu"].add(row["name"])
        elif row["type"] == "person":
            chunk_entities[cid]["persons"].add(row["name"])
        # Prefer content (has context text) over source_text (often just name)
        src = (row["content"] or row["source_text"] or "").strip()
        if src and len(src) > 15:
            chunk_entities[cid]["events"].add(src)
    
    conn.close()
    
    # Load entity name lists
    person_names = get_all_person_names()
    gu_names = get_all_gu_names()
    
    # Build storyline entries
    storyline = []
    
    for sec in sections:
        sec_id = sec["id"]
        chunk_id = sec_id - 2335
        ch = sec["section_num"]
        title = sec["title"] or ""
        content = sec["content"] or ""
        
        arc_info = arc_by_ch.get(ch, {})
        
        # Get entity data for this chunk
        ent_data = chunk_entities.get(chunk_id, {"gu": set(), "persons": set(), "events": set()})
        
        # Determine event type
        combined_text = title + " " + content[:200]
        event_type = extract_event_type(combined_text)
        
        # Generate story summary
        if ent_data["events"]:
            # Use the most descriptive source_text as the story beat
            best_event = max(ent_data["events"], key=len)
            story_summary = best_event[:150]
        else:
            # Extract from content
            story_summary = summarize_text(content)
        
        # Find participants from content if no entity data
        content_persons, content_gu = [], []
        if not ent_data["persons"] and not ent_data["gu"]:
            content_persons, content_gu = extract_participants(
                content[:300], person_names, gu_names
            )
        
        participants = list(ent_data["persons"])[:8] or content_persons[:8]
        gu_involved = list(ent_data["gu"])[:5] or content_gu[:5]
        
        # Determine narrative stage within arc
        arc_ch_start = arc_info.get("chapter_range", [ch, ch])[0] if arc_info else ch
        arc_ch_end = arc_info.get("chapter_range", [ch, ch])[1] if arc_info else ch
        arc_len = max(1, arc_ch_end - arc_ch_start)
        pos = (ch - arc_ch_start) / arc_len if arc_len > 0 else 0
        
        if pos < 0.15:
            stage = "开端"
        elif pos < 0.45:
            stage = "发展"
        elif pos < 0.65:
            stage = "转折"
        elif pos < 0.85:
            stage = "高潮"
        else:
            stage = "结局"
        
        storyline.append({
            "chunk_id": chunk_id,
            "chapter": ch,
            "title": title,
            "summary": story_summary,
            "event_type": event_type,
            "stage": stage,
            "participants": participants,
            "gu_involved": gu_involved,
            "arc_id": arc_info.get("id", ""),
            "arc_title": arc_info.get("title", ""),
            "arc_rank": arc_info.get("rank", ""),
            "has_data": bool(ent_data["events"]),
        })
    
    return storyline

# ─── HTTP 处理 ─────────────────────────────────────────────
class Handler(http.server.BaseHTTPRequestHandler):
    def log_message(self, *a):
        pass

    def _json(self, data):
        self.send_response(200)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(json.dumps(data, ensure_ascii=False, default=str).encode("utf-8"))

    def _html(self, content):
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.end_headers()
        self.wfile.write(content.encode("utf-8"))

    def _error(self, code, msg):
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.end_headers()
        self.wfile.write(json.dumps({"error": msg}, ensure_ascii=False).encode("utf-8"))

    def do_GET(self):
        parsed = urllib.parse.urlparse(self.path)
        path = parsed.path.rstrip("/")
        qs = dict(urllib.parse.parse_qsl(parsed.query))
        
        # ── API ──
        if path == "/api/stats":
            meta = load_meta()
            arcs = load_arcs()
            chs = load_chapters()
            gu = load_entities("gu_worm")
            ch = load_entities("person")
            loc = load_entities("location")
            gu_map = load_map("gu")
            ch_map = load_map("char")
            tl = build_storyline()
            self._json({
                "meta": meta,
                "arcs": len(arcs),
                "chapters": len(chs),
                "total_entries": sum(len(c.get("entries", [])) for c in chs),
                "entities": {"gu_worm": len(gu), "person": len(ch), "location": len(loc)},
                "gu_with_chapters": len(gu_map),
                "chars_with_chapters": len(ch_map),
                "storyline_entries": len(tl),
                "storyline_with_events": sum(1 for t in tl if t["has_data"]),
            })
        
        elif path == "/api/arcs":
            self._json(load_arcs())
        
        elif path.startswith("/api/arcs/"):
            aid = path[10:]
            arcs = load_arcs()
            arc = next((a for a in arcs if a.get("id") == aid), None)
            if not arc:
                return self._error(404, f"Arc {aid} not found")
            chs = load_chapters()
            arc_chs = [c for c in chs if c.get("arc_id") == aid]
            arc["chapters"] = arc_chs
            self._json(arc)
        
        elif path == "/api/chapters":
            chs = load_chapters()
            summary = []
            for c in chs:
                cr = c.get("chapter_range", [0, 0])
                summary.append({"range": cr, "arc_id": c.get("arc_id", ""), "entry_count": len(c.get("entries", [])),
                    "has_data": any(e.get("gu_mentioned") or e.get("char_appear") for e in c.get("entries", []))})
            self._json(summary)
        
        elif path.startswith("/api/chapter/"):
            cr = path[13:]
            chs = load_chapters()
            ch = next((c for c in chs if c.get("chapter_range") and 
                       f"ch{c['chapter_range'][0]:03d}-{c['chapter_range'][1]:03d}" == cr), None)
            if not ch:
                return self._error(404, f"Chapter range {cr} not found")
            self._json(ch)
        
        elif path == "/api/storyline":
            """Storyline with pagination and filtering."""
            page = int(qs.get("page", "1"))
            per_page = int(qs.get("per_page", "20"))
            arc_filter = qs.get("arc", "")
            stage_filter = qs.get("stage", "")
            event_filter = qs.get("event_type", "")
            search = qs.get("q", "").lower()
            
            tl = build_storyline()
            
            if arc_filter:
                tl = [t for t in tl if t["arc_id"] == arc_filter]
            if stage_filter:
                tl = [t for t in tl if t["stage"] == stage_filter]
            if event_filter:
                tl = [t for t in tl if event_filter in t["event_type"]]
            if search:
                tl = [t for t in tl if search in t["title"].lower() or search in t["summary"].lower()
                      or any(search in p.lower() for p in t["participants"])]
            
            total = len(tl)
            total_pages = max(1, (total + per_page - 1) // per_page)
            page = min(page, total_pages)
            start = (page - 1) * per_page
            end = start + per_page
            
            self._json({
                "total": total, "page": page, "total_pages": total_pages,
                "per_page": per_page, "entries": tl[start:end],
            })
        
        elif path == "/api/storyline/arcs":
            """Aggregated storyline by arc."""
            tl = build_storyline()
            arcs = load_arcs()
            result = []
            for a in arcs:
                arc_entries = [t for t in tl if t["arc_id"] == a["id"]]
                if not arc_entries:
                    continue
                # Count event types
                event_types = defaultdict(int)
                stages = defaultdict(int)
                for t in arc_entries:
                    event_types[t["event_type"]] += 1
                    stages[t["stage"]] += 1
                result.append({
                    "id": a["id"],
                    "title": a["title"],
                    "rank": a.get("rank", ""),
                    "chapter_range": a["chapter_range"],
                    "total_sections": len(arc_entries),
                    "with_data": sum(1 for t in arc_entries if t["has_data"]),
                    "event_types": dict(event_types),
                    "stages": dict(stages),
                    "participants": list(set(
                        p for t in arc_entries for p in t["participants"]
                    )),
                })
            self._json(result)
        
        elif path == "/api/entities":
            etype = qs.get("type", "gu_worm")
            entities = load_entities(etype)
            compact = [{"name": e.get("name",""), "rank": e.get("rank",""),
                        "summary": (e.get("summary") or "")[:80],
                        "aliases": e.get("aliases", [])[:3]} for e in entities]
            self._json(compact)
        
        elif path.startswith("/api/entity/"):
            ename = urllib.parse.unquote(path[12:])
            for etype in ("gu_worm", "person", "location"):
                for e in load_entities(etype):
                    if e.get("name") == ename:
                        self._json(e)
                        return
            self._error(404, f"Entity '{ename}' not found")
        
        elif path == "/api/maps":
            mtype = qs.get("type", "gu")
            m = load_map(mtype)
            compact = {}
            for name, chapters in sorted(m.items()):
                compact[name] = {"count": len(chapters), "range": [min(chapters), max(chapters)]}
            self._json(compact)
        
        elif path == "/api/search":
            q = qs.get("q", "").lower()
            if not q:
                return self._json({"results": []})
            results = []
            for etype, label in [("gu_worm", "蛊虫"), ("person", "角色"), ("location", "地点")]:
                for e in load_entities(etype):
                    name = e.get("name", "")
                    summary = e.get("summary", "") or ""
                    if q in name.lower() or q in summary.lower():
                        results.append({"type": label, "name": name, "summary": summary[:100], "rank": e.get("rank", "")})
            self._json({"results": results[:200]})
        
        # ── HTML ──
        elif path == "/" or path == "":
            self._html(HTML_TEMPLATE)
        else:
            self._error(404, "Not found")

# ─── HTML ──────────────────────────────────────────────────
HTML_TEMPLATE = r"""<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>蛊真人 · 知识库</title>
<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: "Noto Sans SC", "Microsoft YaHei", sans-serif; background: #0d1117; color: #c9d1d9; min-height: 100vh; }
.header { background: #161b22; border-bottom: 1px solid #30363d; padding: 16px 24px; display: flex; align-items: center; gap: 16px; }
.header h1 { font-size: 20px; color: #f0f6fc; }
.header .sub { color: #8b949e; font-size: 13px; }
.header .ver { margin-left: auto; font-size: 12px; color: #484f58; }
.tabs { display: flex; background: #161b22; border-bottom: 1px solid #30363d; padding: 0 16px; gap: 0; overflow-x: auto; }
.tab { padding: 10px 20px; cursor: pointer; color: #8b949e; font-size: 14px; border-bottom: 2px solid transparent; transition: all .15s; user-select: none; white-space: nowrap; }
.tab:hover { color: #c9d1d9; background: #1c2128; }
.tab.active { color: #f0f6fc; border-bottom-color: #f78166; font-weight: 600; }
.content { padding: 20px 24px; max-width: 1400px; margin: 0 auto; }
.panel { display: none; }
.panel.active { display: block; }
/* Stats */
.stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 24px; }
.stat-card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; padding: 16px; }
.stat-card .num { font-size: 28px; font-weight: 700; color: #f0f6fc; }
.stat-card .label { font-size: 12px; color: #8b949e; margin-top: 4px; }
.stat-card .sub { font-size: 11px; color: #484f58; margin-top: 2px; }
.card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; margin-bottom: 12px; overflow: hidden; }
.card-header { padding: 12px 16px; cursor: pointer; display: flex; align-items: center; gap: 12px; }
.card-header:hover { background: #1c2128; }
.card-header .arrow { transition: transform .2s; color: #8b949e; font-size: 12px; }
.card-header.expanded .arrow { transform: rotate(90deg); }
.card-header .title { font-weight: 600; flex: 1; }
.card-body { padding: 12px 16px; border-top: 1px solid #21262d; display: none; }
.card-body.open { display: block; }
.loading { text-align: center; padding: 40px; color: #484f58; }
/* Storyline */
.sl-arc { margin-bottom: 28px; }
.sl-arc-header { display: flex; align-items: center; gap: 12px; padding: 12px 16px; background: #161b22; border: 1px solid #30363d; border-radius: 8px; margin-bottom: 12px; cursor: pointer; }
.sl-arc-header:hover { background: #1c2128; }
.sl-arc-header .sl-rank { font-size: 12px; padding: 2px 10px; border-radius: 10px; background: #f7816622; color: #f78166; border: 1px solid #f7816644; }
.sl-arc-header .sl-title { flex: 1; font-weight: 700; font-size: 16px; color: #f0f6fc; }
.sl-arc-header .sl-stat { font-size: 12px; color: #8b949e; }
.sl-arc-body { padding-left: 8px; border-left: 2px solid #30363d; margin-left: 16px; }
/* Storyline events */
.sl-event { position: relative; padding: 10px 16px 10px 36px; margin-bottom: 4px; border-radius: 6px; cursor: pointer; transition: all .15s; }
.sl-event:hover { background: #1c2128; }
.sl-event .sl-dot { position: absolute; left: 10px; top: 14px; width: 10px; height: 10px; border-radius: 50%; border: 2px solid #30363d; background: #0d1117; }
.sl-event.has-data .sl-dot { border-color: #58a6ff; background: #1f6feb33; }
.sl-event .sl-chapter { font-size: 11px; color: #58a6ff; font-weight: 600; min-width: 55px; display: inline-block; }
.sl-event .sl-title { font-size: 14px; color: #f0f6fc; font-weight: 600; }
.sl-event .sl-summary { font-size: 12px; color: #8b949e; line-height: 1.5; margin-top: 3px; }
.sl-event .sl-type { font-size: 10px; padding: 1px 6px; border-radius: 8px; margin-left: 4px; }
.sl-event .sl-stage { font-size: 10px; padding: 1px 6px; border-radius: 8px; background: #21262d; color: #8b949e; margin-left: 4px; }
.sl-event .sl-participants { font-size: 11px; color: #484f58; margin-top: 2px; }
.sl-event .sl-participants span { display: inline-block; padding: 0 4px; margin: 1px; border-radius: 3px; }
.sl-event .sl-participants .person { color: #58a6ff; background: #58a6ff11; }
.sl-event .sl-participants .gu { color: #f78166; background: #f7816611; }
/* Stage badges */
.stage-开端 { border-left: 3px solid #238636; }
.stage-发展 { border-left: 3px solid #58a6ff; }
.stage-转折 { border-left: 3px solid #d29922; }
.stage-高潮 { border-left: 3px solid #f78166; }
.stage-结局 { border-left: 3px solid #bc8cff; }
/* Controls */
.sl-controls { display: flex; gap: 8px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.sl-controls select, .sl-controls input { padding: 6px 10px; background: #0d1117; border: 1px solid #30363d; border-radius: 6px; color: #c9d1d9; font-size: 13px; }
.sl-controls select:focus, .sl-controls input:focus { border-color: #58a6ff; outline: none; }
.sl-count { color: #8b949e; font-size: 13px; margin-left: auto; }
.sl-pagination { display: flex; gap: 4px; justify-content: center; margin-top: 16px; }
.sl-pg { padding: 4px 10px; border: 1px solid #30363d; border-radius: 4px; background: #0d1117; color: #c9d1d9; cursor: pointer; font-size: 13px; }
.sl-pg:hover { background: #1c2128; }
.sl-pg.active { background: #1f6feb; border-color: #1f6feb; }
.sl-pg:disabled { opacity: .3; cursor: default; }
/* Arc overview cards */
.arc-ov { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 12px; margin-bottom: 20px; }
.arc-ov-card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; padding: 14px; cursor: pointer; }
.arc-ov-card:hover { border-color: #58a6ff; }
.arc-ov-card .a-title { font-weight: 700; color: #f0f6fc; }
.arc-ov-card .a-rank { font-size: 11px; color: #f78166; }
.arc-ov-card .a-ch { font-size: 12px; color: #8b949e; }
.arc-ov-card .a-stat { font-size: 11px; color: #484f58; margin-top: 4px; }
.arc-ov-card .a-bar { margin-top: 6px; height: 4px; background: #21262d; border-radius: 2px; overflow: hidden; display: flex; }
.arc-ov-card .a-bar div { height: 100%; }
.arc-ov-card .a-bar .bar-on { background: #3fb950; }
.arc-ov-card .a-bar .bar-off { background: #21262d; }
/* Detail modal */
.modal { display: none; position: fixed; inset: 0; background: #00000088; z-index: 100; align-items: center; justify-content: center; }
.modal.open { display: flex; }
.modal-content { background: #161b22; border: 1px solid #30363d; border-radius: 12px; max-width: 650px; max-height: 80vh; width: 90%; overflow-y: auto; padding: 24px; }
.modal-content h2 { margin-bottom: 8px; }
.modal-content .close { float: right; cursor: pointer; color: #8b949e; font-size: 24px; }
.modal-content .close:hover { color: #f0f6fc; }
::-webkit-scrollbar { width: 8px; }
::-webkit-scrollbar-track { background: #0d1117; }
::-webkit-scrollbar-thumb { background: #30363d; border-radius: 4px; }
</style>
</head>
<body>

<div class="header">
  <h1>🪲 蛊真人 · 知识库</h1>
  <span class="sub">Storyline Explorer</span>
  <span class="ver" id="version"></span>
</div>

<div class="tabs">
  <div class="tab active" onclick="switchTab('overview')">概览</div>
  <div class="tab" onclick="switchTab('storyline')">📖 故事线</div>
  <div class="tab" onclick="switchTab('timeline')">📅 时间线</div>
  <div class="tab" onclick="switchTab('entities')">实体</div>
  <div class="tab" onclick="switchTab('maps')">映射</div>
  <div class="tab" onclick="switchTab('search')">搜索</div>
</div>

<div class="content">
  <div id="panel-overview" class="panel active"></div>
  <div id="panel-storyline" class="panel"></div>
  <div id="panel-timeline" class="panel"></div>
  <div id="panel-entities" class="panel"></div>
  <div id="panel-maps" class="panel"></div>
  <div id="panel-search" class="panel"></div>
</div>

<div class="modal" id="entityModal"><div class="modal-content"><span class="close" onclick="closeModal()">&times;</span><div id="modalBody"></div></div></div>

<script>
// ─── State ────────────────────────────────────────────
const state = {};
async function api(p) { const r = await fetch(p); return r.json(); }

function switchTab(name) {
  document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
  document.querySelector(`.tab[onclick*="${name}"]`).classList.add('active');
  const el = document.getElementById(`panel-${name}`);
  el.classList.add('active');
  if (name === 'overview' && !el.innerHTML.trim()) renderOverview();
  if (name === 'storyline') renderStoryline();
  if (name === 'timeline') renderTimeline();
  if (name === 'entities' && !el.innerHTML.trim()) renderEntities();
  if (name === 'maps') renderMaps();
}

// ─── Overview ─────────────────────────────────────────
async function renderOverview() {
  const el = document.getElementById('panel-overview');
  el.innerHTML = '<div class="loading">加载中...</div>';
  const s = await api('/api/stats');
  const meta = s.meta || {};
  document.getElementById('version').textContent = `v${meta.version || '?'} · ${meta.last_updated || ''}`;
  
  el.innerHTML = `
    <div class="stats-grid">
      <div class="stat-card"><div class="num">${s.storyline_entries}</div><div class="label">故事条条目</div><div class="sub">${s.storyline_with_events} 条有详细事件</div></div>
      <div class="stat-card"><div class="num">${s.arcs}</div><div class="label">篇章</div><div class="sub">覆盖 ${meta.chapters_covered || '?'}</div></div>
      <div class="stat-card"><div class="num">${s.entities.gu_worm}</div><div class="label">蛊虫</div><div class="sub">${s.gu_with_chapters} 只有章节映射</div></div>
      <div class="stat-card"><div class="num">${s.entities.person}</div><div class="label">角色</div><div class="sub">${s.chars_with_chapters} 个有章节映射</div></div>
      <div class="stat-card"><div class="num">${s.entities.location}</div><div class="label">地点</div></div>
      <div class="stat-card"><div class="num">${s.storyline_with_events}</div><div class="label">有数据节</div><div class="sub">共 ${s.storyline_entries} 节</div></div>
    </div>
  `;
  
  // Load arc overview
  const arcs = await api('/api/storyline/arcs');
  el.innerHTML += `<div class="arc-ov">${arcs.map(a => {
    const total = a.total_sections;
    const on = a.with_data;
    const pct = total > 0 ? (on/total*100).toFixed(0) : 0;
    const types = Object.entries(a.event_types).sort((x,y) => y[1]-x[1]).slice(0,4);
    return `<div class="arc-ov-card" onclick="switchTab('storyline')">
      <div class="a-title">${a.title} <span class="a-rank">${a.rank}</span></div>
      <div class="a-ch">第${a.chapter_range[0]}-${a.chapter_range[1]}章 · ${total} 节</div>
      <div class="a-bar"><div class="bar-on" style="width:${pct}%"></div><div class="bar-off" style="width:${100-pct}%"></div></div>
      <div class="a-stat">${on}/${total} 节有数据 (${pct}%) · ${types.map(t => t[0].replace(/^./,'')+t[1]).join(' · ')}</div>
    </div>`;
  }).join('')}</div>`;
}

// ─── Storyline ────────────────────────────────────────
let slState = { page: 1, arc: '', stage: '', event_type: '', search: '', per_page: 30 };

async function renderStoryline() {
  const el = document.getElementById('panel-storyline');
  el.innerHTML = `
    <div class="sl-controls">
      <select id="slArc" onchange="slState.arc=this.value;slState.page=1;loadStoryline()"><option value="">全部篇章</option></select>
      <select onchange="slState.stage=this.value;slState.page=1;loadStoryline()"><option value="">全部阶段</option><option value="开端">开端</option><option value="发展">发展</option><option value="转折">转折</option><option value="高潮">高潮</option><option value="结局">结局</option></select>
      <select onchange="slState.event_type=this.value;slState.page=1;loadStoryline()"><option value="">全部类型</option></select>
      <input id="slSearch" placeholder="搜索故事线..." style="width:160px;" onkeyup="if(event.key==='Enter'){slState.search=this.value;slState.page=1;loadStoryline()}">
      <button class="tab-btn" style="padding:4px 12px;" onclick="slState.search=document.getElementById('slSearch').value;slState.page=1;loadStoryline()">搜索</button>
      <span class="sl-count" id="slCount">加载中...</span>
    </div>
    <div id="slList"></div>
    <div class="sl-pagination" id="slPagination"></div>
  `;
  
  const arcs = await api('/api/arcs');
  const sel = document.getElementById('slArc');
  arcs.forEach(a => { const o = document.createElement('option'); o.value = a.id; o.textContent = `${a.title}`; sel.appendChild(o); });
  
  // Build event type filter from first page
  loadStoryline();
}

async function loadStoryline() {
  const el = document.getElementById('slList');
  if (!el) return;
  el.innerHTML = '<div class="loading">分析故事线...</div>';
  
  const p = new URLSearchParams({page: slState.page, per_page: slState.per_page});
  if (slState.arc) p.set('arc', slState.arc);
  if (slState.stage) p.set('stage', slState.stage);
  if (slState.event_type) p.set('event_type', slState.event_type);
  if (slState.search) p.set('q', slState.search);
  
  const data = await api(`/api/storyline?${p}`);
  document.getElementById('slCount').textContent = `共 ${data.total} 条 · 第 ${data.page}/${data.total_pages} 页`;
  
  // Group by arc for display
  const grouped = {};
  for (const e of data.entries) {
    const key = e.arc_id || 'unknown';
    if (!grouped[key]) grouped[key] = {title: e.arc_title || '未知', rank: e.arc_rank || '', entries: []};
    grouped[key].entries.push(e);
  }
  
  el.innerHTML = Object.entries(grouped).map(([aid, arc]) => `
    <div class="sl-arc">
      <div class="sl-arc-header" onclick="toggleArc(this)">
        <span class="sl-rank">${arc.rank}</span>
        <span class="sl-title">${arc.title}</span>
        <span class="sl-stat">${arc.entries.length} 个事件</span>
        <span class="arrow" style="color:#8b949e;font-size:12px;">▼</span>
      </div>
      <div class="sl-arc-body">
        ${arc.entries.map(e => renderSLEvent(e)).join('')}
      </div>
    </div>
  `).join('');
  
  // Pagination
  const pg = document.getElementById('slPagination');
  if (data.total_pages <= 1) { pg.innerHTML = ''; return; }
  let html = '';
  const pp = data.page, tp = data.total_pages;
  if (pp > 1) html += `<button class="sl-pg" onclick="slState.page=1;loadStoryline()">«</button><button class="sl-pg" onclick="slState.page=${pp-1};loadStoryline()">‹</button>`;
  for (let i = Math.max(1, p-2); i <= Math.min(tp, p+2); i++)
    html += `<button class="sl-pg ${i===p?'active':''}" onclick="slState.page=${i};loadStoryline()">${i}</button>`;
  if (pp < tp) html += `<button class="sl-pg" onclick="slState.page=${pp+1};loadStoryline()">›</button><button class="sl-pg" onclick="slState.page=${tp};loadStoryline()">»</button>`;
  pg.innerHTML = html;
}

function renderSLEvent(e) {
  const has = e.has_data ? 'has-data' : '';
  const stage = e.stage || '';
  const emoji = e.event_type.match(/^./)?.[0] || '📖';
  const label = e.event_type.replace(/^./, '');
  const parts = (e.participants || []).slice(0, 6);
  const gu = (e.gu_involved || []).slice(0, 4);
  return `<div class="sl-event ${has} stage-${stage}" onclick="showEventDetail(e)">
    <div class="sl-dot"></div>
    <div>
      <span class="sl-chapter">第${e.chapter}章</span>
      <span class="sl-type" style="background:#21262d;color:#8b949e;">${e.event_type}</span>
      <span class="sl-stage">${stage}</span>
    </div>
    <div class="sl-title">${e.title}</div>
    <div class="sl-summary">${e.summary}</div>
    ${parts.length ? `<div class="sl-participants">角色: ${parts.map(n => `<span class="person">${n}</span>`).join('')}</div>` : ''}
    ${gu.length ? `<div class="sl-participants">蛊虫: ${gu.map(n => `<span class="gu">${n}</span>`).join('')}</div>` : ''}
  </div>`;
}

function toggleArc(header) {
  const body = header.nextElementSibling;
  const arrow = header.querySelector('.arrow');
  if (body.style.display === 'none') {
    body.style.display = 'block';
    arrow.textContent = '▼';
  } else {
    body.style.display = 'none';
    arrow.textContent = '▶';
  }
}

// ─── Timeline (same as before, shown minimally) ───
let tlState = { page: 1, arc: '', search: '', per_page: 20 };

async function renderTimeline() {
  const el = document.getElementById('panel-timeline');
  el.innerHTML = `
    <div class="sl-controls">
      <select id="tlArc" onchange="tlState.arc=this.value;tlState.page=1;loadTimeline()"><option value="">全部篇章</option></select>
      <input id="tlSearch" placeholder="搜索..." style="width:160px;" onkeyup="if(event.key==='Enter'){tlState.search=this.value;tlState.page=1;loadTimeline()}">
      <button class="tab-btn" style="padding:4px 12px;" onclick="tlState.search=document.getElementById('tlSearch').value;tlState.page=1;loadTimeline()">搜索</button>
      <span class="sl-count" id="tlCount">加载中...</span>
    </div>
    <div id="tlList"></div>
    <div class="sl-pagination" id="tlPagination"></div>
  `;
  const arcs = await api('/api/arcs');
  const sel = document.getElementById('tlArc');
  arcs.forEach(a => { const o = document.createElement('option'); o.value = a.id; o.textContent = `${a.title}`; sel.appendChild(o); });
  loadTimeline();
}

async function loadTimeline() {
  const el = document.getElementById('tlList');
  if (!el) return;
  el.innerHTML = '<div class="loading">加载中...</div>';
  const p = new URLSearchParams({page: tlState.page, per_page: tlState.per_page});
  if (tlState.arc) p.set('arc', tlState.arc);
  if (tlState.search) p.set('q', tlState.search);
  const data = await api(`/api/storyline?${p}`);
  document.getElementById('tlCount').textContent = `共 ${data.total} 条 · 第 ${data.page}/${data.total_pages} 页`;
  el.innerHTML = data.entries.map(e => {
    const has = e.has_data ? 'has-data' : '';
    const parts = (e.participants || []).slice(0,5);
    return `<div class="sl-event ${has}" style="border-left:3px solid #30363d;">
      <div class="sl-dot"></div>
      <div><span class="sl-chapter">第${e.chapter}章</span><span class="sl-type" style="background:#21262d;color:#8b949e;">${e.event_type}</span></div>
      <div class="sl-title">${e.title}</div>
      <div class="sl-summary">${e.summary}</div>
      ${parts.length ? `<div class="sl-participants">${parts.map(n => `<span class="person">${n}</span>`).join('')}</div>` : ''}
    </div>`;
  }).join('');
  
  const pg = document.getElementById('tlPagination');
  if (data.total_pages <= 1) { pg.innerHTML = ''; return; }
  let html = '';
  const pp = data.page, tp = data.total_pages;
  if (pp > 1) html += `<button class="sl-pg" onclick="tlState.page=1;loadTimeline()">«</button><button class="sl-pg" onclick="tlState.page=${pp-1};loadTimeline()">‹</button>`;
  for (let i = Math.max(1, p-2); i <= Math.min(tp, p+2); i++)
    html += `<button class="sl-pg ${i===p?'active':''}" onclick="tlState.page=${i};loadTimeline()">${i}</button>`;
  if (pp < tp) html += `<button class="sl-pg" onclick="tlState.page=${pp+1};loadTimeline()">›</button><button class="sl-pg" onclick="tlState.page=${tp};loadTimeline()">»</button>`;
  pg.innerHTML = html;
}

// ─── Entities & Maps & Search ──────────────────────
async function renderEntities() {
  const el = document.getElementById('panel-entities');
  el.innerHTML = `<div class="tab-bar" id="entTabs"></div><input id="entSearch" style="width:100%;padding:8px 12px;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#c9d1d9;font-size:14px;margin-bottom:12px;" placeholder="搜索..." oninput="filterEntities()"><div id="entityGrid" class="entity-list"><div class="loading">加载中...</div></div>`;
  await loadEntityData('gu_worm');
}
async function loadEntityData(type) {
  const grid = document.getElementById('entityGrid');
  const data = await api(`/api/entities?type=${type}`);
  grid.innerHTML = data.map(e => `<div class="entity-item" onclick="showEntityDetail('${e.name}')"><div class="name">${e.name}</div><div class="meta">${e.rank||''} ${(e.aliases||[]).length?'又名:'+e.aliases.slice(0,2).join('、'):''}</div><div class="summary">${e.summary||''}</div></div>`).join('');
}

async function showEntityDetail(name) {
  const data = await api(`/api/entity/${encodeURIComponent(name)}`);
  if (!data || data.error) { showModal(`<p>未找到: ${name}</p>`); return; }
  let detailsHtml = '';
  if (data.details) for (const [k,v] of Object.entries(data.details)) if(v&&v.length) detailsHtml += `<div class="detail-group"><h4>${k}</h4>${v.map(t=>`<p>${t}</p>`).join('')}</div>`;
  showModal(`<h2>${data.name}</h2><div style="color:#8b949e;font-size:13px;margin-bottom:12px;">${data.rank?`品级: ${data.rank}`:''}${(data.aliases||[]).length?` · 别名: ${data.aliases.join('、')}`:''}${data.first_appear_title?`<div>首次: ${data.first_appear_title}</div>`:''}</div>${data.summary?`<div style="font-size:14px;padding:8px 12px;background:#0d1117;border-radius:6px;margin-bottom:12px;">${data.summary}</div>`:''}${detailsHtml||'<div style="color:#484f58;">暂无详细数据</div>'}`);
}

async function renderMaps() {
  const el = document.getElementById('panel-maps');
  el.innerHTML = `<div class="tab-bar"><button class="tab-btn active" onclick="loadMapData('gu',this)">蛊→章节</button><button class="tab-btn" onclick="loadMapData('char',this)">角色→章节</button></div><input id="mapSearch" style="width:100%;padding:8px 12px;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#c9d1d9;font-size:14px;margin-bottom:12px;" placeholder="搜索..." oninput="filterMaps()"><div id="mapGrid" class="map-grid"><div class="loading">加载中...</div></div>`;
  await loadMapData('gu', document.querySelector('.tab-btn'));
}
async function loadMapData(type, btn) {
  if (btn) { document.querySelectorAll('#panel-maps .tab-btn').forEach(b=>b.classList.remove('active')); btn.classList.add('active'); }
  const grid = document.getElementById('mapGrid');
  const data = await api(`/api/maps?type=${type}`);
  const entries = Object.entries(data).sort((a,b)=>b[1].count-a[1].count);
  const max = entries.length ? entries[0][1].count : 1;
  grid.innerHTML = entries.map(([n,i]) => `<div class="map-item"><div class="m-name">${n}</div><div class="m-chapters">出现 ${i.count} 章 (${i.range[0]}-${i.range[1]})</div><div class="m-bar"><div class="m-bar-fill" style="width:${(i.count/max*100).toFixed(1)}%"></div></div></div>`).join('');
}
function filterMaps() { const q=document.getElementById('mapSearch').value.toLowerCase(); document.querySelectorAll('.map-item').forEach(e=>e.style.display=e.textContent.toLowerCase().includes(q)?'':'block'); }
function filterEntities() { const q=document.getElementById('entSearch').value.toLowerCase(); document.querySelectorAll('.entity-item').forEach(e=>e.style.display=e.textContent.toLowerCase().includes(q)?'':'block'); }

// ─── Modal ────────────────────────────────────────────
function showModal(html) { document.getElementById('modalBody').innerHTML = html; document.getElementById('entityModal').classList.add('open'); }
function closeModal() { document.getElementById('entityModal').classList.remove('open'); }
const modalEl = document.getElementById('entityModal'); modalEl.addEventListener('click', function(e) { if (e.target === this) closeModal(); });
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeModal(); });

// ─── Init ─────────────────────────────────────────────
renderOverview();
</script>
</body>
</html>
"""

# ─── 启动 ──────────────────────────────────────────────────
if __name__ == "__main__":
    print(f"\n  🪲 蛊真人 · 知识库 v3 — 故事线引擎")
    print(f"  {'='*40}")
    print(f"  启动地址: http://localhost:{PORT}")
    print(f"  按 Ctrl+C 停止\n")
    server = http.server.HTTPServer(("0.0.0.0", PORT), Handler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n  已停止")
        server.server_close()
