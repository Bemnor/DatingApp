using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        IPhotoService photoService
    )
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _photoService = photoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers(
        [FromQuery] UserParams userParams
    )
    {
        var currentUser = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        userParams.CurrentUsername = currentUser.UserName;

        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }

        var users = await _userRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(
            new PaginationHeader(
                users.CurrentPage,
                users.PageSize,
                users.TotalCount,
                users.TotalPages
            )
        );

        return Ok(users);
    }

    //essentially whenever the get request URL is "localhost:5000/(some number)"
    [HttpGet("{username}")] // /api/users/2
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        return await _userRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        //get username from jwt token claim (specifying the claim type..)
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
            return NotFound();

        //map from memberUpdateDto to user
        _mapper.Map(memberUpdateDto, user);

        //try saving to DB, return if successful
        if (await _userRepository.SaveAllAsync())
            return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
            return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo { Url = result.SecureUrl.AbsoluteUri, PublicId = result.PublicId };

        if (user.Photos.Count == 0)
            photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _userRepository.SaveAllAsync())
        {
            return CreatedAtAction(
                nameof(GetUser),
                new { username = user.UserName },
                _mapper.Map<PhotoDto>(photo)
            );
        }

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
            return NotFound();

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("this is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
        if (currentMain != null)
            currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _userRepository.SaveAllAsync())
            return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

        if (photo == null)
            return NotFound();

        if (photo.IsMain)
            return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
                return BadRequest(result.Error.Message);
        }

        //entity framework is tracking user so db is automatically sync'ed
        user.Photos.Remove(photo);

        if (await _userRepository.SaveAllAsync())
            return Ok();

        return BadRequest("Problem deleting photo");
    }
}
