using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WS.ZoomPicture
{
    public class ZoomPicture : UserControl
    {
        #region Enums

        // Defines zoom behavior types.
        public enum ZoomType
        {
            MousePosition,  // Zoom relative to mouse position.
            ControlCenter,  // Zoom relative to control center.
            ImageCenter     // Zoom relative to image center.
        }

        // Defines image size adjustment modes.
        public enum SizeMode
        {
            Scrollable,     // Image is scrollable within the control.
            RatioStretch    // Image is stretched while maintaining aspect ratio.
        }

        #endregion Enums

        #region Private Variables

        private Rectangle _imageBounds;                      // Bounds of the zoomed image.
        private double _zoomFactor;                          // Current zoom factor.
        private Image _image;                                // The image to display.
        private Point _startDrag;                            // Starting point for dragging.
        private bool _dragging;                              // Indicates if dragging is active.
        private bool _imageInitialized;                      // Indicates if the image is initialized.
        private ZoomType _zoomMode = ZoomType.MousePosition; // Default zoom mode.
        private double _previousZoomFactor;                  // Previous zoom factor for calculations.
        private int _mouseWheelDivisor = 4000;               // Divisor for mouse wheel zoom sensitivity.
        private int _minimumImageWidth = 100;                // Minimum width of the zoomed image.
        private int _minimumImageHeight = 100;               // Minimum height of the zoomed image.
        private double _maximumZoomFactor = 64;              // Maximum allowed zoom factor.
        private bool _enableMouseWheelZooming = true;        // Enables mouse wheel zooming.
        private bool _enableMouseDragging = true;            // Enables mouse dragging.
        private SizeMode _sizeMode;                          // Current size adjustment mode.

        #endregion Private Variables

        #region Constructor

        public ZoomPicture()
        {
            DoubleBuffered = true;               // Enable double buffering for smooth rendering.
            BackColor = Color.FromKnownColor(KnownColor.AppWorkspace); // Set default background color.
            Size = new Size(200, 200);           // Set default control size.
        }

        #endregion Constructor

        #region Event Overrides

        // Handles mouse down event for dragging.
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

        // Handles mouse move event for dragging.
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

        // Handles control resize event.
        protected override void OnSizeChanged(EventArgs e)
        {
            Select();
            base.OnSizeChanged(e);
        }

        // Handles control move event.
        protected override void OnMove(EventArgs e)
        {
            Select();
            base.OnMove(e);
        }

        // Handles mouse up event to stop dragging.
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragging)
            {
                _dragging = false;
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        // Handles mouse enter event to focus the control.
        protected override void OnMouseEnter(EventArgs e)
        {
            Select();
            base.OnMouseEnter(e);
        }

        // Handles mouse wheel event for zooming.
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

        // Handles painting of the control.
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

        // Handles control load event.
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

        // Handles control resize event.
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

        // Handles drag enter event for image drag-and-drop.
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) ||
                drgevent.Data.GetDataPresent(DataFormats.Bitmap))
            {
                drgevent.Effect = DragDropEffects.Copy;
            }
            else
            {
                drgevent.Effect = DragDropEffects.None;
            }
            base.OnDragEnter(drgevent);
        }

        // Handles drag drop event for image drag-and-drop.
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    try
                    {
                        Image droppedImage = Image.FromFile(files[0]);
                        Image = droppedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to load image: " + ex.Message);
                    }
                }
            }
            else if (drgevent.Data.GetDataPresent(DataFormats.Bitmap))
            {
                Image droppedImage = (Image)drgevent.Data.GetData(DataFormats.Bitmap);
                Image = droppedImage;
            }

            base.OnDragDrop(drgevent);
        }

        #endregion Event Overrides

        #region Private Methods

        // Initializes the image and calculates initial zoom and bounds.
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

        // Validates the zoom factor to ensure it stays within bounds.
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

        // Calculates the zoom factor to fit the image within the control.
        private double FitImageToControl()
        {
            if (_image == null || ClientSize == Size.Empty)
            {
                return 1;
            }

            double sourceAspect = (double)_image.Width / _image.Height;
            double targetAspect = (double)ClientSize.Width / ClientSize.Height;
            if (sourceAspect > targetAspect)
            {
                return (double)ClientSize.Width / _image.Width;
            }
            else
            {
                return (double)ClientSize.Height / _image.Height;
            }
        }

        // Centers the image within the control bounds.
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

        // Calculates the bounds of the zoomed image.
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

        // Finds the zoom center based on the zoom mode.
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

        // Adjusts the image size while maintaining aspect ratio.
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

        // Adjusts the image size for scrollable mode.
        private void Scrollable()
        {
            Width = Image.Width;
            Height = Image.Height;
            CenterImage();
        }

        // Sets the layout based on the size mode.
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

        // Centers the image within the control.
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

        [Category("ZoomPicture")]
        [Description("Enable dragging. Set to False if you implement other means of image scrolling.")]
        public bool EnableMouseDragging
        {
            get { return _enableMouseDragging; }
            set { _enableMouseDragging = value; }
        }

        [Category("ZoomPicture")]
        [Description("Enable mouse wheel zooming. Set to false e.g. if you control zooming with a TrackBar.")]
        public bool EnableMouseWheelZooming
        {
            get { return _enableMouseWheelZooming; }
            set { _enableMouseWheelZooming = true; }
        }

        [Category("ZoomPicture")]
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

        [Category("ZoomPicture")]
        [Description("The maximum zoom magnification.")]
        public double MaximumZoomFactor
        {
            get { return _maximumZoomFactor; }
            set { _maximumZoomFactor = value; }
        }

        [Category("ZoomPicture")]
        [Description("Minimum height of the zoomed image in pixels.")]
        public int MinimumImageHeight
        {
            get { return _minimumImageHeight; }
            set { _minimumImageHeight = value; }
        }

        [Category("ZoomPicture")]
        [Description("Minimum width of the zoomed image in pixels.")]
        public int MinimumImageWidth
        {
            get { return _minimumImageWidth; }
            set { _minimumImageWidth = value; }
        }

        [Category("ZoomPicture")]
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

        [Category("ZoomPicture")]
        [DefaultValue(ZoomType.MousePosition)]
        [Description("Image zooming around the mouse position, image center or control center")]
        public ZoomType ZoomMode
        {
            get { return _zoomMode; }
            set { _zoomMode = value; }
        }

        [Category("ZoomPicture")]
        [Description("Sets the size mode of the image. Options: Scrollable (for scrolling) or RatioStretch (to maintain aspect ratio).")]
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