using Microsoft.AspNetCore.Mvc;

namespace WebApiClientDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private static List<string> Products = new List<string>
    {
        "Product 1",
        "Product 2",
        "Product 3"
    };

    // GET: api/Products
    [HttpGet]
    public ActionResult<IEnumerable<string>> GetProducts()
    {
        return Products;
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    public ActionResult<string> GetProduct(int id)
    {
        return Products[id];
    }

    // POST: api/Products
    [HttpPost]
    public void CreateProduct([FromBody] string value)
    {
        Products.Add(value);
    }

    // PUT: api/Products/5
    [HttpPut("{id}")]
    public void UpdateProduct(int id, [FromBody] string value)
    {
        Products[id] = value;
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    public void DeleteProduct(int id)
    {
        Products.RemoveAt(id);
    }
}