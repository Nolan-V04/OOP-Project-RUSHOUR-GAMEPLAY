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
        private int currentLevelNumber = 1;
        private const int maxLevel = 10;
        private string currentLevelPath = "";

        private ComboBox levelSelector;

        private Panel mainMenuPanel;
        private Panel rulePanel;
        private Panel gamePanel;
        private Panel boardPanel;

        public MainForm()
        {
            this.Text = "Rush Hour Game";
            this.ClientSize = new Size(900, 650);
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

            int buttonWidth = 300;
            int buttonHeight = 50;
            int spacing = 30;
            int centerX = (this.ClientSize.Width - buttonWidth) / 2;
            int startY = (this.ClientSize.Height - (buttonHeight * 6 + spacing * 2)) / 2;

            var playBtn = new Button
            {
                Text = "Play",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(centerX, startY)
            };

            var ruleBtn = new Button
            {
                Text = "Rule of Play",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(centerX, startY + buttonHeight + spacing)
            };

            var quitBtn = new Button
            {
                Text = "Quit",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(centerX, startY + (buttonHeight + spacing) * 2)
            };

            playBtn.Click += (s, e) =>
            {
                mainMenuPanel.Visible = false;
                gamePanel.Visible = true;
                LoadLevel(currentLevelNumber);
            };

            rulePanel = new DoubleBufferedPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            var ruleLabel = new Label
            {
                Text = "Hướng dẫn chơi:\n\n" +
                       "Mục tiêu: Đưa xe đỏ (X) ra lối thoát bên phải.\n" +
                       "Kéo các xe khác để mở đường.\n" +
                       "Sử dụng chiến lược và logic để giải đố!",
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

            boardPanel = new DoubleBufferedPanel()
            {
                Size = new Size(600, 600),
                Location = new Point(20, 20),
                BackColor = Color.White
            };

            boardPanel.Paint += (s, e) => board?.DrawGraphics(e.Graphics, boardPanel.ClientSize);
            boardPanel.MouseDown += (s, e) => MainForm_MouseDown(s, e);
            boardPanel.MouseMove += (s, e) => MainForm_MouseMove(s, e);
            boardPanel.MouseUp += (s, e) => selectedCar = null;

            var infoPanel = new Panel
            {
                Width = 200,
                Dock = DockStyle.Right,
                BackColor = Color.LightSteelBlue
            };

            var undoBtn = new Button { Text = "Undo", Top = 10, Left = 10, Width = 180, Height = 30 };
            var resetBtn = new Button { Text = "Reset", Top = 50, Left = 10, Width = 180, Height = 30 };
            var loadBtn = new Button { Text = "Load", Top = 90, Left = 10, Width = 180, Height = 30 };
            var backBtn = new Button { Text = "Menu", Top = 130, Left = 10, Width = 180, Height = 30 };
            moveLabel = new Label { Text = "Step: 0", Top = 180, Left = 10, AutoSize = true };
            levelSelector = new ComboBox { Top = 210, Left = 10, Width = 180 };
            for (int i = 1; i <= maxLevel; i++) levelSelector.Items.Add($"level{i}");

            undoBtn.Click += (s, e) => Undo();
            resetBtn.Click += (s, e) => LoadLevel(currentLevelNumber);
            loadBtn.Click += (s, e) => LoadLevelFromFile();
            backBtn.Click += (s, e) =>
            {
                gamePanel.Visible = false;
                mainMenuPanel.Visible = true;
            };
            levelSelector.SelectedIndexChanged += (s, e) =>
            {
                if (levelSelector.SelectedItem is string levelName && int.TryParse(levelName.Replace("level", ""), out int num))
                    LoadLevel(num);
            };

            infoPanel.Controls.AddRange(new Control[] {
                undoBtn, resetBtn, loadBtn, backBtn, moveLabel, levelSelector
            });

            gamePanel.Controls.Add(boardPanel);
            gamePanel.Controls.Add(infoPanel);

            this.Controls.Add(gamePanel);
        }

        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (board == null) return;

            int cw = boardPanel.ClientSize.Width / board.Cols;
            int ch = boardPanel.ClientSize.Height / board.Rows;
            int col = e.X / cw;
            int row = e.Y / ch;

            selectedCar = board.GetCarAt(row, col);
            mouseStart = e.Location;
        }

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (selectedCar == null || board == null || e.Button != MouseButtons.Left) return;

            int dx = (e.X - mouseStart.X) / (boardPanel.ClientSize.Width / board.Cols);
            int dy = (e.Y - mouseStart.Y) / (boardPanel.ClientSize.Height / board.Rows);

            if ((selectedCar.IsHorizontal && dx != 0) || (!selectedCar.IsHorizontal && dy != 0))
            {
                SaveState();
                bool moved = board.TryMoveCar(selectedCar, dx, dy);
                if (moved)
                {
                    moveCount++;
                    moveLabel.Text = $"Step: {moveCount}";
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
                moveLabel.Text = $"Step: {moveCount}";
                boardPanel.Invalidate();
            }
        }

        private void LoadLevel(int levelNumber)
        {
            currentLevelNumber = levelNumber;
            currentLevelPath = $"levels/level{levelNumber}.json";

            LevelMap level = LevelLoader.LoadLevel(currentLevelPath)!;
            board = new Board(level.Size[0], level.Size[1]);
            foreach (var v in level.Vehicles)
                board.AddCar(new Car(v.Name, v.Row, v.Col, v.Length, v.Orientation));
            moveCount = 0;
            moveLabel.Text = "Step: 0";
            history.Clear();
            boardPanel.Invalidate();

            levelSelector.SelectedIndex = levelNumber - 1;
        }

        private void LoadLevelFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LevelMap level = LevelLoader.LoadLevel(ofd.FileName)!;
                board = new Board(level.Size[0], level.Size[1]);
                foreach (var v in level.Vehicles)
                    board.AddCar(new Car(v.Name, v.Row, v.Col, v.Length, v.Orientation));
                moveCount = 0;
                moveLabel.Text = "Step: 0";
                history.Clear();
                boardPanel.Invalidate();
            }
        }

        private void ShowWinDialog()
        {
            if (currentLevelNumber < maxLevel)
            {
                var result = MessageBox.Show(
                    $"Bạn đã thắng level {currentLevelNumber}!\nChuyển sang level {currentLevelNumber + 1}?",
                    "Chiến thắng",
                    MessageBoxButtons.YesNo
                );

                if (result == DialogResult.Yes)
                {
                    LoadLevel(currentLevelNumber + 1);
                }
                else
                {
                    gamePanel.Visible = false;
                    mainMenuPanel.Visible = true;
                }
            }
            else
            {
                MessageBox.Show("Bạn đã phá đảo toàn bộ 10 màn chơi! Xuất sắc!", "Hoàn thành");
                gamePanel.Visible = false;
                mainMenuPanel.Visible = true;
            }
        }
    }
}
