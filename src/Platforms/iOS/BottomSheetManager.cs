using System.Diagnostics;
using Foundation;
using System.Runtime.InteropServices;
using UIKit;
namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    static NSObject? _keyboardWillShowObserver;
    private static nfloat _keyboardHeight;
    private static object? _keyboardDidHideObserver;

    static partial void PlatformShow(Window window, BottomSheet sheet, bool animated)
    {
        if (!OperatingSystem.IsIOSVersionAtLeast(15))
        {
            throw new PlatformNotSupportedException("Bottom sheets are only supported on iOS 15+");
        }

        if (window.Handler?.MauiContext is null)
        {
            throw new InvalidOperationException("Unable to determine maui context from window");
        }
        
        sheet.Parent = window;
        var controller = new BottomSheetViewController(window.Handler.MauiContext, sheet);
        sheet.Controller = controller;

        _keyboardWillShowObserver ??= UIKeyboard.Notifications.ObserveWillShow(KeyboardWillShow);
        _keyboardDidHideObserver ??= UIKeyboard.Notifications.ObserveDidHide(KeyboardDidHide);
        
        if (controller.SheetPresentationController is not null)
        {
            controller.SheetPresentationController.PrefersGrabberVisible = sheet.HasHandle;
        }
        
        var largestDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Unknown;

        if (!sheet.HasBackdrop)
        {
            controller.SheetPresentationController.LargestUndimmedDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Large;
        }

        var pageDetents = sheet.GetEnabledDetents().ToList();

        var detents = pageDetents
            .Select((d, index) =>
            {
                if (d is FullscreenDetent)
                {
                    largestDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Large;
                    return UISheetPresentationControllerDetent.CreateLargeDetent();
                }
                else if (d is RatioDetent ratioDetent && ratioDetent.Ratio == .5)
                {
                    largestDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Medium;
                    return UISheetPresentationControllerDetent.CreateMediumDetent();
                }
                if (!OperatingSystem.IsIOSVersionAtLeast(16))
                {
                    return null;
                }
                
                return UISheetPresentationControllerDetent.Create($"detent{index}", (context) =>
                {
                    if (!sheet.CachedDetents.ContainsKey(index))
                    {
                        sheet.CachedDetents.Add(index, (float)d.GetHeight(sheet, context.MaximumDetentValue - _keyboardHeight));
                    }
                    return sheet.CachedDetents[index];
                });
            })
            .Where(d => d is not null)
            .ToList();

        if (detents.Count == 0)
        {
            largestDetentIdentifier = UISheetPresentationControllerDetentIdentifier.Medium;
            detents.Add(UISheetPresentationControllerDetent.CreateMediumDetent());
        }

        controller.SheetPresentationController.Detents = detents.ToArray();

        if (!sheet.HasBackdrop)
        {
            controller.SheetPresentationController.LargestUndimmedDetentIdentifier = largestDetentIdentifier;
        }

        controller.SheetPresentationController.SelectedDetentIdentifier = BottomSheetViewController.GetIdentifierForDetent(sheet.SelectedDetent);

        controller.ModalInPresentation = !sheet.IsCancelable;

        var parent = Platform.GetCurrentUIViewController();

        if (parent is null)
        {
            Trace.WriteLine("Unable to determine parent view controller");
            return;
        }

        parent.PresentViewController(controller, animated, sheet.NotifyShown);
    }

    static void KeyboardDidHide(object sender, UIKeyboardEventArgs e)
    {
        _keyboardHeight = 0;
    }

    static void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
    {
        _keyboardHeight = e.FrameEnd.Height;
    }

    internal static NFloat KeyboardHeight => _keyboardHeight;
}

