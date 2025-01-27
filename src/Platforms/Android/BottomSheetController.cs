using Android.Views;
using Android.Widget;
using Android.Content;
using AndroidX.Core.View;
using AndroidX.AppCompat.App;
using Google.Android.Material.BottomSheet;
using Android.Util;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace The49.Maui.BottomSheet;

public class BottomSheetController
{
    private readonly IMauiContext _mauiContext;
    private readonly BottomSheet _sheet;
    private BottomSheetBehavior? _behavior;
    private BottomSheetDialog? _dialog;
    private BottomSheetDragHandleView? _handle;
    private readonly BottomSheetCallback _bottomSheetCallback;

    public BottomSheetController(IMauiContext mauiContext, BottomSheet sheet)
    {
        _mauiContext = mauiContext;
        _sheet = sheet;
        
        _bottomSheetCallback = new BottomSheetCallback();
        _bottomSheetCallback.StateChanged += BottomSheetCallbackOnStateChanged;
    }
    
    private void BottomSheetCallbackOnStateChanged(object? sender, EventArgs e)
    {
        if (_behavior.State == BottomSheetBehavior.StateHidden)
        {
            Dispose();
            _sheet.NotifyDismissed();
        }

        // UpdateSelectedDetent();
    }
    
    // internal Detent GetDetentForState(int state)
    // {
    //     return _states.FirstOrDefault(kv => kv.Value == state).Key;
    // }
    
    // internal void UpdateSelectedDetent()
    // {
    //     var detent = GetDetentForState(_behavior.State);
    //     if (detent is not null)
    //     {
    //         _sheet.SelectedDetent = detent;
    //     }
    // }

    public void Show(bool animated)
    {
        // Create the BottomSheetDialog
        _dialog = new BottomSheetDialog(_mauiContext.Context);

        // Inflate the bottom sheet layout
        var inflater = LayoutInflater.From(_mauiContext.Context);
        var rootView = inflater.Inflate(Resource.Layout.the49_maui_bottom_sheet_design, null);

        // Initialize the bottom sheet content
        var containerView = _sheet.ToPlatform(_mauiContext); // Get the Maui view as a native Android view
        var bottomSheetContainer = rootView.FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);

        if (bottomSheetContainer != null && containerView != null)
        {
            bottomSheetContainer.RemoveAllViews();
            
            if (_sheet.HasHandle)
            {
                var handle = CreateHandle();
                bottomSheetContainer.AddView(handle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            }
            
            bottomSheetContainer.AddView(containerView);
            
            // Set the dialog content
            _dialog.SetContentView(rootView);
            
            // Access and customize BottomSheetBehavior
            var bottomSheet = _dialog.FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
            if (bottomSheet != null)
            {
                _behavior = BottomSheetBehavior.From(bottomSheet);
                ConfigureBehavior(_behavior);
            }
        }
        
        _sheet.NotifyShowing();
        _dialog.Show();
        _sheet.NotifyShown();
    }
    
    private void AdjustBottomSheetHeight(AView rootView)
    {
        var bottomSheet = rootView.FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
        if (bottomSheet != null)
        {
            var layoutParams = bottomSheet.LayoutParameters;
            layoutParams.Height = ViewGroup.LayoutParams.WrapContent;
            bottomSheet.LayoutParameters = layoutParams;
        }
    }

    public void Dismiss(bool animated)
    {
        if (_dialog is null || !_dialog.IsShowing)
        {
            return;
        }
        
        if (animated)
        {
            _dialog.Dismiss();
        }
        else
        {
            _dialog.Cancel(); // Immediate dismissal without animation
        }

        _dialog = null;
        _sheet.NotifyDismissed();
    }

    private void ConfigureBehavior(BottomSheetBehavior behavior)
    {
        // Calculate available height for the bottom sheet
        var maxHeight = GetAvailableHeight();
        var density = DeviceDisplay.MainDisplayInfo.Density;

        // Configure the BottomSheetBehavior
        behavior.PeekHeight = (int)(maxHeight * density); // Set peek height based on screen height
        behavior.Hideable = _sheet.IsCancelable; // Allow hiding if cancelable
        behavior.FitToContents = true; // Ensure it fits to the content
        behavior.State = BottomSheetBehavior.StateCollapsed; // Start in collapsed state
    }

    private double GetAvailableHeight()
    {
        // Calculate available height based on display metrics
        var windowManager = _mauiContext.Context.GetSystemService(Context.WindowService) as IWindowManager;
        if (windowManager != null)
        {
            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);

            var density = DeviceDisplay.MainDisplayInfo.Density;
            return metrics.HeightPixels / density;
        }

        return 600; // Default height if unable to calculate
    }

    private BottomSheetDragHandleView CreateHandle()
    {
        _handle = new BottomSheetDragHandleView(_mauiContext.Context);

        if (_sheet.HandleColor is not null)
        {
            _handle.SetColorFilter(_sheet.HandleColor.ToPlatform());
        }

        return _handle;
    }
    
    
    
    public void UpdateHandleColor()
    {
        if (_handle is null)
        {
            return;
        }
        if (_sheet.HandleColor is not null)
        {
            _handle.SetColorFilter(_sheet.HandleColor.ToPlatform());
        }
    }
    
    void Dispose()
    {
        // _frame.LayoutChange -= OnLayoutChange;
        // _windowContainer.RemoveFromParent();
    }
}

