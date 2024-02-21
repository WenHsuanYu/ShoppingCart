using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCartUI.Constants;
using ShoppingCartUI.Data;
using ShoppingCartUI.Models;

namespace ShoppingCartUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderDetailsController> _logger;

        public OrderDetailsController(ApplicationDbContext context, ILogger<OrderDetailsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderDetails.Include(o => o.Laptop).Include(o => o.Order).ThenInclude(o => o!.OrderStatus);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var orderDetail = _context.OrderDetails
                .Include(o => o.Laptop)
                .Include(o => o.Order)
                .ThenInclude(o => o!.OrderStatus)
                .Where(o => o.OrderId == orderId);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(await orderDetail.ToListAsync());
        }

        //// GET: OrderDetails/Create
        //public IActionResult Create()
        //{
        //    ViewData["LaptopId"] = new SelectList(_context.Laptops, "Id", "Processor");
        //    ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "UserId");
        //    return View();
        //}

        //// POST: OrderDetails/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,OrderId,LaptopId,Quantity,UnitPrice")] OrderDetail orderDetail)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(orderDetail);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["LaptopId"] = new SelectList(_context.Laptops, "Id", "Processor", orderDetail.LaptopId);
        //    ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "UserId", orderDetail.OrderId);
        //    return View(orderDetail);
        //}

        public async Task<IActionResult> EditOrder(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Where(o => o.Id == orderId).FirstAsync();
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            ViewData["Status"] = new SelectList(_context.OrderStatuses, "StatusName", "StatusName", order.Status);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, [Bind("Id, UserId,CreateDate, Status,Email")] Order? order)
        {
            //var orderDetailList = orderDetail.ToList();

            if (order == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var orderStatus = await _context.OrderStatuses.Where(o => o.StatusName == order.Status).FirstAsync();
                    order.OrderStatusId = orderStatus.Id;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw new Exception("Please try again!");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Status"] = new SelectList(_context.OrderStatuses, "StatusName", "StatusName", order.Status);
            return View(order);
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var orderDetail = _context.OrderDetails
                .Include(o => o.Laptop)
                .Include(o => o.Order)
                .ThenInclude(o => o!.OrderStatus)
                .Where(o => o.OrderId == orderId);
            if (orderDetail == null)
            {
                return NotFound();
            }
            List<SelectList> orderItems = [];
            List<SelectList> laptopItems = [];
            //List<SelectList> statusItems = new List<SelectList>();
            foreach (var item in orderDetail)
            {
                laptopItems.Add(new SelectList(_context.Laptops, "Id", "ModelName", item.LaptopId));
                orderItems.Add(new SelectList(_context.Orders, "Id", "Id", item.OrderId));
                //statusItems.Add(new SelectList(_context.OrderStatuses, "StatusName", "StatusName", item.StatusName));
            }

            ViewData["Laptop"] = laptopItems;
            ViewData["Order"] = orderItems;
            //ViewData["Status"] = statusItems;
            return View(await orderDetail.ToListAsync());
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,OrderId,LaptopId,Quantity,UnitPrice, StatusName")] IEnumerable<OrderDetail> orderDetail)
        {
            var orderDetailList = orderDetail.ToList();
            var order = _context.Find<Order>(orderDetailList.First().OrderId);
            if (ModelState.IsValid)
            {
                try
                {
                    //using (var transaction = _context.Database.BeginTransaction())
                    //{
                    foreach (var item in orderDetailList)
                    {
                        _context.Update(item);
                    }
                    await _context.SaveChangesAsync();
                    //}
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetailList.First().Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw new Exception("Please try again!");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            List<SelectList> orderItems = [];
            List<SelectList> laptopItems = [];
            //List<SelectList> statusItems = new List<SelectList>();
            foreach (var item in orderDetail)
            {
                laptopItems.Add(new SelectList(_context.Laptops, "Id", "ModelName", item.LaptopId));
                orderItems.Add(new SelectList(_context.Orders, "Id", "Id", item.OrderId));
                //statusItems.Add(new SelectList(_context.OrderStatuses, "Id", "StatusName", item.StatusName));
            }

            ViewData["Laptop"] = laptopItems;
            ViewData["Order"] = orderItems;
            //ViewData["Status"] = statusItems;
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Laptop)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderId == orderId);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int orderId)
        {
            var orderDetail = _context.OrderDetails.Where(m => m.OrderId == orderId);
            if (orderDetail != null)
            {
                foreach (var item in orderDetail)
                    _context.OrderDetails.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.Id == id);
        }
    }
}