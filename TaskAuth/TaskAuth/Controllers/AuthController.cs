using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TaskAuth.Helpers;
using TaskAuth.Models;
using TaskAuth.Services;

namespace TaskAuth.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly Utils _utils;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IUserService userService, IRefreshTokenService refreshTokenService,
            IHttpContextAccessor httpContextAccessor, Utils utils)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _httpContextAccessor = httpContextAccessor;
            _utils = utils;
        }

        [HttpPost("test"), Authorize]
        public IActionResult Test()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is not null)
            {
                var id = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = _userService.GetUserById(Guid.Parse(id));
                if (user is not null)
                {
                    return Ok(new
                    {
                        Message = "Gút",
                        Code = 200,
                        Success = true
                    });
                }
            }
            return Unauthorized(new
            {
                Message = "Không đủ quyền truy cập",
                Code = -1000,
                Success = false
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto request)
        {
            string email = request.Email;
            string fullName = request.FullName;
            string password = request.Password;
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var user = await _userService.GetUserByEmail(email);

            if (email.IsNullOrEmpty())
            {
                return BadRequest(new
                {
                    Success = false,
                    Code = -1,
                    Message = "Email là bắt buộc !"
                });
            }

            else if (password.IsNullOrEmpty())
            {
                return BadRequest(new
                {
                    Success = false,
                    Code = -1,
                    Message = "Password là bắt buộc !"
                });
            }

            else if (fullName.IsNullOrEmpty())
            {
                return BadRequest(new
                {
                    Success = false,
                    Code = -1,
                    Message = "Fullname là bắt buộc !"
                });
            }

            else if (!Regex.IsMatch(email, pattern))
            {
                return BadRequest(new
                {
                    Success = false,
                    Code = -1,
                    Message = "Email không hợp lệ !"
                });
            }

            else if (user is not null)
            {
                return Conflict(new
                {
                    Success = false,
                    Message = "Email đã có người sử dụng"
                });
            }

            var _user = await _userService.Register(request);
            return Ok(new
            {
                Success = true,
                Code = 0,
                Message = "Tạo người dùng thành công",
                Data = _user
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthRequest request)
        {
            // user in Database
            var user = await _userService.GetUserByEmail(request.Email);
            if (user is not null)
            {
                string roleName = user.RoleId == 1 ? Utils.RoleName.user.ToString() : Utils.RoleName.admin.ToString();
                var userModel = new UserModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    HashedPassword = user.HashedPassword,
                    Role = roleName
                };

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Code = -100,
                        Message = "Sai mật khẩu"
                    });
                }
                string token = _utils.CreateToken(userModel);


                // create refreshToken not in Database
                var refreshToken = _utils.GetRefreshToken(userModel);

                if (user.RefreshTokenId is null)
                {
                    var _refreshToken = await _refreshTokenService.SaveRefreshToken(refreshToken);
                    // refreshToken saved in Database has Id
                    user.RefreshTokenId = _refreshToken.Id;
                    await _userService.AddRefreshToken(user);
                }
                else
                {
                    // update refreshToken
                    var _refreshToken = await _refreshTokenService.GetRefreshTokenById(user.RefreshTokenId);
                    _refreshToken.Token = refreshToken.Token;
                    _refreshToken.IsUsedAt = refreshToken.IsUsedAt;
                    _refreshToken.IsExpiredAt = refreshToken.IsExpiredAt;
                    _refreshToken.IsRevoke = refreshToken.IsRevoke;
                    _refreshToken.IsUsed = refreshToken.IsUsed;

                    await _refreshTokenService.UpdateRefreshToken(_refreshToken);

                    // delete childenToken
                    await _refreshTokenService.DeleteChildrenRefreshTokenByParentId(user.RefreshTokenId);

                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = refreshToken.IsExpiredAt
                };

                Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
                return Ok(new
                {
                    Success = true,
                    Code = 0,
                    Message = "Đăng nhập thành công",
                    Data = new AuthResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Role = roleName,
                        Token = token
                    }
                });
            }
            return NotFound(new
            {
                Success = false,
                Message = "Tài khoản không tồn tại !"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // get from cookies
            var refreshToken = Request.Cookies["refreshToken"];
            // get from database find by Token (value)
            var _refreshToken = await _refreshTokenService.GetRefreshTokenByValue(refreshToken);


            if (_refreshToken is not null)
            {
                if (_refreshToken.IsExpiredAt > DateTime.Now)
                {
                    if (!_refreshToken.IsRevoke && _refreshToken.IsUsed)
                    {
                        var user = await _userService.GetUserByRefreshTokenId(_refreshToken.Id) ?? await _userService.GetUserByRefreshTokenId(_refreshToken.ParentId);
                        // Is Parent, because User refference to Reffence 
                        if (user is not null)
                        {
                            string roleName = user.RoleId == 1 ? Utils.RoleName.user.ToString() : Utils.RoleName.admin.ToString();
                            var userModel = new UserModel
                            {
                                Email = user.Email,
                                FullName = user.FullName,
                                HashedPassword = user.HashedPassword,
                                Id = user.Id,
                                Role = roleName
                            };

                            string token = _utils.CreateToken(userModel);
                            // add children-refreshToken

                            var newRefreshToken = _utils.GetRefreshToken(userModel);

                            int parentId = _refreshToken.ParentId ?? _refreshToken.Id;

                            newRefreshToken.ParentId = parentId;
                            await _refreshTokenService.SaveRefreshToken(newRefreshToken);

                            _refreshToken.IsUsed = false;

                            await _refreshTokenService.UpdateRefreshToken(_refreshToken);

                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Expires = _refreshToken.IsExpiredAt
                            };

                            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

                            return Ok(new
                            {
                                Success = true,
                                Code = 0,
                                Data = new AuthResponse
                                {
                                    Id = user.Id,
                                    Email = user.Email,
                                    Role = roleName,
                                    Token = token
                                }
                            });
                        }


                    }
                    else if (!_refreshToken.IsRevoke && !_refreshToken.IsUsed)
                    {
                        await _refreshTokenService.RevokeChildrenRefreshTokenByParentId(_refreshToken.ParentId);
                    }
                }

            }
            return Unauthorized(new
            {
                Message = "Không đủ quyền truy cập",
                Code = 401,
                Success = false
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if(!Request.Cookies.ContainsKey("refreshToken"))
            {
                return Ok(new { Success = true });
            }
            var refreshToken = Request.Cookies["refreshToken"];
            // get from database find by Token (value)
            var _refreshToken = await _refreshTokenService.GetRefreshTokenByValue(refreshToken);
            if (_refreshToken is not null)
            {
                if(_refreshToken.IsUsed)
                {
                    var updateRefreshToken = _refreshToken;
                    updateRefreshToken.IsUsed = false;
                    await _refreshTokenService.UpdateRefreshToken(updateRefreshToken);
                    return Ok(new
                    {
                        Success = false,
                        Code = 200,
                        Message = "ok"
                    });
                }
            }
            return Unauthorized(new
            {
                Message = "Không đủ quyền truy cập",
                Code = 401,
                Success = false
            });
        }
    }
}
