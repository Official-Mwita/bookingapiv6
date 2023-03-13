using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly IConfiguration _config;
        public UserController(IConfiguration config) 
        {
            _config = config;
           
        }


        // GET Get a user by ID
        [HttpGet]
        public IActionResult Get()
        {
            Hashtable values = new Hashtable();
            
            values["Success"] = true;
            values["user"] = new MUser()
            {
                UserName = User.FindFirst(ClaimTypes.GivenName)?.Value?? "",
                FullName = User.FindFirst(ClaimTypes.Name)?.Value?? "",
                UserID = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0"),
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? ""

            };

            return new OkObjectResult(values);
        }

        //Update User
       [HttpPut]
        public async Task<IActionResult> Put([FromBody] MUser user)
        {
            Hashtable values = new Hashtable(); //Return values in form of a message and result

            //Get user id
            int userID = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0");

            if (ModelState.IsValid)//Update user
            {
                bool response = await CreateUpdateUser(user, userID);

                if (response)
                {
                    user.Password = " ";
                    values.Add("user", user);
                    values.Add("success", true);

                    return new OkObjectResult(values);
                }

            }

            //Return values in case of an errror
            values.Add("Message", "Error occurred while processing your request. Try again");
            values.Add("Success", false);
            return new BadRequestObjectResult(values);
            

           
        }

        private async Task<bool> CreateUpdateUser(MUser user, int id)
        {
            //Catch error while registering
            try
            {
                using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
                {
                    _connection.OpenAsync().Wait();

                    using (SqlCommand command = new SqlCommand("spInsertUpdateUser", _connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("userID", SqlDbType.NVarChar).Value = id;
                        command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = user.UserName;
                        command.Parameters.AddWithValue("password", SqlDbType.NVarChar).Value = user.Password;
                        command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = user.Email;
                        command.Parameters.AddWithValue("fullName", SqlDbType.NVarChar).Value = user.FullName;
                        command.Parameters.AddWithValue("physicalAddress", SqlDbType.NVarChar).Value = user.PhysicalAddress;
                        command.Parameters.AddWithValue("telephone", SqlDbType.NVarChar).Value = user.Telephone;
                        command.Parameters.AddWithValue("originCountry", SqlDbType.NVarChar).Value = user.OriginCountry;
                        command.Parameters.AddWithValue("employerName", SqlDbType.NVarChar).Value = user.EmployerName;
                        command.Parameters.AddWithValue("experience", SqlDbType.Int).Value = user.Experience;
                        command.Parameters.AddWithValue("position", SqlDbType.NVarChar).Value = user.Position;
                        command.Parameters.AddWithValue("disabilityStatus", SqlDbType.NVarChar).Value = user.DisabilityStatus;

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        reader.ReadAsync().Wait();

                        if (reader.GetInt32(0) == 1) //Success
                        

                            return true;
                       else
                       
                            return false;

                    }

                }



            }
            catch
            {
                //Log error message

                //values.Add("Message", e.Message);
                //values.Add("Success", false);
                //return new BadRequestObjectResult(values);
                return false;
            }

        }
       
    }
}
