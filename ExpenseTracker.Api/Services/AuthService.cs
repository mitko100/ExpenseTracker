﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Api.Contansts;
using ExpenseTracker.Api.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AuthService(
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IdentityUser> RegisterAsync(string email, string password)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            await this.userManager.CreateAsync(user, password);

            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await this.userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException(nameof(email));
            }

            if (!await this.userManager.CheckPasswordAsync(user, password))
            {
                throw new ArgumentException(nameof(password));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtContansts.SECRET);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Issuer = JwtContansts.ISSUER,
                Audience = JwtContansts.AUDIENCE,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);

            return stringToken;
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await this.signInManager.SignOutAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
