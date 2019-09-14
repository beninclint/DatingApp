using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]    
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
            var users = await _repo.GetUsers(userParams);
            var userListDto = _mapper.Map<IEnumerable<UserForListDto>>(users);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(userListDto);

        }

        [HttpGet("{id}", Name = "GetUser"),]
        public async Task<IActionResult> GetUser(int Id)
        {
            var user = await _repo.GetUser(Id);
            var userDetailDto = _mapper.Map<UserForDetailDto>(user);
            return Ok(userDetailDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int Id, UserForUpdateDto userForUpdateDto)
        {
            if (Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var userfromRepo = await _repo.GetUser(Id);
            _mapper.Map(userForUpdateDto, userfromRepo);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            throw new Exception($"Updating user {Id} failed on save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int Id, int recipientId)
        {
            if (Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var like = await _repo.GetLike(Id, recipientId);
            if (like != null)
            {
                return BadRequest("You have already liked this user");
            }
            var user = await _repo.GetUser(recipientId);
            if (user == null)
                return BadRequest("User not found!");
            like = new Like()
            {
                LikerId = Id,
                LikeeId = recipientId
            };
            _repo.Add(like);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest("Failed to like user.");

        }
    }
}