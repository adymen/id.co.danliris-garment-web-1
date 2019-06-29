﻿using Barebone.Controllers;
using Manufactures.Domain.GarmentCuttingIns.Commands;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Domain.GarmentPreparings.Repositories;
using Manufactures.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Manufactures.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("cutting-ins")]
    public class GarmentCuttingInController : ControllerApiBase
    {
        private readonly IGarmentCuttingInRepository _garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository _garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository _garmentCuttingInDetailRepository;
        private readonly IGarmentPreparingItemRepository _garmentPreparingItemRepository;

        public GarmentCuttingInController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _garmentCuttingInRepository = Storage.GetRepository<IGarmentCuttingInRepository>();
            _garmentCuttingInItemRepository = Storage.GetRepository<IGarmentCuttingInItemRepository>();
            _garmentCuttingInDetailRepository = Storage.GetRepository<IGarmentCuttingInDetailRepository>();
            _garmentPreparingItemRepository = Storage.GetRepository<IGarmentPreparingItemRepository>();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int size = 25, string order = "{}", [Bind(Prefix = "Select[]")]List<string> select = null, string keyword = null, string filter = "{}")
        {
            VerifyUser();

            var query = _garmentCuttingInRepository.Read(page, size, order, keyword, filter);
            var count = query.Count();
            List<GarmentCuttingInListDto> garmentCuttingInListDtos = _garmentCuttingInRepository.Find(query).Select(cutIn =>
            {
                var items = _garmentCuttingInItemRepository.Query.Where(o => o.CutInId == cutIn.Identity).Select(cutInItem => new
                {
                    cutInItem.UENNo,
                    details = _garmentCuttingInDetailRepository.Query.Where(o => o.CutInItemId == cutInItem.Identity).Select(cutInDetail => new
                    {
                        cutInDetail.CuttingInQuantity,
                        cutInDetail.ProductCode
                    })
                }).ToList();

                return new GarmentCuttingInListDto(cutIn)
                {
                    UENNos = items.Select(i => i.UENNo).ToList(),
                    Products = items.SelectMany(i => i.details.Select(d => d.ProductCode)).ToList(),
                    TotalCuttingInQuantity = items.Sum(i => i.details.Sum(d => d.CuttingInQuantity))
                };
            }).ToList();

            await Task.Yield();
            return Ok(garmentCuttingInListDtos, info: new
            {
                page,
                size,
                count
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Guid guid = Guid.Parse(id);

            VerifyUser();

            GarmentCuttingInDto garmentCuttingInDto = _garmentCuttingInRepository.Find(o => o.Identity == guid).Select(cutIn => new GarmentCuttingInDto(cutIn)
            {
                Items = _garmentCuttingInItemRepository.Find(o => o.CutInId == cutIn.Identity).Select(cutInItem => new GarmentCuttingInItemDto(cutInItem)
                {
                    Details = _garmentCuttingInDetailRepository.Find(o => o.CutInItemId == cutInItem.Identity).Select(cutInDetail => new GarmentCuttingInDetailDto(cutInDetail)
                    {
                        PreparingRemainingQuantity = _garmentPreparingItemRepository.Query.Where(o => o.Identity == cutInDetail.PreparingItemId).Select(o => o.RemainingQuantity).FirstOrDefault() + cutInDetail.PreparingQuantity,
                    }).ToList()
                }).ToList()
            }
            ).FirstOrDefault();

            await Task.Yield();
            return Ok(garmentCuttingInDto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PlaceGarmentCuttingInCommand command)
        {
            try
            {
                VerifyUser();

                var order = await Mediator.Send(command);

                return Ok(order.Identity);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateGarmentCuttingInCommand command)
        {
            Guid guid = Guid.Parse(id);

            command.SetIdentity(guid);

            VerifyUser();

            var order = await Mediator.Send(command);

            return Ok(order.Identity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            Guid guid = Guid.Parse(id);

            VerifyUser();

            RemoveGarmentCuttingInCommand command = new RemoveGarmentCuttingInCommand(guid);
            var order = await Mediator.Send(command);

            return Ok(order.Identity);
        }
    }
}