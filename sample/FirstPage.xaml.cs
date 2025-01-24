using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The49.Maui.BottomSheet.Sample;

public partial class FirstPage : ContentPage
{
    public FirstPage()
    {
        InitializeComponent();
    }

    private async void OnClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MainPage");
    }
}