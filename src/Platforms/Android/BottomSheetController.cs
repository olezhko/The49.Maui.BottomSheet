using System.Diagnostics;
using Android.Views;
using Android.Widget;
using Android.Content;
using AndroidX.Core.View;
using AndroidX.AppCompat.App;
using Google.Android.Material.BottomSheet;
using Android.Util;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Google.Android.Material.Color;
using System.Reflection.Metadata;

namespace The49.Maui.BottomSheet;

public class BottomSheetController
{
    private readonly IMauiContext _mauiContext;
    private readonly BottomSheet _bottomSheet;
    private readonly BottomSheetDialog _bottomSheetDialog;
    private BottomSheetDragHandleView? _handle;
    private readonly BottomSheetCallback _bottomSheetCallback;
    private readonly IDialogInterfaceOnDismissListener _dismissListener;
    private FrameLayout _bottomSheetContainer;
    bool? _isBackgroundLight;

    public BottomSheetController(IMauiContext mauiContext, BottomSheet sheet)
    {
        _mauiContext = mauiContext;
        _bottomSheet = sheet;

        _bottomSheetDialog = new BottomSheetDialog(_mauiContext.Context);
        _bottomSheetDialog.ShowEvent += OnShow;
        _bottomSheetDialog.DismissEvent += OnDismissed;
        _bottomSheetDialog.Behavior.MaxHeight = GetMaxHeight();

        _bottomSheetCallback = new BottomSheetCallback();
        _bottomSheetCallback.StateChanged += BottomSheetCallbackOnStateChanged;
    }

    private int GetMaxHeight()
    {
        return _mauiContext.Context.Resources?.DisplayMetrics?.HeightPixels ?? 0;
    }

    private void OnShow(object? sender, EventArgs e)
    {
        _bottomSheet.NotifyShown();
    }

    private void OnDismissed(object? sender, EventArgs e)
    {
        _bottomSheet.NotifyDismissed();
        _bottomSheetDialog.Behavior.RemoveBottomSheetCallback(_bottomSheetCallback);
    }

    private void BottomSheetCallbackOnStateChanged(object? sender, BottomSheetStateChangedEventArgs e)
    {
        Debug.WriteLine($"Callback state changed: {e.State}");
        
        if (e.State == BottomSheetBehavior.StateHidden)
        {
            Dispose();
            _bottomSheet.NotifyDismissed();
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
        var inflater = LayoutInflater.From(_mauiContext.Context);
        var rootView = inflater.Inflate(Resource.Layout.the49_maui_bottom_sheet_design, null);

        var containerView = _bottomSheet.ToPlatform(_mauiContext); // Get the Maui view as a native Android view
        _bottomSheetContainer = rootView.FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);

        _bottomSheetContainer.RemoveAllViews();

        _bottomSheetContainer.SetMinimumHeight(_bottomSheetDialog.Behavior.MaxHeight);

        if (_bottomSheet.HasHandle)
        {
            AddHandle();
        }

        _bottomSheetContainer.AddView(containerView);

        // Set the dialog content
        _bottomSheetDialog.SetContentView(rootView);

        var maxHeight = GetAvailableHeight();
        var density = DeviceDisplay.MainDisplayInfo.Density;

        _bottomSheetDialog.Behavior.PeekHeight = (int)(maxHeight * density);
        _bottomSheetDialog.Behavior.Hideable = _bottomSheet.IsCancelable;
        _bottomSheetDialog.Behavior.FitToContents = true;
        _bottomSheetDialog.Behavior.State = BottomSheetBehavior.StateCollapsed;

        UpdateBackground();
        UpdateHasBackdrop();

        if (!animated)
        {
            _bottomSheetDialog.Window.SetWindowAnimations(0);
        }
        
        _bottomSheetDialog.Behavior.AddBottomSheetCallback(_bottomSheetCallback);
        _bottomSheetDialog.DismissEvent += OnDismissed;
        _bottomSheetDialog.SetCancelable(_bottomSheet.IsCancelable);
        
        _bottomSheet.NotifyShowing();
        _bottomSheetDialog.Show();
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
        if (_bottomSheetDialog is null || !_bottomSheetDialog.IsShowing)
        {
            return;
        }
      
        if (animated)
        {
            _bottomSheetDialog.Dismiss();
        }
        else
        {
            _bottomSheetDialog.Cancel();
        }
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

        return 600;
    }

    private void AddHandle()
    {
        _handle = new BottomSheetDragHandleView(_mauiContext.Context);

        if (_bottomSheet.HandleColor is not null)
        {
            _handle.SetColorFilter(_bottomSheet.HandleColor.ToPlatform());
        }

        _bottomSheetContainer.AddView(_handle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
    }
    
    
    
    public void UpdateHandleColor()
    {
        if (_handle is null)
        {
            return;
        }
        if (_bottomSheet.HandleColor is not null)
        {
            _handle.SetColorFilter(_bottomSheet.HandleColor.ToPlatform());
        }
    }
    
    void Dispose()
    {
        // _frame.LayoutChange -= OnLayoutChange;
        // _windowContainer.RemoveFromParent();

        if (_bottomSheetDialog is not null)
        {
            _bottomSheetDialog.Behavior.RemoveBottomSheetCallback(_bottomSheetCallback);
        }
    }

    #region Handler Methods

    public void UpdateBackground()
    {
        if (_bottomSheet is null || _bottomSheetContainer is null)
        {
            return;
        }

        Paint paint = _bottomSheet.BackgroundBrush;
        if (_bottomSheetContainer is not null)
        {
            if (_bottomSheet.CornerRadius != -1)
            {
                SheetRadiusDrawable drawable;
                if (_bottomSheetContainer.Background is not SheetRadiusDrawable)
                {
                    drawable = new SheetRadiusDrawable();
                    _bottomSheetContainer.Background = drawable;
                }
                else
                {
                    drawable = (SheetRadiusDrawable)_bottomSheetContainer.Background;
                }
                drawable.SetCornerRadius(_bottomSheetContainer.Context.ToPixels(_bottomSheet.CornerRadius));
            }
            if (paint is not null)
            {
                var platformColor = paint.ToColor().ToPlatform();
                if (_bottomSheetContainer.Background is SheetRadiusDrawable sheetDrawable)
                {
                    sheetDrawable.SetColor(platformColor);
                }
                else
                {
                    _bottomSheetContainer.BackgroundTintList = ColorStateList.ValueOf(platformColor);
                }
            }
        }
        // Try to find the background color to automatically change the status bar icons so they will
        // still be visible when the bottomsheet slides underneath the status bar.
        ColorStateList backgroundTint = ViewCompat.GetBackgroundTintList(_bottomSheetContainer);

        if (backgroundTint != null)
        {
            // First check for a tint
            _isBackgroundLight = MaterialColors.IsColorLight(backgroundTint.DefaultColor);
        }
        else if (_bottomSheetContainer.Background is ColorDrawable)
        {
            // Then check for the background color
            _isBackgroundLight = MaterialColors.IsColorLight(((ColorDrawable)_bottomSheetContainer.Background).Color);
        }
        else
        {
            // Otherwise don't change the status bar color
            _isBackgroundLight = null;
        }
    }

    public void UpdateHasBackdrop()
    {
        if (_bottomSheet is null || _bottomSheetDialog is null || _bottomSheetDialog.Window is null)
        {
            return;
        }

        var window = _bottomSheetDialog.Window;

        if (_bottomSheet.HasBackdrop)
        {
            window.AddFlags(WindowManagerFlags.DimBehind);
        }
        else
        {
            window.ClearFlags(WindowManagerFlags.DimBehind);
        }
    }

    #endregion Handler Methods
}
