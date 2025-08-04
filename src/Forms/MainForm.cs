using System;
using System.Drawing;
using System.Windows.Forms;
using RushHourGame.GameCore;
using RushHourGame.Models;
using System.Collections.Generic;

namespace RushHourGame.Forms
{
    public class MainForm : Form
    {
        private Board? board;
        private Car? selectedCar = null;
        private Point mouseStart;
        private int moveCount = 0;
        private Label moveLabel;
        private Stack<List<Car>> history = new();
        private string currentLevelPath = "levels/level1.json";
        private ComboBox levelSelector;

        private Panel mainMenuPanel;
        private Panel rulePanel;

        private Panel gamePanel;
        private Panel boardPanel;


        public MainForm()
        {
            this.Text = "Rush Hour Game";
            this.ClientSize = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            InitializeMainMenu();
            InitializeGameUI();

            mainMenuPanel.Visible = true;
            gamePanel.Visible = false;
        }

        private void InitializeMainMenu()
        {
            mainMenuPanel = new DoubleBufferedPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var playBtn = new Button
            {
                Text = "Play",
                Size = new Size(200, 40),
                Location = new Point(200, 120)
            };

            var ruleBtn = new Button
            {
                Text = "Rule of Play",
                Size = new Size(200, 40),
                Location = new Point(200, 180)
            };

            var quitBtn = new Button
            {
                Text = "Quit",
                Size = new Size(200, 40),
                Location = new Point(200, 240)
            };

            playBtn.Click += (s, e) =>
            {
                mainMenuPanel.Visible = false;
                gamePanel.Visible = true;
                ReloadLevel();
            };
            rulePanel = new DoubleBufferedPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            var ruleLabel = new Label
            {
                Text = "📜 Hướng dẫn chơi:\n\n" +
                    "🎯 Mục tiêu: Đưa xe đỏ (X) ra lối thoát bên phải.\n" +
                    "🟥 Kéo các xe khác để mở đường.\n" +
                    "🧠 Sử dụng chiến lược và logic để giải đố!",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            var backBtn = new Button
            {
                Text = "Quay lại Menu",
                Size = new Size(150, 40),
                Location = new Point(50, 250)
            };
            backBtn.Click += (s, e) =>
            {
                rulePanel.Visible = false;
                mainMenuPanel.Visible = true;
            };

            rulePanel.Controls.Add(ruleLabel);
            rulePanel.Controls.Add(backBtn);
            this.Controls.Add(rulePanel);

            

            ruleBtn.Click += (s, e) =>
            {
                mainMenuPanel.Visible = false;
                rulePanel.Visible = true;
            };

            quitBtn.Click += (s, e) => Application.Exit();

            mainMenuPanel.Controls.AddRange(new Control[] { playBtn, ruleBtn, quitBtn });
            this.Controls.Add(mainMenuPanel);
        }

        private void InitializeGameUI()
        {
            gamePanel = new DoubleBufferedPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };

            var topPanel = new DoubleBufferedPanel()
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            boardPanel = new DoubleBufferedPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var undoBtn = new Button { Text = "Undo", Left = 10, Top = 10, Width = 80, Height = 30 };
            var resetBtn = new Button { Text = "Reset", Left = 100, Top = 10, Width = 80, Height = 30 };
            var loadBtn = new Button { Text = "Load", Left = 190, Top = 10, Width = 80, Height = 30 };
            var backBtn = new Button { Text = "Menu", Left = 280, Top = 10, Width = 80, Height = 30 };
            moveLabel = new Label { Text = "Bước: 0", Left = 370, Top = 15, AutoSize = true };

            levelSelector = new ComboBox { Left = 460, Top = 10, Width = 100 };
            for (int i = 1; i <= 10; i++) levelSelector.Items.Add($"level{i}");
            levelSelector.SelectedIndexChanged += (s, e) =>
            {
                if (levelSelector.SelectedItem is string levelName)
                    LoadLevel($"levels/{levelName}.json");
            };

            undoBtn.Click += (s, e) => Undo();
            resetBtn.Click += (s, e) => ReloadLevel();
            loadBtn.Click += (s, e) => LoadLevelFromFile();
            backBtn.Click += (s, e) =>
            {
                gamePanel.Visible = false;
                mainMenuPanel.Visible = true;
            };

            topPanel.Controls.AddRange(new Control[]
            {
                undoBtn, resetBtn, loadBtn, backBtn, moveLabel, levelSelector
            });

            // Gán xử lý bản đồ vào boardPanel
            boardPanel.Paint += (s, e) => board?.DrawGraphics(e.Graphics, boardPanel.ClientSize);
            boardPanel.MouseDown += (s, e) => MainForm_MouseDown(s, e);
            boardPanel.MouseMove += (s, e) => MainForm_MouseMove(s, e);
            boardPanel.MouseUp += (s, e) => selectedCar = null;

            gamePanel.Controls.Add(boardPanel);
            gamePanel.Controls.Add(topPanel);

            this.Controls.Add(gamePanel);
        }


        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (board == null) return;

            int cw = this.ClientSize.Width / board.Cols;
            int ch = this.ClientSize.Height / board.Rows;
            int col = e.X / cw;
            int row = e.Y / ch;

            selectedCar = board.GetCarAt(row, col);
            mouseStart = e.Location;
        }

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (selectedCar == null || board == null || e.Button != MouseButtons.Left) return;

            int dx = (e.X - mouseStart.X) / (this.ClientSize.Width / board.Cols);
            int dy = (e.Y - mouseStart.Y) / (this.ClientSize.Height / board.Rows);

            if ((selectedCar.IsHorizontal && dx != 0) || (!selectedCar.IsHorizontal && dy != 0))
            {
                SaveState();
                bool moved = board.TryMoveCar(selectedCar, dx, dy);
                if (moved)
                {
                    moveCount++;
                    moveLabel.Text = $"Bước: {moveCount}";
                    mouseStart = e.Location;
                    boardPanel.Invalidate();

                    if (board.IsWin())
                        ShowWinDialog();
                }
            }
        }

        private void SaveState()
        {
            if (board != null)
            {
                var snapshot = board.CloneCars();
                history.Push(snapshot);
            }
        }

        private void Undo()
        {
            if (board != null && history.Count > 0)
            {
                board.Cars = history.Pop();
                moveCount = Math.Max(0, moveCount - 1);
                moveLabel.Text = $"Bước: {moveCount}";
                boardPanel.Invalidate();
            }
        }

        private void ReloadLevel() => LoadLevel(currentLevelPath);

        private void LoadLevel(string path)
        {
            currentLevelPath = path;
            LevelMap level = LevelLoader.LoadLevel(path)!;
            board = new Board(level.Size[0], level.Size[1]);
            foreach (var v in level.Vehicles)
                board.AddCar(new Car(v.Name, v.Row, v.Col, v.Length, v.Orientation));
            moveCount = 0;
            moveLabel.Text = "Bước: 0";
            history.Clear();
            boardPanel.Invalidate();
        }

        private void LoadLevelFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadLevel(ofd.FileName);
            }
        }

        private void ShowWinDialog()
        {
            var result = MessageBox.Show("🎉 Bạn đã thắng! Chơi lại?", "Chiến thắng", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) ReloadLevel();
            else Application.Exit();
        }
    }
}
