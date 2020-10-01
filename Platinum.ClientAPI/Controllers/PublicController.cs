﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Platinum.ClientAPI.Auth;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;

namespace Platinum.ClientAPI.Controllers
{

    [Route("[controller]")]
    [BasicAuth("public")]
    [ApiController]
    public class PublicController : ControllerBase
    {

        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            try
            {
                List<PublicControllerCategory> categories = new List<PublicControllerCategory>();
                using (Dal db = new Dal())
                {
                    using (DbDataReader reader = db.ExecuteReader("select Id,name,'https://allegro.pl/'+routeUrl as Url from websiteCategories with (nolock)"))
                    {
                        while (reader.Read())
                        {
                            PublicControllerCategory tempCat = new PublicControllerCategory()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Url = reader.GetString(2)
                            };
                            categories.Add(tempCat);
                        }
                    }
                }

                return new JsonResult(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetCategory/{categoryId}")]
        public IActionResult GetCategory(int categoryId)
        {
            try
            {
                bool parse = int.TryParse(categoryId.ToString(), out _);
                if (!parse)
                {
                    return BadRequest("Category Id is invalid.");
                }
                List<PublicControllerCategory> categories = new List<PublicControllerCategory>();
                using (Dal db = new Dal())
                {
                    using (DbDataReader reader = db.ExecuteReader($"select Id,name,'https://allegro.pl/'+routeUrl as Url from websiteCategories with (nolock) where Id = " + categoryId + ";"))
                    {
                        while (reader.Read())
                        {
                            PublicControllerCategory tempCat = new PublicControllerCategory()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Url = reader.GetString(2)
                            };
                            categories.Add(tempCat);
                        }
                    }
                }
                if (categories.Count == 0)
                {
                    return NotFound("Not found category with id " + categoryId);
                }
                return new JsonResult(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("BeginScroll/{userId}/{categoryId}/{pageSize}")]
        public IActionResult BeginScroll(int userId, int categoryId,int pageSize)
        {
            if(IsUserSame(Request.Headers, userId))
            {
                try
                {
                    return new JsonResult(ElasticController.Instance.BeginScroll(categoryId, userId, pageSize));
                }catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Forbid("Your account is not set to passed user id");
            }
            return Ok();
        }

        [HttpGet("ContinueScroll/{userId}/{scrollId}")]
        public IActionResult ContinueScroll(int userId,string scrollId)
        {
            if (IsUserSame(Request.Headers,userId))
            {
                try
                {
                    return new JsonResult(ElasticController.Instance.ContinueScroll(scrollId));
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                Forbid("Your account is not set to passed user id");
            }
            return Ok();
        }
        
        [HttpGet("GetAttributes/{userId}/{categoryId}")]
        public IActionResult GetAttributes(int userId,int categoryId)
        {
            if (IsUserSame(Request.Headers,userId))
            {
                try
                {
                    var mappings = ElasticController.Instance.GetIndexMappings(categoryId, userId);
                    List<MappingApiElement> mappingApiElements = new List<MappingApiElement>();
                    foreach (var map in mappings)
                    {
                        mappingApiElements.Add(new MappingApiElement(map));
                    }
                    return new JsonResult(mappingApiElements);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Forbid("Your account is not set to passed user id");
            }
        }

        [HttpGet("BeginFilteredScroll/{attributes}")]
        public IActionResult BeginFilteredScroll(string attrbiutes)
        {
            Dictionary<string,string> dict = new Dictionary<string, string>();
           // dict.Add("price",20);
            int b = 0x0;
            return Ok();
        }

        private bool IsUserSame(IHeaderDictionary headers, int userId)
        {
            try
            {
                string authHeader = headers["Authorization"];
                if (authHeader != null)
                {
                    var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                    if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (headers.ContainsKey("Authorization"))
                        {
                            var credentials = Encoding.UTF8
                                                .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                                .Split(':', 2);
                            string login = credentials[0];

                            using (Dal db = new Dal())
                            {
                                int userCount = (int)db.ExecuteScalar("SELECT COUNT(*) FROM WebApiUsers where login = '" + login + "' and Id = " + userId);
                                return userCount > 0;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}