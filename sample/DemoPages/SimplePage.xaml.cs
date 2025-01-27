using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace The49.Maui.BottomSheet.Sample.DemoPages;

public class ListAction
{
    public string Title { get; set; }
    public ICommand Command { get; set; }
}

public partial class SimplePage : BottomSheet
{
    public ObservableCollection<ListAction> Actions => new()
    {
        new ListAction
        {
            Title = "Share",
            Command = new Command(() => { }),
        },
        new ListAction
        {
            Title = "Copy",
            Command = new Command(() => { }),
        },
        new ListAction
        {
            Title = "Open in browser",
            Command = new Command(() => { }),
        },
         new ListAction
        {
            Title = "Resize",
            Command = new Command(Resize),
        },
        new ListAction
        {
            Title = "Dismiss",
            Command = new Command(() => DismissAsync()),
        }
    };
    public SimplePage()
    {
        InitializeComponent();
        
        this.Showing += OnShowing;
        this.Shown += OnShown;
        this.Dismissed += OnDismissed;
    }

    private void OnShowing(object? sender, EventArgs e)
    {
        Debug.WriteLine("Showing");
    }
    
    private void OnShown(object? sender, EventArgs e)
    {
        Debug.WriteLine("Shown");
    }
    
    private void OnDismissed(object? sender, DismissOrigin e)
    {
        Debug.WriteLine($"Dismissed, origin: {e}");
        
        this.Showing -= OnShowing;
        this.Shown -= OnShown;
        this.Dismissed -= OnDismissed;
    }


    void Resize()
    {
        divider.HeightRequest = 32;
    }

    public VisualElement Divider => divider;

    public void SetExtraContent(View view)
    {
        extra.Content = view;
    }
}