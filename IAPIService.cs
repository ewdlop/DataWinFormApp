namespace DataWinFormApp;

public interface IAPIService
{
    Task CallApi(string accessToken);
}