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
            
            displayHeaderButton.Click += (s, ev) => RotateImage(arrowImage);
        }
    }

    private void RotateImage(Image image)
    {
        Console.WriteLine("fail 1");
        var rotationTransform = image.RenderTransform as RotateTransform;
        Console.WriteLine("fail 2");
        if (rotationTransform == null)
        {
            rotationTransform = new RotateTransform {Angle = 0};
            image.RenderTransform = rotationTransform;
        }
        Console.WriteLine("fail 3");
        rotationTransform.Angle = (rotationTransform.Angle + 180);
    }
}