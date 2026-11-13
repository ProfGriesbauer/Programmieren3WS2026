using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOPGames
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IGamePlayer _CurrentPlayer = null;
        IPaintGame _CurrentPainter = null;
        IGameRules _CurrentRules = null;
        IGamePlayer _CurrentPlayer1 = null;
        IGamePlayer _CurrentPlayer2 = null;

        System.Windows.Threading.DispatcherTimer _PaintTimer = null;

        // Fullscreen view: window settings before entering fullscreen, restored when leaving it.
        bool _IsFullscreen = false;
        WindowState _SavedWindowState;
        WindowStyle _SavedWindowStyle;
        ResizeMode _SavedResizeMode;
        Rect _SavedBounds;
        GridLength _SavedSideColumnWidth;
        Thickness _SavedRootMargin;
        Thickness _SavedCanvasMargin;

        public MainWindow()
        {   
            ArenaGameRegistration.RegisterArenaGame();

            //REGISTER YOUR CLASSES HERE
            //Painters
            OOPGamesManager.Singleton.RegisterPainter(new X_TicTacToePaint());
            OOPGamesManager.Singleton.RegisterPainter(new G10_VierGewinntPaint());
            OOPGamesManager.Singleton.RegisterPainter(new G8_VierGewinntPaint());
            OOPGamesManager.Singleton.RegisterPainter(new G9_VierGewinntPainter());
            OOPGamesManager.Singleton.RegisterPainter(new FG_ConnectFourPaint());
            OOPGamesManager.Singleton.RegisterPainter(new Gruppe2_ConnectFourPainter());
            OOPGamesManager.Singleton.RegisterPainter(new Gruppe2_SubwayPainter());
            OOPGamesManager.Singleton.RegisterPainter(new Gruppe1_ConnectFourPainter());
            OOPGamesManager.Singleton.RegisterPainter(new G5_4GWPainter());
            OOPGamesManager.Singleton.RegisterPainter(new G5_FabrikPainter());
            OOPGamesManager.Singleton.RegisterPainter(new G1_VierGewinntPaint());
            OOPGamesManager.Singleton.RegisterPainter(new Gruppe3DragRacePainter());
            OOPGamesManager.Singleton.RegisterPainter(new HayDayPainter());
            //Rules
            OOPGamesManager.Singleton.RegisterRules(new X_TicTacToeRules());
            OOPGamesManager.Singleton.RegisterRules(new G10_VierGewinntRules());
            OOPGamesManager.Singleton.RegisterRules(new G8_VierGewinntRules());
            OOPGamesManager.Singleton.RegisterRules(new G9_VierGewinntRules());
            OOPGamesManager.Singleton.RegisterRules(new FG_ConnectFourRules());
            OOPGamesManager.Singleton.RegisterRules(new Gruppe2_ConnectFourRules());
            OOPGamesManager.Singleton.RegisterRules(new Gruppe2_SubwayRules());
            OOPGamesManager.Singleton.RegisterRules(new Gruppe1_ConnectFourRules());
            OOPGamesManager.Singleton.RegisterRules(new G5_4GWRules());
            OOPGamesManager.Singleton.RegisterRules(new G5_FabrikRules());
            OOPGamesManager.Singleton.RegisterRules(new G1_VierGewinntRules());
            OOPGamesManager.Singleton.RegisterRules(new Gruppe3DragRaceRules());
            OOPGamesManager.Singleton.RegisterRules(new HayDayRules());
            //Players
            OOPGamesManager.Singleton.RegisterPlayer(new X_TicTacToeHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new X_TicTacToeComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G10_VierGewinntHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G10_VierGewinntComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G8_VierGewinntHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G8_VierGewinntComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G9_Human4GewinntPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G9_Computer4GewinntPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new FG_ConnectFourHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new FG_ConnectFourComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe2_ConnectFourHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe2_ConnectFourComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe2_SubwayHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe1_ConnectFourHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe1_ConnectFourComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G5_4GWHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G5_4GWComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G5_FabrikHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G1_VierGewinntHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new G1_VierGewinntComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe3DragRaceHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new Gruppe3DragRaceComputerPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new HayDayHumanPlayer());

            InitializeComponent();
            PaintList.ItemsSource = OOPGamesManager.Singleton.Painters;
            Player1List.ItemsSource = OOPGamesManager.Singleton.Players;
            Player2List.ItemsSource = OOPGamesManager.Singleton.Players;
            RulesList.ItemsSource = OOPGamesManager.Singleton.Rules;

            // LOCAL ARENA AUTOSTART: optional helper is excluded from Git.
            Loaded += StartArenaLocalScript;

            _PaintTimer = new System.Windows.Threading.DispatcherTimer();
            _PaintTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            _PaintTimer.Tick += _PaintTimer_Tick;
            _PaintTimer.Start();
        }

        private void StartArenaLocalScript(object sender, RoutedEventArgs e)
        {
            Loaded -= StartArenaLocalScript;

            string scriptPath = FindLocalArenaStartupScript();
            if (scriptPath == null)
            {
                return;
            }

            string powerShellPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = powerShellPath,
                WorkingDirectory = System.IO.Path.GetDirectoryName(scriptPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add("-TargetProcessId");
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString());

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        MessageBox.Show("Das Arena-Startskript konnte nicht gestartet werden.",
                            "Arena-Autostart", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show("Das Arena-Startskript konnte nicht gestartet werden:\n" + ex.Message,
                    "Arena-Autostart", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Das Arena-Startskript konnte nicht gestartet werden:\n" + ex.Message,
                    "Arena-Autostart", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string FindLocalArenaStartupScript()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = System.IO.Path.Combine(
                    directory.FullName, "start-arena.local.ps1");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private void _PaintTimer_Tick(object sender, EventArgs e)
        {
            if (_CurrentPainter != null &&
                _CurrentRules != null)
            {
                if (_CurrentPainter is IPaintGame2 &&
                    _CurrentRules.CurrentField != null &&
                    _CurrentRules.CurrentField.CanBePaintedBy(_CurrentPainter))
                {
                    ((IPaintGame2)_CurrentPainter).TickPaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                }

                if (_CurrentRules is IGameRules2)
                {
                    ((IGameRules2)_CurrentRules).TickGameCall();
                }
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            List<IGamePlayer> activePlayers = new List<IGamePlayer>();
            _CurrentPlayer1 = null;
            if (Player1List.SelectedItem is IGamePlayer)
            {
                _CurrentPlayer1 = ((IGamePlayer)Player1List.SelectedItem).Clone();
                _CurrentPlayer1.SetPlayerNumber(1);
                activePlayers.Add(_CurrentPlayer1);
            }
            _CurrentPlayer2 = null;
            if (Player2List.SelectedItem is IGamePlayer)
            {
                _CurrentPlayer2 = ((IGamePlayer)Player2List.SelectedItem).Clone();
                _CurrentPlayer2.SetPlayerNumber(2);
                activePlayers.Add(_CurrentPlayer2);
            }

            _CurrentPlayer = null;
            _CurrentPainter = PaintList.SelectedItem as IPaintGame;
            _CurrentRules = RulesList.SelectedItem as IGameRules;

            OOPGamesManager.Singleton.RegisterActivePlayers(activePlayers);
            OOPGamesManager.Singleton.RegisterActivePainter(_CurrentPainter);
            OOPGamesManager.Singleton.RegisterActiveRules(_CurrentRules);

            if (_CurrentRules is IGameRules2)
            {
                ((IGameRules2)_CurrentRules).StartedGameCall();
            }

            if (_CurrentPainter != null && 
                _CurrentRules != null && _CurrentRules.CurrentField.CanBePaintedBy(_CurrentPainter))
            {
                _CurrentPlayer = _CurrentPlayer1;
                Status.Text = "Game startet!";
                Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                _CurrentRules.ClearField();
                _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                DoComputerMoves();

                // Keys go to the focused control first. Without this the Start button would keep the focus and
                // take Enter and Space as another click, and a list would take the arrow keys.
                PaintCanvas.Focus();
            }
        }

        private void ShowGameState()
        {
            if (_CurrentRules == null)
            {
                return;
            }

            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                Status.Text = "Player " + winner + " Won!";
                return;
            }

            if (!_CurrentRules.MovesPossible)
            {
                Status.Text = "Game Over!";
            }
        }

        private void DoComputerMoves()
        {
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                ShowGameState();
            }
            else
            {
                while (_CurrentRules.MovesPossible &&
                       winner <= 0 &&
                       _CurrentPlayer is IComputerGamePlayer)
                {
                    IPlayMove pm = ((IComputerGamePlayer)_CurrentPlayer).GetMove(_CurrentRules.CurrentField);
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }

                    winner = _CurrentRules.CheckIfPLayerWon();
                    if (winner > 0)
                    {
                        ShowGameState();
                    }
                    else if (!_CurrentRules.MovesPossible)
                    {
                        ShowGameState();
                    }
                }
            }
        }

        // Painters without IPaintGame2 only paint after a move. Repaint them when the canvas size changes
        // (view switch, F11, resizing the window), so the game is drawn for the new size right away.
        // IPaintGame2 painters are skipped: they paint every tick anyway, and an extra PaintGameField call
        // can have side effects.
        private void PaintCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_CurrentPainter != null &&
                !(_CurrentPainter is IPaintGame2) &&
                _CurrentRules != null &&
                _CurrentRules.CurrentField != null &&
                _CurrentRules.CurrentField.CanBePaintedBy(_CurrentPainter))
            {
                _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
            }
        }

        private void PaintCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // A click into the game brings the keyboard focus back, e.g. after a click into a list.
            PaintCanvas.Focus();

            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                ShowGameState();
            }
            else if (!_CurrentRules.MovesPossible)
            {
                ShowGameState();
            }
            else
            {
                if (_CurrentRules.MovesPossible &&
                    _CurrentPlayer is IHumanGamePlayer)
                {
                    IPlayMove pm = ((IHumanGamePlayer)_CurrentPlayer).GetMove(new ClickSelection((int)e.GetPosition(PaintCanvas).X, 
                        (int)e.GetPosition(PaintCanvas).Y, (int)e.ChangedButton), _CurrentRules.CurrentField);
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPainter.PaintGameField(PaintCanvas, _CurrentRules.CurrentField);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }

                    DoComputerMoves();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _PaintTimer.Tick -= _PaintTimer_Tick;
            _PaintTimer.Stop();
            _PaintTimer = null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_CurrentRules is Gruppe2_SubwayRules && e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // F11 belongs to the framework and switches the view, also before a game is started.
            // It is never passed on to the game. Esc is left to the games, many of them use it.
            if (e.Key == Key.F11)
            {
                if (_IsFullscreen) ExitFullscreen(true);
                else EnterFullscreen();
                e.Handled = true;
                return;
            }

            if (_CurrentRules == null) return;
            int winner = _CurrentRules.CheckIfPLayerWon();
            if (winner > 0)
            {
                ShowGameState();
            }
            else if (!_CurrentRules.MovesPossible)
            {
                ShowGameState();
            }
            else
            {
                if (_CurrentRules.MovesPossible &&
                    _CurrentPlayer is IHumanGamePlayer)
                {
                    IPlayMove pm = ((IHumanGamePlayer)_CurrentPlayer).GetMove(new KeySelection(e.Key), _CurrentRules.CurrentField);
                    // The key belongs to the game now. Marked as handled, so that WPF does not also use arrow keys
                    // and Tab to move the keyboard focus away from the game to the next control.
                    e.Handled = true;
                    if (pm != null)
                    {
                        _CurrentRules.DoMove(pm);
                        _CurrentPlayer = _CurrentPlayer == _CurrentPlayer1 ? _CurrentPlayer2 : _CurrentPlayer1;
                        Status.Text = "Player " + _CurrentPlayer.PlayerNumber + "'s turn!";
                    }

                    DoComputerMoves();
                }
            }
        }

        private void FullscreenButton_Checked(object sender, RoutedEventArgs e)
        {
            EnterFullscreen();
        }

        private void NormalViewButton_Checked(object sender, RoutedEventArgs e)
        {
            ExitFullscreen(true);
        }

        // Windows snap (Win+Left/Right) or Win+Down turn the borderless window into a normal window, which would
        // leave the fullscreen layout in a small frameless window. Leave fullscreen, but keep the size and position
        // Windows just chose instead of jumping back to the window from before fullscreen.
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (_IsFullscreen && WindowState == WindowState.Normal)
            {
                ExitFullscreen(false);
            }
        }

        private void EnterFullscreen()
        {
            if (_IsFullscreen) return;

            _SavedWindowState = WindowState;
            _SavedWindowStyle = WindowStyle;
            _SavedResizeMode = ResizeMode;
            // A maximized window reports its maximized size in Left/Top/Width/Height, so its normal size comes from
            // RestoreBounds. A normal (possibly snapped) window is saved as it is shown: RestoreBounds would return
            // the size from before snapping.
            _SavedBounds = WindowState == WindowState.Maximized ? RestoreBounds : new Rect(Left, Top, ActualWidth, ActualHeight);
            _SavedSideColumnWidth = SideColumn.Width;
            _SavedRootMargin = RootGrid.Margin;
            _SavedCanvasMargin = PaintCanvas.Margin;

            // Normal first: only a window that is maximized after becoming borderless covers the taskbar. A window
            // that is already maximized and only becomes borderless keeps the size of the work area.
            // NoResize also hides the resize grip in the bottom right corner.
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;

            // The control row stays visible and spans the whole width below the game.
            SidePanel.Visibility = Visibility.Collapsed;
            SideColumn.Width = new GridLength(0);
            RootGrid.Margin = new Thickness(0);
            PaintCanvas.Margin = new Thickness(0);

            // Set only after the window changes above, so that their StateChanged events are ignored.
            _IsFullscreen = true;
            FullscreenButton.IsChecked = true;
        }

        // restoreWindow: false when Windows itself has already placed the window (snap), which is kept.
        private void ExitFullscreen(bool restoreWindow)
        {
            if (!_IsFullscreen) return;
            // Cleared first, so that the StateChanged events below are ignored.
            _IsFullscreen = false;

            PaintCanvas.Margin = _SavedCanvasMargin;
            RootGrid.Margin = _SavedRootMargin;
            SideColumn.Width = _SavedSideColumnWidth;
            SidePanel.Visibility = Visibility.Visible;

            if (restoreWindow) WindowState = WindowState.Normal;
            WindowStyle = _SavedWindowStyle;
            ResizeMode = _SavedResizeMode;
            if (restoreWindow)
            {
                Left = _SavedBounds.Left;
                Top = _SavedBounds.Top;
                Width = _SavedBounds.Width;
                Height = _SavedBounds.Height;
                WindowState = _SavedWindowState;
            }

            NormalViewButton.IsChecked = true;
        }
    }
}
