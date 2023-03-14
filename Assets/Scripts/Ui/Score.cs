public class Score
{
    private int _currentScore;
    public int CurrentScore
    {
        get { return _currentScore; }
        set
        {
            _currentScore = value;
            ScoreChanged?.Invoke(_currentScore);
        }
    }
    public System.Action<int> ScoreChanged;
}
