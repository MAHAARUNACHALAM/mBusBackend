
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace mBus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusController : ControllerBase
    {
        IConfiguration configuration;
        SqlConnection con;
        public BusController(IConfiguration config)
        {
            configuration = config;

            con = new SqlConnection(configuration.GetConnectionString("DB"));
        }

        [HttpPost]
        [Route("addbus")]
        //execute this if token is valid through authorization header '/api/User/Validation'
        public IActionResult AddBus([FromBody] BusSchema bus)
        {
            //Display the bus details
            Console.WriteLine(bus.BusNumber);
            Console.WriteLine(bus.Name);
            Console.WriteLine(bus.Source);
            Console.WriteLine(bus.Destination);
            Console.WriteLine(bus.Departure);
            Console.WriteLine(bus.Arrival);
            Console.WriteLine(bus.TotalSeats);
            Console.WriteLine(bus.AvailableSeats);
            Console.WriteLine(bus.Price);
            Console.WriteLine(bus.BusType);

            //read and add to db
            con.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Bus (BusNumber, Name, Source, Destination, Departure, Arrival, TotalSeats, AvailableSeats, Price, BusType)VALUES (@BusNumber, @Name, @Source, @Destination, @Departure, @Arrival, @TotalSeats, @AvailableSeats, @Price, @BusType)", con);
            cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
            cmd.Parameters.AddWithValue("@Name", bus.Name);
            cmd.Parameters.AddWithValue("@Source", bus.Source);
            cmd.Parameters.AddWithValue("@Destination", bus.Destination);
            cmd.Parameters.AddWithValue("@Departure", bus.Departure.ToString());
            cmd.Parameters.AddWithValue("@Arrival", bus.Arrival.ToString());
            cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
            cmd.Parameters.AddWithValue("@AvailableSeats", bus.AvailableSeats);
            cmd.Parameters.AddWithValue("@Price", bus.Price);
            cmd.Parameters.AddWithValue("@BusType", bus.BusType);
            cmd.ExecuteNonQuery();
            con.Close();
            return Ok("Bus Added Successfully");
        }
        //edit bus
        [HttpPost]
        [Route("editbus")]
        //execute this if token is valid through authorization header '/api/User/Validation'
        //takes the reference id and the updated details
        public IActionResult EditBus([FromBody] BusEditSchema bus)
        {
            //read and update to db
            con.Open();
            SqlCommand cmd = new SqlCommand("update Bus set BusNumber=@BusNumber,Name=@Name,Source=@Source,Destination=@Destination,Departure=@Departure,Arrival=@Arrival,TotalSeats=@TotalSeats,AvailableSeats=@AvailableSeats,Price=@Price,BusType=@BusType  where ReferenceId=@ReferenceId", con);
            cmd.Parameters.AddWithValue("@ReferenceId",bus.ReferenceId);
            cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
            cmd.Parameters.AddWithValue("@Name", bus.Name);
            cmd.Parameters.AddWithValue("@Source", bus.Source);
            cmd.Parameters.AddWithValue("@Destination", bus.Destination);
            cmd.Parameters.AddWithValue("@Departure", bus.Departure);
            cmd.Parameters.AddWithValue("@Arrival", bus.Arrival);
            cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
            cmd.Parameters.AddWithValue("@AvailableSeats", bus.AvailableSeats);
            cmd.Parameters.AddWithValue("@Price", bus.Price);
            cmd.Parameters.AddWithValue("@BusType", bus.BusType);
            cmd.ExecuteNonQuery();
            con.Close();
            return Ok("Bus Updated Successfully");
        }
        //delete bus with reference id
        [HttpPost]
        public IActionResult DeleteBus([FromBody] int referenceId)
        {
            con.Open();
            //delete bus from db where referenceId matches
            SqlCommand cmd = new SqlCommand("delete * from Bus where ReferenceId=@ReferenceId ");
            cmd.Parameters.AddWithValue("@ReferenceId", referenceId);
            cmd.ExecuteNonQuery();
            con.Close();
            return Ok("Bus deleted successfully");

        }
        [HttpPost]
        [Route("getbus")]
        public IActionResult GetBus([FromBody] GetBusSchema Details)
        {
            //get all buses from db
            con.Open();
            SqlCommand cmd = new SqlCommand("select * from Bus where Source=@Source and Destination=@Destination", con);
            cmd.Parameters.AddWithValue("@Source", Details.Source);
            cmd.Parameters.AddWithValue("@Destination", Details.Destination);
            SqlDataReader dr = cmd.ExecuteReader();
            List<BusEditSchema> busList = new List<BusEditSchema>();
            while (dr.Read())
            {
                //extract only date from deparature and check with the date given
                string date = dr["Departure"].ToString().Substring(0, 10);
                if (date == Details.Date)
                {
                    BusEditSchema bus = new BusEditSchema();
                    bus.ReferenceId = dr["ReferenceId"].ToString();
                    bus.BusNumber = dr["BusNumber"].ToString();
                    bus.Name = dr["Name"].ToString();
                    bus.Source = dr["Source"].ToString();
                    bus.Destination = dr["Destination"].ToString();
                    bus.Departure = dr["Departure"].ToString();
                    bus.Arrival = dr["Arrival"].ToString();
                    bus.TotalSeats = Convert.ToInt32(dr["TotalSeats"]);
                    bus.AvailableSeats = Convert.ToInt32(dr["AvailableSeats"]);
                    bus.Price = Convert.ToInt32(dr["Price"]);
                    bus.BusType = dr["BusType"].ToString();
                    busList.Add(bus);
                }
                
            }
            con.Close();
            
            return Ok(busList);
        }

    }
}
