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
                        Role = RoleName.User.ToString().ToLower()
                    }
                });
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

                // create new refresh token and set userId for refresh token after 
                refreshToken.User = user;
                await _refreshTokenService.SaveRefreshToken(refreshToken);

                //set coookies
                Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = refreshToken.IsExpiredAt
                });

                string roleName = user.RoleId == 1 ? RoleName.User.ToString() : RoleName.Admin.ToString();
                return Ok(new
                {
                    Success = true,
                    Code = 0,
                    Message = "Đăng nhập thành công",
                    Data = new
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email.ToString(),
                        Role = roleName.ToLower(),
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
            // if exists refresh token
            if (_refreshToken is not null)
            {
                if (_refreshToken.IsUsed)
                {
                    if (_refreshToken.IsExpiredAt > DateTime.Now)
                    {
                        // find user by userId in refresh token
                        var user = await _userService.GetUserById(_refreshToken.UserId);
                        if (user is not null)
                        {
                            string roleName = user.RoleId == 1 ? RoleName.User.ToString() : RoleName.Admin.ToString();
                            string token = _utils.GenerateToken(user);
                            var newRefreshToken = _utils.GenerateRefreshToken();
                            newRefreshToken.UserId = user.Id;
                            // set parentId for newRefreshToken
                            string parentId = _refreshToken.ParentId ?? _refreshToken.Id;
                            newRefreshToken.ParentId = parentId;
                            _refreshToken.IsUsed = false;
                            await _refreshTokenService.UpdateRefreshToken(_refreshToken);
                            await _refreshTokenService.SaveRefreshToken(newRefreshToken);
                            Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
                            {
                                HttpOnly = true,
                                Expires = newRefreshToken.IsExpiredAt
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
                }
                // if it isUsed = false
                else
                {
                    string parentId = _refreshToken.ParentId ?? _refreshToken.Id;
                    if (!_refreshToken.IsRevoke)
                    {
                        var tokenIsRevoked = await _refreshTokenService.GetRefreshTokenInBrachIsRevoke(parentId);
                        if (tokenIsRevoked is not null)
                        {
                            await _refreshTokenService.DeleteChildrenRefreshTokenByParentId(parentId);
                            // delete parent
                            await _refreshTokenService.DeleteById(parentId);
                            Response.Cookies.Delete("refreshToken", new CookieOptions
                            {
                                HttpOnly = true,
                            });
                            return Unauthorized(new
                            {
                                Message = "Không đủ quyền truy cập",
                                Code = 4011,
                                Success = false
                            });
                        }
                        else
                        {
                            // update isRevoke = true
                            _refreshToken.IsRevoke = true;
                            await _refreshTokenService.UpdateRefreshToken(_refreshToken);
                            return Unauthorized(new
                            {
                                Message = "Không đủ quyền truy cập- old-rt",
                                Code = 4013,
                                Success = false
                            });
                        }
                    }
                    else
                    {
                        // delete all child
                        await _refreshTokenService.DeleteChildrenRefreshTokenByParentId(parentId);
                        // delete parent
                        await _refreshTokenService.DeleteById(parentId);
                        Response.Cookies.Delete("refreshToken", new CookieOptions
                        {
                            HttpOnly = true,
                        });
                        return Unauthorized(new
                        {
                            Message = "Không đủ quyền truy cập",
                            Code = 4011,
                            Success = false
                        });
                    }

                }
            }
            // refreshToken not exists
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
            // get token from cookies
            var refreshToken = Request.Cookies["refreshToken"];
            // get from database find by Token (value)
            var _refreshToken = await _refreshTokenService.GetRefreshTokenByValue(refreshToken);
            // if refresh token exists in database
            if (_refreshToken is not null)
            {
                // if it isUsed
                if (_refreshToken.IsUsed)
                {
                    // if it have parent
                    if (_refreshToken.ParentId is not null)
                    {
                        string parentId = _refreshToken.ParentId;
                        await _refreshTokenService.DeleteChildrenRefreshTokenByParentId(parentId);
                        await _refreshTokenService.DeleteById(parentId);
                    }
                    // if it is parent -> it don't have children
                    else
                    {
                        await _refreshTokenService.DeleteById(_refreshToken.Id);
                    }
                    // delete cookies
                    Response.Cookies.Delete("refreshToken", new CookieOptions
                    {
                        HttpOnly = true,
                    });
                    return Ok(new
                    {
                        Success = true,
                        Code = 200,
                        Message = "ok"
                    });
                }
                else
                {
                    // không nên xóa cái bị revoke vì nó là cái để đánh dấu cho kiểm tra refresh
                    // delete token isUsed = false if it not is parent
                    // isRevoke = false, isUsed = false and it is child -> remove it
                    if(!_refreshToken.IsRevoke && _refreshToken.ParentId is not null)
                    {
                        await _refreshTokenService.DeleteById(_refreshToken.Id);
                        Response.Cookies.Delete("refreshToken", new CookieOptions
                        {
                            HttpOnly = true,
                        });
                        return Ok(new
                        {
                            Success = false,
                            Code = 200,
                            Message = "ok"
                        });
                    }
                    // if revoke = true doing nothing
                    
                }
            }
            // if not exist send response remove cookie to client
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
            });
            return Ok(new { Success = true });
        }

        // test auth
        private IActionResult HandleTest()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is not null)
            {
                var id = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = _userService.GetUserById(id);
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
