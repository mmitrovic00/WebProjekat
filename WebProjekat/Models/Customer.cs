﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebProjekat.Models
{
	public class Customer : User
	{
		public List<Order> Orders { get; set; }
	}
}
