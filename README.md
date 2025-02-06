# ZoomPicture Control

A customizable `UserControl` for displaying and manipulating images with zooming, dragging, and size adjustment features. Built using **Windows Forms** (WinForms) and compatible with **WPF** via `WindowsFormsHost`.

---

## Features

- **Zooming**: Zoom in and out using the mouse wheel or programmatically.
- **Dragging**: Drag the image to navigate when zoomed in.
- **Size Modes**: Supports two size modes:
  - **Scrollable**: The image retains its original size and can be scrolled.
  - **RatioStretch**: The image is stretched to fit the control while maintaining its aspect ratio.
- **Drag-and-Drop**: Load images by dragging and dropping them into the control.
- **Customizable**: Control zoom limits, mouse wheel sensitivity, and more.

## Installation

### For WinForms Applications
1. Add the `ZoomPicture` control to your WinForms project.
2. Drag and drop the control from the toolbox onto your form.
3. Set the `Image` property to load an image.

### For WPF Applications
1. Add references to `WindowsFormsIntegration` and `System.Windows.Forms`.
2. Use `WindowsFormsHost` to embed the `ZoomPicture` control in your WPF window.
   ```xml
   <WindowsFormsHost>
       <wf:ZoomPicture x:Name="zoomPictureControl" />
   </WindowsFormsHost>

## Usage
Basic Example (WinForms)
// Create an instance of ZoomPicture
var zoomPicture = new ZoomPicture();

// Set the image
zoomPicture.Image = Image.FromFile("path_to_image.jpg");

// Add the control to a form
this.Controls.Add(zoomPicture);
Properties
Property	Description
Image	The image to display in the control.
ZoomFactor	The current zoom level of the image.
MaximumZoomFactor	The maximum allowed zoom level.
MinimumImageWidth	The minimum width of the zoomed image.
MinimumImageHeight	The minimum height of the zoomed image.
EnableMouseDragging	Enables or disables dragging the image with the mouse.
EnableMouseWheelZooming	Enables or disables zooming with the mouse wheel.
ImageSizeMode	Sets the size mode of the image (Scrollable or RatioStretch).

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Author
Rachid Gharbi
Email: winfosoft@gmail.com
GitHub: Winfosoft
Telegram Channel : 
