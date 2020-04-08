﻿using Infrastructure.Domain.Queries;
using Infrastructure.External.DanLirisClient.Microservice.HttpClientService;
using Manufactures.Domain.GarmentPreparings.Repositories;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.External.DanLirisClient.Microservice;
using Newtonsoft.Json;
using Infrastructure.External.DanLirisClient.Microservice.MasterResult;
using ExtCore.Data.Abstractions;
using Manufactures.Domain.GarmentPreparings.ReadModels;
using static Infrastructure.External.DanLirisClient.Microservice.MasterResult.ExpenditureROResult;
using Microsoft.Extensions.DependencyInjection;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentAvalProducts.Repositories;
using Manufactures.Domain.GarmentDeliveryReturns.Repositories;
using System.IO;
using System.Data;
using OfficeOpenXml;

namespace Manufactures.Application.GarmentPreparings.Queries.GetMonitoringPrepare
{
	public class GetMonitoringPrepareQueryHandler : IQueryHandler<GetMonitoringPrepareQuery, GarmentMonitoringPrepareListViewModel>
	{
		protected readonly IHttpClientService _http;
		private readonly IStorage _storage;

		private readonly IGarmentPreparingRepository garmentPreparingRepository;
		private readonly IGarmentPreparingItemRepository garmentPreparingItemRepository;
		private readonly IGarmentCuttingInRepository garmentCuttingInRepository;
		private readonly IGarmentCuttingInItemRepository garmentCuttingInItemRepository;
		private readonly IGarmentCuttingInDetailRepository garmentCuttingInDetailRepository;
		private readonly IGarmentAvalProductRepository garmentAvalProductRepository;
		private readonly IGarmentAvalProductItemRepository garmentAvalProductItemRepository;
		private readonly IGarmentDeliveryReturnRepository garmentDeliveryReturnRepository;
		private readonly IGarmentDeliveryReturnItemRepository garmentDeliveryReturnItemRepository;

		public GetMonitoringPrepareQueryHandler(IStorage storage, IServiceProvider serviceProvider)
		{
			_storage = storage;
			garmentPreparingRepository = storage.GetRepository<IGarmentPreparingRepository>();
			garmentPreparingItemRepository = storage.GetRepository<IGarmentPreparingItemRepository>();
			garmentCuttingInRepository = storage.GetRepository<IGarmentCuttingInRepository>();
			garmentCuttingInItemRepository = storage.GetRepository<IGarmentCuttingInItemRepository>();
			garmentCuttingInDetailRepository = storage.GetRepository<IGarmentCuttingInDetailRepository>();
			garmentAvalProductRepository = storage.GetRepository<IGarmentAvalProductRepository>();
			garmentAvalProductItemRepository = storage.GetRepository<IGarmentAvalProductItemRepository>();
			garmentDeliveryReturnRepository = storage.GetRepository<IGarmentDeliveryReturnRepository>();
			garmentDeliveryReturnItemRepository = storage.GetRepository<IGarmentDeliveryReturnItemRepository>();
			_http = serviceProvider.GetService<IHttpClientService>();
		}

		public async Task<ExpenditureROResult> GetExpenditureById(List<int> id, string token)
		{
			List<ExpenditureROViewModel> expenditureRO = new List<ExpenditureROViewModel>();

			ExpenditureROResult expenditureROResult = new ExpenditureROResult();
			foreach (var item in id)
			{
				var garmentUnitExpenditureNoteUri = PurchasingDataSettings.Endpoint + $"garment-unit-expenditure-notes/ro-asal/{item}";
				var httpResponse = await _http.GetAsync(garmentUnitExpenditureNoteUri, token);

				if (httpResponse.IsSuccessStatusCode)
				{
					var a = await httpResponse.Content.ReadAsStringAsync();
					Dictionary<string, object> keyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(a);
					var b = keyValues.GetValueOrDefault("data");

					var expenditure = JsonConvert.DeserializeObject<ExpenditureROViewModel>(keyValues.GetValueOrDefault("data").ToString());
					ExpenditureROViewModel expenditureROViewModel = new ExpenditureROViewModel
					{
						ROAsal = expenditure.ROAsal,
						DetailExpenditureId = expenditure.DetailExpenditureId,
						BuyerCode=expenditure.BuyerCode
					};
					expenditureRO.Add(expenditureROViewModel);
				}
				else
				{
					//await GetExpenditureById(id, token);
				}
			}
			expenditureROResult.data = expenditureRO;
			return expenditureROResult;
		}

		class monitoringView
		{
			public string roJob { get; set; }
			public string article { get; set; }
			public string buyerCode { get; set; }
			public string productCode { get; set; }
			public string uomUnit { get; set; }
			public string roAsal { get; set; }
			public string remark { get; set; }
			public double stock { get; set; }
			public double receipt { get; set; }
			public double mainFabricExpenditure { get; set; }
			public double nonMainFabricExpenditure { get; set; }
			public double expenditure { get; set; }
			public double aval { get; set; }
			public double remainQty { get; set; }
			public decimal price { get; set; }
		}

		public async Task<GarmentMonitoringPrepareListViewModel> Handle(GetMonitoringPrepareQuery request, CancellationToken cancellationToken)
		{
			DateTimeOffset dateFrom = new DateTimeOffset(request.dateFrom, new TimeSpan(7,0,0));
			DateTimeOffset dateTo = new DateTimeOffset(request.dateTo, new TimeSpan(7, 0, 0));
			var QueryMutationPrepareNow = from a in garmentPreparingRepository.Query
										  join b in garmentPreparingItemRepository.Query on a.Identity equals b.GarmentPreparingId
										  where  a.UnitId == request.unit && a.ProcessDate <= dateTo 
										  select new { RO = a.RONo, Articles = a.Article, Id = a.Identity, DetailExpend = b.UENItemId, Processdate = a.ProcessDate };
			List<int> detailExpendId = new List<int>();
			foreach (var item in QueryMutationPrepareNow.Distinct())
			{
				detailExpendId.Add(item.DetailExpend);
			}
			ExpenditureROResult dataExpenditure = await GetExpenditureById(detailExpendId, request.token);
			
			var QueryMutationPrepareItemsROASAL = (from a in QueryMutationPrepareNow
												   join b in garmentPreparingItemRepository.Query on a.Id equals b.GarmentPreparingId
												   join c in dataExpenditure.data on b.UENItemId equals c.DetailExpenditureId
												   where b.UENItemId == a.DetailExpend
												   select new {buyerCode= c.BuyerCode,price=b.BasicPrice ,prepareitemid = b.Identity, prepareId = b.GarmentPreparingId, comodityDesc = b.DesignColor, ROs = a.RO, QtyPrepare = b.Quantity, ProductCodes = b.ProductCode, article = a.Articles, roasal = c.ROAsal, remaingningQty = b.RemainingQuantity });
			var QueryCuttingDONow = from a in garmentCuttingInRepository.Query
									join b in garmentCuttingInItemRepository.Query on a.Identity equals b.CutInId
									join c in garmentCuttingInDetailRepository.Query on b.Identity equals c.CutInItemId
									where  a.UnitId == request.unit && a.CuttingInDate <= dateTo
									select new monitoringView {price=0 ,expenditure = 0, aval = 0, buyerCode = (from g in QueryMutationPrepareItemsROASAL where g.ROs==a.RONo select g.buyerCode).FirstOrDefault(), uomUnit = "", stock = a.CuttingInDate < dateFrom ? -c.PreparingQuantity : 0, nonMainFabricExpenditure = a.CuttingType == "Non Main Fabric" && (a.CuttingInDate >= dateFrom )? c.PreparingQuantity : 0, mainFabricExpenditure = a.CuttingType == "Main Fabric" && (a.CuttingInDate >= dateFrom) ? c.PreparingQuantity :0 , remark = c.DesignColor, roJob = a.RONo, receipt = 0, productCode = c.ProductCode, article = a.Article, roAsal = (from a in QueryMutationPrepareItemsROASAL where a.prepareitemid == c.PreparingItemId select a.roasal).FirstOrDefault(), remainQty = 0 };

			var QueryMutationPrepareItemNow = (from d in QueryMutationPrepareNow
											   join e in garmentPreparingItemRepository.Query on d.Id equals e.GarmentPreparingId
											   join c in dataExpenditure.data on e.UENItemId equals c.DetailExpenditureId
											   where e.UENItemId == d.DetailExpend 
											   select new monitoringView { price = Convert.ToDecimal(e.BasicPrice), buyerCode = (from g in QueryMutationPrepareItemsROASAL where g.ROs == d.RO select g.buyerCode).FirstOrDefault(), uomUnit = "", stock = d.Processdate < dateFrom ? e.Quantity : 0, mainFabricExpenditure = 0, nonMainFabricExpenditure = 0, remark = e.DesignColor, roJob = d.RO, receipt = (d.Processdate >= dateFrom ? e.Quantity : 0), productCode = e.ProductCode, article = d.Articles, roAsal = c.ROAsal, remainQty = e.RemainingQuantity }).Distinct();
			
			var QueryAval = from a in garmentAvalProductRepository.Query
							join b in garmentAvalProductItemRepository.Query on a.Identity equals b.APId
							join c in garmentPreparingItemRepository.Query on Guid.Parse(b.PreparingItemId) equals c.Identity
							join d in garmentPreparingRepository.Query on c.GarmentPreparingId equals d.Identity
							where  a.AvalDate <= dateTo && d.UnitId == request.unit
							select new monitoringView { price = 0, expenditure = 0, aval = a.AvalDate >= dateFrom? b.Quantity :0, buyerCode = (from g in QueryMutationPrepareItemsROASAL where g.ROs == a.RONo select g.buyerCode).FirstOrDefault(), uomUnit = "", stock = a.AvalDate < dateFrom ? -b.Quantity : 0, mainFabricExpenditure = 0, nonMainFabricExpenditure = 0, remark = b.DesignColor, roJob = a.RONo, receipt = 0, productCode = b.ProductCode, article = a.Article, roAsal = (from aa in QueryMutationPrepareItemsROASAL where aa.prepareitemid == Guid.Parse(b.PreparingItemId) select aa.roasal).FirstOrDefault(), remainQty = 0 };
			var asss = dateTo;
			var QueryDeliveryReturn = from a in garmentDeliveryReturnRepository.Query
									  join b in garmentDeliveryReturnItemRepository.Query on a.Identity equals b.DRId
									  join c in garmentPreparingItemRepository.Query on b.PreparingItemId equals Convert.ToString(c.Identity)
									  where   a.ReturnDate <= dateTo && a.UnitId == request.unit
									  select new monitoringView { price = 0, expenditure = a.ReturnDate >= dateFrom ? b.Quantity :0, aval = 0, buyerCode = (from g in QueryMutationPrepareItemsROASAL where g.ROs == a.RONo select g.buyerCode).FirstOrDefault(), uomUnit = "", stock = a.ReturnDate < dateFrom ? -b.Quantity : 0, mainFabricExpenditure = 0, nonMainFabricExpenditure = 0, remark = b.DesignColor, roJob = a.RONo, receipt = 0, productCode = b.ProductCode, article = a.Article, roAsal = (from aa in QueryMutationPrepareItemsROASAL where aa.prepareitemid == Guid.Parse(b.PreparingItemId) select aa.roasal).FirstOrDefault(), remainQty = 0 };

			var queryNow = QueryMutationPrepareItemNow.Union(QueryCuttingDONow).Union(QueryAval).Union(QueryDeliveryReturn).AsEnumerable();

			var querySum = queryNow.GroupBy(x => new { x.roAsal, x.roJob, x.article, x.buyerCode, x.productCode, x.remark }, (key, group) => new
			{
				ROAsal = key.roAsal,
				ROJob = key.roJob,
				stock = group.Sum(s => s.stock),
				ProductCode = key.productCode,
				Article = key.article,
				buyer= key.buyerCode,
				Remark = key.remark,
				Price= group.Sum(s=>s.price),
				mainFabricExpenditure = group.Sum(s => s.mainFabricExpenditure),
				nonmainFabricExpenditure = group.Sum(s => s.nonMainFabricExpenditure),
				receipt = group.Sum(s => s.receipt),
				Aval = group.Sum(s => s.aval),
				drQty = group.Sum(s => s.expenditure)
			}).Where(s => s.Price > 0).OrderBy(s => s.ROJob);


			GarmentMonitoringPrepareListViewModel garmentMonitoringPrepareListViewModel = new GarmentMonitoringPrepareListViewModel();
			List<GarmentMonitoringPrepareDto> monitoringPrepareDtos = new List<GarmentMonitoringPrepareDto>();
			foreach (var item in querySum)
			{
				GarmentMonitoringPrepareDto garmentMonitoringPrepareDto = new GarmentMonitoringPrepareDto()
				{
					article = item.Article,
					roJob = item.ROJob,
					productCode = item.ProductCode,
					roAsal = item.ROAsal,
					uomUnit = "MT",
					remainQty = item.stock + item.receipt - item.nonmainFabricExpenditure - item.mainFabricExpenditure - item.Aval - item.drQty,
					stock = item.stock,
					remark = item.Remark,
					receipt = item.receipt,
					aval = item.Aval,
					nonMainFabricExpenditure = item.nonmainFabricExpenditure,
					mainFabricExpenditure = item.mainFabricExpenditure,
					expenditure = item.drQty,
					price=item.Price,
					buyerCode=item.buyer

				};
				monitoringPrepareDtos.Add(garmentMonitoringPrepareDto);
			}
			garmentMonitoringPrepareListViewModel.garmentMonitorings = monitoringPrepareDtos;

			return garmentMonitoringPrepareListViewModel;
		}
	}
}