using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Portal.Apis.Core.Controllers;
using Portal.Apis.Core.DAL.Entities;
using Portal.Apis.Models;
using Portal.Apis.Core.BLL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Portal.Apis.Core.Extensions;

namespace Portal.Apis.Controllers
{    
    // [Route("api/{lang?}/dashboard/[controller]")]
    [Route("api/{lang?}/[controller]")]
    public class NewsCategoryController : BaseController
    {
        private readonly NewsCategoryService Service;
        private readonly IMapper Mapper;
        private readonly TranslateService TranslateService;
        public NewsCategoryController(NewsCategoryService service, IMapper mapper, TranslateService translateService) 
        {
            this.Service = service;
            this.Mapper = mapper;
            this.TranslateService = translateService;
        }       

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetById(int id)
        {
            NewsCategory ent = Service.ByID(id);

            if (ent == null)
                return NotFound();
            try
            {
                NewsCategoryViewModel model = Activator.CreateInstance<NewsCategoryViewModel>();
                model = Mapper.Map<NewsCategoryViewModel>(ent);
                model.LoadEntity(ent);

                return Ok(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("exception", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var request = ParseToDataSourceRequest(Request);
                var query = Service.AllAsQueryable
                    .OrderBy(e => e.Id)
                    .AsQueryable();
                var result = Mapper.Map<IEnumerable<NewsCategoryViewModel>>(query).AsQueryable();                

                return Ok(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("exception", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody]NewsCategoryViewModel model)
        {
            if (ModelState.IsValid && Validate(model))
            {
                NewsCategory entity = Activator.CreateInstance<NewsCategory>();
                entity = Service.AsObjectQuery().AsNoTracking().FirstOrDefault(f => EqualityComparer<int>.Default.Equals(f.Id, model.Id));

                try
                {
                    model.GetKeys(entity);
                    Mapper.Map<NewsCategoryViewModel, NewsCategory>(model, entity);

                    if (Service.TryUpdate(ref entity))
                        model.AfterUpdateEntity(entity);

                    return Ok(entity);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("exception", ex.Message);
                    return BadRequest(ModelState);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("GetAsSelect")]
        public IActionResult GetAsSelect()
        {
            Type type = typeof(NewsCategory);

            if (type.GetProperties().Any(s => s.Name == "Name") && type.GetProperties().Any(s => s.Name == "Id"))
            {
                var result = Service.AllAsQueryable.Select(s => new
                {
                    Value = type.GetProperty("Id").GetValue(s),
                    Text = type.GetProperty("Name").GetValue(s).ToString().Translate(),
                    Selected = false
                });
                return Ok(result);
            }

            return BadRequest(ModelState);
        }

        private bool Validate(NewsCategoryViewModel model)
        {
            if (ModelState.IsValid)
                if (model == null)
                    ModelState.AddModelError(string.Empty, "Model is null");

            return ModelState.IsValid;
        } 
    }
}
