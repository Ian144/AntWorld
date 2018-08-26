using System.Windows;
using System;
using System.Collections.Generic;
using System.Threading;

using Action = System.Action;




namespace AntWorldGui
{
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double NestRadius = 16.0;
        private const double AntRadius = 12.0;
        private const double TrailPointRadius = 16.0;

        private IEnumerator<Types.AntWorld> _antWorldSeq; // a lazy sequence of antworlds
        private readonly Random _rnd = new Random();
        private Thread _drawThread;
        private bool _running = false;
        //private StreamWriter _logger;
        private readonly VisibleEntityCollection _dcol = new VisibleEntityCollection();

        public MainWindow()
        {
            InitializeComponent();

            myAntWorldDisplayControl.InitializeComponent();

            //_logger = new StreamWriter("antworld.txt");

            this.myAntWorldDisplayControl.DataContext = _dcol;
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
                var maybeConfig =   from rvAntsPerNest in numAntsPerNest.Text.GetMaybeInt()
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

                this._antWorldSeq = antWorldSeq.GetEnumerator();

                _running = true;


                //todo: ensure all drawing is in the gui thread, which is undoubtadly not this thread
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
            this._antWorldSeq.MoveNext();
            return this._antWorldSeq.Current;
        }


        private bool RandDraw()
        {
            return _rnd.NextDouble() < 0.05;
        }

        
        private void DrawAntWorld(Types.AntWorld aw)
        {
            Action aa = () =>
                {
                    double magnification = zoomer.Value;
                    var h = this.myAntWorldDisplayControl.ActualHeight;
                    var w = this.myAntWorldDisplayControl.ActualWidth;

                    _dcol.DataPoints.Clear();


                    foreach (var obstacle in aw.obstacles)
                    {
                        var rl = obstacle as Types.IRadLoc;

                        var convLoc = ConvertCoords(obstacle.loc, h, w, magnification);
                        var dp = new VisibleEntity { VariableX = convLoc.X, VariableY = convLoc.Y, Radius = rl.GetRadius * magnification, Type = EntityType.Obstacle };
                        _dcol.DataPoints.Add(dp);
                    }

                    
                    foreach (var ff in aw.foodItems)
                    {
                        var convLoc = ConvertCoords(ff.loc, h, w, magnification);
                        var dp = new VisibleEntity { VariableX = convLoc.X, VariableY = convLoc.Y, Radius = FoodFuncs.CalcFoodItemRadius(ff) * magnification, Type = EntityType.Food };
                        _dcol.DataPoints.Add(dp);
                    }


                    foreach (var ff in aw.trails)
                    {
                        if (this.RandDraw())
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
                    }


                    //var sb = new StringBuilder();

                    foreach (var nest in aw.nests)
                    {
                        var convLoc = ConvertCoords(nest.Loc, h, w, magnification);
                        var dpN = new VisibleEntity { VariableX = convLoc.X, VariableY = convLoc.Y, Radius = NestRadius * magnification, Type = EntityType.Nest };
                        _dcol.DataPoints.Add(dpN);

                        foreach (var ant in nest.Ants)
                        {
                            //sb.AppendLine(string.Format("({0:0.0},{1:0.0}) - {2}", ant.loc.x, ant.loc.y, Ant.AntStateToString(ant)));
                            var convLocA = ConvertCoords(ant.loc, h, w, magnification);
                            var dp = new VisibleEntity { VariableX = convLocA.X, VariableY = convLocA.Y, Radius = AntRadius * magnification, Type = EntityType.Ant };
                            _dcol.DataPoints.Add(dp);
                        }
                    }

                    //sb.AppendLine("---------------------");
                    //_logger.Write(sb.ToString());

                    var allAnts = from nest in aw.nests
                                  from ant in nest.Ants
                                  select ant;

                    int returnToNestHungaryCount = allAnts.Where(_ => _.state.IsReturnToNestHungary).Count();
                    int returnToNestWithFoodCount = allAnts.Where(_ => _.state.IsReturnToNestWithFood).Count();
                    int searchingForFoodCount = allAnts.Where(_ => _.state.IsSearchingForFood).Count();
                    int followingTrailCount = allAnts.Where(_ => _.state.IsFollowingTrail).Count();
                    int inNestCount = allAnts.Where(_ => _.state.IsInNest).Count();
                    int detectedFoodCount = allAnts.Where(_ => _.state.IsDetectedFood).Count();
                    int gettingUnstuckCount = allAnts.Where(_ => _.state.IsGettingUnStuck).Count();

                    var msg =
                        String.Format(
                            "ReturnToNestHungary: {0}\nReturnToNestWithFood: {1}\nSearchingForFood: {2}\nFollowingTrail: {3}\nInNest: {4}\nDetectedFood: {5}\nGettingUnstuck: {6}\n\nnum trail points: {7}",
                            returnToNestHungaryCount,
                            returnToNestWithFoodCount,
                            searchingForFoodCount,
                            followingTrailCount,
                            inNestCount,
                            detectedFoodCount,
                            gettingUnstuckCount,
                            aw.trails.Count);

                    txtOutputAnts.Text = msg;



                    this.myAntWorldDisplayControl.Redraw();
                };

            this.myAntWorldDisplayControl.Dispatcher.Invoke(aa);
        }



        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            myAntWorldDisplayControl.Width = e.NewSize.Width - this.leftStackPanel.Width;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _running = false;
        }

        
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text) && (e.Text.Length > 0);
            base.OnPreviewTextInput(e);
        }


        private static bool AreAllValidNumericChars(IEnumerable<char> str)
        {
            return str.All( Char.IsDigit );
        }
    }
}








