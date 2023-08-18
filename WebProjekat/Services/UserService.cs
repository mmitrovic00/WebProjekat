using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
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

		public bool ChangePassword(PasswordDto userPass, out string mess)
		{
			var user = _userRepository.GetUser(userPass.Email);
			if (user == null) {
				mess = "korisnik ne postoji.";
				return false;
			}

			if (user.Password != Hash(userPass.OldPassword))
			{
				mess = "Neispravna stara lozinka.";
				return false;
			}
			user.Password = Hash(userPass.NewPassword);
			_userRepository.UpdateUser(user);
			mess = "";
			return true;

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
			if (user.Password == Hash(dto.Password))
			{
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
				expires: DateTime.Now.AddMinutes(20), //vazenje tokena u minutama
				signingCredentials: signinCredentials //kredencijali za potpis
			);
			string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
			return tokenString;
		}

		public TokenDTO Registration(RegistrationUserDto userReg, out string mess)
		{
			TokenDTO token = null;
			string userType = "";
			if (userReg.Email == "" || userReg.FirstName == "" || userReg.LastName == "" || userReg.UserName == "" || userReg.Address == "" || userReg.Birthday == "")
			{
				mess = "Morate popuniti sva polja.";
				return token;
			}
			if (userReg.UserType == EUserType.ADMIN)
			{
				mess = "Ne mozete se registrovati kao admin";
				return token;
			}
			if(_userRepository.GetUser(userReg.Email) != null)
			{
				mess = "korisnik sa ovim email-om vec postoji";
				return token;
			}
			userReg.Password = Hash(userReg.Password);
			userType = userReg.UserType.ToString();

			switch (userReg.UserType)
			{
				case EUserType.CUSTOMER:
					_userRepository.AddUser(_mapper.Map<Customer>(userReg));

					break;
				case EUserType.SELLER:
					
					var seller = _mapper.Map<Seller>(userReg);
					seller.Approved = ESeller.IN_PROCESS;
					_userRepository.AddUser(seller);

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

			
			SendEmail(email);

			return true;
		}

		private void SendEmail(string recipientEmail)
		{
			try
			{
				using (var mail = new MailMessage())
				{
					mail.From = new MailAddress("mica.mitrovic10@gmail.com", "Web Projekat");
					mail.To.Add(recipientEmail);
					mail.Subject = "Welcome to Your App!";
					mail.Body = "Welcome to Your App! We're excited to have you on board.";
					mail.IsBodyHtml = true;

					using (var smtp = new SmtpClient("smtp.yourprovider.com"))
					{
						smtp.Port = 587;
						smtp.Credentials = new System.Net.NetworkCredential("mica.mitrovic10@gmail.com", "slavicamaca");
						smtp.EnableSsl = true;

						smtp.Send(mail);
					}
				}
			}
			catch (Exception ex)
			{
				// Handle exceptions
			}
		}
	}
}
