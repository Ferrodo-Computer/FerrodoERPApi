using Application.ViewModels;

namespace PresentationBase.AuthServices.Interfaces
{
    public interface ITokenService
    {
        Task<TokenPair> IssueToken(string login);
    }
}
