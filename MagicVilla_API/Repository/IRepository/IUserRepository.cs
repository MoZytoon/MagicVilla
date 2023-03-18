using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;

namespace MagicVilla_API.Repository.IRepostiory
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        //for JWT
        //Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO);

        //for Identity
        Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO);
    }
}
