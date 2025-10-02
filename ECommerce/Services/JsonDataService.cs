using System.Text.Json;
using ECommerce.Models;

namespace ECommerce.Services
{
    public class JsonDataService
    {
        private readonly string _dataPath;
        private readonly string _usersFile;
        private readonly string _productsFile;
        private readonly string _ordersFile;

        public JsonDataService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            _usersFile = Path.Combine(_dataPath, "users.json");
            _productsFile = Path.Combine(_dataPath, "products.json");
            _ordersFile = Path.Combine(_dataPath, "orders.json");

            if (!Directory.Exists(_dataPath))
                Directory.CreateDirectory(_dataPath);

            InitializeData();
        }

        private void InitializeData()
        {
            // Kullanıcılar
            if (!File.Exists(_usersFile))
            {
                var users = new List<User>
                {
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        Password = "admin123",
                        Email = "admin@ecommerce.com",
                        FullName = "Admin User",
                        Role = UserRole.Admin,
                        CreatedAt = DateTime.Now
                    }
                };
                SaveUsers(users);
            }

            // Ürünler
            if (!File.Exists(_productsFile))
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Laptop",
                        Description = "Güçlü ve hızlı laptop",
                        Price = 15000,
                        Stock = 10,
                        ImageUrl = "/uploads/no-image.jpg",
                        Category = "Elektronik",
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Telefon",
                        Description = "Akıllı telefon",
                        Price = 8000,
                        Stock = 20,
                        ImageUrl = "/uploads/no-image.jpg",
                        Category = "Elektronik",
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Id = 3,
                        Name = "Kulaklık",
                        Description = "Kablosuz kulaklık",
                        Price = 500,
                        Stock = 50,
                        ImageUrl = "/uploads/no-image.jpg",
                        Category = "Aksesuar",
                        CreatedAt = DateTime.Now
                    }
                };
                SaveProducts(products);
            }

            // Siparişler
            if (!File.Exists(_ordersFile))
            {
                SaveOrders(new List<Order>());
            }
        }

        // USERS
        public List<User> GetUsers()
        {
            var json = File.ReadAllText(_usersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFile, json);
        }

        public User GetUserById(int id)
        {
            return GetUsers().FirstOrDefault(u => u.Id == id);
        }

        public User GetUserByUsername(string username)
        {
            return GetUsers().FirstOrDefault(u => u.Username == username);
        }

        public void AddUser(User user)
        {
            var users = GetUsers();
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            user.CreatedAt = DateTime.Now;
            users.Add(user);
            SaveUsers(users);
        }

        // PRODUCTS
        public List<Product> GetProducts()
        {
            var json = File.ReadAllText(_productsFile);
            return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
        }

        public void SaveProducts(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_productsFile, json);
        }

        public Product GetProductById(int id)
        {
            return GetProducts().FirstOrDefault(p => p.Id == id);
        }

        public void AddProduct(Product product)
        {
            var products = GetProducts();
            product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;
            product.CreatedAt = DateTime.Now;
            products.Add(product);
            SaveProducts(products);
        }

        public void UpdateProduct(Product product)
        {
            var products = GetProducts();
            var index = products.FindIndex(p => p.Id == product.Id);
            if (index != -1)
            {
                products[index] = product;
                SaveProducts(products);
            }
        }

        public void DeleteProduct(int id)
        {
            var products = GetProducts();
            products.RemoveAll(p => p.Id == id);
            SaveProducts(products);
        }

        // ORDERS
        public List<Order> GetOrders()
        {
            var json = File.ReadAllText(_ordersFile);
            return JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
        }

        public void SaveOrders(List<Order> orders)
        {
            var json = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_ordersFile, json);
        }

        public Order GetOrderById(int id)
        {
            return GetOrders().FirstOrDefault(o => o.Id == id);
        }

        public void AddOrder(Order order)
        {
            var orders = GetOrders();
            order.Id = orders.Any() ? orders.Max(o => o.Id) + 1 : 1;
            order.OrderDate = DateTime.Now;
            orders.Add(order);
            SaveOrders(orders);
        }

        public void UpdateOrder(Order order)
        {
            var orders = GetOrders();
            var index = orders.FindIndex(o => o.Id == order.Id);
            if (index != -1)
            {
                orders[index] = order;
                SaveOrders(orders);
            }
        }

        public void DeleteOrder(int id)
        {
            var orders = GetOrders();
            orders.RemoveAll(o => o.Id == id);
            SaveOrders(orders);
        }

        public void UpdateUser(User user)
        {
            var users = GetUsers();
            var index = users.FindIndex(u => u.Id == user.Id);
            if (index != -1)
            {
                users[index] = user;
                SaveUsers(users);
            }
        }

        public void DeleteUser(int id)
        {
            var users = GetUsers();
            users.RemoveAll(u => u.Id == id);
            SaveUsers(users);
        }
    }
}