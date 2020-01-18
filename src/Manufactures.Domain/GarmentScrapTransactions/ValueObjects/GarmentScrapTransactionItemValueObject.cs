﻿using Moonlay.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Domain.GarmentScrapTransactions.ValueObjects
{
	public class GarmentScrapTransactionItemValueObject : ValueObject
	{
		public Guid ScrapTransactionId { get;  set; }
		public Guid ScrapClassificationId { get;  set; }
		public string ScrapClassificationName { get;  set; }
		public double Quantity { get;  set; }
		public int UomId { get;  set; }
		public string UomUnit { get;  set; }
		public string Description { get;  set; }
		
		public GarmentScrapTransactionItemValueObject()
		{
		}

		protected override IEnumerable<object> GetAtomicValues()
		{
			throw new NotImplementedException();
		}


	}
}
