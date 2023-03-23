using CQRS.Database;
using CQRS.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CQRS
{
    public class SecurityService
    {
        private IConfiguration _config { get; }
        private Repository _repository { get; }

        public SecurityService(IConfiguration config, Repository repository)
        {
            _config = config;
            _repository = repository;
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                //new Claim(ClaimTypes.NameIdentifier,user.Username),
                //new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("prueba", "hola " + user.Username),
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public User Authenticate(LogIn login)
        {
            return _repository.Users.FirstOrDefault(x => x.Username == login.Username && x.Password == login.Password);
        }

        public bool ValidateToken(ClaimsIdentity identity)
        {
            if (identity.Claims.Count() == 0) return false;

            var id = identity.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

            return true;
        }
    }
}
