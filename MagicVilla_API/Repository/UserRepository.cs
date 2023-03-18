using AutoMapper;
using MagicVilla_API.Data;
using MagicVilla_API.Models;
using MagicVilla_API.Models.Dto;
using MagicVilla_API.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_API.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private string secretKey;


        public UserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;

        }

        public bool IsUniqueUser(string username)
        {
            //for JWT
            //if (_db.LocalUsers.FirstOrDefault(x => x.UserName == username) == null)
            //    return true;

            if (_db.ApplicationUsers.FirstOrDefault(x => x.UserName == username) == null)
                return true;

            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            //check if user is exsist or not valid
            //var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower() && u.Password == loginRequestDTO.Password);
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(user,loginRequestDTO.Password);

            if (user == null && !isValid)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }

            //if user found generate JWT Token
            var tokenHnadler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            //for identity asp.net 
            var roles = await _userManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault()) //if i have more than one one role i need to for loop.
                    //for JWT : new Claim(ClaimTypes.Role,localUser.role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHnadler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                Token = tokenHnadler.WriteToken(token),
                User = _mapper.Map<UserDTO>(user),
                //Role = roles.FirstOrDefault()
            };

            return loginResponseDTO;
        }

        //for JWT => Task<LocalUser> for Identity => Task<UserDTO>
        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            //For JWT
            //LocalUser user = new()
            //{
            //    UserName = registerationRequestDTO.UserName,
            //    Password = registerationRequestDTO.Password,
            //    Role = registerationRequestDTO.Role,
            //    Name = registerationRequestDTO.Name
            //};

            //for identity
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);

                if (result.Succeeded)
                {
                    if (! _roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }

                    if (string.IsNullOrWhiteSpace(registerationRequestDTO.Role))
                        registerationRequestDTO.Role = "customer";

                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == registerationRequestDTO.UserName);

                    return _mapper.Map<UserDTO>(userToReturn);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw ;
            }

            //For JWT
            //_db.LocalUsers.Add(user);
            //await _db.SaveChangesAsync();
            //user.Password = "";
            //return user;
        }
    }
}
