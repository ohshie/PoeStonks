using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace PoeStonks;

public class DisplayHeaderButton : TemplatedControl
{
    public static readonly StyledProperty<ICommand> MyCommandProperty =
        AvaloniaProperty.Register<DisplayHeaderButton, ICommand>(nameof(OnClick));

    public ICommand OnClick
    {
        get => GetValue(MyCommandProperty);
        set => SetValue(MyCommandProperty, value);
    }
    
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
        var rotationTransform = image.RenderTransform as RotateTransform;
        if (rotationTransform == null)
        {
            rotationTransform = new RotateTransform {Angle = 0};
            image.RenderTransform = rotationTransform;
        }
        rotationTransform.Angle = (rotationTransform.Angle + 180);
    }
}