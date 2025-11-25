namespace Hangman
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // When Play button is clicked, show category selection
        private void PlayButton_Clicked(object sender, EventArgs e)
        {
            // Hide the Play button
            PlayButton.IsVisible = false;

            // Show the category selection
            CategorySelection.IsVisible = true;
        }

        // When any category button is clicked
        private async void Category_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // Get the category from CommandParameter
                string selectedCategory = button.CommandParameter?.ToString();

                // Navigate to Game page and pass the selected category
                await Navigation.PushAsync(new Game(selectedCategory));
            }
        }

        // Back button to return to main screen
        private void Back_Clicked(object sender, EventArgs e)
        {
            // Show the Play button again
            PlayButton.IsVisible = true;

            // Hide the category selection
            CategorySelection.IsVisible = false;
        }
    }
}
