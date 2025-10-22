[System.Serializable]
public class Question
{
    public string questionText;
    public string[] options;  // Length = 4
    public int correctIndex;  // Optional: index of correct answer (0-3)
}
