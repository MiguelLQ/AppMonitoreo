using AppMonitoreo.Models;
using AppMonitoreo.PageModels;

namespace AppMonitoreo.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}