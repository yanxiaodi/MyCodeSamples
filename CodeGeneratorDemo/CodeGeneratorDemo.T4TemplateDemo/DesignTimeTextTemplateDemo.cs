
using System;
using System.Threading.Tasks;

namespace CodeGeneratorDemo.T4TemplateDemo.DesignTimeTextTemplateDemo
{
    public partial class Product
    {
        public Guid Id { get; set; }
        public Product(Guid id)
        {
            Id = id;
        }
    }

    public partial class ProductService
    {
        public Task<Product> GetProduct(Guid id)
        {
            return Task.FromResult(new Product(id));
        }

    }
    public partial class Category
    {
        public Guid Id { get; set; }
        public Category(Guid id)
        {
            Id = id;
        }
    }

    public partial class CategoryService
    {
        public Task<Category> GetCategory(Guid id)
        {
            return Task.FromResult(new Category(id));
        }

    }
    public partial class Comment
    {
        public Guid Id { get; set; }
        public Comment(Guid id)
        {
            Id = id;
        }
    }

    public partial class CommentService
    {
        public Task<Comment> GetComment(Guid id)
        {
            return Task.FromResult(new Comment(id));
        }

    }
    public partial class Order
    {
        public Guid Id { get; set; }
        public Order(Guid id)
        {
            Id = id;
        }
    }

    public partial class OrderService
    {
        public Task<Order> GetOrder(Guid id)
        {
            return Task.FromResult(new Order(id));
        }

    }
}