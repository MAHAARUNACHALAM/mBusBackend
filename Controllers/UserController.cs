
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JwtAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IConfiguration configuration;
        SqlConnection con;
        public UserController(IConfiguration config)
        {
            configuration = config;

            con = new SqlConnection(configuration.GetConnectionString("DB"));
        }

        //POST: api/signup
        [HttpPost]
        [Route("signup")]
        //signup method for employee registration and password hashing and save to database
        public IActionResult Signup([FromBody] LoginSchema UserLogin)
        {
            //check if the employee already exists
            if (UserLogin == null)
            {
                return BadRequest("Invalid client request");
            }
            //generate id and convert to int with 3 digit positive
            Random random = new Random();
            int id = random.Next(100, 999);
            
            //hash the password with salt value
            var passwordHash = new PasswordHasher<LoginSchema>();
            UserLogin.Password = passwordHash.HashPassword(UserLogin, UserLogin.Password);
            
            //get list of phone numbers from adminacess table and if the phone number is present in the list then set role_id =2
            con.Open();
            SqlCommand cmd1 = new SqlCommand("select * from AdminAccess", con);
            SqlDataReader reader = cmd1.ExecuteReader();
            if (reader.Read())
            {
                //checking wheather it matches the phone number in request
                if (reader["PhoneNumber"].ToString() == UserLogin.PhoneNumber)
                {
                    //set role_id =2
                    SqlCommand cmd2 = new SqlCommand("insert into UserId values(@Id,@PhoneNumber,@Password,@RoleId)", con);
                    cmd2.Parameters.AddWithValue("@Id", id);
                    cmd2.Parameters.AddWithValue("@PhoneNumber", UserLogin.PhoneNumber);
                    cmd2.Parameters.AddWithValue("@Password", UserLogin.Password);
                    cmd2.Parameters.AddWithValue("@RoleId", 2);
                    cmd2.ExecuteNonQuery();
                    con.Close();
                    return Ok(UserLogin);
                }

            }
            //save the employee to the database
            con.Close();
            
            con.Open();
            SqlCommand cmd = new SqlCommand("insert into UserId values(@Id,@PhoneNumber,@Password,@RoleId)", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@PhoneNumber", UserLogin.PhoneNumber);
            cmd.Parameters.AddWithValue("@Password", UserLogin.Password);
            cmd.Parameters.AddWithValue("@RoleId",3);
            cmd.ExecuteNonQuery();

            con.Close();

            return Ok(UserLogin);
        }

        //POST: api/login
        [HttpPost]
        [Route("login")]
        //login method for employee login and password verification take email and password as input
        //Take only email and password as input from the userschema
        public IActionResult Login([FromBody] LoginSchema UserLogin)
        {
            //check if the employee already exists
            //if username or password is null return bad request
            if (UserLogin.PhoneNumber == null || UserLogin.Password == null)
            {
                return BadRequest("Invalid client request");
            }
            //check if the employee exists in the database
            con.Open();
            SqlCommand cmd = new SqlCommand("select * from UserId where PhoneNumber=@Email", con);
            cmd.Parameters.AddWithValue("@Email",UserLogin.PhoneNumber);
            SqlDataReader reader = cmd.ExecuteReader();
            //Generate jwt token for authentication if passwod matches and also import the necessary package
            if (reader.Read())
            {
                var passwordHash = new PasswordHasher<LoginSchema>();
                var result = passwordHash.VerifyHashedPassword(UserLogin,reader["Password"].ToString(), UserLogin.Password);

                if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, reader["RoleId"].ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                                               configuration["Jwt:Issuer"],
                                               configuration["Jwt:Issuer"],
                                               claims,
                                               expires: DateTime.Now.AddMinutes(30),
                                               signingCredentials: creds);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),                           
                             Roleid = reader["RoleId"],
                             Status = 1,
                        Id = reader["Id"]
                      
                });
                }
            }
            con.Close();
            return Unauthorized();

        }
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }
        [HttpPost]
        [Route("changepassword")]
        public IActionResult changePassword([FromBody] ChangePasswordSchema employee)
        {
            //Check the current password and update it
            if (employee.Email == null || employee.Password == null)
            {
                return BadRequest("Invalid client request");
            }
            //check if current password is correct
            con.Open();
            SqlCommand cmd = new SqlCommand("select * from UserId where Email=@Email", con);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            SqlDataReader reader = cmd.ExecuteReader();
            //update the password if current password is correct
            if (reader.Read())
            {
                var passwordHash = new PasswordHasher<ChangePasswordSchema>();
                var result = passwordHash.VerifyHashedPassword(employee, reader["Password"].ToString(), employee.Password);
                if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                {
                    //hash the password with salt value
                    var passwordHash1 = new PasswordHasher<ChangePasswordSchema>();
                    employee.Password = passwordHash1.HashPassword(employee, employee.Password);
                    //update the password
                    SqlCommand cmd1 = new SqlCommand("update UserId set Password=@Password where Email=@Email", con);
                    cmd1.Parameters.AddWithValue("@Email", employee.Email);
                    cmd1.Parameters.AddWithValue("@Password", employee.NewPassword);
                    cmd1.ExecuteNonQuery();
                    con.Close();
                    return Ok(employee);
                }
            }
            con.Close();
            return Unauthorized();
        }

        //Validate token
        [HttpGet]
        [Route("validate")]
        public IActionResult Validate(String token)
        {
            //Check whether token is valid or not
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            //include try catch block to handle exceptions
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                //if token is valid return ok
                if (validatedToken != null)
                {
                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var RoleId = jwtToken.Claims.First(x => x.Type == "sub").Value;
                    return Ok(RoleId);
                }
                return Unauthorized();
            }
            catch (Exception e)
            {
                return Unauthorized();
            }
           
        }
        //Test get
        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("Test");
        }

        
    }
}
