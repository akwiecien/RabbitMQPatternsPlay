namespace Model;
public class Word
{
    public Guid CorrelationId { get; set; }
    public bool Conclude { get; set; }

    public string Phrase { get; set; }
}
