﻿using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ASP_201MVC.Controllers.RatesController;

namespace ASP_201MVC.Controllers
{
    [Route("api/rates")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public RatesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public object Get([FromQuery] String data)
        {
            return new { result = $"Запит оброблено методом GET + {data}" };
        }

        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {

            int statusCode;
            String result;

            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);

                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        if(rate.Rating == rating)
                        {
                            statusCode = StatusCodes.Status406NotAcceptable;
                            result = $"Дані вже наявні: ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }
                        else
                        {
                            rate.Rating= rating;
                            _dataContext.SaveChanges();
                            statusCode = StatusCodes.Status202Accepted;
                            result = $"Дані оновлено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }

                    }
                    else
                    {
                        _dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = rating
                        });
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status201Created;
                        result = $"Дані внесено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Дані не опрацьовані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }

            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }

        [HttpDelete]
        public object Delete([FromBody] BodyData bodyData)
        {
            int statusCode;
            String result;

            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);

                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        _dataContext.Rates.Remove(rate);
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status202Accepted;
                        result = $"Дані видалено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                    else
                    {statusCode = StatusCodes.Status406NotAcceptable;
                        result = $"Дані відсутні (не можуть бути видалені): ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        _dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = rating
                        });
                        _dataContext.SaveChanges();
                        statusCode = StatusCodes.Status201Created;
                        result = $"Дані внесено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Дані не опрацьовані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }

            HttpContext.Response.StatusCode = statusCode;
            return new { result };
        }

        [HttpPatch]
        public object Patch([FromBody] BodyData bodyData)
        {
            return new { result = $"Запит оброблено методом {HttpContext.Request.Method} {bodyData.Data}" };
        }

        public object Default([FromBody] BodyData bodyData)
        {
            switch (HttpContext.Request.Method)
            {
                case "LINK":
                    {
                        return Link(HttpContext.Request.Method, bodyData);
                    }
                case "UNLINK":
                    {
                        return Unlink(HttpContext.Request.Method, bodyData);
                    }
                default: throw new NotImplementedException();
            }

        }
        private object Link(String Method, BodyData data)
        {
            return new
            {
                result = $"Запит оброблено методом {Method} і прийнято дані - {data.Data}"
            };
        }

        private object Unlink(String Method, BodyData data)
        {
            return new
            {
                result = $"Запит оброблено методом {Method} і прийнято дані - {data.Data}"
            };
        }

        public class BodyData
        {
            public String? Data { get; set; }
            public String? ItemId { get; set; }
            public String? UserId { get; set; }
        }
    }
}
