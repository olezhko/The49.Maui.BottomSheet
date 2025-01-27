using AView = Android.Views.View;
using Google.Android.Material.BottomSheet;

namespace The49.Maui.BottomSheet;

internal class BottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
{
    private readonly WeakEventManager _eventManager = new();
    
    public event EventHandler<BottomSheetStateChangedEventArgs> StateChanged
    {
        add => _eventManager.AddEventHandler(value);
        remove => _eventManager.RemoveEventHandler(value);
    }

    public override void OnSlide(AView bottomSheet, float newState)
    {
    }

    public override void OnStateChanged(AView view, int newState)
    {
        _eventManager.HandleEvent(view, new BottomSheetStateChangedEventArgs(newState), nameof(StateChanged));
    }
}

internal record BottomSheetStateChangedEventArgs(int State);
