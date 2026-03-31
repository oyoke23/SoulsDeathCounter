using System;
using System.Drawing;
using System.Windows.Forms;
using SoulsDeathCounter.Core;

namespace SoulsDeathCounter.UI
{
    public class MainForm : Form
    {
        private readonly DeathCounterService _service;
        private readonly AppSettings _settings;
        private readonly Label _deathLabel;
        private readonly Label _statusLabel;
        private readonly Timer _updateTimer;

        public MainForm()
        {
            _settings = SettingsManager.Load();
            _service = new DeathCounterService();

            _deathLabel = new Label();
            _statusLabel = new Label();
            _updateTimer = new Timer();

            InitializeForm();
            InitializeControls();
            InitializeEvents();
            InitializeTimer();
        }

        private void InitializeForm()
        {
            Text = "Souls Death Counter";
            Size = new Size(400, 200);
            MinimumSize = new Size(200, 100);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            TopMost = true;
            BackColor = _settings.BackgroundColor;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
            Icon = SystemIcons.Application;
        }

        private void InitializeControls()
        {
            _deathLabel.Text = "...";
            _deathLabel.Font = new Font(_settings.FontFamily, _settings.FontSize, _settings.FontStyle);
            _deathLabel.ForeColor = _settings.FontColor;
            _deathLabel.BackColor = Color.Transparent;
            _deathLabel.AutoSize = false;
            _deathLabel.Dock = DockStyle.Fill;
            _deathLabel.TextAlign = ContentAlignment.MiddleCenter;

            _statusLabel.Text = "Searching for game...";
            _statusLabel.Font = new Font("Segoe UI", 9f);
            _statusLabel.ForeColor = Color.LightGray;
            _statusLabel.BackColor = Color.Transparent;
            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 24;
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(_deathLabel);
            Controls.Add(_statusLabel);
        }

        private void InitializeEvents()
        {
            _service.GameDetected += OnGameDetected;
            _service.GameLost += OnGameLost;
            _service.DeathCountChanged += OnDeathCountChanged;

            FormClosing += (s, e) =>
            {
                _updateTimer.Stop();
                _service.Dispose();
            };

            Resize += (s, e) =>
            {
                AdjustFontSize();
            };
        }

        private void InitializeTimer()
        {
            _updateTimer.Interval = 1000;
            _updateTimer.Tick += (s, e) => _service.Update();
            _updateTimer.Start();
        }

        private void OnGameDetected(string gameName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnGameDetected), gameName);
                return;
            }

            Text = $"Deaths - {gameName}";
            _statusLabel.Text = gameName;
            _statusLabel.ForeColor = Color.LightGreen;
        }

        private void OnGameLost()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnGameLost));
                return;
            }

            Text = "Souls Death Counter";
            _deathLabel.Text = "...";
            _statusLabel.Text = "Searching for game...";
            _statusLabel.ForeColor = Color.LightGray;
        }

        private void OnDeathCountChanged(int count)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(OnDeathCountChanged), count);
                return;
            }

            _deathLabel.Text = count.ToString();
            SettingsManager.SaveDeathCount(count);
        }

        private void AdjustFontSize()
        {
            var maxWidth = ClientSize.Width * 0.9f;
            var maxHeight = (ClientSize.Height - _statusLabel.Height) * 0.8f;

            using (var g = CreateGraphics())
            {
                var fontSize = _settings.FontSize;
                var testFont = new Font(_settings.FontFamily, fontSize, _settings.FontStyle);
                var textSize = g.MeasureString(_deathLabel.Text, testFont);

                while ((textSize.Width > maxWidth || textSize.Height > maxHeight) && fontSize > 10)
                {
                    fontSize -= 2;
                    testFont.Dispose();
                    testFont = new Font(_settings.FontFamily, fontSize, _settings.FontStyle);
                    textSize = g.MeasureString(_deathLabel.Text, testFont);
                }

                _deathLabel.Font.Dispose();
                _deathLabel.Font = testFont;
            }
        }
    }
}
