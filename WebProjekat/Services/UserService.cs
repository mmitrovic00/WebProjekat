using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebProjekat.DTO.User;
using WebProjekat.Interfaces;
using WebProjekat.Models;
using WebProjekat.Models.Enum;
using WebProjekat.Repository.Interfaces;

namespace WebProjekat.Services
{
	public class UserService : IUserService
	{
		private readonly IMapper _mapper;
		private readonly IConfigurationSection _secretKey;
		private readonly IUserRepo _userRepository;

		public UserService(IMapper mapper, IUserRepo userRepository, IConfiguration config)
		{
			_mapper = mapper;
			_secretKey = config.GetSection("SecretKey");
			_userRepository = userRepository;
		}

		public bool ChangePassword(PasswordDto userPass)
		{
			var user = _userRepository.GetUser(userPass.Email);
			if (user == null)
				return false;

			if(user.Password == Hash(userPass.OldPassword))
			{
				user.Password = Hash(userPass.NewPassword);
				_userRepository.UpdateUser(user);
				return true;
			}

			return false;

		}
		public string Hash(string password)
		{
			// Create a new instance of the SHA256 hashing algorithm
			SHA256 sha256 = SHA256.Create();

			// Convert the password string to a byte array
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

			// Compute the hash value of the password using the SHA256 algorithm
			byte[] hashBytes = sha256.ComputeHash(passwordBytes);

			// Convert the hash value to a string representation
			string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

			// The resulting hash string can be stored in the database for later verification
			return hash;
		}
		public UserDto GetByEmail(string email)
		{
			var user = _userRepository.GetUser(email);
			if (user == null)
				return null;

			return _mapper.Map<UserDto>(user);
		}

		public TokenDTO LogIn(LogInDto dto)
		{
			TokenDTO token = new TokenDTO();

			var user = _userRepository.GetUser(dto.Email);
			if (user == null)
				return null;

			string role = user.UserType.ToString();
			if(user.Password == Hash(dto.Password))
			{
				if (user.UserType == EUserType.SELLER)
				{
					var seller = (Seller)user;
					if (seller.Approved != ESeller.VERIFIED)
						role = "UNVERIFIED";
				}
				token.Token = CreateToken(role, user.Email);
				token.UserType = user.UserType;
			}
			return token;
		}

		private string CreateToken(string userType, string email)
		{
			List<Claim> claims = new List<Claim>();
			claims.Add(new Claim(ClaimTypes.Role, userType));
			claims.Add(new Claim(ClaimTypes.Name, email));
			SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey.Value));
			var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
			var tokeOptions = new JwtSecurityToken(
				issuer: "https://localhost:44321", //url servera koji je izdao token
				claims: claims, //claimovi
				expires: DateTime.Now.AddMinutes(25), //vazenje tokena u minutama
				signingCredentials: signinCredentials //kredencijali za potpis
			);
			string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
			return tokenString;
		}

		public TokenDTO Registration(RegistrationUserDto userReg, out string mess)
		{
			TokenDTO token = null;
			string userType = "";

			/*if (userReg.UserType == EUserType.ADMIN)
			{
				mess = "Ne mozete se registrovati kao admin";
				return token;
			}*/

			userReg.Password = Hash(userReg.Password);
			userType = userReg.UserType.ToString();

			switch (userReg.UserType)
			{
				case EUserType.CUSTOMER:
					_userRepository.AddUser(_mapper.Map<Customer>(userReg));

					break;
				case EUserType.SELLER:
					userType = "UNVERIFIED";
					var seller = _mapper.Map<Seller>(userReg);
					seller.Approved = ESeller.IN_PROCESS;
					_userRepository.AddUser(seller);

					break;
				case EUserType.ADMIN:
					_userRepository.AddUser(_mapper.Map<Admin>(userReg));
					break;
			}

			token = new TokenDTO()
			{
				Token = CreateToken(userType, userReg.Email),
				UserType = userReg.UserType
			};
			mess = "Uspesna registracija";
			return token;
		}

		public bool UpdateUser(UpdateUserDto newUser)
		{
			var user = _userRepository.GetUser(newUser.Email);

			if (user != null)
			{
				user.FirstName = newUser.FirstName;
				user.LastName = newUser.LastName;
				user.UserName = newUser.UserName;
				user.Address = newUser.Address;
				user.Birthday = newUser.Birthday;
				_userRepository.UpdateUser(user);
				return true;
			}
			return false;
		}

		public List<UserDto> GetCustomers()
		{
			var customers = _mapper.Map<List<Customer>>(_userRepository.GetCustomers());
			return _mapper.Map<List<UserDto>>(customers);
		}

		public List<UserDto> GetSellers()
		{
			var sellers = _mapper.Map<List<Seller>>(_userRepository.GetSellers());
			return _mapper.Map<List<UserDto>>(sellers);
		}

		public bool SetSellerStatus(string email, ESeller status)
		{
			var seller = (Seller)_userRepository.GetUser(email);
			if (seller == null || seller.Approved != ESeller.IN_PROCESS)
				return false;
			seller.Approved = status;
			_userRepository.SetSellerStatus(seller);

			var vr = status.ToString();
			var apiKey = "E768E081003B29BF53422EC3597F30618F3E05C3A893FEFE1118B10338F1D41724E4A0488A9C06372E3A6819FCA263C0";
			var recipientEmail = email;
			var subject = "Status of verification";
			var bodyHtml = $"<h1>Hello, your status of verification is</h1><p>{vr}</p>";
			SendEmail(apiKey, recipientEmail, subject, bodyHtml);

			return true;
		}

		private static void SendEmail(string apiKey, string recipientEmail, string subject, string body)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://api.elasticemail.com/v2/");
				client.DefaultRequestHeaders.Accept.Clear();

				var requestContent = new StringContent(
					$"apikey={Uri.EscapeDataString(apiKey)}&from=mica.mitrovic10@gmail.com&to=" +
					$"{Uri.EscapeDataString(recipientEmail)}&subject={Uri.EscapeDataString(subject)}&bodyHtml=" +
					$"{Uri.EscapeDataString(body)}",
					Encoding.UTF8,
					"application/x-www-form-urlencoded"
				);

				var response = client.PostAsync("email/send", requestContent).Result;

				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Email sent successfully!");
				}
				else
				{
					var errorResponse = response.Content.ReadAsStringAsync().Result;
					Console.WriteLine("Failed to send email. Error message: " + errorResponse);
				}
			}
		}
	}
}
