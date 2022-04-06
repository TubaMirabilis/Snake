using Plugin.SimpleAudioPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Snake
{
    public partial class MainPage : ContentPage
    {
        private Snake snake = new Snake();
        private SnakePoint foodPoint;
        private SwipeGestureRecognizer leftSGR = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        private SwipeGestureRecognizer upSGR = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
        private SwipeGestureRecognizer rightSGR = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        private SwipeGestureRecognizer downSGR = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
        private ISimpleAudioPlayer player1 = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        private ISimpleAudioPlayer player2 = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        private List<SnakePoint> turningPoints = new List<SnakePoint>();
        private int frameTime = 825;
        public MainPage()
        {
            InitializeComponent();
            player1.Load(GetStreamFromFile("beep1.wav"));
            player2.Load(GetStreamFromFile("beep2.wav"));
            theScore.Text = "0";
            diffLabel.Text = "Level 1";
            SpawnSnake();
        }
        private void SpawnSnake()
        {
            foreach(SnakePoint p in snake.snakePoints)
            {
                snakeGrid.Children.Add(new BoxView
                {
                    Color = Color.GreenYellow
                }, p.X, p.Y);
            }
        }
        private void SpawnFood()
        {
            Random rand = new Random();
            int px = rand.Next(0, 21);
            int py = rand.Next(0, 21);
            foodPoint = new SnakePoint(px,py);
            while (snake.snakePoints.Exists(x => x.X == foodPoint.X && x.Y == foodPoint.Y))
            {
                px = rand.Next(0, 21);
                py = rand.Next(0, 21);
                foodPoint = new SnakePoint(px, py);
            }
            snakeGrid.Children.Add(new BoxView
            {
                Color = Color.GreenYellow
            }, foodPoint.X, foodPoint.Y);
        }
        private async Task MoveSnake()
        {
            bool foodExists = true;
            int highScore = Preferences.Get("High Score", 0);
            while (foodExists == true)
            {
                if (App.isPaused == true)
                {
                    GenerateResumeButton();
                    while (App.isPaused == true)
                    {
                        if (App.isPaused == true)
                        {
                            await Task.Delay(frameTime);
                        }
                        else if (App.isPaused == false)
                        {
                            break;
                        }
                    }
                }
                await Task.Delay(frameTime);
                PreventBadInput();
                CheckForFood();
                bool onTurningPoint = false;
                foreach (SnakePoint p in snake.snakePoints)
                {
                    onTurningPoint = turningPoints.Exists(x => x.X == p.X && x.Y == p.Y);
                    if (onTurningPoint == true)
                    {
                        p.Direction = turningPoints.Find(x => x.X == p.X && x.Y == p.Y).Direction;
                    }
                    if (p.Direction == SnakePoint.Bearing.Left)
                    {
                        p.X--;
                    }
                    else if (p.Direction == SnakePoint.Bearing.Up)
                    {
                        p.Y--;
                    }
                    else if (p.Direction == SnakePoint.Bearing.Right)
                    {
                        p.X++;
                    }
                    else if (p.Direction == SnakePoint.Bearing.Down)
                    {
                        p.Y++;
                    }
                }
                foreach (SnakePoint p in turningPoints)
                {
                    if (snake.snakePoints.Exists(x => x.X == p.X && x.Y == p.Y) == false)
                    {
                        turningPoints.Remove(p);
                        break;
                    }
                }
                if (CollisionExists(snake.snakePoints[0])
                    || snake.snakePoints[0].X < 0 || snake.snakePoints[0].X > 20
                    || snake.snakePoints[0].Y < 0 || snake.snakePoints[0].Y > 20)
                {
                    DisplayHighScore(highScore, snake.snakePoints.Count - 5);
                    GenerateTryAgainButton();
                    break;
                }
                if (snake.snakePoints.Count == 441)
                {
                    foodExists = false;
                }
                snakeGrid.Children.Clear();
                snakeGrid.Children.Add(new BoxView
                {
                    Color = Color.GreenYellow
                }, foodPoint.X, foodPoint.Y);
                SpawnSnake();
                player1.Play();
            }
        }
        private void CheckForFood()
        {
            var direction = snake.snakePoints.Last().Direction;
            if (snake.snakePoints[0].X == foodPoint.X && snake.snakePoints[0].Y == foodPoint.Y)
            {
                player2.Play();
                if (snake.snakePoints.Last().Direction == SnakePoint.Bearing.Left)
                {
                    snake.snakePoints.Add(new SnakePoint(snake.snakePoints.Last().X + 1, snake.snakePoints.Last().Y));
                    snake.snakePoints.Last().Direction = direction;
                }
                else if (snake.snakePoints.Last().Direction == SnakePoint.Bearing.Up)
                {
                    snake.snakePoints.Add(new SnakePoint(snake.snakePoints.Last().X, snake.snakePoints.Last().Y + 1));
                    snake.snakePoints.Last().Direction = direction;
                }
                else if (snake.snakePoints.Last().Direction == SnakePoint.Bearing.Right)
                {
                    snake.snakePoints.Add(new SnakePoint(snake.snakePoints.Last().X - 1, snake.snakePoints.Last().Y));
                    snake.snakePoints.Last().Direction = direction;
                }
                else if (snake.snakePoints.Last().Direction == SnakePoint.Bearing.Down)
                {
                    snake.snakePoints.Add(new SnakePoint(snake.snakePoints.Last().X, snake.snakePoints.Last().Y - 1));
                    snake.snakePoints.Last().Direction = direction;
                }
                snakeGrid.Children.Add(new BoxView
                {
                    Color = Color.DarkGreen
                }, foodPoint.X, foodPoint.Y);
                theScore.Text = (snake.snakePoints.Count - 5).ToString();
                SpawnFood();
            }
        }
        private bool CollisionExists(SnakePoint s)
        {
            var overlappingPoints = new List<SnakePoint>();
            foreach (SnakePoint p in snake.snakePoints)
            {
                if (p.X == s.X && p.Y == s.Y)
                {
                    overlappingPoints.Add(p);
                }
            }
            if (overlappingPoints.Count > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void PreventBadInput()
        {
            if (snake.snakePoints[0].Direction == SnakePoint.Bearing.Left)
            {
                upSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                downSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                rightSGR.Threshold = uint.MaxValue;
            }
            else if (snake.snakePoints[0].Direction == SnakePoint.Bearing.Up)
            {
                leftSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                rightSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                downSGR.Threshold = uint.MaxValue;
            }
            else if (snake.snakePoints[0].Direction == SnakePoint.Bearing.Right)
            {
                upSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                downSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                leftSGR.Threshold = uint.MaxValue;
            }
            else if (snake.snakePoints[0].Direction == SnakePoint.Bearing.Down)
            {
                leftSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                rightSGR.Threshold = (uint)SwipeGestureRecognizer.ThresholdProperty.DefaultValue;
                upSGR.Threshold = uint.MaxValue;
            }
        }
        private void DisplayHighScore(int savedScore, int newScore)
        {
            if (savedScore < newScore && newScore > 0)
            {
                diffStack.Children.Add(new Label
                {
                    Text = "That's a new high score!  Well done!",
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                });
                Preferences.Set("High Score", newScore);
            }
            else if (savedScore == newScore && newScore > 0)
            {
                diffStack.Orientation = StackOrientation.Vertical;
                diffStack.Children.Add(new Label
                {
                    Text = "You matched your previous high score!",
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                });
                diffStack.Children.Add(new Label
                {
                    Text = "Thanks for playing!",
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                });
            }
            diffStack.Opacity = 1;
        }
        public void GenerateResumeButton()
        {
            playButton.Clicked -= playButton_Clicked;
            playButton.Clicked += resumeButton_Clicked;
            playButton.Text = "RESUME REPTILIAN OPERATIONS";
            playButton.Opacity = 1;
            playButton.IsEnabled = true;
        }
        private void GenerateTryAgainButton()
        {
            playButton.Clicked -= playButton_Clicked;
            playButton.Clicked -= resumeButton_Clicked;
            playButton.Clicked += NewGameButton_Clicked;
            playButton.Text = "TAP HERE TO PLAY AGAIN";
            playButton.Opacity = 1;
            playButton.IsEnabled = true;
        }
        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    snake.snakePoints[0].Direction = SnakePoint.Bearing.Left;
                    turningPoints.Add(new SnakePoint(snake.snakePoints[0].X, snake.snakePoints[0].Y));
                    turningPoints.Last().Direction = snake.snakePoints[0].Direction;
                    break;
                case SwipeDirection.Up:
                    snake.snakePoints[0].Direction = SnakePoint.Bearing.Up;
                    turningPoints.Add(new SnakePoint(snake.snakePoints[0].X, snake.snakePoints[0].Y));
                    turningPoints.Last().Direction = snake.snakePoints[0].Direction;
                    break;
                case SwipeDirection.Right:
                    snake.snakePoints[0].Direction = SnakePoint.Bearing.Right;
                    turningPoints.Add(new SnakePoint(snake.snakePoints[0].X, snake.snakePoints[0].Y));
                    turningPoints.Last().Direction = snake.snakePoints[0].Direction;
                    break;
                case SwipeDirection.Down:
                    snake.snakePoints[0].Direction = SnakePoint.Bearing.Down;
                    turningPoints.Add(new SnakePoint(snake.snakePoints[0].X, snake.snakePoints[0].Y));
                    turningPoints.Last().Direction = snake.snakePoints[0].Direction;
                    break;
            }
        }
        private async void playButton_Clicked(object sender, EventArgs e)
        {
            snakeGrid.GestureRecognizers.Add(leftSGR);
            snakeGrid.GestureRecognizers.Add(upSGR);
            snakeGrid.GestureRecognizers.Add(rightSGR);
            snakeGrid.GestureRecognizers.Add(downSGR);
            foreach (SwipeGestureRecognizer x in snakeGrid.GestureRecognizers)
            {
                x.Swiped += OnSwiped;
            }
            diffSlider.IsEnabled = false;
            playButton.IsEnabled = false;
            if (App.isPaused == true)
            {
                App.isPaused = false;
            }
            await Task.WhenAll(playButton.FadeTo(0, 500), diffStack.FadeTo(0, 500), diffSlider.FadeTo(0, 500));
            diffStack.Children.Clear();
            SpawnFood();
            MoveSnake();
        }
        private async void resumeButton_Clicked(object sender, EventArgs e)
        {
            App.isPaused = false;
            await playButton.FadeTo(0, 500);
        }
        private async void NewGameButton_Clicked(object sender, EventArgs e)
        {
            var mainPage = new MainPage();
            await Navigation.PushModalAsync(mainPage);
        }
        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var newStep = Math.Round(e.NewValue);
            diffSlider.Value = newStep;
            diffLabel.Text = $"Level {diffSlider.Value}";
            switch (diffSlider.Value)
            {
                case 1:
                    frameTime = 450;
                    break;
                case 2:
                    frameTime = 375;
                    break;
                case 3:
                    frameTime = 300;
                    break;
                case 4:
                    frameTime = 225;
                    break;
                case 5:
                    frameTime = 150;
                    break;
            }
        }
        private Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("Snake.sounds." + filename);
            return stream;
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}