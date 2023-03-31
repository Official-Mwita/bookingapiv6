using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly SqlConnection _connection;
        public BookingController(IConfiguration config) 
        {

            _connection = new SqlConnection(config.GetConnectionString("connString"));
        }
        // GET: api/<BookingController>
        //Get all bookings. Only admin
        [HttpGet]
        [Authorize(Policy = "AdminUser")]
        [Route("/api/Booking/GetAll")]
        public async Task<JsonResult> GetAll(int? start, int? end)
        {

            try
            {

                List<SortedDictionary<string, string>> bookings = new List<SortedDictionary<string, string>>();
                JsonResult result = new JsonResult(bookings);

                //Set defaults for start and end indeces if not given
                start = start ?? 0;
                end = end ?? 100000;

               // end = end > 1000 ? 100 : end;

                using (_connection)
                {
                    //Connect to database then read booking records
                    _connection.OpenAsync().Wait();

                    using (SqlCommand command = new SqlCommand("spSelectAllBookings", _connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("start", SqlDbType.Int).Value = start;
                        command.Parameters.AddWithValue("end", SqlDbType.Int).Value = 10000;

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            bookings.Add(customBooking(new MBooking
                            {
                                BookingId = reader.GetInt64(0),
                                ExternalSchemeAdmin = reader.GetString(1),
                                CourseDate = reader.GetDateTime(2).Date.ToString(),
                                BookingType = reader.GetString(3),
                                RetirementSchemeName = reader.GetString(4),
                                SchemePosition = reader.GetString(5),
                                TrainingVenue = reader.GetString(6),
                                PaymentMode = reader.GetString(7),
                                AdditionalRequirements = reader.GetString(8),
                                UserId = reader.GetInt64(9)

                            }));

                        }

                    }

                }

                return result;

            }
            catch(Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        [HttpGet]
        public async Task<JsonResult> Get([FromQuery]int? bookingID)
        {
            bookingID = bookingID ?? 0;
            int userID = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "-1");
            if (bookingID == 0)
            {
                
                if(userID == 21 || userID == 23 || userID == 19)
                {
                    return await GetAll(0, 100000000);
                }
                return await UserBooking(userID);
            }
            else
            {
                MBooking? booking = null;
                MUser? user = null;

                //Select a booking by ID
                try
                {
                    using (_connection)
                    {
                        await _connection.OpenAsync();

                        using (SqlCommand command = new SqlCommand("spSelectBookingbyID", _connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("bookingID", SqlDbType.Int).Value = bookingID;
                            command.Parameters.AddWithValue("userID", SqlDbType.Int).Value = userID;

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (reader.Read())
                                {
                                    booking = new MBooking
                                    {
                                        BookingId = reader.GetInt64(0),
                                        ExternalSchemeAdmin = reader.GetString(1),
                                        CourseDate = reader.GetDateTime(2).Date.ToString(),
                                        BookingType = reader.GetString(3),
                                        RetirementSchemeName = reader.GetString(4),
                                        SchemePosition = reader.GetString(5),
                                        TrainingVenue = reader.GetString(6),
                                        PaymentMode = reader.GetString(7),
                                        AdditionalRequirements = reader.GetString(8),
                                        UserId = reader.GetInt64(9)

                                    };

                                    user = new MUser
                                    {
                                        UserID = userID,
                                        DisabilityStatus = reader.GetString(10),
                                        Email = reader.GetString(11),
                                        EmployerName = reader.GetString(12),
                                        Experience = reader.GetInt32(13),
                                        FullName = reader.GetString(14),
                                        IdNumber = reader.GetString(15),
                                        PhysicalAddress = reader.GetString(16),
                                        Position = reader.GetString(17),
                                        Telephone = reader.GetString(18)

                                    };                               

                                }

                            }
                        }
                    }

                }
                catch
                {
                    //Error occured with the request. Set status code to bad request
                    HttpContext.Response.StatusCode = 409;
                }

                Hashtable userbooking = new Hashtable
                {
                    { "booking", booking},
                    { "user", user }
                };
                
                return new JsonResult(userbooking);
            }
            


            //try
            //{

            //    List<MBooking> bookings = new List<MBooking>();
            //    JsonResult result = new JsonResult(bookings);

            //    //If not admin or owner of the record return empty list
            //    if (!User.HasClaim(MUser.ADMIN_TYPE, "admin")
            //        && (int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0") != userID)
            //        )
            //        return result;



            //    //end = end > 1000 ? 100 : end;

            //    //Connect to database then read booking records
            //    _connection.OpenAsync().Wait();

            //    using (SqlCommand command = new SqlCommand("spSelectUserBookings", _connection))
            //    {
            //        command.CommandType = CommandType.StoredProcedure;
            //        command.Parameters.AddWithValue("id", SqlDbType.Int).Value = userID;

            //        SqlDataReader reader = await command.ExecuteReaderAsync();
            //        while (reader.Read())
            //        {
            //            bookings.Add(new MBooking
            //            {
            //                BookingId = reader.GetInt64(0),
            //                ExternalSchemeAdmin = reader.GetString(1),
            //                CourseDate = reader.GetDateTime(2).Date.ToString(),
            //                BookingType = reader.GetString(3),
            //                RetirementSchemeName = reader.GetString(4),
            //                SchemePosition = reader.GetString(5),
            //                TrainingVenue = reader.GetString(6),
            //                PaymentMode = reader.GetString(7),
            //                AdditionalRequirements = reader.GetString(8),
            //                UserId = reader.GetInt64(9)

            //            });

            //        }

            //    }

            //    return result;

            //}
            //catch (Exception ex)
            //{
            //    return new JsonResult(ex.Message);
            //}
        }

        [HttpGet("user")]
        //Get a record per user
        public async Task<JsonResult> UserBooking(int? userID)
        {
           

            try
            {

                List<SortedDictionary<string, string>> bookings = new List<SortedDictionary<string, string>>();
                JsonResult result = new JsonResult(bookings);

                userID = userID ?? (User.HasClaim(MUser.ADMIN_TYPE, "admin") ? 0 : int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value??"0"));

                //If not admin or owner of the record return empty list
                if (!User.HasClaim(MUser.ADMIN_TYPE, "admin")
                    && (int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value??"0") != userID)
                    )
                    return result;


               
                //end = end > 1000 ? 100 : end;

                //Connect to database then read booking records
                _connection.OpenAsync().Wait();

                using (SqlCommand command = new SqlCommand("spSelectUserBookings", _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("id", SqlDbType.Int).Value = userID;

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        bookings.Add(customBooking(new MBooking
                        {
                            BookingId = reader.GetInt64(0),
                            ExternalSchemeAdmin = reader.GetString(1),
                            CourseDate = reader.GetDateTime(2).Date.ToString(),
                            BookingType = reader.GetString(3),
                            RetirementSchemeName = reader.GetString(4),
                            SchemePosition = reader.GetString(5),
                            TrainingVenue = reader.GetString(6),
                            PaymentMode = reader.GetString(7),
                            AdditionalRequirements = reader.GetString(8),
                            UserId = reader.GetInt64(9)

                        }));

                    }

                }

                return result;

            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetbyDate")]
        public async Task<JsonResult> GetByDate(string? startdate="yyyy-mm-dd", string? enddate = "yyyy-mm-dd")
        {
            try
            {
                List<SortedDictionary<string, string>> bookings = new List<SortedDictionary<string, string>>();
                JsonResult result = new JsonResult(bookings);

                int userId = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "-1");

                if (User.HasClaim(MUser.ADMIN_TYPE, "admin"))
                    userId = 0;


                DateOnly start, end, _default;

                DateOnly.TryParse("default", out _default);


                DateOnly.TryParse(startdate, out start);
                DateOnly.TryParse(enddate, out end);

                if(start == _default)
                {
                    start = end;
                }
                
                
                if(start > end)
                {
                    end= start;
                }


                //Connect to database then read booking records
                _connection.OpenAsync().Wait();

                using (SqlCommand command = new SqlCommand("spSelectBookingDate", _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("start", SqlDbType.NVarChar).Value = start.ToLongDateString();
                    command.Parameters.AddWithValue("end", SqlDbType.NVarChar).Value = end.ToLongDateString();
                    command.Parameters.AddWithValue("userID", SqlDbType.Int).Value = userId;

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                      
                        bookings.Add(customBooking(new MBooking
                        {
                            BookingId = reader.GetInt64(0),
                            ExternalSchemeAdmin = reader.GetString(1),
                            CourseDate = reader.GetDateTime(2).Date.ToString(),
                            BookingType = reader.GetString(3),
                            RetirementSchemeName = reader.GetString(4),
                            SchemePosition = reader.GetString(5),
                            TrainingVenue = reader.GetString(6),
                            PaymentMode = reader.GetString(7),
                            AdditionalRequirements = reader.GetString(8),
                            UserId = reader.GetInt64(9)

                        }));


                    }
                }


                return result;

            }
            catch(Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }


        // POST api/<BookingController>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] UserBooking uBooking)
        {
            if(ModelState.IsValid) //Process data
            {
               Hashtable resBody = new Hashtable();

                //If user id is not the same as logged in user id return
                int userId = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0");

                //if(User.Identity.IsAuthenticated)//save booking else return unauthorized
                {
                    //Engulf in a exception
                    try
                    {
                        using (_connection)
                        {
                            //Connect to database then read booking records
                            _connection.OpenAsync().Wait();

                            using (SqlCommand command = new SqlCommand("spInsertUpdateBooking", _connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("bookingId", SqlDbType.Int).Value = 0;
                                command.Parameters.AddWithValue("externalSchemeAdmin", SqlDbType.NVarChar).Value = uBooking.Booking.ExternalSchemeAdmin;
                                command.Parameters.AddWithValue("bookingType", SqlDbType.NVarChar).Value = uBooking.Booking.BookingType;
                                command.Parameters.AddWithValue("retirementSchemeName", SqlDbType.NVarChar).Value = uBooking.Booking.RetirementSchemeName;
                                command.Parameters.AddWithValue("schemePosition", SqlDbType.NVarChar).Value = uBooking.Booking.SchemePosition;
                                command.Parameters.AddWithValue("trainingVenue", SqlDbType.NVarChar).Value = uBooking.Booking.TrainingVenue;
                                command.Parameters.AddWithValue("paymentMode", SqlDbType.NVarChar).Value = uBooking.Booking.PaymentMode;
                                command.Parameters.AddWithValue("additionalRequirements", SqlDbType.NVarChar).Value = uBooking.Booking.AdditionalRequirements;
                                command.Parameters.AddWithValue("userId ", SqlDbType.Int).Value = userId;
                                command.Parameters.AddWithValue("disabilityStatus", SqlDbType.NVarChar).Value = uBooking.User.DisabilityStatus;
                                command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = uBooking.User.Email;
                                command.Parameters.AddWithValue("employerName", SqlDbType.NVarChar).Value = uBooking.User.EmployerName;
                                command.Parameters.AddWithValue("experience", SqlDbType.Int).Value = uBooking.User.Experience;
                                command.Parameters.AddWithValue("fullName", SqlDbType.NVarChar).Value = uBooking.User.FullName;
                                command.Parameters.AddWithValue("idNumber", SqlDbType.NVarChar).Value = uBooking.User.IdNumber;
                                command.Parameters.AddWithValue("physicalAddress", SqlDbType.NVarChar).Value = uBooking.User.PhysicalAddress;
                                command.Parameters.AddWithValue("position", SqlDbType.NVarChar).Value = uBooking.User.Position;
                                command.Parameters.AddWithValue("telephone", SqlDbType.NVarChar).Value = uBooking.User.Telephone;

                                SqlDataReader reader = await command.ExecuteReaderAsync();

                                reader.Read();
                                uBooking.Booking.BookingId = (int)reader.GetDecimal(0);
                                uBooking.Booking.UserId = userId;


                            }

                        }

                        //Return resulst
                        resBody.Add("Success", true);

                        //Create a custom booking to match specification
                        Hashtable customUBooking = new Hashtable
                        {
                            { "user", uBooking.User },
                            { "booking", customBooking(uBooking.Booking) }
                        };
                        resBody.Add("Booking", customUBooking);

                        return new OkObjectResult(resBody);

                    }
                    catch(Exception ex)
                    {
                        resBody.Add("Error_Message", ex.Message);
                        resBody.Add("Success", false);

                        return new BadRequestObjectResult(resBody);

                    }


                }

            }
            else
            {
                //Error occured
                return new BadRequestResult();
            }
        }

        [HttpDelete()]
        public async Task<IActionResult> Delete(int bookingID)
        {
            //Get user ID
            int userId = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0");

            try
            {
                using (_connection)
                {
                    //Connect to database then read booking records
                    _connection.OpenAsync().Wait();

                    using (SqlCommand command = new SqlCommand("spDeleteBooking", _connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("bookingId", SqlDbType.Int).Value = bookingID;
                        command.Parameters.AddWithValue("userId", SqlDbType.NVarChar).Value = userId;

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                    }
                }

                //Return deleted record
                var res = new { message = $"Booking with ID {bookingID} will deleted", status = true};

                return new OkObjectResult(res);
            }
            catch
            {
                return BadRequest();
            }

        }

        // put api/<BookingController>
        [HttpPut]
        [Route("UpdateBooking")]
        public async Task<IActionResult> Update([FromBody] UserBooking uBooking)
        {
            if (ModelState.IsValid) //Process data
            {
                Hashtable resBody = new Hashtable();

                //If user id is not the same as logged in user id return
                int userId = int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "0");

                if (User.Identity?.IsAuthenticated?? false)//save booking else return unauthorized
                {
                    //Engulf in a exception
                    try
                    {
                        using (_connection)
                        {
                            //Connect to database then read booking records
                            _connection.OpenAsync().Wait();

                            using (SqlCommand command = new SqlCommand("spInsertUpdateBooking", _connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("bookingId", SqlDbType.Int).Value = uBooking.Booking.BookingId;
                                command.Parameters.AddWithValue("externalSchemeAdmin", SqlDbType.NVarChar).Value = uBooking.Booking.ExternalSchemeAdmin;
                                command.Parameters.AddWithValue("bookingType", SqlDbType.NVarChar).Value = uBooking.Booking.BookingType;
                                command.Parameters.AddWithValue("retirementSchemeName", SqlDbType.NVarChar).Value = uBooking.Booking.RetirementSchemeName;
                                command.Parameters.AddWithValue("schemePosition", SqlDbType.NVarChar).Value = uBooking.Booking.SchemePosition;
                                command.Parameters.AddWithValue("trainingVenue", SqlDbType.NVarChar).Value = uBooking.Booking.TrainingVenue;
                                command.Parameters.AddWithValue("paymentMode", SqlDbType.NVarChar).Value = uBooking.Booking.PaymentMode;
                                command.Parameters.AddWithValue("additionalRequirements", SqlDbType.NVarChar).Value = uBooking.Booking.AdditionalRequirements;
                                command.Parameters.AddWithValue("userId ", SqlDbType.Int).Value = userId;
                                command.Parameters.AddWithValue("disabilityStatus", SqlDbType.NVarChar).Value = uBooking.User.DisabilityStatus;
                                command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = uBooking.User.Email;
                                command.Parameters.AddWithValue("employerName", SqlDbType.NVarChar).Value = uBooking.User.EmployerName;
                                command.Parameters.AddWithValue("experience", SqlDbType.Int).Value = uBooking.User.Experience;
                                command.Parameters.AddWithValue("fullName", SqlDbType.NVarChar).Value = uBooking.User.FullName;
                                command.Parameters.AddWithValue("idNumber", SqlDbType.NVarChar).Value = uBooking.User.IdNumber;
                                command.Parameters.AddWithValue("physicalAddress", SqlDbType.NVarChar).Value = uBooking.User.PhysicalAddress;
                                command.Parameters.AddWithValue("position", SqlDbType.NVarChar).Value = uBooking.User.Position;
                                command.Parameters.AddWithValue("telephone", SqlDbType.NVarChar).Value = uBooking.User.Telephone;

                                SqlDataReader reader = await command.ExecuteReaderAsync();

                                uBooking.Booking.UserId = userId;

                            }

                        }

                        //Return resulst
                        resBody.Add("status", true);
                        resBody.Add("Booking", customBooking(uBooking.Booking));

                        return new OkObjectResult(resBody);

                    }
                    catch (Exception ex)
                    {
                        resBody.Add("Error_Message", ex.Message);
                        resBody.Add("status", false);

                        return new BadRequestObjectResult(resBody);

                    }


                }

                //Not authenticated. Exit with error
                return new UnauthorizedResult();


            }
            else
            {
                //Error occured
                return new BadRequestResult();
            }
        }

        //Used to create a custom booking to suit current design
        //That is returning specific data as specificed in the front-end
        private SortedDictionary<string, string> customBooking(MBooking mBooking)
        {
            SortedDictionary<string, string> customBooking = new SortedDictionary<string, string>
            {
                { "bookingId", "" + mBooking.BookingId },
                { "externalSchemeAdmin", mBooking.ExternalSchemeAdmin },
                { "bookingType", mBooking.BookingType },
                { "retirementSchemeName", mBooking.RetirementSchemeName },
                { "schemePosition", mBooking.SchemePosition },
                { "trainingVenue", mBooking.TrainingVenue }
            };

            return customBooking;
        }
    }

    public class UserBooking
    {
        public MUser User { get; set; } = new MUser();

        public MBooking Booking { get; set; } = new MBooking();

    }
}
