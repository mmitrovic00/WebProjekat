using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.Models;

namespace WebProjekat.Repository.Interfaces
{
	public interface IItemRepo
	{
        void AddItem(Item item);
        List<Item> GetItems();
        Item GetItem(int id);
        void UpdateItem(Item item);
        void DeleteItem(Item item);
        List<int> GetProductsBySeller(string sellerID);
    }
}
