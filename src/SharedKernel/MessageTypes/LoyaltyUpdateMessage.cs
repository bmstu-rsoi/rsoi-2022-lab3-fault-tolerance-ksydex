namespace SharedKernel.MessageTypes;

public class LoyaltyUpdateMessage
{
    public string UserName { get; set; } = "";
    public int CountDelta { get; set; } = 0;
}