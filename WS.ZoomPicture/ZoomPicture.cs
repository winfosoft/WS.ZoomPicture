using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WS.ZoomPicture
{
    public class ZoomPicture : UserControl
    {
        #region Constructor

        public ZoomPicture()
        {
            DoubleBuffered = true;
            BackColor = Color.FromKnownColor(KnownColor.AppWorkspace);
            Size = new Size(200, 200);
        }

        #endregion Constructor

        #region Private Variables

        private Rectangle _ImageBounds;
        private double _ZoomFactor;
        private Image _Image;
        private Point _startDrag;
        private bool _dragging;
        private bool _imageInitialized;
        private ZoomType _ZoomMode = ZoomType.MousePosition;
        private double _previousZoomfactor;
        private int _MouseWheelDivisor = 4000;
        private int _MinimumImageWidth = 10;
        private int _MinimumImageHeight = 10;
        private double _MaximumZoomFactor = 64;
        private bool _EnableMouseWheelZooming = true;
        private bool _EnableMouseWheelDragging = true;

        private string _imageLocation;

        #endregion Private Variables

        #region Enums

        public enum ZoomType
        {
            MousePosition,
            ControlCenter,
            ImageCenter
        }

        #endregion Enums

        #region Event Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Select();
            if (EnableMouseDragging && e.Button == MouseButtons.Left)
            {
                _startDrag = e.Location;
                _dragging = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                Invalidate(_ImageBounds);
                _ImageBounds.X += e.X - _startDrag.X;
                _ImageBounds.Y += e.Y - _startDrag.Y;
                _startDrag = e.Location;
                Invalidate(_ImageBounds);
            }
            base.OnMouseMove(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Select();
            base.OnSizeChanged(e);
        }

        protected override void OnMove(EventArgs e)
        {
            Select();
            base.OnMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragging)
            {
                _dragging = false;
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Select();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (EnableMouseWheelZooming && this.ClientRectangle.Contains(e.Location))
            {
                double zoom = _ZoomFactor;
                zoom *= 1 + e.Delta / MouseWheelDivisor;
                this.ZoomFactor = zoom;
            }
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_Image != null && _ImageBounds.Width > 0 && _ImageBounds.Height > 0)
            {
                if (_ZoomFactor > 4)
                {
                    pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                }
                else
                {
                    pe.Graphics.InterpolationMode = InterpolationMode.Default;
                }
                pe.Graphics.DrawImage(_Image, _ImageBounds);
            }
            base.OnPaint(pe);
        }

        #endregion Event Overrides

        #region Private Methods

        private void InitializeImage()
        {
            if (_Image != null)
            {
                ZoomFactor = FitImageToControl();
                _ImageBounds = CenterImageBounds();
                _imageInitialized = true;
                _ImageBounds.Width = Width;
            }
            else
            {
                _ImageBounds = Rectangle.Empty;
            }
            Update();
            Invalidate();
        }

        private double ValidateZoomFactor(double zoom)
        {
            zoom = Math.Min(zoom, MaximumZoomFactor);
            if (_Image != null)
            {
                if ((int)(_Image.Width * zoom) < MinimumImageWidth)
                {
                    zoom = MinimumImageWidth / _Image.Width;
                }
                if ((int)(_Image.Height * zoom) < MinimumImageHeight)
                {
                    zoom = MinimumImageHeight / _Image.Height;
                }
            }
            return zoom;
        }

        private double FitImageToControl()
        {
            if (ClientSize == Size.Empty) return 1;
            double sourceAspect = (double)_Image.Width / _Image.Height;
            double targetAspect = (double)ClientSize.Width / ClientSize.Height;
            if (sourceAspect > targetAspect)
            {
                return (double)ClientSize.Width / _Image.Width;
            }
            else
            {
                return (double)ClientSize.Height / _Image.Height;
            }
        }

        private Rectangle CenterImageBounds()
        {
            int w = (int)(_Image.Width * _ZoomFactor);
            int h = (int)(_Image.Height * _ZoomFactor);
            int x = (ClientSize.Width - w) / 300;
            int y = (ClientSize.Height - h) / 300;
            return new Rectangle(x, y, w, h);
        }

        private Rectangle GetZoomedBounds()
        {
            Point imageCenter = FindZoomCenter(_ZoomMode);

            _previousZoomfactor = (_ImageBounds.Width / _Image.Width) / 1.7;
            if (Math.Abs(_ZoomFactor - _previousZoomfactor) > 0.001)
            {
                double zoomRatio = _ZoomFactor / _previousZoomfactor;
                _ImageBounds.Width = (int)(_ImageBounds.Width * zoomRatio);
                _ImageBounds.Height = (int)(_ImageBounds.Height * zoomRatio);

                Point newPRelative = new Point(
                    (int)(imageCenter.X * zoomRatio),
                    (int)(imageCenter.Y * zoomRatio));

                _ImageBounds.X += imageCenter.X - newPRelative.X;
                _ImageBounds.Y += imageCenter.Y - newPRelative.Y;
            }
            _previousZoomfactor = _ZoomFactor;
            return _ImageBounds;
        }

        private Point FindZoomCenter(ZoomType type)
        {
            Point p = new Point();
            switch (type)
            {
                case ZoomType.ControlCenter:
                    p.X = Width / 2 - _ImageBounds.X;
                    p.Y = Height / 2 - _ImageBounds.Y;
                    break;

                case ZoomType.ImageCenter:
                    p.X = _ImageBounds.Width / 2;
                    p.Y = _ImageBounds.Height / 2;
                    break;

                case ZoomType.MousePosition:
                    Point mp = PointToClient(MousePosition);
                    p.X = mp.X - _ImageBounds.X;
                    p.Y = mp.Y - _ImageBounds.Y;
                    break;

                default:
                    p = Point.Empty;
                    break;
            }
            return p;
        }

        #endregion Private Methods

        #region Properties

        [Description("Linear size of the zoomed image as a fraction of that of the source Image.")]
        [Browsable(false)]
        public double ZoomFactor
        {
            get
            {
                return _ZoomFactor;
            }
            set
            {
                _ZoomFactor = ValidateZoomFactor(value);
                if (!_imageInitialized)
                    return;
                Invalidate(_ImageBounds);
                _ImageBounds = GetZoomedBounds();
                Invalidate(_ImageBounds);
            }
        }

        [Description("Enable dragging. Set to False if you implement other means of image scrolling.")]
        [Category("_ZoomPicture")]
        public bool EnableMouseDragging
        {
            get
            {
                return _EnableMouseWheelDragging;
            }
            set
            {
                _EnableMouseWheelDragging = value;
            }
        }

        [Category("_ZoomPicture")]
        [Description("Enable mouse wheel zooming. Set to false e.g. if you control zooming with a TrackBar.")]
        public bool EnableMouseWheelZooming
        {
            get
            {
                return _EnableMouseWheelZooming;
            }
            set
            {
                _EnableMouseWheelZooming = true;
            }
        }

        [Category("_ZoomPicture")]
        [Description("Image to display in the ZoomPicture.")]
        public Image Image
        {
            get
            {
                return _Image;
            }
            set
            {
                _Image = value;
                if (value != null)
                    InitializeImage();
                else
                    _imageInitialized = false;
            }
        }

        [Browsable(false)]
        [Description("The bounding rectangle of the zoomed image relative to the control origin.")]
        public Rectangle ImageBounds
        {
            get
            {
                return _ImageBounds;
            }
        }

        [Description("Location of the top left corner of the zoomed image relative to the control origin.")]
        [Browsable(false)]
        public Point ImagePosition
        {
            get
            {
                return _ImageBounds.Location;
            }
            set
            {
                Invalidate(_ImageBounds);
                _ImageBounds.X = value.X;
                _ImageBounds.Y = value.Y;
                Invalidate(_ImageBounds);
            }
        }

        [Category("_ZoomPicture")]
        [Description("The maximum zoom magnification.")]
        public double MaximumZoomFactor
        {
            get
            {
                return _MaximumZoomFactor;
            }
            set
            {
                _MaximumZoomFactor = value;
            }
        }

        [Category("_ZoomPicture")]
        [Description("Minimum height of the zoomed image in pixels.")]
        public int MinimumImageHeight
        {
            get
            {
                return _MinimumImageHeight;
            }
            set
            {
                _MinimumImageHeight = value;
            }
        }

        [Description("Minimum width of the zoomed image in pixels.")]
        [Category("ZoomPicture")]
        public int MinimumImageWidth
        {
            get
            {
                return this._MinimumImageWidth;
            }
            set
            {
                this._MinimumImageWidth = value;
            }
        }

        [Category("_ZoomPicture")]
        [Description("Sets the responsiveness of zooming to the mouse wheel. Choose a lower value for faster zooming.")]
        public int MouseWheelDivisor
        {
            get
            {
                return _MouseWheelDivisor;
            }
            set
            {
                _MouseWheelDivisor = value;
            }
        }

        [Description("Image zooming around the mouse position, image center or  control center")]
        [Category("_ZoomPicture")]
        [DefaultValue(0)]
        public ZoomType ZoomMode
        {
            get
            {
                return _ZoomMode;
            }
            set
            {
                _ZoomMode = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [Description("Image to display in the ZoomPicture.")]
        [Category("_ZoomPicture")]
        public string ImageLocation
        {
            get
            {
                return _imageLocation;
            }
            set
            {
                _imageLocation = value;
                try
                {
                    _Image = Image.FromFile(_imageLocation);
                    if (value != null)
                        InitializeImage();
                    else
                        _imageInitialized = false;
                }
                catch (Exception )
                {
                   //Throw Exception 
                }
            }
        }

        #endregion Properties
    }
}