# ZoomPicture Control

A customizable `UserControl` for displaying and manipulating images with zooming, dragging, and size adjustment features. Built using **Windows Forms** (WinForms) 

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

## Usage

### Basic Example (WinForms)

```csharp
// Create an instance of ZoomPicture
var zoomPicture = new ZoomPicture();

// Set the image
zoomPicture.Image = Image.FromFile("path_to_image.jpg");

// Add the control to a form
this.Controls.Add(zoomPicture);
```

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## Author
**Rachid Gharbi**  
📧 **Email**: [winfosoft@gmail.com](mailto:winfosoft@gmail.com)  
📢 **Telegram Channel**: [@winfosoft_Channel](https://t.me/winfosoft_Channel)
