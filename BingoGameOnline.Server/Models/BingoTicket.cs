namespace BingoGameOnline.Server.Models
{
    public class BingoTicket
    {
        // 5 tickets, each 3x9
        public int?[][] Numbers { get; set; } = new int?[15][];
    }
}
