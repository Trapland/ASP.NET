using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ASP_201MVC.Controllers.RatesController;

namespace ASP_201MVC.Controllers
{
    [Route("api/rates")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        [HttpGet]
        public object Get([FromQuery] String data)
        {
            return new { result = $"Запит оброблено методом GET + {data}" };
        }

        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {
            return new { result = $"Запит оброблено методом POST {bodyData.Data}" };
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
            public String Data { get; set; } = null!;
        }
    }
}
