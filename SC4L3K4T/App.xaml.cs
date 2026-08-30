namespace SC4L3K4T
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(
        new NavigationPage(new MainPage()))
            {
                Title = "SC4L3K4T"
            };
        }
    }
}
