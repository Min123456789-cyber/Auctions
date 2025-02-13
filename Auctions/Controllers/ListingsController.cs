﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auctions.Data;
using Auctions.Models;
using Auctions.Data.Services;
using System.Security.Claims;

namespace Auctions.Controllers
{
    public class ListingsController : Controller
    {
        private readonly IListingService _listingService;
        private readonly IBidsService _bidsService;
        private readonly IWebHostEnvironment _environment;
        private readonly ICommentService _commentService;

        public ListingsController(IListingService listingService, 
            IWebHostEnvironment environment, 
            IBidsService bidsService, 
            ICommentService commentService)
        {
            _listingService = listingService;
            _bidsService = bidsService;
            _environment = environment;
            _commentService = commentService;
        }

        // GET: Listings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _listingService.GetAll();
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> MyListings()
        {
            var applicationDbContext = _listingService.GetAll();
            return View("Index", await applicationDbContext.Where(l=>l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> MyBids()
        {
            var applicationDbContext = _bidsService.GetAll();
            return View(await applicationDbContext.Where(l=>l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking().ToListAsync());
        }

        //GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var listing = await _listingService.GetById(id);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        //GET: Listings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Listings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ListingVM listing)
        {
            if(listing.Image != null)
            {
                string uploadDir = Path.Combine(_environment.WebRootPath, "Images");
                string fileName = listing.Image.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using(var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    listing.Image.CopyTo(fileStream);
                }

                var listObj = new Listing
                {
                    Title = listing.Title,
                    Description = listing.Description,
                    Price = listing.Price,
                    IdentityUserId = listing.IdentityUserId,
                    ImagePath = filePath,
                };
                await _listingService.Add(listObj);
                return RedirectToAction("Index");
            }
            return View(listing);
        }
        [HttpPost]
        public async Task<ActionResult> AddBid([Bind("Id, Price, ListingId, IdentityUserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                await _bidsService.Add(bid);
            }
            var listing = await _listingService.GetById(bid.ListingId);
            listing.Price = bid.Price;
            await _listingService.SaveChanges();

            return View("Details", listing);
        }

        public async Task<ActionResult> CloseBidding(int id)
        {
            var listing = await _listingService.GetById(id);
            listing.IsSold = true;
            await _listingService.SaveChanges();
            return View("Details", listing);
        }

        [HttpPost]
        public async Task<ActionResult> AddComment([Bind("Id, Content, ListingId, IdentityUserId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                await _commentService.Add(comment);
            }
            var listing = await _listingService.GetById(comment.ListingId);
            return View("Details", listing);
        }
        //// GET: Listings/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Listings == null)
        //    {
        //        return NotFound();
        //    }

        //    var listing = await _context.Listings.FindAsync(id);
        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
        //    return View(listing);
        //}

        //// POST: Listings/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImagePath,IsSold,IdentityUserId")] Listing listing)
        //{
        //    if (id != listing.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(listing);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ListingExists(listing.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
        //    return View(listing);
        //}

        //// GET: Listings/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Listings == null)
        //    {
        //        return NotFound();
        //    }

        //    var listing = await _context.Listings
        //        .Include(l => l.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(listing);
        //}

        //// POST: Listings/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Listings == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Listings'  is null.");
        //    }
        //    var listing = await _context.Listings.FindAsync(id);
        //    if (listing != null)
        //    {
        //        _context.Listings.Remove(listing);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ListingExists(int id)
        //{
        //  return (_context.Listings?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
