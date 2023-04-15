using ChatService.Web.Dtos.Profile;

namespace ChatService.Web.Storage;

public interface IProfileStore
{
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);
}