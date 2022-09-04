using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Minesweeper {
	/// <summary>
	/// Interaction logic for Game.xaml
	/// </summary>
	public partial class Game : Window {
		int gridSizeX = 0, gridSizeY = 0;
		int[,] gameGrid;
		int[,] visibleCells;
		List<Vec2> minePositions = new List<Vec2>();
		Dictionary<Button, Vec2> flagPositions = new Dictionary<Button, Vec2>();
		public Game(int _x, int _y) {
			InitializeComponent();

			gridSizeX = _x;
			gridSizeY = _y;
			gameGrid = new int[gridSizeX, gridSizeY];
			visibleCells = new int[gridSizeX, gridSizeY];

			// Define grid in WPF
			for (int i = 0; i < gridSizeX; i++) {
				RowDefinition rd = new RowDefinition();
				grid.RowDefinitions.Add(rd);
			}
			for (int i = 0; i < gridSizeY; i++) {
				ColumnDefinition cd = new ColumnDefinition();
				grid.ColumnDefinitions.Add(cd);
			}

			// Initial population of grid
			for (int x = 0; x < gridSizeX; x++) {
				for (int y = 0; y < gridSizeY; y++) {

					// Initialize to grid zero
					gameGrid[x, y] = 0;
					visibleCells[x, y] = 0;

					// Make button
					Button btn = new Button();
					btn.Click += cellButton_click;
					btn.MouseRightButtonUp += cellButton_rightClick;
					Grid.SetRow(btn, y);
					Grid.SetColumn(btn, x);
					grid.Children.Add(btn);
				}
			}

			// Number of mines to place = sqrt(x * y)
			int minesToPlace = (int)Math.Sqrt(gridSizeX * gridSizeY);
			minesToPlace += (gridSizeX * gridSizeY) / 12;

			// Place mines
			Random rand = new Random();
			while (minesToPlace != 0) {
				int x = rand.Next(gridSizeX);
				int y = rand.Next(gridSizeY);

				// If a mine, reiterate
				if (gameGrid[x, y] == 9)
					continue;

				// Set as a mine
				gameGrid[x, y] = 9;
				minePositions.Add(new Vec2(x, y));
				minesToPlace--;
			}

			// grid cells neighbouring the mines
			for (int x = 0; x < gridSizeX; x++) {
				for (int y = 0; y < gridSizeY; y++) {
					adjacentCells(new Vec2(x, y), (cell, adj) => {
						// Make sure cell is a mine
						if (gameGrid[cell.x, cell.y] != 9)
							return;
						// If adjacent is not a mine, increment nearby mine count
						if (gameGrid[adj.x, adj.y] != 9) gameGrid[adj.x, adj.y]++;
					});
				}
			}

			// Debug
			//updateGridButtons();
		}

		public void updateGridButtons() {
			// Update buttons
			foreach (Button btn in grid.Children) {
				int y = Grid.GetRow(btn);
				int x = Grid.GetColumn(btn);
				btn.Content = gameGrid[x, y] != 0 ? gameGrid[x, y].ToString() : "";
				if (gameGrid[x, y] == 9)
					btn.Background = Brushes.IndianRed;
			}
		}

		public void CheckForWin() {
			if (minePositions.Count == flagPositions.Count) {
				// Iterate thorugh mine positions and check for a flag at same location
				foreach (Vec2 pos in minePositions) {
					if (!flagPositions.ContainsValue(pos)) {
						return;
					}
				}
				// Win
				MessageBox.Show("     You win!     ", "Minesweeper");
				this.Close();
			}
		}

		public void adjacentCells(Vec2 pos, Action<Vec2, Vec2> action) {
			/*
			 * Using exceptions instead of conditionals is 2000 times slower
            try { action(pos, new Vec2(pos.x - 1, pos.y    )); } catch { } finally {
            try { action(pos, new Vec2(pos.x    , pos.y - 1)); } catch { } finally {
            try { action(pos, new Vec2(pos.x + 1, pos.y    )); } catch { } finally {
            try { action(pos, new Vec2(pos.x    , pos.y + 1)); } catch { } finally {
            try { action(pos, new Vec2(pos.x - 1, pos.y - 1)); } catch { } finally {
            try { action(pos, new Vec2(pos.x - 1, pos.y + 1)); } catch { } finally {
            try { action(pos, new Vec2(pos.x + 1, pos.y - 1)); } catch { } finally {
            try { action(pos, new Vec2(pos.x + 1, pos.y + 1)); } catch { } finally {
            }}}}}}}}
            */
			// Bounds checking
			if (pos.x > 0)										action(pos, new Vec2(pos.x - 1, pos.y    ));
			if (pos.y > 0)										action(pos, new Vec2(pos.x    , pos.y - 1));
			if (pos.x < gridSizeX - 1)							action(pos, new Vec2(pos.x + 1, pos.y    ));
			if (pos.y < gridSizeY - 1)							action(pos, new Vec2(pos.x    , pos.y + 1));
			if (pos.x > 0 && pos.y > 0)							action(pos, new Vec2(pos.x - 1, pos.y - 1));
			if (pos.x > 0 && pos.y < gridSizeY - 1)				action(pos, new Vec2(pos.x - 1, pos.y + 1));
			if (pos.x < gridSizeX - 1 && pos.y > 0)				action(pos, new Vec2(pos.x + 1, pos.y - 1));
			if (pos.x < gridSizeX - 1 && pos.y < gridSizeY - 1) action(pos, new Vec2(pos.x + 1, pos.y + 1));
		}
		public void adjacentVacantCells(Vec2 pos, Action<Vec2, Vec2> action) {
			// Bounds checking
			if (pos.x > 0)			   action(pos, new Vec2(pos.x - 1, pos.y));
			if (pos.y > 0)			   action(pos, new Vec2(pos.x, pos.y - 1));
			if (pos.x < gridSizeX - 1) action(pos, new Vec2(pos.x + 1, pos.y));
			if (pos.y < gridSizeY - 1) action(pos, new Vec2(pos.x, pos.y + 1));
		}

		private void makeVisible(Button btn, Vec2 pos) {
			btn.Background = Brushes.FloralWhite;
			visibleCells[pos.x, pos.y] = 1;

			if (gameGrid[pos.x, pos.y] > 0 && gameGrid[pos.x, pos.y] < 9) {
				// Paint text according to number of adjacent mines
				switch (gameGrid[pos.x, pos.y]) {
				case 1: { btn.Foreground = Brushes.Blue; } break;
				case 2: { btn.Foreground = Brushes.Green; } break;
				case 3: { btn.Foreground = Brushes.Red; } break;
				case 4: { btn.Foreground = Brushes.Purple; } break;
				}
				btn.Content = gameGrid[pos.x, pos.y];
				btn.FontWeight = FontWeights.Bold;
			}
		}

		// Places flags
		private void cellButton_rightClick(object sender, EventArgs e) {
			Button btn = (Button)sender;
			int y = Grid.GetRow((UIElement)sender);
			int x = Grid.GetColumn((UIElement)sender);
			if (visibleCells[x, y] == 0) {
				if (flagPositions.ContainsKey(btn)) {
					flagPositions.Remove(btn);
					btn.Content = "";
				} else {
					Image img = new Image();
					img.Source = new BitmapImage(new Uri("/res/flag.png", UriKind.Relative));
					btn.Content = img;

					flagPositions.Add(btn, new Vec2(x, y));
				}
			}
			CheckForWin();
		}

		private void cellButton_click(object sender, EventArgs e) {
			
			int y = Grid.GetRow((UIElement)sender);
			int x = Grid.GetColumn((UIElement)sender);

			// Can't click if flagged
			if (flagPositions.ContainsKey((Button)sender))
				return;

			if (gameGrid[x, y] == 0) {
				// Expand area -> Find all connected vacant cells with value 0
				// Add starting point
				Dictionary<Vec2, bool> connectedVacantCells = new Dictionary<Vec2, bool>();
				connectedVacantCells.Add(new Vec2(x, y), false);
				int searchedCells = 0;
				while (searchedCells != connectedVacantCells.Count) {
					List<Vec2> branchToCells = new List<Vec2>();
					foreach (KeyValuePair<Vec2, bool> kv in connectedVacantCells) {
						if (kv.Value == false) {
							branchToCells.Add(kv.Key);
						}
					}
					foreach (Vec2 branchingCell in branchToCells) {
						connectedVacantCells.Remove(branchingCell);
						connectedVacantCells.Add(branchingCell, true);
						adjacentVacantCells(branchingCell, (cell, adj) => {
							// Add all adjacent vacant cells that are equal to 0 (zero)
							if (gameGrid[adj.x, adj.y] == 0 && !connectedVacantCells.ContainsKey(adj)) 
								connectedVacantCells.Add(adj, false);
						});
						searchedCells++;
					}
				}
				// Make all 8 fields around each cell with value 0 visible
				foreach (KeyValuePair<Vec2, bool> kv in connectedVacantCells) {
					foreach (Button btn in grid.Children) {
						if (Grid.GetRow(btn) == kv.Key.y && Grid.GetColumn(btn) == kv.Key.x) {
							makeVisible(btn, kv.Key);
						}
					}
					adjacentCells(kv.Key, (cell, adj) => {
						foreach (Button btn in grid.Children) {
							if (Grid.GetRow(btn) == adj.y && Grid.GetColumn(btn) == adj.x) {
								makeVisible(btn, adj);
							}
						}
					});
				}

			} else if (gameGrid[x, y] > 0 && gameGrid[x, y] < 9) {
				// Show single cell
				// Find button
				foreach (Button btn in grid.Children) {
					if (Grid.GetRow(btn) == y && Grid.GetColumn(btn) == x) {
						// Paint white
						btn.Background = Brushes.FloralWhite;
						// Paint text according to number of adjacent mines
						switch (gameGrid[x, y]) {
						case 1: { btn.Foreground = Brushes.Blue; } break;
						case 2: { btn.Foreground = Brushes.Green; } break;
						case 3: { btn.Foreground = Brushes.Red; } break;
						case 4: { btn.Foreground = Brushes.Purple; } break;
						}
						btn.Content = gameGrid[x, y];
						btn.FontWeight = FontWeights.Bold;
					}
				}
			} else if (gameGrid[x, y] == 9) {
				// Show all mines and turn them red

				foreach (Vec2 mine in minePositions) {
					foreach (Button btn in grid.Children) {
						if (Grid.GetRow(btn) == mine.y && Grid.GetColumn(btn) == mine.x) {
							//makeVisible(btn, mine);
							Image img = new Image();
							img.Source = new BitmapImage(new Uri("/res/mine.png", UriKind.Relative));
							btn.Content = img;
						}
					}

				}
				// Game over
				MessageBox.Show("     Game over!     ", "Minesweeper");
				this.Close();

			}

			CheckForWin();
		}
	}
}
