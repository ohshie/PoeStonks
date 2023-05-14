using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace PoeStonks;

public class DisplayHeaderButton : TemplatedControl
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var displayHeaderButton = e.NameScope.Find<Button>("DisplayHeaderButton");
        var arrowImage = e.NameScope.Find<Image>("SortingArrow");

        if (displayHeaderButton != null && arrowImage != null)
        {
            displayHeaderButton.AddHandler(PointerEnteredEvent, (s, ev) => arrowImage.Opacity = 0.3, handledEventsToo: true);
            displayHeaderButton.AddHandler(PointerExitedEvent, (s, ev) => arrowImage.Opacity = 0.0, handledEventsToo: true);
            
            arrowImage.LayoutUpdated += (s, ev) => UpdateImageCenterPoint(arrowImage);
            displayHeaderButton.Click += (s, ev) => RotateImage(arrowImage);
            
        }
    }

    private void RotateImage(Image image)
    {
        var rotationTransform = image.RenderTransform as RotateTransform;

        if (rotationTransform != null)
        {
            rotationTransform = new();
            image.RenderTransform = rotationTransform;
        }
        
        rotationTransform.Angle = (rotationTransform.Angle + 180) % 360;
    }
    
    private void UpdateImageCenterPoint(Image image)
    {
        var rotateTransform = image.RenderTransform as RotateTransform;

        if (rotateTransform != null)
        {
            rotateTransform.CenterX = image.Bounds.Width / 2;
            rotateTransform.CenterY = image.Bounds.Height / 2;
        }
    }
}