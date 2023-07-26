using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebProjekat.DTO;
using WebProjekat.Interfaces;
using WebProjekat.Models;
using WebProjekat.Repository.Interfaces;

namespace WebProjekat.Services
{
	public class ItemService : IItemService
	{
		private readonly IMapper _mapper;
		private readonly IConfigurationSection _secretKey;
		private readonly IItemRepo _itemRepository;

		public ItemService(IMapper mapper, IItemRepo itemRepository, IConfiguration config)
		{
			_mapper = mapper;
			_secretKey = config.GetSection("SecretKey");
			_itemRepository = itemRepository;
		}
		public bool DeleteItem(int itemId, string sellerId, out string message)
		{
			Item item = _itemRepository.GetItem(itemId);
			if (item == null)
			{
				message = "";
				return false;
			}

			if (!item.SellerId.Equals(sellerId))
			{
				message = "";
				return false;
			}

			_itemRepository.DeleteItem(item);
			message = "Uspesno obrisan proizvod";

			return true;
		}

		public List<ItemDto> GetItems()
		{
			var items = _mapper.Map<List<Item>>(_itemRepository.GetItems());
			return _mapper.Map<List<ItemDto>>(items);
		}

		public List<ItemDto> ItemsBySeller(string sellerId)
		{
			var items = _itemRepository.GetItems();
			return _mapper.Map<List<ItemDto>>(items.Where(x => x.SellerId.Equals(sellerId)));
		}

		public void NewItem(ItemDto item, string sellerId)
		{
			Item newItem = _mapper.Map<Item>(item);
			newItem.SellerId = sellerId;
			_itemRepository.AddItem(newItem);
		}

		public bool UpdateItem(ItemDto newItem, string sellerId, out string message)
		{
			Item item = _itemRepository.GetItem(newItem.ItemId);
			if(item == null)
			{
				message = "";
				return false;
			}

			if (!item.SellerId.Equals(sellerId))
			{
				message = "";
				return false;
			}

			item.ItemName = newItem.ItemName;
			item.Amount = newItem.Amount;
			item.Price = newItem.Price;
			item.Description = newItem.Description;

			_itemRepository.UpdateItem(item);

			message = "Uspesno ste azurirali proizvod.";
			return true;
		}
	}
}
