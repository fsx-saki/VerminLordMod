#!/usr/bin/env python3
"""
蛊真人 · 小说知识库 Web 展示 (v2 — 带时间线)
启动: python3 /tmp/novel_viewer.py
访问: http://localhost:8080
"""
import json, os, yaml, http.server, urllib.parse, functools, re

BASE = "/home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/helps/novel_db"
PORT = 8080

# ─── YAML 加载 ─────────────────────────────────────────────
yaml.add_representer(type(None), lambda d, v: d.represent_scalar('tag:yaml.org,2002:null', ''))
yaml.SafeDumper.ignore_aliases = lambda *a: True

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

# ─── 时间线数据生成 ─────────────────────────────────────
@functools.lru_cache(maxsize=1)
def build_timeline():
    """Generate chronological timeline from all available data."""
    chapters = load_chapters()
    arcs = load_arcs()
    
    # Build arc lookup by chapter number
    arc_by_ch = {}
    for a in arcs:
        for c in range(a["chapter_range"][0], a["chapter_range"][1] + 1):
            arc_by_ch[c] = {
                "id": a["id"],
                "title": a["title"],
                "rank": a.get("rank", ""),
            }
    
    # Build entity first appearance lookup
    entity_first = {}  # chapter_number -> list of (name, type)
    for etype, label in [("gu_worm", "gu_worm"), ("person", "person"), ("location", "location")]:
        ents = load_entities(etype)
        for e in ents:
            # Get first appearance from gu/char_to_chapters maps
            m = load_map("gu" if etype == "gu_worm" else "char")
            chs = m.get(e.get("name", ""), [])
            if chs:
                first_ch = chs[0]
                if first_ch not in entity_first:
                    entity_first[first_ch] = []
                entity_first[first_ch].append({
                    "name": e.get("name", ""),
                    "type": label,
                    "rank": e.get("rank", ""),
                })
    
    # Build timeline entries per chapter
    timeline = []
    for ch_file in chapters:
        for entry in ch_file.get("entries", []):
            ch = entry.get("chapter", 0)
            if not ch:
                continue
            
            arc_info = arc_by_ch.get(ch, {})
            
            # Get first appearances in this chapter
            first_apps = entity_first.get(ch, [])
            
            # Categorize entities
            gu_list = entry.get("gu_mentioned", [])
            char_list = entry.get("char_appear", [])
            events = entry.get("events", [])
            
            has_data = bool(gu_list or char_list or events)
            
            timeline.append({
                "chapter": ch,
                "title": entry.get("title", ""),
                "content": (entry.get("content") or "")[:150],
                "arc_id": arc_info.get("id", ""),
                "arc_title": arc_info.get("title", ""),
                "rank": arc_info.get("rank", ""),
                "has_data": has_data,
                "gu_count": len(gu_list),
                "char_count": len(char_list),
                "event_count": len(events),
                "gu_mentioned": gu_list[:8],  # preview
                "char_appear": char_list[:8],
                "events": events[:5],
                "first_appearances": first_apps[:5],
            })
    
    return timeline

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
            tl = build_timeline()
            self._json({
                "meta": meta,
                "arcs": len(arcs),
                "chapters": len(chs),
                "total_entries": sum(len(c.get("entries", [])) for c in chs),
                "entities": {"gu_worm": len(gu), "person": len(ch), "location": len(loc)},
                "gu_with_chapters": len(gu_map),
                "chars_with_chapters": len(ch_map),
                "entity_details_coverage": meta.get("data_completion", {}).get("chapters_with_entity_data", 0),
                "timeline_entries": len(tl),
                "timeline_with_events": sum(1 for t in tl if t["has_data"]),
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
                summary.append({
                    "range": cr,
                    "arc_id": c.get("arc_id", ""),
                    "entry_count": len(c.get("entries", [])),
                    "has_data": any(e.get("gu_mentioned") or e.get("char_appear") for e in c.get("entries", [])),
                })
            self._json(summary)
        
        elif path.startswith("/api/chapter/"):
            cr = path[13:]
            chs = load_chapters()
            ch = next((c for c in chs if c.get("chapter_range") and 
                       f"ch{c['chapter_range'][0]:03d}-{c['chapter_range'][1]:03d}" == cr), None)
            if not ch:
                return self._error(404, f"Chapter range {cr} not found")
            self._json(ch)
        
        elif path == "/api/timeline":
            """Timeline with pagination and filtering."""
            page = int(qs.get("page", "1"))
            per_page = int(qs.get("per_page", "30"))
            arc_filter = qs.get("arc", "")
            has_data_filter = qs.get("has_data", "")  # "true", "false", or ""
            search = qs.get("q", "").lower()
            
            tl = build_timeline()
            
            # Apply filters
            if arc_filter:
                tl = [t for t in tl if t["arc_id"] == arc_filter]
            if has_data_filter == "true":
                tl = [t for t in tl if t["has_data"]]
            elif has_data_filter == "false":
                tl = [t for t in tl if not t["has_data"]]
            if search:
                tl = [t for t in tl if search in t["title"].lower() or 
                      search in t["arc_title"].lower() or
                      any(search in g for g in t["gu_mentioned"]) or
                      any(search in c for c in t["char_appear"]) or
                      any(search in e for e in t["events"])]
            
            total = len(tl)
            total_pages = max(1, (total + per_page - 1) // per_page)
            page = min(page, total_pages)
            start = (page - 1) * per_page
            end = start + per_page
            page_data = tl[start:end]
            
            self._json({
                "total": total,
                "page": page,
                "total_pages": total_pages,
                "per_page": per_page,
                "entries": page_data,
            })
        
        elif path == "/api/entities":
            etype = qs.get("type", "gu_worm")
            entities = load_entities(etype)
            compact = []
            for e in entities:
                compact.append({
                    "name": e.get("name", ""),
                    "rank": e.get("rank", ""),
                    "summary": (e.get("summary") or "")[:80],
                    "aliases": e.get("aliases", [])[:3],
                })
            self._json(compact)
        
        elif path.startswith("/api/entity/"):
            ename = urllib.parse.unquote(path[12:])
            for etype in ("gu_worm", "person", "location"):
                entities = load_entities(etype)
                for e in entities:
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
                        results.append({
                            "type": label, "name": name,
                            "summary": summary[:100],
                            "rank": e.get("rank", ""),
                        })
            for a in load_arcs():
                if q in a.get("title", "").lower():
                    results.append({"type": "篇章", "name": a.get("title"), "summary": (a.get("summary") or "")[:100]})
            for c in load_chapters():
                for e in c.get("entries", []):
                    for ev in e.get("events", []):
                        if q in ev.lower():
                            results.append({"type": "事件", "name": f"第{e['chapter']}章 · {(e.get('title') or '')[:30]}", "summary": ev[:120]})
                            break
                    if len(results) > 200:
                        break
                if len(results) > 200:
                    break
            self._json({"results": results[:200]})
        
        # ── HTML ──
        elif path == "/" or path == "":
            self._html(HTML_TEMPLATE)
        else:
            self._error(404, "Not found")

# ─── HTML 模板 ─────────────────────────────────────────────
HTML_TEMPLATE = r"""<!DOCTYPE html>
<html lang="zh-CN">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>蛊真人 · 知识库</title>
<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: "Noto Sans SC", "Microsoft YaHei", sans-serif; background: #0d1117; color: #c9d1d9; min-height: 100vh; }
a { color: #58a6ff; text-decoration: none; }
/* Header */
.header { background: #161b22; border-bottom: 1px solid #30363d; padding: 16px 24px; display: flex; align-items: center; gap: 16px; }
.header h1 { font-size: 20px; color: #f0f6fc; }
.header .sub { color: #8b949e; font-size: 13px; }
.header .ver { margin-left: auto; font-size: 12px; color: #484f58; }
/* Tabs */
.tabs { display: flex; background: #161b22; border-bottom: 1px solid #30363d; padding: 0 16px; gap: 0; overflow-x: auto; }
.tab { padding: 10px 20px; cursor: pointer; color: #8b949e; font-size: 14px; border-bottom: 2px solid transparent; transition: all .15s; user-select: none; white-space: nowrap; }
.tab:hover { color: #c9d1d9; background: #1c2128; }
.tab.active { color: #f0f6fc; border-bottom-color: #f78166; font-weight: 600; }
/* Content */
.content { padding: 20px 24px; max-width: 1400px; margin: 0 auto; }
.panel { display: none; }
.panel.active { display: block; }
/* Stats */
.stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 24px; }
.stat-card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; padding: 16px; }
.stat-card .num { font-size: 28px; font-weight: 700; color: #f0f6fc; }
.stat-card .label { font-size: 12px; color: #8b949e; margin-top: 4px; }
.stat-card .sub { font-size: 11px; color: #484f58; margin-top: 2px; }
/* Cards */
.card { background: #161b22; border: 1px solid #30363d; border-radius: 8px; margin-bottom: 12px; overflow: hidden; }
.card-header { padding: 12px 16px; cursor: pointer; display: flex; align-items: center; gap: 12px; }
.card-header:hover { background: #1c2128; }
.card-header .arrow { transition: transform .2s; color: #8b949e; font-size: 12px; }
.card-header.expanded .arrow { transform: rotate(90deg); }
.card-header .title { font-weight: 600; flex: 1; }
.card-header .badge { font-size: 11px; padding: 2px 8px; border-radius: 10px; background: #21262d; color: #8b949e; }
.card-header .badge.blue { background: #1f6feb22; color: #58a6ff; border: 1px solid #1f6feb44; }
.card-header .badge.green { background: #23863622; color: #3fb950; border: 1px solid #23863644; }
.card-header .badge.orange { background: #d2992222; color: #d29922; border: 1px solid #d2992244; }
.card-body { padding: 12px 16px; border-top: 1px solid #21262d; display: none; }
.card-body.open { display: block; }
/* Entity list */
.entity-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 8px; }
.entity-item { background: #0d1117; border: 1px solid #21262d; border-radius: 6px; padding: 10px 12px; cursor: pointer; transition: all .15s; }
.entity-item:hover { border-color: #30363d; background: #161b22; }
.entity-item .name { color: #f0f6fc; font-weight: 600; font-size: 14px; }
.entity-item .meta { font-size: 11px; color: #8b949e; margin-top: 2px; }
.entity-item .summary { font-size: 12px; color: #484f58; margin-top: 4px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
/* Modal */
.modal { display: none; position: fixed; inset: 0; background: #00000088; z-index: 100; align-items: center; justify-content: center; }
.modal.open { display: flex; }
.modal-content { background: #161b22; border: 1px solid #30363d; border-radius: 12px; max-width: 700px; max-height: 80vh; width: 90%; overflow-y: auto; padding: 24px; }
.modal-content h2 { margin-bottom: 8px; }
.modal-content .close { float: right; cursor: pointer; color: #8b949e; font-size: 24px; }
.modal-content .close:hover { color: #f0f6fc; }
.modal-content .detail-group { margin: 12px 0; }
.modal-content .detail-group h4 { color: #58a6ff; font-size: 13px; margin-bottom: 4px; }
.modal-content .detail-group p { font-size: 13px; line-height: 1.6; color: #c9d1d9; padding: 4px 8px; background: #0d1117; border-radius: 4px; margin: 2px 0; }
/* Search */
.search-box { margin-bottom: 16px; display: flex; gap: 8px; }
.search-box input { flex: 1; padding: 8px 12px; background: #0d1117; border: 1px solid #30363d; border-radius: 6px; color: #c9d1d9; font-size: 14px; }
.search-box input:focus { border-color: #58a6ff; outline: none; }
.search-box button { padding: 8px 20px; background: #238636; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; }
.search-box button:hover { background: #2ea043; }
.search-result { padding: 8px 12px; border-bottom: 1px solid #21262d; cursor: pointer; }
.search-result:hover { background: #1c2128; }
.search-result .s-type { font-size: 10px; padding: 1px 6px; border-radius: 8px; background: #21262d; color: #8b949e; }
.search-result .s-name { font-weight: 600; color: #f0f6fc; }
.search-result .s-summary { font-size: 12px; color: #484f58; }
/* Loading */
.loading { text-align: center; padding: 40px; color: #484f58; }
/* Arc list */
.arc-row { display: flex; align-items: center; gap: 12px; padding: 10px 16px; border-bottom: 1px solid #21262d; cursor: pointer; }
.arc-row:hover { background: #1c2128; }
.arc-row .rank { font-size: 11px; padding: 2px 8px; border-radius: 10px; background: #21262d; color: #f78166; }
.arc-row .arc-title { flex: 1; font-weight: 600; }
.arc-row .arc-ch { font-size: 12px; color: #8b949e; }
.arc-row .arc-stat { font-size: 12px; color: #484f58; }
/* Chapter entry */
.ch-entry { padding: 8px 12px; border-bottom: 1px solid #0d1117; }
.ch-entry .ch-num { display: inline-block; min-width: 60px; font-weight: 600; color: #58a6ff; font-size: 13px; }
.ch-entry .ch-title { font-size: 13px; }
.ch-entry .ch-tags { display: flex; gap: 4px; flex-wrap: wrap; margin-top: 4px; }
.ch-entry .ch-tag { font-size: 10px; padding: 1px 6px; border-radius: 8px; background: #21262d; color: #8b949e; }
/* Timeline */
.tl-controls { display: flex; gap: 8px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.tl-controls select, .tl-controls input { padding: 6px 10px; background: #0d1117; border: 1px solid #30363d; border-radius: 6px; color: #c9d1d9; font-size: 13px; }
.tl-controls select:focus, .tl-controls input:focus { border-color: #58a6ff; outline: none; }
.tl-controls .tl-count { color: #8b949e; font-size: 13px; margin-left: auto; }
.tl-pagination { display: flex; gap: 4px; justify-content: center; margin-top: 16px; }
.tl-pg { padding: 4px 10px; border: 1px solid #30363d; border-radius: 4px; background: #0d1117; color: #c9d1d9; cursor: pointer; font-size: 13px; }
.tl-pg:hover { background: #1c2128; }
.tl-pg.active { background: #1f6feb; border-color: #1f6feb; }
.tl-pg:disabled { opacity: .3; cursor: default; }
/* Timeline item */
.tl-item { position: relative; padding: 12px 16px 12px 48px; border-left: 2px solid #30363d; margin-left: 12px; }
.tl-item:last-child { border-left-color: transparent; }
.tl-item::before { content: ''; position: absolute; left: -5px; top: 16px; width: 8px; height: 8px; border-radius: 50%; background: #30363d; }
.tl-item.has-data::before { background: #58a6ff; }
.tl-item.has-first::before { background: #f78166; }
.tl-dot { position: absolute; left: -8px; top: 16px; width: 12px; height: 12px; border-radius: 50%; border: 2px solid #30363d; background: #0d1117; }
.tl-item.has-data .tl-dot { border-color: #58a6ff; background: #1f6feb33; }
.tl-item.has-first .tl-dot { border-color: #f78166; background: #f7816633; }
.tl-header { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.tl-ch { color: #58a6ff; font-weight: 700; font-size: 15px; min-width: 80px; }
.tl-arc { font-size: 11px; padding: 1px 6px; border-radius: 8px; background: #21262d; color: #8b949e; }
.tl-rank { font-size: 11px; color: #f78166; }
.tl-title { font-size: 14px; color: #f0f6fc; }
.tl-body { margin-top: 6px; }
.tl-content { font-size: 12px; color: #484f58; line-height: 1.5; max-height: 40px; overflow: hidden; margin-bottom: 4px; }
.tl-tags { display: flex; gap: 4px; flex-wrap: wrap; }
.tl-tag { font-size: 10px; padding: 1px 6px; border-radius: 8px; background: #21262d; color: #8b949e; }
.tl-tag.gu { background: #f7816622; color: #f78166; border: 1px solid #f7816644; }
.tl-tag.char { background: #58a6ff22; color: #58a6ff; border: 1px solid #58a6ff44; }
.tl-tag.first { background: #d2992222; color: #d29922; border: 1px solid #d2992244; }
.tl-tag.event { background: #3fb95022; color: #3fb950; border: 1px solid #3fb95044; }
.tl-event-list { margin-top: 6px; }
.tl-event-item { font-size: 12px; color: #8b949e; padding: 2px 8px; background: #0d1117; border-radius: 4px; margin: 2px 0; }
.tl-first-list { margin-top: 4px; }
.tl-first-item { font-size: 11px; color: #d29922; padding: 1px 6px; background: #d2992211; border-radius: 3px; display: inline-block; margin: 1px 2px; }
/* Tab buttons */
.tab-bar { display: flex; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.tab-btn { padding: 6px 16px; border-radius: 20px; border: 1px solid #30363d; background: transparent; color: #8b949e; cursor: pointer; font-size: 13px; }
.tab-btn.active { background: #1f6feb22; border-color: #1f6feb; color: #58a6ff; }
/* Map grid */
.map-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 6px; }
.map-item { background: #0d1117; border: 1px solid #21262d; border-radius: 6px; padding: 8px 10px; }
.map-item .m-name { font-size: 13px; color: #f0f6fc; font-weight: 600; }
.map-item .m-chapters { font-size: 11px; color: #8b949e; margin-top: 2px; }
.map-item .m-bar { margin-top: 4px; height: 3px; background: #21262d; border-radius: 2px; overflow: hidden; }
.map-item .m-bar-fill { height: 100%; background: #58a6ff; border-radius: 2px; }
/* Scrollbar */
::-webkit-scrollbar { width: 8px; }
::-webkit-scrollbar-track { background: #0d1117; }
::-webkit-scrollbar-thumb { background: #30363d; border-radius: 4px; }
</style>
</head>
<body>

<div class="header">
  <h1>🪲 蛊真人 · 知识库</h1>
  <span class="sub">Reverend Insanity Novel DB</span>
  <span class="ver" id="version"></span>
</div>

<div class="tabs">
  <div class="tab active" onclick="switchTab('overview')">概览</div>
  <div class="tab" onclick="switchTab('timeline')">📅 时间线</div>
  <div class="tab" onclick="switchTab('arcs')">篇章</div>
  <div class="tab" onclick="switchTab('chapters')">章节</div>
  <div class="tab" onclick="switchTab('entities')">实体</div>
  <div class="tab" onclick="switchTab('maps')">映射</div>
  <div class="tab" onclick="switchTab('search')">搜索</div>
</div>

<div class="content">
  <div id="panel-overview" class="panel active"></div>
  <div id="panel-timeline" class="panel"></div>
  <div id="panel-arcs" class="panel"></div>
  <div id="panel-chapters" class="panel"></div>
  <div id="panel-entities" class="panel"></div>
  <div id="panel-maps" class="panel"></div>
  <div id="panel-search" class="panel"></div>
</div>

<!-- Entity Detail Modal -->
<div class="modal" id="entityModal">
  <div class="modal-content">
    <span class="close" onclick="closeModal()">&times;</span>
    <div id="modalBody"></div>
  </div>
</div>

<script>
// ─── State ────────────────────────────────────────────
let state = {};
async function api(path) { const r = await fetch(path); return r.json(); }

// ─── Tab Switching ────────────────────────────────────
function switchTab(name) {
  document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
  document.querySelector(`.tab[onclick*="${name}"]`).classList.add('active');
  const el = document.getElementById(`panel-${name}`);
  el.classList.add('active');
  // Lazy render
  if (name === 'overview' && !el.innerHTML.trim()) renderOverview();
  if (name === 'timeline') renderTimeline();
  if (name === 'arcs' && !el.innerHTML.trim()) renderArcs();
  if (name === 'chapters') renderChapters();
  if (name === 'entities' && !el.innerHTML.trim()) renderEntities();
  if (name === 'maps') renderMaps();
}

// ─── Overview ─────────────────────────────────────────
async function renderOverview() {
  const el = document.getElementById('panel-overview');
  el.innerHTML = '<div class="loading">加载中...</div>';
  const stats = await api('/api/stats');
  const meta = stats.meta || {};
  document.getElementById('version').textContent = `v${meta.version || '?'} · ${meta.last_updated || ''}`;
  
  el.innerHTML = `
    <div class="stats-grid">
      <div class="stat-card"><div class="num">${stats.timeline_entries}</div><div class="label">时间线条目</div><div class="sub">${stats.timeline_with_events} 条有详细事件</div></div>
      <div class="stat-card"><div class="num">${stats.chapters}</div><div class="label">章节文件</div><div class="sub">${stats.total_entries} 个章节条目</div></div>
      <div class="stat-card"><div class="num">${stats.arcs}</div><div class="label">篇章</div><div class="sub">覆盖 ${meta.chapters_covered || '?'}</div></div>
      <div class="stat-card"><div class="num">${stats.entities.gu_worm}</div><div class="label">蛊虫</div><div class="sub">${stats.gu_with_chapters} 只有章节映射</div></div>
      <div class="stat-card"><div class="num">${stats.entities.person}</div><div class="label">角色</div><div class="sub">${stats.chars_with_chapters} 个有章节映射</div></div>
      <div class="stat-card"><div class="num">${stats.entity_details_coverage}</div><div class="label">数据覆盖</div><div class="sub">共 ${stats.total_entries} 章，前 ${stats.entity_details_coverage} 章有实体数据</div></div>
    </div>
    <div class="card">
      <div class="card-header" onclick="toggleCard(this)">
        <span class="arrow">▶</span>
        <span class="title">篇章一览</span>
      </div>
      <div class="card-body" id="arcSummaryList"></div>
    </div>
  `;
  
  const arcs = await api('/api/arcs');
  document.getElementById('arcSummaryList').innerHTML = arcs.map(a => `
    <div class="arc-row" onclick="switchTab('arcs')">
      <span class="rank">${a.rank || '?'}</span>
      <span class="arc-title">${a.title}</span>
      <span class="arc-ch">第${a.chapter_range[0]}-${a.chapter_range[1]}章</span>
      <span class="arc-stat">${a.major_events?.length ? `📋 ${a.major_events.length} 事件` : '⬜ 骨架'}</span>
    </div>
  `).join('');
}

// ─── Timeline ─────────────────────────────────────────
let tlState = { page: 1, arc: '', has_data: '', search: '', per_page: 30 };

async function renderTimeline() {
  const el = document.getElementById('panel-timeline');
  el.innerHTML = `
    <div class="tl-controls">
      <select id="tlArcFilter" onchange="tlState.arc=this.value; tlState.page=1; loadTimeline()">
        <option value="">全部篇章</option>
      </select>
      <select id="tlDataFilter" onchange="tlState.has_data=this.value; tlState.page=1; loadTimeline()">
        <option value="">全部状态</option>
        <option value="true">有数据</option>
        <option value="false">骨架</option>
      </select>
      <input id="tlSearch" placeholder="搜索时间线..." style="width:180px;" onkeyup="if(event.key==='Enter'){tlState.search=this.value; tlState.page=1; loadTimeline()}">
      <button class="tab-btn" onclick="tlState.search=document.getElementById('tlSearch').value; tlState.page=1; loadTimeline()" style="padding:4px 12px;">搜索</button>
      <span class="tl-count" id="tlCount">加载中...</span>
    </div>
    <div id="tlList"></div>
    <div class="tl-pagination" id="tlPagination"></div>
  `;
  
  // Load arc options
  const arcs = await api('/api/arcs');
  const sel = document.getElementById('tlArcFilter');
  arcs.forEach(a => { const o = document.createElement('option'); o.value = a.id; o.textContent = `${a.title} (${a.rank})`; sel.appendChild(o); });
  
  loadTimeline();
}

async function loadTimeline() {
  const el = document.getElementById('tlList');
  if (!el) return;
  el.innerHTML = '<div class="loading">加载时间线...</div>';
  
  const params = new URLSearchParams({ page: tlState.page, per_page: tlState.per_page });
  if (tlState.arc) params.set('arc', tlState.arc);
  if (tlState.has_data) params.set('has_data', tlState.has_data);
  if (tlState.search) params.set('q', tlState.search);
  
  const data = await api(`/api/timeline?${params}`);
  document.getElementById('tlCount').textContent = `共 ${data.total} 条 · 第 ${data.page}/${data.total_pages} 页`;
  
  const noArcs = tlState.arc || tlState.search;
  
  el.innerHTML = data.entries.map(e => {
    const hasData = e.has_data || e.first_appearances.length > 0;
    const firstClass = e.first_appearances.length > 0 ? 'has-first' : '';
    const dataClass = hasData ? 'has-data' : '';
    
    let tags = '';
    if (e.gu_count > 0) tags += `<span class="tl-tag gu">${e.gu_count}蛊</span>`;
    if (e.char_count > 0) tags += `<span class="tl-tag char">${e.char_count}角</span>`;
    if (e.event_count > 0) tags += `<span class="tl-tag event">${e.event_count}事件</span>`;
    if (!hasData && !noArcs) tags += `<span class="tl-tag">骨架</span>`;
    
    let firstApps = '';
    if (e.first_appearances.length > 0) {
      firstApps = `<div class="tl-first-list">${e.first_appearances.map(f => 
        `<span class="tl-first-item">✨ ${f.type === 'gu_worm' ? '🪲' : f.type === 'person' ? '👤' : '📍'} ${f.name}${f.rank ? ' 『'+f.rank+'』' : ''}</span>`
      ).join('')}</div>`;
    }
    
    let eventList = '';
    if (e.events.length > 0) {
      eventList = `<div class="tl-event-list">${e.events.map(ev => 
        `<div class="tl-event-item">${ev}</div>`
      ).join('')}</div>`;
    }
    
    return `<div class="tl-item ${dataClass} ${firstClass}">
      <div class="tl-dot"></div>
      <div class="tl-header">
        <span class="tl-ch">第${e.chapter}章</span>
        <span class="tl-arc">${e.arc_title || e.arc_id || '?'}</span>
        ${e.rank ? `<span class="tl-rank">${e.rank}</span>` : ''}
      </div>
      <div class="tl-title">${e.title}</div>
      <div class="tl-body">
        <div class="tl-content">${e.content || ''}</div>
        ${tags ? `<div class="tl-tags">${tags}</div>` : ''}
        ${firstApps}
        ${eventList}
      </div>
    </div>`;
  }).join('');
  
  // Pagination
  const pg = document.getElementById('tlPagination');
  if (data.total_pages <= 1) { pg.innerHTML = ''; return; }
  let html = '';
  const p = data.page, tp = data.total_pages;
  if (p > 1) html += `<button class="tl-pg" onclick="tlState.page=1; loadTimeline()">«</button><button class="tl-pg" onclick="tlState.page=${p-1}; loadTimeline()">‹</button>`;
  for (let i = Math.max(1, p-2); i <= Math.min(tp, p+2); i++) {
    html += `<button class="tl-pg ${i===p?'active':''}" onclick="tlState.page=${i}; loadTimeline()">${i}</button>`;
  }
  if (p < tp) html += `<button class="tl-pg" onclick="tlState.page=${p+1}; loadTimeline()">›</button><button class="tl-pg" onclick="tlState.page=${tp}; loadTimeline()">»</button>`;
  pg.innerHTML = html;
}

// ─── Arcs ─────────────────────────────────────────────
async function renderArcs() {
  const el = document.getElementById('panel-arcs');
  el.innerHTML = '<div class="loading">加载中...</div>';
  const arcs = await api('/api/arcs');
  el.innerHTML = arcs.map(a => `
    <div class="card">
      <div class="card-header" onclick="toggleCard(this); loadArcChapters(this, '${a.id}')">
        <span class="arrow">▶</span>
        <span class="rank" style="font-size:11px;padding:2px 8px;border-radius:10px;background:#21262d;color:#f78166;">${a.rank||'?'}</span>
        <span class="title">${a.title}</span>
        <span class="badge">第${a.chapter_range[0]}-${a.chapter_range[1]}章</span>
        <span class="badge green">新蛊 ${a.new_gu?.length||0}</span>
        <span class="badge blue">新角 ${a.new_chars?.length||0}</span>
        <span class="badge orange">事件 ${a.major_events?.length||0}</span>
      </div>
      <div class="card-body">
        ${a.summary ? `<div style="font-size:13px;color:#8b949e;margin-bottom:8px;">${a.summary}</div>` : ''}
        ${(a.major_events||[]).length ? `<div style="margin-bottom:8px;"><b style="font-size:12px;color:#58a6ff;">主要事件</b><div style="font-size:12px;color:#c9d1d9;line-height:1.6;margin-top:4px;">${a.major_events.slice(0,10).map(e => `· ${e}`).join('<br>')}</div></div>` : ''}
        <div id="arc-chapters-${a.id}" style="margin-top:8px;"></div>
      </div>
    </div>
  `).join('');
}

async function loadArcChapters(header, arcId) {
  const body = header.nextElementSibling;
  if (!header.classList.contains('expanded')) return;
  const container = document.getElementById(`arc-chapters-${arcId}`);
  if (container.dataset.loaded) return;
  container.dataset.loaded = '1';
  container.innerHTML = '<div class="loading" style="padding:8px;">加载章节...</div>';
  const arc = await api(`/api/arcs/${arcId}`);
  if (!arc.chapters) { container.innerHTML = ''; return; }
  container.innerHTML = arc.chapters.map(c => `
    <div class="ch-entry">
      <span class="ch-num">${c.chapter_range[0]}-${c.chapter_range[1]}</span>
      <span class="ch-title">${c.entry_count} 个条目</span>
      ${c.has_data ? '<span class="ch-tag" style="background:#23863622;color:#3fb950;">有数据</span>' : '<span class="ch-tag">骨架</span>'}
    </div>
  `).join('');
}

// ─── Chapters ─────────────────────────────────────────
async function renderChapters() {
  const el = document.getElementById('panel-chapters');
  const chapters = await api('/api/chapters');
  const totalWithData = chapters.filter(c => c.has_data).length;
  el.innerHTML = `
    <div style="margin-bottom:12px;display:flex;gap:16px;align-items:center;flex-wrap:wrap;">
      <span style="color:#8b949e;font-size:13px;">共 ${chapters.length} 个章节文件</span>
      <span style="color:#3fb950;font-size:13px;">✅ ${totalWithData} 个有实体数据</span>
      <span style="color:#484f58;font-size:13px;">⬜ ${chapters.length - totalWithData} 个骨架</span>
      <input id="chFilter" style="margin-left:auto;padding:6px 12px;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#c9d1d9;font-size:13px;width:200px;" placeholder="过滤章节..." oninput="filterChapters()">
    </div>
    <div id="chList">${chapters.map(c => renderChItem(c)).join('')}</div>
  `;
}

function renderChItem(c) {
  const r = c.range;
  const label = `ch${String(r[0]).padStart(3,'0')}-${String(r[1]).padStart(3,'0')}`;
  return `<div class="ch-entry" data-range="${label}" data-arc="${c.arc_id}" data-hasdata="${c.has_data}" onclick="openChapter('${label}')">
    <span class="ch-num">${r[0]}-${r[1]}</span>
    <span class="ch-title">${c.arc_id.replace('_',' · ')}</span>
    <span class="ch-tags">
      <span class="ch-tag">${c.entry_count} 条目</span>
      ${c.has_data ? '<span class="ch-tag" style="background:#23863622;color:#3fb950;">有数据</span>' : '<span class="ch-tag">骨架</span>'}
    </span>
  </div>`;
}

function filterChapters() {
  const q = document.getElementById('chFilter').value.toLowerCase();
  document.querySelectorAll('#chList .ch-entry').forEach(e => {
    const match = !q || e.dataset.range.includes(q) || e.dataset.arc.includes(q) || 
      (q === 'data' && e.dataset.hasdata === 'true') || (q === 'skeleton' && e.dataset.hasdata === 'false');
    e.style.display = match ? '' : 'none';
  });
}

async function openChapter(label) {
  const data = await api(`/api/chapter/${label}`);
  const entries = data.entries || [];
  showModal(`
    <h2 style="margin-bottom:4px;">章节 ${label}</h2>
    <div style="color:#8b949e;font-size:13px;margin-bottom:12px;">篇章: ${data.arc_id || '无'}</div>
    ${entries.map(e => `
      <div style="background:#0d1117;border:1px solid #21262d;border-radius:6px;padding:10px 12px;margin-bottom:8px;">
        <div style="display:flex;align-items:center;gap:8px;margin-bottom:4px;">
          <span style="color:#58a6ff;font-weight:600;font-size:13px;">第${e.chapter}章</span>
          <span style="color:#c9d1d9;font-size:12px;">${e.title}</span>
        </div>
        <div style="font-size:12px;color:#8b949e;line-height:1.5;max-height:60px;overflow:hidden;margin-bottom:4px;">${e.content||''}</div>
        <div style="display:flex;gap:8px;flex-wrap:wrap;">
          ${(e.gu_mentioned||[]).length ? `<span class="ch-tag" style="color:#f78166;">蛊: ${e.gu_mentioned.slice(0,5).join('、')}${e.gu_mentioned.length > 5 ? `...(+${e.gu_mentioned.length-5})` : ''}</span>` : ''}
          ${(e.char_appear||[]).length ? `<span class="ch-tag" style="color:#58a6ff;">角: ${e.char_appear.slice(0,5).join('、')}${e.char_appear.length > 5 ? `...(+${e.char_appear.length-5})` : ''}</span>` : ''}
          ${(e.events||[]).length ? `<span class="ch-tag" style="color:#3fb950;">${e.events.length} 事件</span>` : ''}
        </div>
      </div>
    `).join('')}
  `);
}

// ─── Entities ─────────────────────────────────────────
async function renderEntities() {
  const el = document.getElementById('panel-entities');
  el.innerHTML = `
    <div class="tab-bar">
      <button class="tab-btn active" onclick="switchEntityTab('gu_worm', this)">蛊虫 (${state.guCount || '?'})</button>
      <button class="tab-btn" onclick="switchEntityTab('person', this)">角色 (${state.charCount || '?'})</button>
      <button class="tab-btn" onclick="switchEntityTab('location', this)">地点 (${state.locCount || '?'})</button>
    </div>
    <input id="entSearch" style="width:100%;padding:8px 12px;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#c9d1d9;font-size:14px;margin-bottom:12px;" placeholder="搜索实体..." oninput="filterEntities()">
    <div id="entityGrid" class="entity-list"><div class="loading">加载中...</div></div>
  `;
  await loadEntityData('gu_worm');
}

async function loadEntityData(type) {
  const grid = document.getElementById('entityGrid');
  const data = await api(`/api/entities?type=${type}`);
  grid.innerHTML = data.map(e => `
    <div class="entity-item" onclick="showEntity('${e.name}')">
      <div class="name">${e.name}</div>
      <div class="meta">${e.rank ? '『' + e.rank + '』' : ''} ${(e.aliases||[]).length ? '又名: ' + e.aliases.slice(0,2).join('、') : ''}</div>
      <div class="summary">${e.summary || ''}</div>
    </div>
  `).join('');
  if (type === 'gu_worm') state.guCount = data.length;
  if (type === 'person') state.charCount = data.length;
  if (type === 'location') state.locCount = data.length;
}

function switchEntityTab(type, btn) {
  document.querySelectorAll('#panel-entities .tab-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
  loadEntityData(type);
}

function filterEntities() {
  const q = document.getElementById('entSearch').value.toLowerCase();
  document.querySelectorAll('.entity-item').forEach(e => {
    e.style.display = e.textContent.toLowerCase().includes(q) ? '' : 'block';
  });
}

async function showEntity(name) {
  const data = await api(`/api/entity/${encodeURIComponent(name)}`);
  if (!data || data.error) { showModal(`<p>未找到: ${name}</p>`); return; }
  let detailsHtml = '';
  if (data.details) {
    for (const [k, v] of Object.entries(data.details)) {
      if (v && v.length) {
        detailsHtml += `<div class="detail-group"><h4>${k}</h4>${v.map(t => `<p>${t}</p>`).join('')}</div>`;
      }
    }
  }
  showModal(`
    <h2>${data.name}</h2>
    <div style="color:#8b949e;font-size:13px;margin-bottom:12px;">
      ${data.rank ? `<span>品级: ${data.rank}</span> · ` : ''}
      ${(data.aliases||[]).length ? `<span>别名: ${data.aliases.join('、')}</span>` : ''}
      ${data.first_appear_title ? `<div>首次出现: ${data.first_appear_title}</div>` : ''}
    </div>
    ${data.summary ? `<div style="font-size:14px;line-height:1.6;margin-bottom:12px;padding:8px 12px;background:#0d1117;border-radius:6px;">${data.summary}</div>` : ''}
    ${detailsHtml || '<div style="color:#484f58;">暂无详细数据</div>'}
  `);
}

// ─── Maps ─────────────────────────────────────────────
async function renderMaps() {
  const el = document.getElementById('panel-maps');
  el.innerHTML = `
    <div class="tab-bar">
      <button class="tab-btn active" onclick="loadMapData('gu', this)">蛊 → 章节</button>
      <button class="tab-btn" onclick="loadMapData('char', this)">角色 → 章节</button>
    </div>
    <input id="mapSearch" style="width:100%;padding:8px 12px;background:#0d1117;border:1px solid #30363d;border-radius:6px;color:#c9d1d9;font-size:14px;margin-bottom:12px;" placeholder="搜索..." oninput="filterMaps()">
    <div id="mapGrid" class="map-grid"><div class="loading">加载中...</div></div>
  `;
  await loadMapData('gu', document.querySelector('.tab-btn'));
}

async function loadMapData(type, btn) {
  if (btn) {
    document.querySelectorAll('#panel-maps .tab-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
  }
  const grid = document.getElementById('mapGrid');
  const data = await api(`/api/maps?type=${type}`);
  const entries = Object.entries(data).sort((a, b) => b[1].count - a[1].count);
  const maxCount = entries.length ? entries[0][1].count : 1;
  grid.innerHTML = entries.map(([name, info]) => `
    <div class="map-item">
      <div class="m-name">${name}</div>
      <div class="m-chapters">出现 ${info.count} 章 (${info.range[0]}-${info.range[1]})</div>
      <div class="m-bar"><div class="m-bar-fill" style="width:${(info.count / maxCount * 100).toFixed(1)}%"></div></div>
    </div>
  `).join('');
}

function filterMaps() {
  const q = document.getElementById('mapSearch').value.toLowerCase();
  document.querySelectorAll('.map-item').forEach(e => {
    e.style.display = e.textContent.toLowerCase().includes(q) ? '' : 'block';
  });
}

// ─── Search ───────────────────────────────────────────
async function doSearch() {
  const q = document.getElementById('searchInput').value.trim();
  if (!q) return;
  const el = document.getElementById('searchResults');
  el.innerHTML = '<div class="loading">搜索中...</div>';
  const data = await api(`/api/search?q=${encodeURIComponent(q)}`);
  if (!data.results || !data.results.length) {
    el.innerHTML = '<div style="color:#484f58;padding:20px;text-align:center;">未找到结果</div>';
    return;
  }
  el.innerHTML = data.results.map(r => `
    <div class="search-result" onclick="${r.type === '蛊虫' || r.type === '角色' || r.type === '地点' ? `showEntity('${r.name}')` : 'switchTab(\'overview\')'}">
      <span class="s-type">${r.type}</span>
      <span class="s-name">${r.name}</span>
      <div class="s-summary">${(r.summary || r.rank || '').slice(0, 120)}</div>
    </div>
  `).join('');
}

// ─── Modal ────────────────────────────────────────────
function showModal(html) {
  document.getElementById('modalBody').innerHTML = html;
  document.getElementById('entityModal').classList.add('open');
}
function closeModal() {
  document.getElementById('entityModal').classList.remove('open');
}
document.getElementById('entityModal').addEventListener('click', function(e) {
  if (e.target === this) closeModal();
});
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeModal(); });

// ─── Shared ───────────────────────────────────────────
function toggleCard(header) {
  header.classList.toggle('expanded');
  header.nextElementSibling.classList.toggle('open');
}

// ─── Init ─────────────────────────────────────────────
renderOverview();
</script>
</body>
</html>
"""

# ─── 启动 ──────────────────────────────────────────────────
if __name__ == "__main__":
    print(f"\n  🪲 蛊真人 · 知识库 Web 展示 v2")
    print(f"  {'='*40}")
    print(f"  数据目录: {BASE}")
    print(f"  启动地址: http://localhost:{PORT}")
    print(f"  按 Ctrl+C 停止\n")
    server = http.server.HTTPServer(("0.0.0.0", PORT), Handler)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n  已停止")
        server.server_close()
