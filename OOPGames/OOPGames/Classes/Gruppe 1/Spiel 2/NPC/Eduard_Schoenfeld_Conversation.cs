using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ConversationOption
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Eine Antwortmöglichkeit im Gespräch.
    // =====================================================
    public class G1C_ConversationOption
    {
        public G1C_ConversationOption(string text, Action execute)
        {
            Text = text;
            Execute = execute;
        }

        public string Text { get; }
        public Action Execute { get; }
    }

    // =====================================================
    // Klasse: G1C_Notice
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Kurzer Hinweis wie „Vertrauen +5“ oder „+40 Chips“.
    // =====================================================
    public class G1C_Notice
    {
        public string Text { get; set; }
        public bool Positive { get; set; }
        public int EndTick { get; set; }
    }

    // =====================================================
    // Klasse: G1C_Conversation
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Mehrstufiges, datenbasiertes Gespräch mit einem NPC. Begrüßung, Antworten und
    //               angebotene Optionen hängen von Beziehungsstatus, Erinnerungen, Stimmung und
    //               Persönlichkeit ab. Werte steigen nur mit Wartezeiten/Obergrenzen (kein Farmen).
    // =====================================================
    public class G1C_Conversation
    {
        public const int MaxOptions = 4;
        private const int SocialCooldownTicks = 1500;  // 60 s zwischen belohnten Gesprächen
        private const int MaxSocialTrust = 30;         // Plaudern/Komplimente bringen insgesamt höchstens +30
        private const int ApologyCooldownTicks = 1500;
        private const int NoticeTicks = 70;
        private const int BrokeLimit = 50;
        private const int PauseTaskTrust = 50;

        private readonly G1C_NpcSystem _system;
        private readonly List<G1C_ConversationOption> _options = new List<G1C_ConversationOption>();
        private readonly List<G1C_Notice> _notices = new List<G1C_Notice>();
        private G1C_InteractionType? _gameOffered;

        public G1C_Conversation(G1C_NpcSystem system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
        }

        public bool IsOpen { get; private set; }
        public G1C_NpcCharacter Npc { get; private set; }
        public string Line { get; private set; } = "";
        public G1C_NpcExpression Expression { get; private set; }
        public int Selected { get; private set; }
        public IReadOnlyList<G1C_ConversationOption> Options { get { return _options; } }
        public IReadOnlyList<G1C_Notice> Notices { get { return _notices; } }

        private int Tick { get { return _system.Tick; } }
        private G1C_NpcMemory Memory { get { return Npc.Memory; } }
        private G1C_RelationshipStatus Status { get { return Npc.Relationship.Status; } }
        private bool PlayerBroke { get { return _system.Money.Balance < BrokeLimit; } }

        /// <summary>Gewähltes Minispiel; wird nach dem Schließen vom GameManager gestartet.</summary>
        public G1C_InteractionType? RequestedGame { get; private set; }

        // ------------------------------------------------------------------ Steuerung

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gespräch beginnen – Erinnerung aktualisieren, Begrüßung wählen, Optionen anbieten.
        public void Open(G1C_NpcCharacter npc, G1C_InteractionType? gameOffered)
        {
            Npc = npc ?? throw new ArgumentNullException(nameof(npc));
            _gameOffered = gameOffered;
            RequestedGame = null;
            _notices.Clear();
            IsOpen = true;

            bool firstMeeting = Memory.Conversations == 0;
            if (firstMeeting && !Memory.ReputationApplied)
            {
                // Der Ruf eilt dem Spieler voraus – aber nur beim Kennenlernen
                Memory.ReputationApplied = true;
                int bonus = (_system.Reputation - 50) / 5;
                if (bonus != 0) Adjust(bonus, 0);
            }

            string greeting = BuildGreeting(firstMeeting);
            Memory.Conversations++;
            RefreshStatus();
            Say(greeting, GreetingExpression());
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Tasten: 1–4 direkt wählen, W/S blättern, E bestätigen, Q beenden.
        public void HandleKey(Key key)
        {
            if (!IsOpen) return;

            int number = G1C_InputManager.ToNumber(key);
            if (number >= 1 && number <= _options.Count) { Choose(number - 1); return; }

            if (G1C_InputManager.IsBack(key)) Close();
            else if (G1C_InputManager.IsConfirm(key)) Choose(Selected);
            else if (_options.Count > 0 && (key == Key.W || key == Key.Up)) Selected = (Selected + _options.Count - 1) % _options.Count;
            else if (_options.Count > 0 && (key == Key.S || key == Key.Down)) Selected = (Selected + 1) % _options.Count;
        }

        // Entwickler: Eduard Schoenfeld
        public void Choose(int index)
        {
            if (!IsOpen || index < 0 || index >= _options.Count) return;
            _options[index].Execute();
        }

        public void Close()
        {
            IsOpen = false;
        }

        public G1C_InteractionType? TakeRequestedGame()
        {
            G1C_InteractionType? game = RequestedGame;
            RequestedGame = null;
            return game;
        }

        public void UpdateNotices(int tick)
        {
            _notices.RemoveAll(n => tick >= n.EndTick);
        }

        // ------------------------------------------------------------------ Begrüßung

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Begrüßung aus Erinnerungen und Beziehung wählen.
        private string BuildGreeting(bool firstMeeting)
        {
            if (firstMeeting) return Npc.Personality.FirstMeeting;

            string text;
            string personal;
            if (!string.IsNullOrEmpty(Memory.PendingRemark))
            {
                text = Memory.PendingRemark;
                Memory.PendingRemark = null;
            }
            else if (Memory.WasEverInsulted && Npc.Relationship.Trust < 40 && !G1C_NpcRelationship.IsHostile(Status))
            {
                text = "Ich habe nicht vergessen, was du gesagt hast.";
            }
            else if (Npc.Personality.Greetings.TryGetValue(Status, out personal))
            {
                text = personal;
            }
            else
            {
                text = DefaultGreeting();
            }

            if (Status == G1C_RelationshipStatus.GoodFriend && PlayerBroke)
                text += "\nDu siehst aus, als könntest du Hilfe gebrauchen.";
            return text;
        }

        private string DefaultGreeting()
        {
            switch (Status)
            {
                case G1C_RelationshipStatus.Acquaintance: return Memory.Conversations >= 4 ? "Ach, du schon wieder!" : "Hallo nochmal.";
                case G1C_RelationshipStatus.Friend: return "Hey, schön dich wiederzusehen!";
                case G1C_RelationshipStatus.GoodFriend: return "Da bist du ja! Ich hab mich schon gefragt, wo du steckst.";
                case G1C_RelationshipStatus.Rival: return "Na, bereit für die nächste Niederlage?";
                case G1C_RelationshipStatus.RespectedRival: return "Du bist besser geworden. Das muss ich zugeben.";
                case G1C_RelationshipStatus.Opponent: return "Was willst du schon wieder?";
                case G1C_RelationshipStatus.ArchEnemy: return "Verschwinde. Sofort.";
                default: return "Hallo.";
            }
        }

        private G1C_NpcExpression GreetingExpression()
        {
            if (G1C_NpcRelationship.IsHostile(Status)) return G1C_NpcExpression.Angry;
            if (G1C_NpcRelationship.IsRival(Status)) return G1C_NpcExpression.Thoughtful;
            if (G1C_NpcRelationship.IsFriendly(Status) && Npc.Mood != G1C_NpcMood.Angry && Npc.Mood != G1C_NpcMood.Sad)
                return G1C_NpcExpression.Happy;
            return Npc.MoodExpression;
        }

        // ------------------------------------------------------------------ Optionen

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Hauptauswahl – Umfang und Ton hängen vom Beziehungsstatus ab.
        private void ShowRoot()
        {
            _options.Clear();
            Selected = 0;

            if (G1C_NpcRelationship.IsHostile(Status))
            {
                // Gegner: kurze, abweisende Gespräche – aber Versöhnung bleibt möglich
                AddOption("Es tut mir leid.", Apologize);
                bool refusesTable = Status == G1C_RelationshipStatus.ArchEnemy && Npc.Personality.Competitive;
                if (_gameOffered.HasValue && !refusesTable) AddOption("Ich will trotzdem spielen.", PlayGame);
                AddOption("Schon gut, ich gehe.", Close);
                return;
            }

            if (_gameOffered.HasValue) AddOption(GameOptionText(), PlayGame);

            G1C_CollectTask task = _system.Tasks.FindFor(Npc.Name);
            if (task != null) AddTaskOption(task);
            else if (Npc.Personality.HasPauseTask && !Memory.HelpedByPlayer) AddOption("Du wirkst nervös. Alles okay?", AskAboutNerves);

            AddOption("Lass uns plaudern.", SmallTalk);
            bool goodFriendOffer = Status == G1C_RelationshipStatus.GoodFriend && PlayerBroke;
            AddOption(goodFriendOffer ? "Ja, ein paar Chips wären toll." : "Kannst du mir etwas Geld geben?", AskForMoney);
        }

        private string GameOptionText()
        {
            if (_gameOffered == G1C_InteractionType.Poker)
            {
                if (Status == G1C_RelationshipStatus.RespectedRival) return "Bereit für ein ernstes Duell?";
                if (Status == G1C_RelationshipStatus.Rival) return "Revanche!";
                return "Ich möchte pokern.";
            }
            return "Ich möchte spielen.";
        }

        private void AddTaskOption(G1C_CollectTask task)
        {
            if (task.State == G1C_TaskState.Active)
            {
                if (task.AllCollected) AddOption("Hier, alles eingesammelt!", () => TurnInTask(task));
                else AddOption(task.Title + " (" + task.CollectedCount + "/" + task.Total + ")", () => TaskProgress(task));
            }
            else if (task.CanStart(Tick, Npc))
            {
                AddOption(task.Repeatable ? "Hast du Arbeit für mich?" : "Kann ich dir irgendwie helfen?", () => OfferTask(task));
            }
        }

        private void AddOption(string text, Action action)
        {
            if (_options.Count < MaxOptions) _options.Add(new G1C_ConversationOption(text, action));
        }

        // ------------------------------------------------------------------ Antworten

        // Entwickler: Eduard Schoenfeld
        private void PlayGame()
        {
            RequestedGame = _gameOffered;
            Close();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Plaudern – Unbekannte geben nur allgemeine Infos, Freunde erzählen Persönliches.
        private void SmallTalk()
        {
            G1C_NpcPersonality p = Npc.Personality;
            bool trusted = Npc.Relationship.Trust >= 60;

            if (G1C_NpcRelationship.IsRival(Status) && !trusted)
                Say("Reden ist nicht mein Ding. Spielen schon.", G1C_NpcExpression.Thoughtful);
            else if (trusted)
                Say(p.PersonalStory, G1C_NpcExpression.Happy);
            else
                Say(p.GeneralInfo, G1C_NpcExpression.Thoughtful);

            if (Tick - Memory.LastSocialRewardTick >= SocialCooldownTicks && GrantSocialTrust(3))
                Memory.LastSocialRewardTick = Tick;

            _options.Clear();
            Selected = 0;
            AddOption("Du machst das echt gut hier.", Compliment);
            AddOption(p.Pride >= 0.6 ? "Du kannst wohl nichts außer angeben." : "Du kannst wohl nicht spielen.", Insult);
            AddOption("Zurück.", () => { Say("Was gibt's noch?", Npc.MoodExpression); ShowRoot(); });
        }

        // Entwickler: Eduard Schoenfeld
        private void Compliment()
        {
            if (Tick - Memory.LastComplimentTick >= SocialCooldownTicks && GrantSocialTrust(5))
            {
                Memory.LastComplimentTick = Tick;
                Memory.Compliments++;
                Npc.TrySetMood(G1C_NpcMood.Happy, Tick);
                Say(Npc.Personality.Pride >= 0.6 ? "Ich weiß. Aber danke." : "Danke, das ist lieb von dir!", G1C_NpcExpression.Happy);
            }
            else
            {
                Say("Das hast du vorhin schon gesagt.", G1C_NpcExpression.Thoughtful);
            }
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Beleidigung – Vertrauen sinkt, NPC merkt es sich, der Ruf leidet öffentlich.
        private void Insult()
        {
            Memory.Insults++;
            Memory.WasEverInsulted = true;
            G1C_NpcPersonality p = Npc.Personality;
            Adjust(-15, p.Pride >= 0.6 ? 0 : -3);
            Npc.TrySetMood(G1C_NpcMood.Angry, Tick, force: true);
            _system.ChangeReputation(-2);
            AddNotice("Ruf -2", false);

            if (p.Pride >= 0.6) Say("Pass auf, wie du mit mir redest!", G1C_NpcExpression.Angry);
            else if (p.RefusesMoneyByRule) Say("Das muss ich mir nicht anhören.", G1C_NpcExpression.Angry);
            else Say("Das ... war gemein.", G1C_NpcExpression.Sad);
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Entschuldigung – macht Feindschaften umkehrbar (mit Wartezeit).
        private void Apologize()
        {
            if (Tick - Memory.LastApologyTick < ApologyCooldownTicks)
            {
                Say("Worte allein reichen mir nicht.", G1C_NpcExpression.Angry);
                ShowRoot();
                return;
            }
            Memory.LastApologyTick = Tick;
            if (Memory.Insults > 0) Memory.Insults--;
            Adjust(8, 0);
            Say("Na gut. Ich nehme das zur Kenntnis.", G1C_NpcExpression.Thoughtful);
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        private void AskForMoney()
        {
            G1C_MoneyRequestResult result = _system.MoneyRequests.Handle(Npc, _system.Money, Tick, _system.Reputation);
            if (result.Granted) AddNotice("+" + result.Amount + " Chips", true);
            Adjust(result.TrustDelta, 0);
            Say(result.Reply, result.Expression);
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        private void OfferTask(G1C_CollectTask task)
        {
            Say(task.Title + ": " + task.Total + "x " + task.ItemName + " im Casino.\nDafür gibt es " + task.Reward +
                " Chips aus der Hauskasse.", G1C_NpcExpression.Thoughtful);
            _options.Clear();
            Selected = 0;
            AddOption("Mach ich!", () =>
            {
                task.Start();
                Say("Danke! Du siehst sie auf dem Boden glitzern. Lauf einfach drüber.", G1C_NpcExpression.Happy);
                ShowRoot();
            });
            AddOption("Lieber nicht.", () => { Say("Schade. Vielleicht später.", G1C_NpcExpression.Neutral); ShowRoot(); });
        }

        private void TaskProgress(G1C_CollectTask task)
        {
            Say("Es fehlen noch " + (task.Total - task.CollectedCount) + "x " + task.ItemName + ". Schau dich im Casino um.",
                G1C_NpcExpression.Neutral);
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Abgabe – Belohnung kommt genau einmal aus der Hauskasse.
        private void TurnInTask(G1C_CollectTask task)
        {
            int reward = task.TurnIn(Tick);
            if (reward > 0)
            {
                _system.Money.Deposit(reward);
                AddNotice("+" + reward + " Chips", true);
                Memory.TasksCompleted++;
                Adjust(task.TrustReward, task.RespectReward);
                Npc.TrySetMood(G1C_NpcMood.Happy, Tick);
                Say("Super, danke! Hier sind deine " + reward + " Chips.", G1C_NpcExpression.Happy);
                _system.Events.OnTaskCompleted(Npc);
            }
            ShowRoot();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Lenas Gesprächsaufgabe, Stufe 1.
        private void AskAboutNerves()
        {
            Say("Ich habe heute fast alles verloren. Nur noch eine Runde,\ndann hole ich es zurück ...", G1C_NpcExpression.Sad);
            _options.Clear();
            Selected = 0;
            AddOption("Mach doch mal eine Pause.", ConvincePause);
            AddOption("Spiel weiter, du schaffst das!", () =>
            {
                Say("Danke ... glaube ich. Das sagen alle hier.", G1C_NpcExpression.Surprised);
                ShowRoot();
            });
            AddOption("Zurück.", () => { Say("Schon gut.", Npc.MoodExpression); ShowRoot(); });
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Lenas Gesprächsaufgabe, Stufe 2 – gelingt nur mit genug Vertrauen.
        private void ConvincePause()
        {
            if (Npc.Relationship.Trust < PauseTaskTrust)
            {
                Say("Du kennst mich doch kaum ... warum sollte ich auf dich hören?", G1C_NpcExpression.Thoughtful);
                ShowRoot();
                return;
            }
            Memory.HelpedByPlayer = true;
            Memory.TasksCompleted++;
            Adjust(10, 6);
            Npc.ChangeBaseMood(G1C_NpcMood.Neutral);
            Npc.TrySetMood(G1C_NpcMood.Happy, Tick, force: true);
            Say("Du hast recht. Ich mache eine Pause.\nDanke, dass du dich kümmerst.", G1C_NpcExpression.Happy);
            _system.Events.OnTaskCompleted(Npc);
            ShowRoot();
        }

        // ------------------------------------------------------------------ Hilfen

        private void Say(string text, G1C_NpcExpression expression)
        {
            Line = text ?? "";
            Expression = expression;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gesprächs-Vertrauen nur bis zur Obergrenze vergeben.
        private bool GrantSocialTrust(int amount)
        {
            int remaining = MaxSocialTrust - Memory.SocialTrustGained;
            if (remaining <= 0) return false;
            int applied = Adjust(Math.Min(amount, remaining), 0);
            Memory.SocialTrustGained += Math.Min(amount, remaining);
            return applied >= 0;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Werte ändern, Hinweise anzeigen, Status neu bestimmen. Gibt die Vertrauensänderung zurück.
        private int Adjust(int trustDelta, int respectDelta)
        {
            int trust = Npc.Relationship.ChangeTrust(trustDelta);
            int respect = Npc.Relationship.ChangeRespect(respectDelta);
            if (trust != 0) AddNotice("Vertrauen " + (trust > 0 ? "+" : "") + trust, trust > 0);
            if (respect != 0) AddNotice("Respekt " + (respect > 0 ? "+" : "") + respect, respect > 0);
            RefreshStatus();
            return trust;
        }

        private void RefreshStatus()
        {
            if (Npc.Relationship.UpdateStatus(Memory, Npc.Personality))
            {
                AddNotice("Beziehung: " + G1C_NpcRelationship.DisplayName(Status), !G1C_NpcRelationship.IsHostile(Status));
            }
        }

        private void AddNotice(string text, bool positive)
        {
            _notices.Add(new G1C_Notice { Text = text, Positive = positive, EndTick = Tick + NoticeTicks });
            if (_notices.Count > 4) _notices.RemoveAt(0);
        }
    }
}
