namespace SMT.HRM.UI
{
    using System.Windows.Controls;
    using System.Windows.Navigation;


    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();

            this.Title = Utility.GetResourceStr("HomePageTitle");
        }

        /// <summary>
        ///     Executes when the user navigates to this page.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
    }
}