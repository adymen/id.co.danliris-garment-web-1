﻿using Infrastructure.Domain.Repositories;
using Manufactures.Domain.GarmentFinishingOuts.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manufactures.Domain.GarmentFinishingOuts.Repositories
{
    public interface IGarmentFinishingOutRepository : IAggregateRepository<GarmentFinishingOut, GarmentFinishingOutReadModel>
    {
        IQueryable<GarmentFinishingOutReadModel> Read(int page, int size, string order, string keyword, string filter);
    }
}