namespace DotNet.Push.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            string deviceToken = Preferences.Get("DeviceToken", String.Empty);

            CounterBtn.Text = $"Device Token is {deviceToken}";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}