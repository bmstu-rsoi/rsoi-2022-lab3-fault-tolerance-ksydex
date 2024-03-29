using Gateway.Common;
using Gateway.Data.Dtos;
using SharedKernel.Exceptions;

namespace Gateway.Services;

public class LoyaltyClientService : ClientServiceBase
{
    protected override string BaseUri => "http://loyalty_service:80";

    public LoyaltyClientService() : base()
    {
    }

    public async Task<LoyaltyDto?> GetAsync(string userName)
    {
        try
        {
            return (await GetAllAsync(1, 1, userName))?.FirstOrDefault();
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("Loyalty Service unavailable");
        }
    }

    public async Task<List<LoyaltyDto>?> GetAllAsync(int page, int size, string userName)
        => await Client.GetAsync<List<LoyaltyDto>>(BuildUri("api/v1/loyalty"), null, new Dictionary<string, string>
        {
            { "page", page.ToString() },
            { "size", size.ToString() },
            { "UserName", userName }
        });


    public async Task<LoyaltyDto?> UpdateAsync(int id, LoyaltyDto dto)
        => await Client.PatchAsync<LoyaltyDto, LoyaltyDto>(BuildUri("api/v1/loyalty/" + id), dto);

    public void Update(int id, LoyaltyDto dto)
    {
    }
}