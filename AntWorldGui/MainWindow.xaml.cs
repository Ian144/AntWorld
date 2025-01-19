using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace AntWorldGui;

class AntStateCounts
{
    public int ReturnToNestHungaryCount { get; set; }
    public int ReturnToNestWithFoodCount { get; set; }
    public int SearchingForFoodCount { get; set; }
    public int FollowingTrailCount { get; set; }
    public int InNestCount { get; set; }
    public int DetectedFoodCount { get; set; }
    public int GettingUnstuckCount { get; set; }
}



/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const double NestRadius = 16.0;
    private const double AntRadius = 12.0;

    private const double TrailPointRadius = 16.0;

    //private StreamWriter _logger;
    private readonly VisibleEntityCollection _dcol = new();
    private readonly Random _rnd = new();

    private IEnumerator<Types.AntWorld> _antWorldSeq; // a lazy sequence of antworlds
    private Thread _drawThread;
    private bool _running;

    public MainWindow()
    {
        InitializeComponent();

        myAntWorldDisplayControl.InitializeComponent();

        //_logger = new StreamWriter("antworld.txt");

        myAntWorldDisplayControl.DataContext = _dcol;
    }

    private static Point ConvertCoords(Types.Location loc, double height, double width, double magnification)
    {
        return ConvertCoords(loc.x, loc.y, height, width, magnification);
    }

    private static Point ConvertCoords(double x, double y, double height, double width, double magnification)
    {
        return new Point(x * magnification + width / 2.0, y * magnification + height / 2.0);
    }

    private void btnStartStop_Click(object sender, RoutedEventArgs e)
    {
        if (!_running)
        {
            var maybeConfig = from rvAntsPerNest in numAntsPerNest.Text.GetMaybeInt()
                from rvNumNests in numNests.Text.GetMaybeInt()
                from rvNumFoodItems in numFoodItems.Text.GetMaybeInt()
                from rvNumObstacles in numObstacles.Text.GetMaybeInt()
                from rvWorldSize in sizeX.Text.GetMaybeInt()
                select new
                {
                    AntsPerNest = rvAntsPerNest,
                    NumNests = rvNumNests,
                    NumFoodItems = rvNumFoodItems,
                    NumObstacles = rvNumObstacles,
                    WorldSize = rvWorldSize
                };

            if (!maybeConfig.HasValue())
            {
                txtOutputAnts.Text = "invalid config";
                return;
            }

            var config = maybeConfig.Value();

            btnStartStop.Content = "Stop";

            // generates an infinite sequence of antworld instances
            var antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(
                config.AntsPerNest,
                config.NumNests,
                config.NumFoodItems,
                config.NumObstacles,
                config.WorldSize);

            _antWorldSeq = antWorldSeq.GetEnumerator();

            _running = true;

            //todo: ensure all drawing is in the gui thread, which is undoubtedly not this thread
            var ts = new ThreadStart(() =>
            {
                while (_running)
                {
                    var aw = GetNextAntWorld();
                    DrawAntWorld(aw);
                    //Thread.Sleep(20);
                }
            });

            _drawThread = new Thread(ts);
            _drawThread.Start();
        }
        else
        {
            _running = false;
            btnStartStop.Content = "Start";
        }
    }

    private Types.AntWorld GetNextAntWorld()
    {
        _antWorldSeq.MoveNext();
        return _antWorldSeq.Current;
    }

    private bool RandDraw()
    {
        return _rnd.NextDouble() < 0.01;
    }

    private void DrawAntWorld(Types.AntWorld aw)
    {
                
        myAntWorldDisplayControl.Dispatcher.Invoke(() =>
        {
            myAntWorldDisplayControl.Redraw();
            
            var magnification = zoomer.Value;
            var h = myAntWorldDisplayControl.ActualHeight;
            var w = myAntWorldDisplayControl.ActualWidth;

            _dcol.DataPoints.Clear();
            foreach (var obstacle in aw.obstacles)
            {
                Types.IRadLoc rl = obstacle;

                var convLoc = ConvertCoords(obstacle.loc, h, w, magnification);
                var dp = new VisibleEntity
                {
                    VariableX = convLoc.X, VariableY = convLoc.Y, Radius = rl.GetRadius * magnification,
                    Type = EntityType.Obstacle
                };
                _dcol.DataPoints.Add(dp);
            }

            foreach (var ff in aw.foodItems)
            {
                var convLoc = ConvertCoords(ff.loc, h, w, magnification);
                var dp = new VisibleEntity
                {
                    VariableX = convLoc.X, VariableY = convLoc.Y,
                    Radius = FoodFuncs.CalcFoodItemRadius(ff) * magnification, Type = EntityType.Food
                };
                _dcol.DataPoints.Add(dp);
            }

            foreach (var ff in aw.trails)
                if (RandDraw())
                {
                    var convLoc = ConvertCoords(ff.Key.x, ff.Key.y, h, w, magnification);
                    var dp = new VisibleEntity
                    {
                        VariableX = convLoc.X,
                        VariableY = convLoc.Y,
                        Radius = TrailPointRadius * magnification,
                        Type = EntityType.TrailPoint
                    };
            
                    _dcol.DataPoints.Add(dp);
                }

            //var sb = new StringBuilder();

            foreach (var nest in aw.nests)
            {
                var convLoc = ConvertCoords(nest.Loc, h, w, magnification);
                var dpN = new VisibleEntity
                {
                    VariableX = convLoc.X, VariableY = convLoc.Y, Radius = NestRadius * magnification,
                    Type = EntityType.Nest
                };
                _dcol.DataPoints.Add(dpN);

                foreach (var ant in nest.Ants)
                {
                    //sb.AppendLine(string.Format("({0:0.0},{1:0.0}) - {2}", ant.loc.x, ant.loc.y, Ant.AntStateToString(ant)));
                    var convLocA = ConvertCoords(ant.loc, h, w, magnification);
                    var dp = new VisibleEntity
                    {
                        VariableX = convLocA.X, VariableY = convLocA.Y, Radius = AntRadius * magnification,
                        Type = EntityType.Ant
                    };
                    _dcol.DataPoints.Add(dp);
                }
            }

            //sb.AppendLine("---------------------");
            //_logger.Write(sb.ToString());

            // var allAnts = 
            //     from nest in aw.nests
            //     from ant in nest.Ants
            //     select ant;
            //
            // var ants = allAnts as Types.Ant[] ?? allAnts.ToArray();
            //
            // var counts = new AntStateCounts();
            //
            // foreach (var ant in aw.nests.SelectMany(nest => nest.Ants))
            // {
            //     // Determine the ant's state and increment the corresponding counter
            //     if (ant.state.IsReturnToNestHungary) counts.ReturnToNestHungaryCount++;
            //     else if (ant.state.IsReturnToNestWithFood) counts.ReturnToNestWithFoodCount++;
            //     else if (ant.state.IsSearchingForFood) counts.SearchingForFoodCount++;
            //     else if (ant.state.IsFollowingTrail) counts.FollowingTrailCount++;
            //     else if (ant.state.IsInNest) counts.InNestCount++;
            //     else if (ant.state.IsDetectedFood) counts.DetectedFoodCount++;
            //     else if (ant.state.IsGettingUnStuck) counts.GettingUnstuckCount++;
            // }
            //
            // var msg =
            //     string.Format(
            //         "ReturnToNestHungary: {0}\nReturnToNestWithFood: {1}\nSearchingForFood: {2}\nFollowingTrail: {3}\nInNest: {4}\nDetectedFood: {5}\nGettingUnstuck: {6}\n\nnum trail points: {7}",
            //         counts.ReturnToNestHungaryCount,
            //         counts.ReturnToNestWithFoodCount,
            //         counts.SearchingForFoodCount,
            //         counts.FollowingTrailCount,
            //         counts.InNestCount,
            //         counts.DetectedFoodCount,
            //         counts.GettingUnstuckCount,
            //         aw.trails.Count);
            //
            // txtOutputAnts.Text = msg;
        });
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        myAntWorldDisplayControl.Width = e.NewSize.Width - leftStackPanel.Width;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _running = false;
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !AreAllValidNumericChars(e.Text) && e.Text.Length > 0;
        base.OnPreviewTextInput(e);
    }

    private static bool AreAllValidNumericChars(IEnumerable<char> str)
    {
        return str.All(char.IsDigit);
    }
}

