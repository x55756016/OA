using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace AgParticles.UserControls
{
    public partial class ParticleControl : UserControl
    {
        #region Private Fields

        List<Particle> particles = new List<Particle>();
        Random R = new Random();
        Brush[] brushes;
        bool isBrushesValid = false;

        #endregion

        public ParticleControl()
        {
            InitializeComponent();

            // initialize DPs
            this.Speed = 20;
            this.OriginVariance = 5;
            this.ParticleSize = 12;
            this.ParticleSizeVariance = 5;
            this.MaxParticleCount = 500;
            this.Density = 0.5;
            this.OffsetX = 400;
            this.OffsetY = 200;
            this.Life = 10;
            this.LifeVariance = 10;
            this.StartColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
            this.EndColor = Color.FromArgb(0x00, 0xFF, 0x00, 0x00);

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        #region Event Handlers

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            UpdateParticles();
        }

        #endregion

        #region Private Methods

        private void GenerateBrushes(int count)
        {
            brushes = new Brush[count];
            Color from = this.StartColor;
            Color to = this.EndColor;

            for (int i = 0; i < count; i++)
            {
                double progress = (1.0 / (double)count) * (double)i;

                if (this.Fuzziness > 0.01)
                {
                    // use gradient brushes to create "fuzziness"

                    RadialGradientBrush r = new RadialGradientBrush();

                    GradientStop stop1 = new GradientStop();
                    stop1.Color = InterpolateColor(this.StartColor, this.EndColor, progress);
                    stop1.Offset = 1 - this.Fuzziness;

                    Color alphaFrom = Color.FromArgb(0x00, StartColor.R, StartColor.G, StartColor.B);
                    Color alphaTo = Color.FromArgb(0x00, EndColor.R, EndColor.G, EndColor.B);

                    GradientStop stop2 = new GradientStop();
                    stop2.Color = InterpolateColor(alphaFrom, alphaTo, progress);
                    stop2.Offset = 1.0;

                    r.GradientStops.Add(stop1);
                    r.GradientStops.Add(stop2);

                    brushes[i] = r;
                }
                else
                {
                    // just solid brushes

                    SolidColorBrush b = new SolidColorBrush(InterpolateColor(StartColor, EndColor, progress));
                    brushes[i] = b;
                }
            }

            isBrushesValid = true;
        }

        double elapsed = 0.1;
        List<Particle> deadList = new List<Particle>();

        private void UpdateParticles()
        {
            if (!isBrushesValid) GenerateBrushes(20);

            // update exsting particles

            deadList.Clear();

            foreach (Particle p in this.particles)
            {
                // calculate the "life" of the particle
                p.Life -= p.Decay * elapsed;

                if (p.Life <= 0.0)
                {
                    deadList.Add(p);
                }
                else
                {
                    // update size
                    p.Size = p.StartSize * (p.Life / p.StartLife);
                    double scale = p.Size / p.StartSize;
                    p.Ellipse.Width = p.Size;
                    p.Ellipse.Height = p.Size;

                    // update position
                    p.Position.X = p.Position.X + (p.Velocity.X * elapsed);
                    p.Position.Y = p.Position.Y + (p.Velocity.Y * elapsed);
                    p.Position.Z = p.Position.Z + (p.Velocity.Z * elapsed);
                    TranslateTransform t = (p.Ellipse.RenderTransform as TranslateTransform);
                    t.X = p.Position.X;
                    t.Y = p.Position.Y;

                    // update color/brush
                    int colorIndex = (int)((double)brushes.Length * scale);
                    p.Ellipse.Fill = brushes[colorIndex];
                }
            }

            // create new particles (up to 10 or MaxParticleCount)

            for (int i = 0; i < 10 && this.particles.Count < this.MaxParticleCount; i++)
            {
                // attempt to recycle ellipses if they are in the deadlist
                if (deadList.Count - 1 >= i)
                {
                    SpawnParticle(deadList[i].Ellipse);
                    deadList[i].Ellipse = null;
                }
                else
                {
                    SpawnParticle(null);
                }
            }

            foreach (Particle p in deadList)
            {
                if (p.Ellipse != null) ParticleHost.Children.Remove(p.Ellipse);
                this.particles.Remove(p);
            }
        }

        private Color InterpolateColor(Color from, Color to, double progress)
        {
            //return Colors.Red;
            byte[] finalBytes = { 0, 0, 0, 0 };
            byte[] fromBytes = { from.A, from.R, from.G, from.B };
            byte[] toBytes = { to.A, to.R, to.G, to.B };

            for (int i = 0; i < 4; i++)
            {
                byte fB = fromBytes[i];
                byte tB = toBytes[i];

                double dif = ((double)(fB - tB) * progress + (double)tB);
                dif = Math.Min(255, Math.Max(0, dif));

                finalBytes[i] = (byte)dif;
            }

            return Color.FromArgb(finalBytes[0], finalBytes[1], finalBytes[2], finalBytes[3]);
        }

        private void SpawnParticle(Ellipse e)
        {
            double x = RandomWithVariance(OffsetX, OriginVariance);
            double y = RandomWithVariance(OffsetY, OriginVariance);
            double z = 10 * (R.NextDouble() * OriginVariance);
            double speed = this.Speed; //RandomWithVariance(this.Speed, this.SpeedVariance);
            double life = RandomWithVariance(this.Life, this.LifeVariance);
            double size = RandomWithVariance(this.ParticleSize, this.ParticleSizeVariance);

            Particle p = new Particle();
            p.Position = new Point3D(x, y, z);
            p.StartLife = life;
            p.Life = life;
            p.StartSize = size;
            p.Size = size;
            p.Decay = 1.0;

            TranslateTransform t;

            if (e != null)
            {
                //e.Fill = brushes[0];
                e.Fill = null;
                e.Width = e.Height = size;
                p.Ellipse = e;

                t = e.RenderTransform as TranslateTransform;
            }
            else
            {
                p.Ellipse = new Ellipse();
                //p.Ellipse.Fill = brushes[0];
                p.Ellipse.Width = p.Ellipse.Height = size;
                this.ParticleHost.Children.Add(p.Ellipse);

                t = new TranslateTransform();
                p.Ellipse.RenderTransform = t;
                p.Ellipse.RenderTransformOrigin = new Point(0.5, 0.5);

            }

            t.X = p.Position.X;
            t.Y = p.Position.Y;

            double velocityMultiplier = (R.NextDouble() + 0.25) * speed;
            double vX = (1.0 - (R.NextDouble() * 2.0)) * velocityMultiplier;
            double vY = (1.0 - (R.NextDouble() * 2.0)) * velocityMultiplier;

            p.Velocity = new Point3D(vX, vY, 0);

            this.particles.Add(p);
        }


        #endregion

        private double RandomWithVariance(double midvalue, double variance)
        {
            double min = Math.Max(midvalue - (variance / 2), 0);
            double max = midvalue + (variance / 2);
            double value = min + ((max - min) * R.NextDouble());
            return value;
        }

        #region Static Methods


        #endregion

        #region StartColor (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public Color StartColor
        {
            get { return (Color)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }
        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register("StartColor", typeof(Color), typeof(ParticleControl), new PropertyMetadata(new PropertyChangedCallback(OnStartColorChanged)));

        private static void OnStartColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParticleControl)d).OnStartColorChanged(e);
        }

        protected virtual void OnStartColorChanged(DependencyPropertyChangedEventArgs e)
        {
            this.isBrushesValid = false;
        }

        #endregion

        #region EndColor (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public Color EndColor
        {
            get { return (Color)GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty, value); }
        }
        public static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register("EndColor", typeof(Color), typeof(ParticleControl), new PropertyMetadata(new PropertyChangedCallback(OnEndColorChanged)));

        private static void OnEndColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParticleControl)d).OnEndColorChanged(e);
        }

        protected virtual void OnEndColorChanged(DependencyPropertyChangedEventArgs e)
        {
            this.isBrushesValid = false;
        }

        #endregion

        #region MaxParticleCount (DependencyProperty)

        public int MaxParticleCount
        {
            get { return (int)GetValue(MaxParticleCountProperty); }
            set { SetValue(MaxParticleCountProperty, value); }
        }
        public static readonly DependencyProperty MaxParticleCountProperty =
            DependencyProperty.Register("MaxParticleCount", typeof(int), typeof(ParticleControl), null);

        #endregion

        #region ParticleSize (DependencyProperty)

        public double ParticleSize
        {
            get { return (double)GetValue(ParticleSizeProperty); }
            set { SetValue(ParticleSizeProperty, value); }
        }
        public static readonly DependencyProperty ParticleSizeProperty =
            DependencyProperty.Register("ParticleSize", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region ParticleSizeVariance (DependencyProperty)

        public double ParticleSizeVariance
        {
            get { return (double)GetValue(ParticleSizeVarianceProperty); }
            set { SetValue(ParticleSizeVarianceProperty, value); }
        }
        public static readonly DependencyProperty ParticleSizeVarianceProperty =
            DependencyProperty.Register("ParticleSizeVariance", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region OffsetX (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double OffsetX
        {
            get { return (double)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); }
        }
        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register("OffsetX", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region OffsetY (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double OffsetY
        {
            get { return (double)GetValue(OffsetYProperty); }
            set { SetValue(OffsetYProperty, value); }
        }
        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register("OffsetY", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region OriginVariance (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double OriginVariance
        {
            get { return (double)GetValue(OriginVarianceProperty); }
            set { SetValue(OriginVarianceProperty, value); }
        }
        public static readonly DependencyProperty OriginVarianceProperty =
            DependencyProperty.Register("OriginVariance", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region Speed (DependencyProperty)

        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region Life (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double Life
        {
            get { return (double)GetValue(LifeProperty); }
            set { SetValue(LifeProperty, value); }
        }
        public static readonly DependencyProperty LifeProperty =
            DependencyProperty.Register("Life", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region LifeVariance (DependencyProperty)

        public double LifeVariance
        {
            get { return (double)GetValue(LifeVarianceProperty); }
            set { SetValue(LifeVarianceProperty, value); }
        }
        public static readonly DependencyProperty LifeVarianceProperty =
            DependencyProperty.Register("LifeVariance", typeof(double), typeof(ParticleControl), null);

        #endregion

        #region Fuzziness (DependencyProperty)

        /// <summary>
        /// A number between 0 and 1 that indicates how fuzzy the particles will appear.
        /// </summary>
        public double Fuzziness
        {
            get { return (double)GetValue(FuzzinessProperty); }
            set { SetValue(FuzzinessProperty, value); }
        }
        public static readonly DependencyProperty FuzzinessProperty =
            DependencyProperty.Register("Fuzziness", typeof(double), typeof(ParticleControl), new PropertyMetadata(new PropertyChangedCallback(OnFuzzinessChanged)));

        private static void OnFuzzinessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParticleControl)d).OnFuzzinessChanged(e);
        }

        protected virtual void OnFuzzinessChanged(DependencyPropertyChangedEventArgs e)
        {
            isBrushesValid = false;
        }

        #endregion

        #region Density (DependencyProperty)

        public double Density
        {
            get { return (double)GetValue(DensityProperty); }
            set { SetValue(DensityProperty, value); }
        }
        public static readonly DependencyProperty DensityProperty =
            DependencyProperty.Register("Density", typeof(double), typeof(ParticleControl), null);

        #endregion

        private class Particle
        {
            public Point3D Position;
            public Point3D Velocity;
            public double StartLife;
            public double Life;
            public double Decay;
            public double StartSize;
            public double Size;
            public Ellipse Ellipse;
        }
    }

    class Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X, Y, Z;
    }

}
