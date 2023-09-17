using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TaskAuth.Entities;
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
        private readonly JwtUtility _utils;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IUserService userService, IRefreshTokenService refreshTokenService,
            IHttpContextAccessor httpContextAccessor, JwtUtility utils)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _httpContextAccessor = httpContextAccessor;
            _utils = utils;
        }

        [HttpPost("test"), Authorize]
        public IActionResult Test()
        {
            return HandleTest();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(SignupRequest request)
        {
            return await RegisterHandle(request);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            return await LoginHandle(request);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            return await RefreshTokenHandle();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return await LogoutHandle();
        }

        private async Task<IActionResult> RegisterHandle(SignupRequest request)
        {
            string email = request.Email;
            string fullName = request.FullName;
            string password = request.Password;
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
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
            {
                var user = await _userService.GetUserByEmail(email);
                if (user is not null)
                {
                    return Conflict(new
                    {
                        Success = false,
                        Message = "Email đã có người sử dụng"
                    });
                }
                else
                {
                    var _user = await _userService.Register(request);
                    return Ok(new
                    {
                        Success = true,
                        Code = 0,
                        Message = "Tạo người dùng thành công",
                        Data = new
                        {
                            Id = _user.Id.ToString(),
                            Email = _user.Email.ToString(),
                            FullName = _user.FullName.ToString(),
                            Role = RoleName.user.ToString()
                        }
                    });
                }
            }
        }

        private async Task<IActionResult> LoginHandle(LoginRequest request)
        {
            var user = await _userService.GetUserByEmail(request.Email);
            if (user is not null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Code = -100,
                        Message = "Sai mật khẩu"
                    });
                }
                // generate accessToken
                string token = _utils.GenerateToken(user);

                // create refreshToken
                var refreshToken = _utils.GenerateRefreshToken();

                // create new refresh token and update user with new refresh token
                if (user.RefreshTokenId is null)
                {
                    var _refreshToken = await _refreshTokenService.SaveRefreshToken(refreshToken);

                    user.RefreshTokenId = _refreshToken.Id;
                    await _userService.AddRefreshToken(user);
                }
                // user have refresh token and update refresh token data
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

                    // delete old childenToken
                    await _refreshTokenService.DeleteChildrenRefreshTokenByParentId(user.RefreshTokenId);
                }

                // set coookies
                Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = refreshToken.IsExpiredAt
                });

                string roleName = user.RoleId == 1 ? RoleName.user.ToString() : RoleName.admin.ToString();
                return Ok(new
                {
                    Success = true,
                    Code = 0,
                    Message = "Đăng nhập thành công",
                    Data = new
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email.ToString(),
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

        private async Task<IActionResult> RefreshTokenHandle()
        {
            // if refresh token from cookie is not exists
            if (!Request.Cookies.ContainsKey("refreshToken"))
            {
                return Unauthorized(new
                {
                    Message = "Không đủ quyền truy cập",
                    Code = 4011,
                    Success = false
                });
            }
            // get refreshToken from cookies
            var refreshToken = Request.Cookies["refreshToken"];

            // get from database find by Token (value)
            var _refreshToken = await _refreshTokenService.GetRefreshTokenByValue(refreshToken);
            // is valid refresh token
            if (_refreshToken is not null)
            {
                if (_refreshToken.IsExpiredAt > DateTime.Now)
                {
                    if (!_refreshToken.IsRevoke && _refreshToken.IsUsed)
                    {
                        var user = await _userService.GetUserByRefreshTokenId(_refreshToken.Id) ?? await _userService.GetUserByRefreshTokenId(_refreshToken.ParentId);
                        // Is Parent, because User refference to RefreshToken 
                        if (user is not null)
                        {
                            string roleName = user.RoleId == 1 ? RoleName.user.ToString() : RoleName.admin.ToString();

                            string token = _utils.GenerateToken(user);
                            // add children-refreshToken

                            var newRefreshToken = _utils.GenerateRefreshToken();

                            int parentId = _refreshToken.ParentId ?? _refreshToken.Id;

                            newRefreshToken.ParentId = parentId;

                            await _refreshTokenService.SaveRefreshToken(newRefreshToken);

                            _refreshToken.IsUsed = false;

                            await _refreshTokenService.UpdateRefreshToken(_refreshToken);

                            Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
                            {
                                HttpOnly = true,
                                Expires = _refreshToken.IsExpiredAt
                            });

                            return Ok(new
                            {
                                Success = true,
                                Code = 0,
                                Data = new
                                {
                                    Id = user.Id.ToString(),
                                    Email = user.Email.ToString(),
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
            // invalid refreh token
            // refreshToken exists or not exists
            // send response remove cookie to client
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
            });
            return Unauthorized(new
            {
                Message = "Không đủ quyền truy cập",
                Code = 4012,
                Success = false
            });
        }

        private async Task<IActionResult> LogoutHandle()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            // get from database find by Token (value)
            var _refreshToken = await _refreshTokenService.GetRefreshTokenByValue(refreshToken);
            // if refresh token existing
            if (_refreshToken is not null)
            {
                // if refresh token is Used
                if (_refreshToken.IsUsed)
                {
                    var updateRefreshToken = _refreshToken;
                    // set is used = false, update refresh token
                    updateRefreshToken.IsUsed = false;
                    await _refreshTokenService.UpdateRefreshToken(updateRefreshToken);
                    // is children
                    if (_refreshToken.ParentId is not null)
                    {
                        return Ok(new
                        {
                            Success = true,
                            Code = 200,
                            Message = "ok"
                        });
                    }
                    // is parent
                    else
                    {
                        return Ok(new
                        {
                            Success = false,
                            Code = 200,
                            Message = "ok"
                        });
                    }
                }
            }
            // refreshToken exists or not exists
            // send response remove cookie to client
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
            });
            return Ok(new { Success = true });
        }

        // check auth
        private IActionResult HandleTest()
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
    }
}
