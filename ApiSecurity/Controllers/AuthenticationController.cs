﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiSecurity.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthenticationController(IConfiguration config)
    {
        _config = config;
    }
    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int UserId, string UserName);

    [HttpPost("token")]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);

        if (user is null)
        {
            return Unauthorized(); 
        }

        var token = GenerateToken(user);
        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _config.GetValue<string>("Authentication:SecretKey")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new();
        claims.Add(new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.UniqueName, user.UserName));

        var token = new JwtSecurityToken(
            _config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Autehntication:Audience"),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private UserData? ValidateCredentials(AuthenticationData data)
    {
        if (CompareValues(data.UserName, "tcorey") &&
            CompareValues(data.Password,"Test123"))
        {
            return new UserData(1, data.UserName!);
        }

        if (CompareValues(data.UserName, "sstorm") &&
            CompareValues(data.Password, "Test123"))
        {
            return new UserData(2, data.UserName!);
        }

        return null;
    }

    private bool CompareValues(string? actual, string expected)
    {
        if (actual is not null)
        {
            if (actual.Equals(expected))
            {
                return true;
            }
        }
        return false; 
    }
}
