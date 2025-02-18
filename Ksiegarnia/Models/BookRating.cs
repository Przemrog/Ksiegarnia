using Microsoft.ML.Data;

public class BookRating
{
    [KeyType(count: 10000)]
    public uint BookId { get; set; }
    public float Label { get; set; }
}
