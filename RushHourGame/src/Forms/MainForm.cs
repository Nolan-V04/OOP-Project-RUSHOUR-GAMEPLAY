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

        public MainForm()
        {
            
            this.Text = "Rush Hour Game";
            this.ClientSize = new Size(600, 600);
            this.DoubleBuffered = true;

            // UI
            var undoBtn = new Button { Text = "Undo", Left = 10, Top = 10, Width = 80 };
            var resetBtn = new Button { Text = "Reset", Left = 100, Top = 10, Width = 80 };
            var loadBtn = new Button { Text = "Load", Left = 190, Top = 10, Width = 80 };
            moveLabel = new Label { Text = "Bước: 0", Left = 280, Top = 15, AutoSize = true };

            levelSelector = new ComboBox { Left = 360, Top = 10, Width = 100 };
            for (int i = 1; i <= 10; i++) levelSelector.Items.Add($"level{i}");
            levelSelector.SelectedIndexChanged += (s, e) =>
            {
                if (levelSelector.SelectedItem is string levelName)
                    LoadLevel($"levels/{levelName}.json");
            };

            undoBtn.Click += (s, e) => Undo();
            resetBtn.Click += (s, e) => ReloadLevel();
            loadBtn.Click += (s, e) => LoadLevelFromFile();

            this.Controls.AddRange(new Control[] { undoBtn, resetBtn, loadBtn, moveLabel, levelSelector });

            // Không dùng trực tiếp MouseEventHandler nữa
            this.Paint += (s, e) => board?.DrawGraphics(e.Graphics, this.ClientSize);
            this.MouseDown += (s, e) => MainForm_MouseDown(s, e);
            this.MouseMove += (s, e) => MainForm_MouseMove(s, e);
            this.MouseUp += (s, e) => selectedCar = null;
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
                    Invalidate();

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
                Invalidate();
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
            Invalidate();
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
            var result = MessageBox.Show("Bạn đã thắng! Chơi lại?", "Chiến thắng", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) ReloadLevel();
            else Application.Exit();
        }
    }
}
