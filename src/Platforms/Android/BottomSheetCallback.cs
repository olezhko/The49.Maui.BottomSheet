using AView = Android.Views.View;
using Google.Android.Material.BottomSheet;

namespace The49.Maui.BottomSheet;

public class BottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
{
    private readonly WeakEventManager _eventManager = new();
    
    public event EventHandler StateChanged;
    
    public override void OnSlide(AView bottomSheet, float newState)
    {}

    public override void OnStateChanged(AView view, int newState)
    {
        _eventManager.HandleEvent(this, EventArgs.Empty, nameof(StateChanged));
    }
}