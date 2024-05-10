﻿using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    //AccountController Variable Assignment
    private readonly DataContext _context;

    private readonly ITokenService _tokenService;

    //constructor
    public AccountController(DataContext context, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _context = context;
    }

    //an httpPost request
    //Register a User to the Database
    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
            return BadRequest("Username is taken");

        //hmac is an encryption class
        //placing the 'using' tag there makes sure the "HMACSHA512" Obj we created will be Garbage-Collected.
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password.ToLower())), //transforming password to bytes then encrypting the bytes
            PasswordSalt = hmac.Key //adding salt(shuffling encrypted string)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto { Username = user.UserName, Token = _tokenService.CreateToken(user) };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        //Find User using loginDto
        //SingleOrDefaultAsync() finds the first unique item that matches its query, if 2 or more items match; throws exception.
        var user = await _context.Users.SingleOrDefaultAsync(user =>
            user.UserName == loginDto.Username
        );

        if (user == null)
            return Unauthorized("invalid username");

        //Use User.PasswordSalt to get the 'key' to recreate the same HMAC class which encrypted the user's password.
        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
                return Unauthorized("invalid password");
        }
        return new UserDto { Username = user.UserName, Token = _tokenService.CreateToken(user) };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }
}
