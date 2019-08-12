using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();
            var userListDto = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(userListDto);

        }

        [HttpGet("{id}")]
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
            _mapper.Map(userForUpdateDto,userfromRepo);
            if(await _repo.SaveAll()) {
            return NoContent();
            }
            throw new Exception ($"Updating user {Id} failed on save");
        }
    }
}