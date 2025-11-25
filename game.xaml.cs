using System.ComponentModel;
using System.Diagnostics;

namespace Hangman;

public partial class Game : ContentPage, INotifyPropertyChanged
{
    public string Spotlight
    {
        get => spotlight;
        set
        {
            spotlight = value;
            OnPropertyChanged();
        }
    }

    private List<char> _allLetters = new();

    public List<char> AllLetters
    {
        get => _allLetters;
        set
        {
            _allLetters = value;
            OnPropertyChanged();
        }
    }

    private string hintText;
    public string HintText
    {
        get => hintText;
        set
        {
            hintText = value;
            OnPropertyChanged();
        }
    }

    private string categoryText;
    public string CategoryText
    {
        get => categoryText;
        set
        {
            categoryText = value;
            OnPropertyChanged();
        }
    }

    public string Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged();
        }
    }

    private bool isWinVisible;
    public bool IsWinVisible
    {
        get => isWinVisible;
        set
        {
            isWinVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isHintVisible = true;
    public bool IsHintVisible
    {
        get => isHintVisible;
        set
        {
            isHintVisible = value;
            OnPropertyChanged();
        }
    }

    public string GameStatus
    {
        get => gameStatus;
        set
        {
            gameStatus = value;
        }
    }

    private string hangmanImage = "hangman1.png";
    public string HangmanImage
    {
        get => hangmanImage;
        set
        {
            hangmanImage = value;
            OnPropertyChanged();
        }
    }

    private string scoreImage = "wrong0.png";
    public string ScoreImage
    {
        get => scoreImage;
        set
        {
            scoreImage = value;
            OnPropertyChanged();
        }
    }

    private bool isKeyboardVisible = true;
    public bool IsKeyboardVisible
    {
        get => isKeyboardVisible;
        set
        {
            isKeyboardVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isNextVisible;
    public bool IsNextVisible
    {
        get => isNextVisible;
        set
        {
            isNextVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isRetryVisible;
    public bool IsRetryVisible
    {
        get => isRetryVisible;
        set
        {
            isRetryVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isExitVisible;
    public bool IsExitVisible
    {
        get => isExitVisible;
        set
        {
            isExitVisible = value;
            OnPropertyChanged();
        }
    }

    #region Fields

    private List<(string Word, string Hint)> wordList = new();
    private Random Rand = new();
    private string Answer = "";
    private string Hint = "";
    private string spotlight;
    private string selectedCategory = ""; // Store the selected category
    int Mistakes = 0;
    List<char> guessed = new List<char>();
    private string gameStatus;
    private string message;

    #endregion

    // NEW: Constructor that accepts category parameter
    public Game(string category)
    {
        InitializeComponent();
        AllLetters.AddRange("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        selectedCategory = category;
        BindingContext = this;
        _ = LoadWordsAsync();
    }

    private async Task LoadWordsAsync()
    {
        try
        {
            // Determine which file to load based on selected category
            string fileName = $"hangman_words/{selectedCategory}.txt";

            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true);
            var content = await reader.ReadToEndAsync();
            content = content.Replace("\uFEFF", "").Replace("\u200B", "").Trim();

            var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                line = line.Replace("\uFEFF", "");

                var parts = line.Split('~');
                if (parts.Length >= 2)
                {
                    var word = parts[0].Trim();
                    var hint = parts[1].Trim();
                    wordList.Add((word, hint));
                }
            }

            if (wordList.Count == 0)
            {
                return;
            }

            PickRandomWord();
            CalculateWord(Answer, guessed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex}");
        }
    }

    private void PickRandomWord()
    {
        if (wordList.Count == 0)
            return;

        var selected = wordList[Rand.Next(wordList.Count)];
        Answer = selected.Word.ToUpper();
        Hint = selected.Hint;
    }

    private void CalculateWord(string answer, List<char> guessed)
    {
        var temp =
            answer.Select(x => (guessed.IndexOf(x) >= 0 ? x : '_'))
            .ToArray();
        Spotlight = string.Join(' ', temp);
        HintText = $"Hint: {Hint}";
        
        // Display category name with nice formatting
        CategoryText = $"Category: {char.ToUpper(selectedCategory[0]) + selectedCategory.Substring(1)}";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn != null)
        {
            var letter = btn.Text;
            btn.IsEnabled = false;
            HandleGuess(letter[0]);
        }
    }

    private void HandleGuess(char letter)
    {
        letter = char.ToUpper(letter);

        // Skip if already guessed
        if (guessed.Contains(letter))
            return;

        guessed.Add(letter);

        //Correct guess
        if (Answer.Contains(letter))
        {
            CalculateWord(Answer, guessed);
            CheckIfGameWon();
        }
        //Wrong guess
        else
        {
            Mistakes++;
            UpdateStatus();
            CheckIfGameLost();
        }
    }

    private void CheckIfGameWon()
    {
        if (Spotlight.Replace(" ", "") == Answer)
        {
            IsHintVisible = false;
            IsWinVisible = true;
            IsKeyboardVisible = false;

            IsNextVisible = true;
            IsExitVisible = true;
            IsRetryVisible = false;
            Message = "YOU WIN!";
        }
    }

    private void CheckIfGameLost()
    {
        if (Mistakes == 3)
        {
            IsHintVisible = false;
            IsWinVisible = true;
            IsKeyboardVisible = false;

            IsRetryVisible = true;
            IsExitVisible = true;
            IsNextVisible = false;
            Message = $"OOPS... YOU DIED!";
            Spotlight = string.Join(" ", Answer.ToUpper().ToCharArray());
        }
    }

    private void UpdateStatus()
    {
        if (Mistakes >= 0 && Mistakes <= 3)
        {
            HangmanImage = $"hangman{Mistakes + 1}.png";
        }

        if (Mistakes >= 0 && Mistakes <= 4)
        {
            ScoreImage = $"wrong{Mistakes}.png";
        }
    }

    private void Next_Clicked(object sender, EventArgs e)
    {
        ResetGame();
        PickRandomWord();
        CalculateWord(Answer, guessed);
    }

    private void Retry_Clicked(object sender, EventArgs e)
    {
        ResetGame();
        CalculateWord(Answer, guessed);
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    private void ResetGame()
    {
        guessed.Clear();
        Mistakes = 0;
        IsWinVisible = false;
        IsHintVisible = true;
        IsKeyboardVisible = true;
        IsNextVisible = false;
        IsRetryVisible = false;
        IsExitVisible = false;
        Message = "";
        HangmanImage = "hangman1.png";
        ScoreImage = "wrong0.png";

        EnableAllLetterButtons();
    }

    private void EnableAllLetterButtons()
    {
        var layout = this.Content.FindByName<Layout>("KeyboardLayout");

        if (layout == null)
            return;

        foreach (var view in layout.Children)
        {
            if (view is Button btn)
                btn.IsEnabled = true;
        }
    }
}
