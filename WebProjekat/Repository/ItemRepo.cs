using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.Infrastructure;
using WebProjekat.Models;
using WebProjekat.Repository.Interfaces;

namespace WebProjekat.Repository
{
	public class ItemRepo : IItemRepo
	{
		private readonly WSDBContext _dbContext;

		private readonly object lockObject = new object();


		public ItemRepo(WSDBContext dbContext)
		{
			_dbContext = dbContext;
		}
		public void AddItem(Item item)
		{
			_dbContext.Items.Add(item);
			_dbContext.SaveChanges();
		}

		public void DeleteItem(Item item)
		{
			_dbContext.Items.Remove(item);
			_dbContext.SaveChanges();
		}

		public Item GetItem(int id)
		{
			return _dbContext.Items.Where(x => x.ItemId.Equals(id)).FirstOrDefault();
		}

		public List<Item> GetItems()
		{
			return _dbContext.Items.ToList();
		}

		public List<int> GetItemsBySeller(string sellerID)
		{
			return _dbContext.Items.Where(x => x.SellerId.Equals(sellerID))
									.Select(x => x.ItemId)
									.ToList();
		}

		public void UpdateItem(Item item)
		{
			lock (lockObject)
			{
				_dbContext.Items.Update(item);
				_dbContext.SaveChanges();
			}
		}
	}
}
