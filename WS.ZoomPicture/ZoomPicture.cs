using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WS.ZoomPicture
{
    public class ZoomPicture : UserControl
    {
        #region Enums

        public enum ZoomType
        {
            MousePosition,
            ControlCenter,
            ImageCenter
        }

        public enum SizeMode
        {
            Scrollable,
            RatioStretch
        }

        #endregion Enums

        #region Private Variables

        private Rectangle _imageBounds;
        private double _zoomFactor;
        private Image _image;
        private Point _startDrag;
        private bool _dragging;
        private bool _imageInitialized;
        private ZoomType _zoomMode = ZoomType.MousePosition;
        private double _previousZoomFactor;
        private int _mouseWheelDivisor = 4000;
        private int _minimumImageWidth = 10;
        private int _minimumImageHeight = 10;
        private double _maximumZoomFactor = 64;
        private bool _enableMouseWheelZooming = true;
        private bool _enableMouseDragging = true;
        private SizeMode _sizeMode;

        #endregion Private Variables

        #region Constructor

        public ZoomPicture()
        {
            DoubleBuffered = true;
            BackColor = Color.FromKnownColor(KnownColor.AppWorkspace);
            Size = new Size(200, 200);
        }

        #endregion Constructor

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
                Invalidate(_imageBounds);
                _imageBounds.X += e.X - _startDrag.X;
                _imageBounds.Y += e.Y - _startDrag.Y;
                _startDrag = e.Location;
                Invalidate(_imageBounds);
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
            if (EnableMouseWheelZooming && ClientRectangle.Contains(e.Location))
            {
                double zoom = _zoomFactor;
                zoom *= 1 + (double)e.Delta / _mouseWheelDivisor;
                ZoomFactor = zoom;
            }
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_image != null && !_imageInitialized)
            {
                InitializeImage();
            }
            if (_zoomFactor > 4)
            {
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
            else
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            }

            if (Image != null)
            {
                e.Graphics.DrawImage(Image, _imageBounds);
            }
            base.OnPaint(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_image != null)
            {
                _imageInitialized = false;
                InitializeImage();
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_image != null)
            {
                _zoomFactor = FitImageToControl();
                _imageBounds = CenterImageBounds();

                Invalidate();
            }
        }

        #endregion Event Overrides

        #region Private Methods

        private void InitializeImage()
        {
            if (_image != null)
            {
                ZoomFactor = FitImageToControl();
                _imageBounds = CenterImageBounds();
            }
            _imageInitialized = true;

            Invalidate();
        }

        private double ValidateZoomFactor(double zoom)
        {
            zoom = Math.Min(zoom, MaximumZoomFactor);
            if (_image != null)
            {
                if ((int)(_image.Width * zoom) < MinimumImageWidth)
                {
                    zoom = (double)MinimumImageWidth / _image.Width;
                }
                if ((int)(_image.Height * zoom) < MinimumImageHeight)
                {
                    zoom = (double)MinimumImageHeight / _image.Height;
                }
            }
            return zoom;
        }

        private double FitImageToControl()
        {
            if (_image == null || ClientSize == Size.Empty)
            {
                return 1;
            }

            double sourceAspect = (double)_image.Width / _image.Height;
            double targetAspect = (double)ClientSize.Width / ClientSize.Height;
            return sourceAspect > targetAspect
                ? (double)ClientSize.Width / _image.Width
                : (double)ClientSize.Height / _image.Height;
        }

        private Rectangle CenterImageBounds()
        {
            if (_image == null)
            {
                return Rectangle.Empty;
            }
            int w = (int)(_image.Width * _zoomFactor);
            int h = (int)(_image.Height * _zoomFactor);
            int x = (ClientSize.Width - w) / 2;
            int y = (ClientSize.Height - h) / 2;
            return new Rectangle(x, y, w, h);
        }

        private Rectangle GetZoomedBounds()
        {
            Point imageCenter = FindZoomCenter(_zoomMode);

            _previousZoomFactor = (double)_imageBounds.Width / _image.Width;
            if (Math.Abs(_zoomFactor - _previousZoomFactor) > 0.001)
            {
                double zoomRatio = _zoomFactor / _previousZoomFactor;
                _imageBounds.Width = (int)(_imageBounds.Width * zoomRatio);
                _imageBounds.Height = (int)(_imageBounds.Height * zoomRatio);

                Point newPRelative = new Point(
                    (int)(imageCenter.X * zoomRatio),
                    (int)(imageCenter.Y * zoomRatio)
                );

                _imageBounds.X += imageCenter.X - newPRelative.X;
                _imageBounds.Y += imageCenter.Y - newPRelative.Y;
            }
            _previousZoomFactor = _zoomFactor;
            return _imageBounds;
        }

        private Point FindZoomCenter(ZoomType type)
        {
            Point p = Point.Empty;
            switch (type)
            {
                case ZoomType.ControlCenter:
                    p.X = Width / 2 - _imageBounds.X;
                    p.Y = Height / 2 - _imageBounds.Y;
                    break;

                case ZoomType.ImageCenter:
                    p.X = _imageBounds.Width / 2;
                    p.Y = _imageBounds.Height / 2;
                    break;

                case ZoomType.MousePosition:
                    Point mp = PointToClient(MousePosition);
                    p.X = mp.X - _imageBounds.X;
                    p.Y = mp.Y - _imageBounds.Y;
                    break;
            }
            return p;
        }

        private void RatioStretch()
        {
            float pRatio = (float)Width / Height;
            float imRatio = (float)Image.Width / Image.Height;

            if (Width >= Image.Width && Height >= Image.Height)
            {
                Width = Image.Width;
                Height = Image.Height;
            }
            else if (Width > Image.Width && Height < Image.Height)
            {
                Height = Height;
                Width = (int)(Height * imRatio);
            }
            else if (Width < Image.Width && Height > Image.Height)
            {
                Width = Width;
                Height = (int)(Width / imRatio);
            }
            else if (Width < Image.Width && Height < Image.Height)
            {
                if (Width >= Height)
                {
                    if (Image.Width >= Image.Height && imRatio >= pRatio)
                    {
                        Width = Width;
                        Height = (int)(Width / imRatio);
                    }
                    else
                    {
                        Height = Height;
                        Width = (int)(Height * imRatio);
                    }
                }
                else
                {
                    if (Image.Width < Image.Height && imRatio < pRatio)
                    {
                        Height = Height;
                        Width = (int)(Height * imRatio);
                    }
                    else
                    {
                        Width = Width;
                        Height = (int)(Width / imRatio);
                    }
                }
            }
            CenterImage();
        }

        private void Scrollable()
        {
            Width = Image.Width;
            Height = Image.Height;
            CenterImage();
        }

        private void SetLayout()
        {
            if (Image == null) return;

            if (_sizeMode == SizeMode.RatioStretch)
            {
                RatioStretch();
            }
            else
            {
                AutoScroll = false;
                Scrollable();
                AutoScroll = true;
            }
        }

        private void CenterImage()
        {
            int top = (int)((Height - Height) / 2.0);
            int left = (int)((Width - Width) / 2.0);

            if (top < 0) top = 0;
            if (left < 0) left = 0;

            Top = top;
            Left = left;
        }

        #endregion Private Methods

        #region Public Properties

        [Category("_ZoomPicture")]
        [Description("Enable dragging. Set to False if you implement other means of image scrolling.")]
        public bool EnableMouseDragging
        {
            get { return _enableMouseDragging; }
            set { _enableMouseDragging = value; }
        }

        [Category("_ZoomPicture")]
        [Description("Enable mouse wheel zooming. Set to false e.g. if you control zooming with a TrackBar.")]
        public bool EnableMouseWheelZooming
        {
            get { return _enableMouseWheelZooming; }
            set { _enableMouseWheelZooming = true; }
        }

        [Category("_ZoomPicture")]
        [Description("Image to display in the ZoomPicture.")]
        public Image Image
        {
            get { return _image; }
            set
            {
                _image = value;
                if (value != null)
                {
                    InitializeImage();
                }
                else
                {
                    _imageInitialized = false;
                }
            }
        }

        [Browsable(false)]
        [Description("The bounding rectangle of the zoomed image relative to the control origin.")]
        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        [Browsable(false)]
        [Description("Location of the top left corner of the zoomed image relative to the control origin.")]
        public Point ImagePosition
        {
            get { return _imageBounds.Location; }
            set
            {
                Invalidate(_imageBounds);
                _imageBounds.X = value.X;
                _imageBounds.Y = value.Y;
                Invalidate(_imageBounds);
            }
        }

        [Category("_ZoomPicture")]
        [Description("The maximum zoom magnification.")]
        public double MaximumZoomFactor
        {
            get { return _maximumZoomFactor; }
            set { _maximumZoomFactor = value; }
        }

        [Category("_ZoomPicture")]
        [Description("Minimum height of the zoomed image in pixels.")]
        public int MinimumImageHeight
        {
            get { return _minimumImageHeight; }
            set { _minimumImageHeight = value; }
        }

        [Category("_ZoomPicture")]
        [Description("Minimum width of the zoomed image in pixels.")]
        public int MinimumImageWidth
        {
            get { return _minimumImageWidth; }
            set { _minimumImageWidth = value; }
        }

        [Category("_ZoomPicture")]
        [Description("Sets the responsiveness of zooming to the mouse wheel. Choose a lower value for faster zooming.")]
        public int MouseWheelDivisor
        {
            get { return _mouseWheelDivisor; }
            set { _mouseWheelDivisor = value; }
        }

        [Browsable(false)]
        [Description("Linear size of the zoomed image as a fraction of that of the source Image.")]
        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                _zoomFactor = ValidateZoomFactor(value);
                if (_imageInitialized)
                {
                    Invalidate(_imageBounds);
                    _imageBounds = GetZoomedBounds();
                    Invalidate(_imageBounds);
                }
            }
        }

        [Category("_ZoomPicture")]
        [DefaultValue(ZoomType.MousePosition)]
        [Description("Image zooming around the mouse position, image center or control center")]
        public ZoomType ZoomMode
        {
            get { return _zoomMode; }
            set { _zoomMode = value; }
        }

        public SizeMode ImageSizeMode
        {
            get { return _sizeMode; }
            set
            {
                _sizeMode = value;
                AutoScroll = (_sizeMode == SizeMode.Scrollable);
                SetLayout();
            }
        }

        #endregion Public Properties
    }
}