using Xamarin.Forms;

namespace Snake
{
    public partial class App : Application
    {
        public static bool isPaused = false;
        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            isPaused = true;
        }

        protected override void OnResume()
        {
            
        }
    }
}
