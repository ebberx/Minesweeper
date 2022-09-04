using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Minesweeper {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		TextBox xbox, ybox;
		public MainWindow() {
			InitializeComponent();

			// Iamge
			Image img = new Image();
			img.Source = new BitmapImage(new Uri("/res/mine.png", UriKind.Relative));
			grid.Children.Add(img);

			// Size message
			Label maxSizeLabel = new Label();
			maxSizeLabel.Content = "Max size is 42x42";
			maxSizeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			maxSizeLabel.Margin = new Thickness(0, 0, 0, 0);
			Grid.SetRow(maxSizeLabel, 1);
			grid.Children.Add(maxSizeLabel);

			// Input fields + labels
			xbox = new TextBox();
			ybox = new TextBox();
			xbox.Margin = new Thickness(-50, -100, 0, 0);
			ybox.Margin = new Thickness(50, -100, 0, 0);
			xbox.Width = 30;
			xbox.Height = 20;
			ybox.Width = 30;
			ybox.Height = 20;

			Label xlabel = new Label();
			Label ylabel = new Label();
			xlabel.Content = "X:";
			ylabel.Content = "Y:";
			xlabel.Margin = new Thickness(-100, 26, 0, 0);
			ylabel.Margin = new Thickness(0, 26, 0, 0);
			xlabel.HorizontalAlignment = HorizontalAlignment.Center;
			ylabel.HorizontalAlignment = HorizontalAlignment.Center;

			Grid.SetRow(xbox, 1);
			Grid.SetRow(ybox, 1);
			Grid.SetRow(xlabel, 1);
			Grid.SetRow(ylabel, 1);

			grid.Children.Add(xbox);
			grid.Children.Add(ybox);
			grid.Children.Add(xlabel);
			grid.Children.Add(ylabel);

			// Start game button
			Button start_but = new Button();
			start_but.Width = 200;
			start_but.Height = 50;
			start_but.Content = "Start Game";
			start_but.Click += start_but_Click;
			Grid.SetRow(start_but, 1);
			grid.Children.Add(start_but);


		}

		public void start_but_Click(object sender, RoutedEventArgs e) {

			// Empty/Null? yeeeeet
			if (String.IsNullOrEmpty(xbox.Text) || String.IsNullOrEmpty(ybox.Text))
				return;

			///////////////////////////////////////////////////////////////////
			/// Check if text boxes contain letters

			// Parse values from text fields
			int x = 0, y = 0;
			try {
				x = Int32.Parse(xbox.Text); y = Int32.Parse(ybox.Text);
			} catch (Exception ex) {
				// who needs exceptions anyway
			}

			// Value boundary checking
			if (x <= 0 || x > 42 || y <= 0 || y > 42)
				return;

			// Start the game
			Game game = new Game(x, y);
			game.Show();
			game.Closed += (object? sender, EventArgs e) => { this.Show(); };
			this.Hide();
		}
	}
}
