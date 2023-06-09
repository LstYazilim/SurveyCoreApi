﻿using AutoMapper;
using BusinessLayer.Abstract;
using BusinessLayer.Security;
using EntityLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Survey.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IJwtManager _jwtManager;

        public AuthController(IUserService userService, IMapper mapper, IJwtManager jwtManager)
        {
            _userService = userService;
            _mapper = mapper;
            _jwtManager = jwtManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            IActionResult response = Unauthorized();

            var user = await _userService.GetUserByMailAndPassword(loginDto.Email, loginDto.Password);
            if (user != null)
            {
                var tokenString = _jwtManager.GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserLoginDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            await _userService.CreateUser(user);

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            if (users == null || users.Count == 0)
            {
                return NotFound();
            }

            var userDtos = _mapper.Map<List<UserLoginDto>>(users);
            return Ok(userDtos);
        }
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = _mapper.Map<UserLoginDto>(user);
            return Ok(userDto);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserById(id); 
            return Ok();
        }
    }
}