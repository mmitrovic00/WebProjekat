using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.DTO.User;
using WebProjekat.Models.Enum;

namespace WebProjekat.Interfaces
{
	public interface IUserService
	{
		UserDto GetByEmail(string email);
		bool UpdateUser(UpdateUserDto newUser);
		bool ChangePassword(PasswordDto data, out string mess);
		TokenDTO Registration(RegistrationUserDto dto, out string mess);
		TokenDTO LogIn(LogInDto dto);

		List<UserDto> GetCustomers();
		List<UserDto> GetSellers();
		bool SetSellerStatus(string email, ESeller status);
	}
}
