using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;

namespace VK_UI3.SnowFlake
{
    [TemplatePart(Name = nameof(PART_Canvas), Type = typeof(Canvas))]
    public partial class FireworkEffect : Control
    {
        private const string PART_Canvas = "PART_Canvas";
        private Canvas _canvas;

        private readonly Random _random = new();
        private readonly List<Rocket> _rockets = [];
        private readonly List<Particle> _particles = [];
        private bool _isRunning = false;
        private double _lastSpawnTimeMs;

        // Кэшированные размеры canvas
        private double _canvasWidth;
        private double _canvasHeight;

        // Object Pools
        private readonly Stack<Ellipse> _shapePool = new();
        private readonly Stack<TranslateTransform> _transformPool = new();
        private readonly Stack<Rocket> _rocketPool = new();
        private readonly Stack<Particle> _particlePool = new();

        // Кэш кистей для цветов
        private readonly Dictionary<Color, SolidColorBrush> _colorBrushCache = new();

        // Stopwatch для точного времени
        private readonly Stopwatch _stopwatch = new();

        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register(nameof(AutoStart), typeof(bool), typeof(FireworkEffect),
                new PropertyMetadata(true, OnAutoStartChanged));

        public int RocketCount
        {
            get { return (int)GetValue(RocketCountProperty); }
            set { SetValue(RocketCountProperty, value); }
        }

        public static readonly DependencyProperty RocketCountProperty =
            DependencyProperty.Register(nameof(RocketCount), typeof(int), typeof(FireworkEffect),
                new PropertyMetadata(5));

        public double SpawnInterval
        {
            get { return (double)GetValue(SpawnIntervalProperty); }
            set { SetValue(SpawnIntervalProperty, value); }
        }

        public static readonly DependencyProperty SpawnIntervalProperty =
            DependencyProperty.Register(nameof(SpawnInterval), typeof(double), typeof(FireworkEffect),
                new PropertyMetadata(1.5));

        public double AnimationSpeed
        {
            get { return (double)GetValue(AnimationSpeedProperty); }
            set { SetValue(AnimationSpeedProperty, value); }
        }

        public static readonly DependencyProperty AnimationSpeedProperty =
            DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(FireworkEffect),
                new PropertyMetadata(1.0));

        public bool UseRainbowColors
        {
            get { return (bool)GetValue(UseRainbowColorsProperty); }
            set { SetValue(UseRainbowColorsProperty, value); }
        }

        public static readonly DependencyProperty UseRainbowColorsProperty =
            DependencyProperty.Register(nameof(UseRainbowColors), typeof(bool), typeof(FireworkEffect),
                new PropertyMetadata(true));

        public Color RocketColor
        {
            get { return (Color)GetValue(RocketColorProperty); }
            set { SetValue(RocketColorProperty, value); }
        }

        public static readonly DependencyProperty RocketColorProperty =
            DependencyProperty.Register(nameof(RocketColor), typeof(Color), typeof(FireworkEffect),
                new PropertyMetadata(Color.FromArgb(255, 255, 50, 50)));

        public int MaxParticles
        {
            get { return (int)GetValue(MaxParticlesProperty); }
            set { SetValue(MaxParticlesProperty, value); }
        }

        public static readonly DependencyProperty MaxParticlesProperty =
            DependencyProperty.Register(nameof(MaxParticles), typeof(int), typeof(FireworkEffect),
                new PropertyMetadata(2000));

        private static readonly Color[] RainbowColors = new Color[]
        {
            Color.FromArgb(255, 255, 0, 0),
            Color.FromArgb(255, 255, 165, 0),
            Color.FromArgb(255, 255, 255, 0),
            Color.FromArgb(255, 0, 255, 0),
            Color.FromArgb(255, 0, 191, 255),
            Color.FromArgb(255, 0, 0, 255),
            Color.FromArgb(255, 128, 0, 128),
            Color.FromArgb(255, 255, 192, 203),
        };

        public FireworkEffect()
        {
            DefaultStyleKey = typeof(FireworkEffect);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = GetTemplateChild(PART_Canvas) as Canvas;

            Debug.WriteLine($"OnApplyTemplate: Canvas = {(_canvas != null ? "Found" : "Not Found")}");

            SizeChanged -= OnSizeChanged;
            SizeChanged += OnSizeChanged;

            Unloaded -= OnUnLoaded;
            Unloaded += OnUnLoaded;

            if (AutoStart)
            {
                LayoutUpdated += FireworkEffect_LayoutUpdated;
            }
        }

        private void FireworkEffect_LayoutUpdated(object sender, object e)
        {
            LayoutUpdated -= FireworkEffect_LayoutUpdated;
            Start();
        }

        private static void OnAutoStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (FireworkEffect)d;
            if (ctl != null && (bool)e.NewValue && !ctl._isRunning)
            {
                ctl.Start();
            }
            else if (ctl != null && !(bool)e.NewValue && ctl._isRunning)
            {
                ctl.Stop();
            }
        }

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            Stop();
            SizeChanged -= OnSizeChanged;
            Unloaded -= OnUnLoaded;
        }

        // Object Pool методы
        private Ellipse GetShapeFromPool()
        {
            if (_shapePool.Count > 0)
                return _shapePool.Pop();
            return new Ellipse();
        }

        private void ReturnShapeToPool(Ellipse shape)
        {
            shape.Opacity = 1;
            shape.Fill = null;
            shape.RenderTransform = null;
            _shapePool.Push(shape);
        }

        private TranslateTransform GetTransformFromPool()
        {
            if (_transformPool.Count > 0)
                return _transformPool.Pop();
            return new TranslateTransform();
        }

        private void ReturnTransformToPool(TranslateTransform transform)
        {
            transform.X = 0;
            transform.Y = 0;
            _transformPool.Push(transform);
        }

        private SolidColorBrush GetBrushForColor(Color color)
        {
            if (!_colorBrushCache.TryGetValue(color, out var brush))
            {
                brush = new SolidColorBrush(color);
                _colorBrushCache[color] = brush;
            }
            return brush;
        }

        private Rocket GetRocketFromPool()
        {
            if (_rocketPool.Count > 0)
                return _rocketPool.Pop();
            return new Rocket();
        }

        private void ReturnRocketToPool(Rocket rocket)
        {
            rocket.Shape = null;
            rocket.Transform = null;
            _rocketPool.Push(rocket);
        }

        private Particle GetParticleFromPool()
        {
            if (_particlePool.Count > 0)
                return _particlePool.Pop();
            return new Particle();
        }

        private void ReturnParticleToPool(Particle particle)
        {
            particle.Shape = null;
            particle.Transform = null;
            _particlePool.Push(particle);
        }

        public void Start()
        {
            Debug.WriteLine("Start called");

            if (_isRunning)
            {
                Debug.WriteLine("Already running");
                return;
            }

            if (_canvas == null)
            {
                Debug.WriteLine("Canvas is null, retrying in 100ms");
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    Start();
                };
                timer.Start();
                return;
            }

            if (_canvas.ActualWidth == 0 || _canvas.ActualHeight == 0)
            {
                Debug.WriteLine($"Canvas size is zero: {_canvas.ActualWidth}x{_canvas.ActualHeight}, retrying");
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    Start();
                };
                timer.Start();
                return;
            }

            _canvasWidth = _canvas.ActualWidth;
            _canvasHeight = _canvas.ActualHeight;

            Debug.WriteLine($"Starting fireworks on canvas {_canvasWidth}x{_canvasHeight}");

            _isRunning = true;
            _stopwatch.Restart();
            _lastSpawnTimeMs = 0;

            ClearAll();

            CompositionTarget.Rendering -= UpdateFireworks;
            CompositionTarget.Rendering += UpdateFireworks;

            for (int i = 0; i < Math.Min(RocketCount, 3); i++)
            {
                SpawnRocket();
            }

            Debug.WriteLine($"Started with {_rockets.Count} rockets");
        }

        public void Stop()
        {
            Debug.WriteLine("Stop called");

            if (!_isRunning) return;

            _isRunning = false;
            _stopwatch.Stop();
            CompositionTarget.Rendering -= UpdateFireworks;
            ClearAll();
        }

        public void ClearAll()
        {
            if (_canvas == null) return;

            foreach (var rocket in _rockets)
            {
                if (rocket.Shape != null)
                {
                    _canvas.Children.Remove(rocket.Shape);
                    ReturnShapeToPool(rocket.Shape);
                }
                if (rocket.Transform != null)
                    ReturnTransformToPool(rocket.Transform);
                ReturnRocketToPool(rocket);
            }

            foreach (var particle in _particles)
            {
                if (particle.Shape != null)
                {
                    _canvas.Children.Remove(particle.Shape);
                    ReturnShapeToPool(particle.Shape);
                }
                if (particle.Transform != null)
                    ReturnTransformToPool(particle.Transform);
                ReturnParticleToPool(particle);
            }

            _rockets.Clear();
            _particles.Clear();
        }

        private void SpawnRocket()
        {
            if (_canvas == null || _rockets.Count >= RocketCount) return;

            var rocketShape = GetShapeFromPool();
            var transform = GetTransformFromPool();

            double startX = _random.NextDouble() * _canvasWidth;
            double targetX = _random.NextDouble() * _canvasWidth;
            double targetY = _random.NextDouble() * (_canvasHeight * 0.3) + 50;

            Color rocketColor = UseRainbowColors
                ? RainbowColors[_random.Next(RainbowColors.Length)]
                : RocketColor;

            rocketShape.Width = 4;
            rocketShape.Height = 8;
            rocketShape.Fill = GetBrushForColor(rocketColor);
            rocketShape.Opacity = 1;

            transform.X = startX - 2;
            transform.Y = _canvasHeight - 4;
            rocketShape.RenderTransform = transform;

            _canvas.Children.Add(rocketShape);

            var rocket = GetRocketFromPool();
            rocket.Shape = rocketShape;
            rocket.Transform = transform;
            rocket.X = startX;
            rocket.Y = _canvasHeight;
            rocket.StartX = startX;
            rocket.StartY = _canvasHeight;
            rocket.TargetX = targetX;
            rocket.TargetY = targetY;
            rocket.ControlPointX = (startX + targetX) / 2 + (_random.NextDouble() - 0.5) * 200;
            rocket.Progress = 0;
            rocket.BaseSpeed = 0.008 + _random.NextDouble() * 0.012;
            rocket.Color = rocketColor;
            rocket.ExplodeAt = 0.4 + _random.NextDouble() * 0.5;

            _rockets.Add(rocket);
        }

        private void Explode(Rocket rocket)
        {
            if (_canvas == null) return;

            int particleCount = _random.Next(30, 80);
            double explodeX = rocket.X;
            double explodeY = rocket.Y;

            Color[] colors = UseRainbowColors ? RainbowColors : null;

            for (int i = 0; i < particleCount; i++)
            {
                var particleShape = GetShapeFromPool();
                var transform = GetTransformFromPool();

                double angle = _random.NextDouble() * Math.PI * 2;
                double speed = _random.NextDouble() * 8 + 2;
                double size = _random.NextDouble() * 4 + 2;
                double lifeTime = 1.5 + _random.NextDouble() * 2;

                Color particleColor = colors != null
                    ? colors[_random.Next(colors.Length)]
                    : rocket.Color;

                particleShape.Width = size;
                particleShape.Height = size;
                particleShape.Fill = GetBrushForColor(particleColor);
                particleShape.Opacity = 1;

                transform.X = explodeX - size / 2;
                transform.Y = explodeY - size / 2;
                particleShape.RenderTransform = transform;

                _canvas.Children.Add(particleShape);

                var particle = GetParticleFromPool();
                particle.Shape = particleShape;
                particle.Transform = transform;
                particle.X = explodeX;
                particle.Y = explodeY;
                particle.VelX = Math.Cos(angle) * speed;
                particle.VelY = Math.Sin(angle) * speed;
                particle.Life = lifeTime;
                particle.MaxLife = lifeTime;
                particle.Size = size;
                particle.Color = particleColor;
                particle.Gravity = _random.NextDouble() * 0.3 + 0.15;

                _particles.Add(particle);
            }

            if (rocket.Shape != null)
            {
                _canvas.Children.Remove(rocket.Shape);
                ReturnShapeToPool(rocket.Shape);
            }
            if (rocket.Transform != null)
                ReturnTransformToPool(rocket.Transform);
            ReturnRocketToPool(rocket);
            _rockets.Remove(rocket);
        }

        private void UpdateFireworks(object sender, object e)
        {
            if (!_isRunning) return;
            if (_canvas == null || _canvasWidth == 0 || _canvasHeight == 0) return;

            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            double deltaTime = 0.016 * AnimationSpeed;

            // Спавн новых ракет
            if (_rockets.Count < RocketCount && (currentTime - _lastSpawnTimeMs) >= SpawnInterval)
            {
                SpawnRocket();
                _lastSpawnTimeMs = currentTime;
            }

            // Обновление ракет
            for (int i = _rockets.Count - 1; i >= 0; i--)
            {
                var rocket = _rockets[i];
                rocket.Progress += rocket.BaseSpeed * AnimationSpeed;

                if (rocket.Progress >= 1 || (rocket.Progress >= rocket.ExplodeAt && rocket.ExplodeAt < 1))
                {
                    Explode(rocket);
                    continue;
                }

                double t = rocket.Progress;
                double mt = 1 - t;
                double mt2 = mt * mt;
                double t2 = t * t;

                rocket.Transform.X = mt2 * rocket.StartX + 2 * mt * t * rocket.ControlPointX + t2 * rocket.TargetX - 2;
                rocket.Transform.Y = mt2 * rocket.StartY + 2 * mt * t * rocket.ControlPointY + t2 * rocket.TargetY - 4;

                rocket.X = rocket.Transform.X + 2;
                rocket.Y = rocket.Transform.Y + 4;
            }

            // Обновление частиц
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Life -= deltaTime;

                if (particle.Life <= 0)
                {
                    if (particle.Shape != null)
                    {
                        _canvas.Children.Remove(particle.Shape);
                        ReturnShapeToPool(particle.Shape);
                    }
                    if (particle.Transform != null)
                        ReturnTransformToPool(particle.Transform);

                    ReturnParticleToPool(particle);
                    _particles.RemoveAt(i);
                    continue;
                }

                particle.VelY += particle.Gravity * AnimationSpeed;

                particle.Transform.X += particle.VelX * AnimationSpeed;
                particle.Transform.Y += particle.VelY * AnimationSpeed;

                float lifeRatio = (float)(particle.Life / particle.MaxLife);

                if (Math.Abs(particle.Shape.Opacity - lifeRatio) > 0.01)
                {
                    particle.Shape.Opacity = lifeRatio;
                }

                particle.X = particle.Transform.X;
                particle.Y = particle.Transform.Y;
            }

            // Ограничение количества частиц
            while (_particles.Count > MaxParticles && _particles.Count > 0)
            {
                var oldest = _particles[0];
                if (oldest.Shape != null)
                {
                    _canvas.Children.Remove(oldest.Shape);
                    ReturnShapeToPool(oldest.Shape);
                }
                if (oldest.Transform != null)
                    ReturnTransformToPool(oldest.Transform);
                ReturnParticleToPool(oldest);
                _particles.RemoveAt(0);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_canvas == null) return;

            _canvasWidth = e.NewSize.Width;
            _canvasHeight = e.NewSize.Height;
            _canvas.Width = _canvasWidth;
            _canvas.Height = _canvasHeight;

            Debug.WriteLine($"Size changed: {_canvasWidth}x{_canvasHeight}");
        }
    }

    internal class Rocket
    {
        public Ellipse? Shape { get; set; }
        public TranslateTransform? Transform { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double ControlPointX { get; set; }
        public double ControlPointY => StartY - Math.Abs(TargetX - StartX) * 0.5;
        public double Progress { get; set; }
        public double BaseSpeed { get; set; }
        public Color Color { get; set; }
        public double ExplodeAt { get; set; }
    }

    internal class Particle
    {
        public Ellipse? Shape { get; set; }
        public TranslateTransform? Transform { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
        public double Size { get; set; }
        public Color Color { get; set; }
        public double Gravity { get; set; }
    }
}