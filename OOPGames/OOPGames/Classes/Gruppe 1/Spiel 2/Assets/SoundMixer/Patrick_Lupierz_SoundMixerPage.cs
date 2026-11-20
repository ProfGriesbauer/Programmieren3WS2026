namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SoundMixerPage
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Die Handy-App des Sound-Mixers als eine einzige Web-Seite (HTML, CSS, JavaScript ohne Bibliothek).
    //               Sie wird vom G1C_SoundMixerServer ausgeliefert und spricht dessen /api/-Adressen an.
    //               Aufbau: PIN-Eingabe, globale Regler (Gesamt, Musik, Effekte, gleichzeitige Töne, Vogel-Radius, Stumm),
    //               dann je Kategorie die Sounds mit Lautstärke, Häufigkeit (Chance und Mindestabstand), Ein/Aus und
    //               Probehören. Änderungen werden sofort gesendet und im Spiel gespeichert.
    // =====================================================
    public static class G1C_SoundMixerPage
    {
        public const string Manifest = """
{"name":"Casino Sound-Mixer","short_name":"Sound-Mixer","start_url":"/","display":"standalone","background_color":"#14101f","theme_color":"#14101f","icons":[{"src":"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Crect width='100' height='100' rx='20' fill='%2314101f'/%3E%3Ctext x='50' y='66' font-size='56' text-anchor='middle'%3E%F0%9F%8E%9A%3C/text%3E%3C/svg%3E","sizes":"any","type":"image/svg+xml"}]}
""";

        public const string Html = """
<!doctype html>
<html lang="de">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
<meta name="theme-color" content="#14101f">
<meta name="apple-mobile-web-app-capable" content="yes">
<meta name="mobile-web-app-capable" content="yes">
<link rel="manifest" href="/manifest.webmanifest">
<title>Sound-Mixer</title>
<style>
:root{--bg:#14101f;--card:#211a33;--card2:#2b2342;--text:#f3eefc;--dim:#a99fc4;--accent:#f8c93c;--ok:#5be08a;--bad:#ff6b7a}
*{box-sizing:border-box;-webkit-tap-highlight-color:transparent}
body{margin:0;background:var(--bg);color:var(--text);font:16px/1.4 system-ui,-apple-system,Segoe UI,Roboto,sans-serif;padding:0 16px 40px;padding-bottom:max(40px,env(safe-area-inset-bottom))}
header{position:sticky;top:0;background:var(--bg);padding:max(12px,env(safe-area-inset-top)) 0 10px;z-index:5;border-bottom:1px solid #33294d}
h1{margin:0;font-size:20px}
#status{font-size:13px;color:var(--dim);margin-top:2px;min-height:18px}
h2{font-size:15px;text-transform:uppercase;letter-spacing:.08em;color:var(--accent);margin:26px 0 10px}
.card{background:var(--card);border-radius:14px;padding:14px;margin:10px 0}
.row{display:flex;align-items:center;gap:10px;margin:12px 0 4px}
.row label{flex:0 0 112px;font-size:14px;color:var(--dim)}
.row output{flex:0 0 76px;white-space:nowrap;text-align:right;font-variant-numeric:tabular-nums;font-size:14px}
input[type=range]{flex:1;height:36px;accent-color:var(--accent);min-width:0}
.title{display:flex;align-items:center;gap:10px}
.title b{flex:1;font-size:16px}
.desc{font-size:13px;color:var(--dim);margin:2px 0 6px}
.badge{font-size:11px;padding:2px 8px;border-radius:99px;background:var(--card2);color:var(--dim)}
.badge.warn{color:var(--accent)}
button{background:var(--card2);color:var(--text);border:0;border-radius:10px;padding:10px 14px;font-size:15px;min-height:44px}
button:active{background:#3a2f5c}
button.play{min-width:46px;padding:8px 12px}
button.big{width:100%;margin-top:8px}
.switch{position:relative;width:50px;height:30px;flex:0 0 50px}
.switch input{opacity:0;width:100%;height:100%;position:absolute;margin:0;z-index:2}
.switch span{position:absolute;inset:0;background:#4a3f6d;border-radius:30px;transition:.15s}
.switch span:before{content:"";position:absolute;width:24px;height:24px;left:3px;top:3px;background:#fff;border-radius:50%;transition:.15s}
.switch input:checked+span{background:var(--ok)}
.switch input:checked+span:before{transform:translateX(20px)}
.stepper{display:flex;align-items:center;gap:12px;flex:1;justify-content:flex-end}
.stepper b{min-width:24px;text-align:center;font-size:18px}
details{margin:8px 0}
summary{cursor:pointer;list-style:none;padding:12px 4px;font-weight:600;font-size:16px}
summary::-webkit-details-marker{display:none}
summary:after{content:" ▾";color:var(--dim)}
.login{max-width:340px;margin:18vh auto 0;text-align:center}
.login input{width:100%;font-size:32px;text-align:center;letter-spacing:.4em;padding:12px;border-radius:12px;border:2px solid #4a3f6d;background:var(--card);color:var(--text)}
.err{color:var(--bad);min-height:22px;margin-top:8px}
.off{opacity:.5}
</style>
</head>
<body>
<header><h1>🎚 Casino Sound-Mixer</h1><div id="status">Verbinde …</div></header>
<main id="app"></main>
<script>
"use strict";
var pin = new URLSearchParams(location.search).get("pin") || localStorage.getItem("g1c-pin") || "";
var state = null, timers = {}, app = document.getElementById("app"), statusEl = document.getElementById("status");
function $(h){var t=document.createElement("template");t.innerHTML=h.trim();return t.content.firstChild;}
async function api(path, body){
  var opt = {headers:{"X-Pin":pin}};
  if (body !== undefined){opt.method="POST";opt.body=JSON.stringify(body);}
  var r = await fetch(path,opt);
  if (r.status === 401){ throw new Error("pin"); }
  if (r.status === 429){ throw new Error("limit"); }
  return r.json();
}
function login(msg){
  app.innerHTML = "";
  var box = $('<div class="login"><h2>PIN eingeben</h2><p class="desc">Die vierstellige PIN steht im Spiel auf dem Bildschirm (Menü B → F11).</p><input id="pin" inputmode="numeric" maxlength="4" autocomplete="off"><div class="err" id="err"></div><button class="big" id="go">Verbinden</button></div>');
  app.appendChild(box);
  box.querySelector("#err").textContent = msg || "";
  var input = box.querySelector("#pin"); input.value = pin; input.focus();
  function go(){ pin = input.value.trim(); localStorage.setItem("g1c-pin", pin); load(); }
  box.querySelector("#go").onclick = go;
  input.onkeydown = function(e){ if (e.key === "Enter") go(); };
}
async function load(){
  try { state = await api("/api/state"); }
  catch(e){
    statusEl.textContent = "Nicht verbunden";
    login(e.message === "pin" ? "PIN falsch." : e.message === "limit" ? "Zu viele Versuche – kurz warten." : "Keine Verbindung zum Spiel.");
    return;
  }
  render();
  statusEl.textContent = "Verbunden";
}
function send(id, field, value){
  var key = id + "/" + field;
  clearTimeout(timers[key]);
  timers[key] = setTimeout(function(){ api("/api/set",{id:id,field:field,value:value}).catch(function(){statusEl.textContent="Senden fehlgeschlagen";}); }, 90);
}
function slider(label, min, max, step, value, fmt, onChange){
  var row = $('<div class="row"><label></label><input type="range"><output></output></div>');
  var input = row.querySelector("input"), out = row.querySelector("output");
  row.querySelector("label").textContent = label;
  input.min = min; input.max = max; input.step = step; input.value = value;
  out.textContent = fmt(value);
  input.oninput = function(){ out.textContent = fmt(+input.value); onChange(+input.value); };
  return row;
}
function pct(v){return v + " %";}
function secs(v){return (v/1000).toFixed(1).replace(".", ",") + " s";}
function toggle(checked, onChange){
  var s = $('<label class="switch"><input type="checkbox"><span></span></label>');
  var i = s.querySelector("input"); i.checked = checked; i.onchange = function(){ onChange(i.checked); };
  return s;
}
function render(){
  app.innerHTML = "";
  var g = state.global;
  app.appendChild($("<h2>Gesamt</h2>"));
  var card = $('<div class="card"></div>');
  card.appendChild(slider("Gesamtlautstärke", 0, 100, 1, g.master, pct, function(v){send("_global","master",v);}));
  card.appendChild(slider("Musik", 0, 100, 1, g.music, pct, function(v){send("_global","music",v);}));
  card.appendChild(slider("Effekte", 0, 100, 1, g.effects, pct, function(v){send("_global","effects",v);}));
  card.appendChild(slider("Vogel-Radius", 1, 12, 1, g.birdRadius, function(v){return v + " Kacheln";}, function(v){send("_global","birdradius",v);}));
  var max = $('<div class="row"><label>Gleichzeitig</label><div class="stepper"><button>−</button><b></b><button>+</button></div></div>');
  var b = max.querySelector("b"), bs = max.querySelectorAll("button"); b.textContent = g.max;
  function step(d){ g.max = Math.max(1, Math.min(8, g.max + d)); b.textContent = g.max; send("_global","max",g.max); }
  bs[0].onclick = function(){step(-1);}; bs[1].onclick = function(){step(1);};
  card.appendChild(max);
  card.appendChild($('<div class="desc">Wie viele Effekte höchstens gleichzeitig hörbar sind. Bei 1 gibt es nie zwei Töne auf einmal; wichtigere Töne (Jackpot) verdrängen unwichtigere (Schritte).</div>'));
  var mute = $('<div class="row"><label>Alles stumm</label><span style="flex:1"></span></div>');
  mute.appendChild(toggle(g.muted, function(v){ api("/api/set",{id:"_global",field:"muted",value:v?1:0}); }));
  card.appendChild(mute);
  var reset = $('<button class="big">Alles auf Standard zurücksetzen</button>');
  reset.onclick = async function(){ if (confirm("Alle Einstellungen zurücksetzen?")) { await api("/api/reset",{}); load(); } };
  card.appendChild(reset);
  app.appendChild(card);

  state.categories.forEach(function(cat, index){
    var list = state.sounds.filter(function(s){return s.category === cat;});
    if (!list.length) return;
    var d = $('<details></details>'); if (index === 0) d.open = true;
    var sum = $('<summary></summary>'); sum.textContent = cat + " (" + list.length + ")";
    d.appendChild(sum);
    list.forEach(function(s){ d.appendChild(soundCard(s)); });
    app.appendChild(d);
  });
}
function soundCard(s){
  var c = $('<div class="card"></div>');
  var t = $('<div class="title"><b></b><span class="badge"></span></div>');
  t.querySelector("b").textContent = s.name;
  var badge = t.querySelector(".badge");
  if (s.files > 0){ badge.textContent = s.files + (s.files === 1 ? " Datei" : " Dateien"); }
  else if (s.hasFallback){ badge.textContent = "eingebaut"; badge.className = "badge warn"; }
  else { badge.textContent = "ohne Datei"; badge.className = "badge warn"; }
  var play = $('<button class="play">▶</button>'); play.onclick = function(){ api("/api/play",{id:s.id}); };
  t.appendChild(play);
  t.appendChild(toggle(s.enabled, function(v){ s.enabled = v; c.classList.toggle("off", !v); send(s.id,"enabled",v?1:0); }));
  c.appendChild(t);
  if (!s.enabled) c.classList.add("off");
  var d = $('<div class="desc"></div>'); d.textContent = s.description; c.appendChild(d);
  c.appendChild(slider("Lautstärke", 0, 100, 1, s.volume, pct, function(v){send(s.id,"volume",v);}));
  if (!s.isMusic && !s.loop){
    c.appendChild(slider("Häufigkeit", 0, 100, 1, s.chance, pct, function(v){send(s.id,"chance",v);}));
    c.appendChild(slider("Mindestabstand", 0, 30000, 100, s.cooldownMs, secs, function(v){send(s.id,"cooldown",v);}));
  }
  return c;
}
async function poll(){
  if (!state) return;
  try{
    var st = await api("/api/status");
    var text = "Verbunden";
    if (!st.enabled) text += " · Ton im Spiel aus";
    if (st.music) text += " · Musik: " + st.music;
    if (st.active.length) text += " · spielt: " + st.active.join(", ");
    statusEl.textContent = text;
  } catch(e){ statusEl.textContent = "Verbindung verloren …"; }
}
setInterval(poll, 2000);
if (pin) load(); else login("");
</script>
</body>
</html>
""";
    }
}
