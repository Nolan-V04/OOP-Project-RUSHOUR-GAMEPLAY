using System.Collections.Generic;
using System.Text;
using RushHourGame.GameCore;

namespace RushHourGame.GameCore
{
    public static class Algorithm
    {
        public static int FindShortestSolution(Board board)
        {
            var start = board.CloneCars();
            var queue = new PriorityQueue<(List<Car> state, int g), int>();
            var visited = new HashSet<string>();
            queue.Enqueue((start, 0), Heuristic(board, start));
            visited.Add(StateKey(start));

            while (queue.Count > 0)
            {
                var (current, g) = queue.Dequeue();
                if (IsWinState(board, current)) return g;

                foreach (var next in GenerateNextStates(board, current))
                {
                    string key = StateKey(next);
                    if (!visited.Contains(key))
                    {
                        visited.Add(key);
                        int h = Heuristic(board, next);
                        queue.Enqueue((next, g + 1), g + 1 + h);
                    }
                }
            }
            return -1;
        }

        private static string StateKey(List<Car> cars)
        {
            var sb = new StringBuilder();
            foreach (var c in cars)
                sb.Append($"{c.Name}:{c.Row}:{c.Col};");
            return sb.ToString();
        }

        private static IEnumerable<List<Car>> GenerateNextStates(Board board, List<Car> cars)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                var car = cars[i];
                for (int d = -1; d <= 1; d += 2)
                {
                    for (int step = 1; step <= 4; step++)
                    {
                        var clone = cars.Select(c => c.Clone()).ToList();
                        var movingCar = clone[i];
                        int dx = movingCar.IsHorizontal ? d * step : 0;
                        int dy = movingCar.IsHorizontal ? 0 : d * step;
                        if (CanMove(board, clone, movingCar, dx, dy))
                        {
                            movingCar.Row += dy;
                            movingCar.Col += dx;
                            yield return clone;
                        }
                        else break;
                    }
                }
            }
        }

        private static bool IsWinState(Board board, List<Car> cars)
        {
            var main = cars.FirstOrDefault(c => c.Name == "X");
            return main != null && main.Col + main.Length == board.Cols;
        }

        private static int Heuristic(Board board, List<Car> cars)
        {
            var main = cars.First(c => c.Name == "X");
            int dist = board.Cols - (main.Col + main.Length);
            int block = 0;
            for (int c = main.Col + main.Length; c < board.Cols; c++)
                if (cars.Any(car => car != main && car.Occupies(main.Row, c)))
                    block++;
            return dist + block;
        }

        private static bool CanMove(Board board, List<Car> cars, Car car, int dx, int dy)
        {
            int newRow = car.Row + dy;
            int newCol = car.Col + dx;
            for (int i = 0; i < car.Length; i++)
            {
                int r = newRow + (car.IsHorizontal ? 0 : i);
                int c = newCol + (car.IsHorizontal ? i : 0);
                if (r < 0 || r >= board.Rows || c < 0 || c >= board.Cols ||
                    cars.Any(other => other != car && other.Occupies(r, c)))
                    return false;
            }
            return true;
        }
    }
}