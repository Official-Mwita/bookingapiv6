using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
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

                List<Hashtable> bookings = new List<Hashtable>();
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
            if(bookingID == 0)
            {
                return await UserBooking(int.Parse(User.FindFirst(ClaimTypes.PrimarySid)?.Value ?? "-1"));
            }
            else
            {
                Hashtable sample = new Hashtable();
                sample.Add("sample", "sample 1");
                sample.Add("sample2", "sample 1");
                sample.Add("sample3", "sample 1");
                //Select a booking by ID
                return new JsonResult(sample);
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

                List<Hashtable> bookings = new List<Hashtable>();
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
                List<Hashtable> bookings = new List<Hashtable>();
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

                if(true)//save booking else return unauthorized
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

                                SqlDataReader reader = await command.ExecuteReaderAsync();

                                reader.Read();
                                uBooking.Booking.BookingId = (int)reader.GetDecimal(0);
                                uBooking.Booking.UserId = userId;


                            }

                        }

                        //Return resulst
                        resBody.Add("Success", true);
                        resBody.Add("Booking", uBooking);

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

        // put api/<BookingController>
        [HttpPut]
        [Route("UpdateBooking")]
        public async Task<IActionResult> Update([FromBody] MBooking booking)
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
                                command.Parameters.AddWithValue("bookingId", SqlDbType.Int).Value = booking.BookingId;
                                command.Parameters.AddWithValue("externalSchemeAdmin", SqlDbType.NVarChar).Value = booking.ExternalSchemeAdmin;
                                command.Parameters.AddWithValue("bookingType", SqlDbType.NVarChar).Value = booking.BookingType;
                                command.Parameters.AddWithValue("retirementSchemeName", SqlDbType.NVarChar).Value = booking.RetirementSchemeName;
                                command.Parameters.AddWithValue("schemePosition", SqlDbType.NVarChar).Value = booking.SchemePosition;
                                command.Parameters.AddWithValue("trainingVenue", SqlDbType.NVarChar).Value = booking.TrainingVenue;
                                command.Parameters.AddWithValue("paymentMode", SqlDbType.NVarChar).Value = booking.PaymentMode;
                                command.Parameters.AddWithValue("additionalRequirements", SqlDbType.NVarChar).Value = booking.AdditionalRequirements;
                                command.Parameters.AddWithValue("userId ", SqlDbType.Int).Value = userId;

                                SqlDataReader reader = await command.ExecuteReaderAsync();

                                booking.UserId = userId;

                            }

                        }

                        //Return resulst
                        resBody.Add("status", true);
                        resBody.Add("Booking", booking);

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
        private Hashtable customBooking(MBooking mBooking)
        {
            Hashtable customBooking = new Hashtable
            {
                { "bookingId", mBooking.BookingId },
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
