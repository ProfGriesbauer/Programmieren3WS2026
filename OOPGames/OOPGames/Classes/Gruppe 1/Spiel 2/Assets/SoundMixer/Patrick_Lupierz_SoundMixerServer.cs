using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SoundMixerServer
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: "Handy-App" zum Einstellen aller Sounds (Lautstärke, Häufigkeit, wie viele Töne gleichzeitig,
    //               Vogel-Radius, Stumm, Probehören). Es ist eine kleine Web-App: Der Server läuft nur auf ausdrücklichen
    //               Wunsch (Menü B, dann F11) im Spiel, zeigt Adresse und PIN an, und das Handy öffnet die Adresse im
    //               Browser (im WLAN des Rechners). Dort lässt sie sich über "Zum Startbildschirm hinzufügen" wie eine App
    //               ablegen. Keine Zusatzbibliothek: System.Net.Sockets und System.Text.Json sind Teil von .NET.
    // Sicherheit:   Nur nach dem Einschalten aktiv, nur im lokalen Netz, jede Anfrage an die Einstellungen braucht die
    //               4-stellige PIN (Header X-Pin oder ?pin=), zu viele Fehlversuche sperren kurz, Anfragegröße begrenzt,
    //               keine Dateizugriffe über die Adresse. Beim ersten Start fragt die Windows-Firewall nach Freigabe.
    // Threads:      Der Server läuft in Hintergrund-Threads. Einstellungen werden über das Schloss in G1C_SoundSettings
    //               geändert; alles, was Töne betrifft (Probehören, Stumm), geht per PostCommand in den nächsten
    //               Spiel-Tick (G1C_SoundSystem.Pump), weil WPF-MediaPlayer nur dort benutzt werden darf.
    // =====================================================
    public sealed class G1C_SoundMixerServer
    {
        public const int PreferredPort = 8765;
        private const int MaxRequestBytes = 65536;

        private readonly G1C_SoundSystem _sound;
        private readonly List<DateTime> _failures = new List<DateTime>();
        private TcpListener _listener;
        private volatile bool _running;

        public G1C_SoundMixerServer(G1C_SoundSystem sound)
        {
            _sound = sound ?? throw new ArgumentNullException(nameof(sound));
        }

        public bool Running { get { return _running; } }
        public int Port { get; private set; }
        public string Pin { get; private set; }
        public string LastError { get; private set; }
        public int RequestCount { get; private set; }

        /// <summary>Adressen, unter denen das Handy den Mixer findet (nur IPv4 im lokalen Netz).</summary>
        public IList<string> Urls
        {
            get
            {
                List<string> urls = new List<string>();
                foreach (string ip in LocalAddresses()) urls.Add("http://" + ip + ":" + Port + "/");
                return urls;
            }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Server starten. loopbackOnly = nur dieser Rechner (Tests). Erst ab preferredPort aufwärts den ersten
        //          freien Port nehmen. Die PIN ist vier zufällige Ziffern (oder die übergebene).
        public bool Start(bool loopbackOnly = false, int preferredPort = PreferredPort, string pin = null)
        {
            if (_running) return true;
            for (int port = preferredPort; port < preferredPort + 12; port++)
            {
                try
                {
                    _listener = new TcpListener(loopbackOnly ? IPAddress.Loopback : IPAddress.Any, port);
                    _listener.Start();
                    Port = port;
                    break;
                }
                catch (SocketException ex)
                {
                    LastError = ex.Message;
                    _listener = null;
                }
            }
            if (_listener == null) return false;
            Pin = pin ?? RandomNumberGenerator.GetInt32(1000, 10000).ToString();
            _running = true;
            Thread thread = new Thread(AcceptLoop) { IsBackground = true, Name = "G1C-SoundMixer" };
            thread.Start();
            return true;
        }

        public void Stop()
        {
            _running = false;
            try { if (_listener != null) _listener.Stop(); } catch (Exception) { }
            _listener = null;
        }

        private void AcceptLoop()
        {
            TcpListener listener = _listener;
            while (_running && listener != null)
            {
                TcpClient client;
                try { client = listener.AcceptTcpClient(); }
                catch (Exception) { break; }
                ThreadPool.QueueUserWorkItem(_ => Serve(client));
            }
        }

        // ------------------------------------------------------------------ HTTP

        private void Serve(TcpClient client)
        {
            using (client)
            {
                try
                {
                    client.ReceiveTimeout = 5000;
                    client.SendTimeout = 5000;
                    NetworkStream stream = client.GetStream();
                    string method, target, body;
                    Dictionary<string, string> headers;
                    if (!ReadRequest(stream, out method, out target, out headers, out body))
                    {
                        Write(stream, 400, "text/plain", "Bad Request");
                        return;
                    }
                    RequestCount++;
                    string path = target, query = "";
                    int q = target.IndexOf('?');
                    if (q >= 0) { path = target.Substring(0, q); query = target.Substring(q + 1); }
                    Response response = Handle(method, path, query, headers, body);
                    Write(stream, response.Status, response.ContentType, response.Body);
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                }
            }
        }

        private static bool ReadRequest(NetworkStream stream, out string method, out string target,
                                        out Dictionary<string, string> headers, out string body)
        {
            method = target = body = "";
            headers = new Dictionary<string, string>();
            byte[] buffer = new byte[MaxRequestBytes + 1024];
            int total = 0, headerEnd = -1;
            while (total < buffer.Length)
            {
                int read = stream.Read(buffer, total, buffer.Length - total);
                if (read <= 0) break;
                total += read;
                headerEnd = IndexOf(buffer, total, new byte[] { 13, 10, 13, 10 });
                if (headerEnd >= 0) break;
                if (total > 16384) return false;
            }
            if (headerEnd < 0) return false;
            string head = Encoding.ASCII.GetString(buffer, 0, headerEnd);
            string[] lines = head.Split(new[] { "\r\n" }, StringSplitOptions.None);
            string[] first = lines[0].Split(' ');
            if (first.Length < 2) return false;
            method = first[0].ToUpperInvariant();
            target = first[1];
            foreach (string line in lines.Skip(1))
            {
                int colon = line.IndexOf(':');
                if (colon > 0) headers[line.Substring(0, colon).Trim().ToLowerInvariant()] = line.Substring(colon + 1).Trim();
            }
            int length = 0;
            string lengthText;
            if (headers.TryGetValue("content-length", out lengthText) && (!int.TryParse(lengthText, out length) || length < 0 || length > MaxRequestBytes))
                return false;
            int bodyStart = headerEnd + 4;
            while (total - bodyStart < length && total < buffer.Length)
            {
                int read = stream.Read(buffer, total, buffer.Length - total);
                if (read <= 0) break;
                total += read;
            }
            body = Encoding.UTF8.GetString(buffer, bodyStart, Math.Min(length, total - bodyStart));
            return true;
        }

        private static int IndexOf(byte[] data, int length, byte[] pattern)
        {
            for (int i = 0; i <= length - pattern.Length; i++)
            {
                int j = 0;
                while (j < pattern.Length && data[i + j] == pattern[j]) j++;
                if (j == pattern.Length) return i;
            }
            return -1;
        }

        private static void Write(NetworkStream stream, int status, string contentType, string body)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body ?? "");
            string reason = status == 200 ? "OK" : status == 400 ? "Bad Request" : status == 401 ? "Unauthorized" :
                            status == 404 ? "Not Found" : status == 429 ? "Too Many Requests" : "Error";
            string head = "HTTP/1.1 " + status + " " + reason + "\r\nContent-Type: " + contentType + "; charset=utf-8\r\n" +
                          "Content-Length: " + bytes.Length + "\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\n" +
                          "Connection: close\r\n\r\n";
            byte[] headBytes = Encoding.ASCII.GetBytes(head);
            stream.Write(headBytes, 0, headBytes.Length);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        // ------------------------------------------------------------------ Routen (ohne Netzwerk testbar)

        public sealed class Response
        {
            public int Status { get; set; }
            public string ContentType { get; set; }
            public string Body { get; set; }
        }

        private static Response Json(int status, object value)
        {
            return new Response { Status = status, ContentType = "application/json", Body = JsonSerializer.Serialize(value) };
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Eine Anfrage beantworten. Die Seite selbst und die Manifest-Datei sind ohne PIN erreichbar, alles
        //          unter /api/ nur mit richtiger PIN.
        public Response Handle(string method, string path, string query, IDictionary<string, string> headers, string body)
        {
            if (method == "GET" && (path == "/" || path == "/index.html"))
                return new Response { Status = 200, ContentType = "text/html", Body = G1C_SoundMixerPage.Html };
            if (method == "GET" && path == "/manifest.webmanifest")
                return new Response { Status = 200, ContentType = "application/manifest+json", Body = G1C_SoundMixerPage.Manifest };
            if (!path.StartsWith("/api/", StringComparison.Ordinal))
                return Json(404, new { error = "nicht gefunden" });

            Response denied = CheckPin(headers, query);
            if (denied != null) return denied;

            try
            {
                if (method == "GET" && path == "/api/state") return Json(200, BuildState());
                if (method == "GET" && path == "/api/status") return Json(200, BuildStatus());
                if (method == "POST" && path == "/api/set") return HandleSet(body);
                if (method == "POST" && path == "/api/play") return HandlePlay(body);
                if (method == "POST" && path == "/api/reset")
                {
                    _sound.Settings.ResetDefaults();
                    return Json(200, new { ok = true });
                }
            }
            catch (JsonException)
            {
                return Json(400, new { error = "ungültiges JSON" });
            }
            return Json(404, new { error = "nicht gefunden" });
        }

        private Response CheckPin(IDictionary<string, string> headers, string query)
        {
            lock (_failures)
            {
                _failures.RemoveAll(t => (DateTime.UtcNow - t).TotalSeconds > 60);
                if (_failures.Count >= 8) return Json(429, new { error = "zu viele Fehlversuche – kurz warten" });
            }
            string given = null;
            if (headers != null && headers.TryGetValue("x-pin", out given)) { }
            if (string.IsNullOrEmpty(given) && !string.IsNullOrEmpty(query))
            {
                foreach (string part in query.Split('&'))
                    if (part.StartsWith("pin=", StringComparison.Ordinal)) given = Uri.UnescapeDataString(part.Substring(4));
            }
            bool ok = given != null && Pin != null &&
                      CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(given), Encoding.UTF8.GetBytes(Pin));
            if (ok) return null;
            lock (_failures) { _failures.Add(DateTime.UtcNow); }
            return Json(401, new { error = "PIN falsch" });
        }

        private Response HandleSet(string body)
        {
            using (JsonDocument doc = JsonDocument.Parse(body ?? ""))
            {
                JsonElement root = doc.RootElement;
                string id = root.GetProperty("id").GetString();
                string field = root.GetProperty("field").GetString();
                int value = (int)Math.Round(root.GetProperty("value").GetDouble());
                if (id == G1C_SoundSettings.GlobalId && field == "muted")
                {
                    bool muted = value != 0;
                    _sound.PostCommand(() => _sound.SetMuted(muted));
                    return Json(200, new { ok = true });
                }
                bool ok = _sound.Settings.Set(id, field, value);
                return ok ? Json(200, new { ok = true }) : Json(400, new { error = "unbekannter Sound oder unbekanntes Feld" });
            }
        }

        private Response HandlePlay(string body)
        {
            using (JsonDocument doc = JsonDocument.Parse(body ?? ""))
            {
                string id = doc.RootElement.GetProperty("id").GetString();
                if (G1C_SoundCatalog.Find(id) == null) return Json(400, new { error = "unbekannter Sound" });
                _sound.PostCommand(() => _sound.Preview(id));
                return Json(200, new { ok = true });
            }
        }

        private object BuildStatus()
        {
            return new { music = _sound.CurrentMusic, active = _sound.ActiveEffects, muted = _sound.Muted, enabled = _sound.Enabled };
        }

        private object BuildState()
        {
            G1C_SoundSettings s = _sound.Settings;
            return new
            {
                global = new { master = s.Master, music = s.Music, effects = s.Effects, max = s.MaxSimultaneous, birdRadius = s.BirdRadius, muted = _sound.Muted },
                categories = G1C_SoundCatalog.Categories,
                status = BuildStatus(),
                sounds = G1C_SoundCatalog.All.Select(d =>
                {
                    G1C_SoundEntrySettings e = s.Get(d.Id);
                    return new
                    {
                        id = d.Id, name = d.Name, description = d.Description, category = d.Category, loop = d.Loop, isMusic = d.IsMusic,
                        files = G1C_SoundFiles.For(d).Length, hasFallback = d.Fallback != null,
                        volume = e.Volume, chance = e.Chance, cooldownMs = e.CooldownMs, enabled = e.Enabled,
                        defaults = new { volume = d.Volume, chance = d.Chance, cooldownMs = d.CooldownMs }
                    };
                }).ToArray()
            };
        }

        // ------------------------------------------------------------------ Adressen

        // Entwickler: Patrick Lupierz – IPv4-Adressen der aktiven Netzwerkkarten, private Netze zuerst
        public static IList<string> LocalAddresses()
        {
            List<string> found = new List<string>();
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (UnicastIPAddressInformation info in nic.GetIPProperties().UnicastAddresses)
                        if (info.Address.AddressFamily == AddressFamily.InterNetwork) found.Add(info.Address.ToString());
                }
            }
            catch (Exception) { }
            return found.OrderBy(a => a.StartsWith("192.168.") ? 0 : a.StartsWith("10.") ? 1 : a.StartsWith("172.") ? 2 : 3).ToList();
        }
    }
}
