using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.Infrastructure;
using WebProjekat.Models;
using WebProjekat.Models.Enum;
using WebProjekat.Repository.Interfaces;

namespace WebProjekat.Repository
{
	public class UserRepo : IUserRepo
	{
		private readonly WSDBContext _dbContext;
		private readonly object lockObject = new object();

		public UserRepo(WSDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void AddUser(User user)
		{
			_dbContext.Users.Add(user);
			_dbContext.SaveChanges();
		}

		public List<User> GetCustomers()
		{
			return _dbContext.Users.Where(x => x.UserType == EUserType.CUSTOMER).ToList();
		}

		

		public List<User> GetSellers()
		{
			return _dbContext.Users.Where(x => x.UserType == EUserType.SELLER).ToList();
		}

		public User GetUser(string email)
		{
			return _dbContext.Users.Where(x => x.Email.Equals(email)).FirstOrDefault();
		}

		public List<User> GetUsers()
		{
			return _dbContext.Users.ToList();
		}

		public void SetSellerStatus(Seller seller)
		{
			lock (lockObject)
			{
				_dbContext.Users.Update(seller);
				_dbContext.SaveChanges();
			}
		}

		public void UpdateUser(User user)
		{
			lock (lockObject)
			{
				_dbContext.Users.Update(user);
				_dbContext.SaveChanges();
			}
		}
	}
}
