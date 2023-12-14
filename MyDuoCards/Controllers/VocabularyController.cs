﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyDuoCards.Models;
using MyDuoCards.Models.DBModels;
using MyDuoCards.Models.Extensions;
using MyDuoCards.Models.ViewModels;
using System.Diagnostics;



namespace MyDuoCards.Controllers
{
	[Authorize]
    public class VocabularyController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;

        public VocabularyController(ILogger<HomeController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }


		public async Task<IActionResult> Index(int id = 1)
        {
			var user = await _context.Users
				.Include(usr => usr.Attandances)
				.SingleOrDefaultAsync(u => u.Login == User.Identity.Name);

			var model = await _context.RuWords
				.Include(ruWord => ruWord.EnWord)
					.ThenInclude(enWord => enWord.Dictionaries)
						.ThenInclude(dict => dict.User)
				.Skip((id - 1) * 30)
				.Take(30)
				.ToListAsync();

			return View(model);
        }

		public async Task<IActionResult> AddTheDictionary(int id)
		{
			var ruWord = await _context.RuWords.FindAsync(id);

			var user = await _context.Users
				.SingleOrDefaultAsync(u => u.Login == User.Identity.Name);

			Random rand = new Random();
			_context.Dictionaries.Add(new Dictionary()
			{
				UserId = user.Id,
				EnWordId = ruWord.EnWordId,
				DictionaryStatementId = rand.Next(_context.DictionaryStatements.Count()) + 1
			});
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}


		public async Task<IActionResult> RemoveDictionary(int id)
		{
			var ruWord = _context.RuWords
				.Include(ruWord => ruWord.EnWord)
				.Where(w => w.Id == id)
				.SingleOrDefault();

			var user = await _context.Users
				.Include(u => u.Dictionaries)
					.ThenInclude(d => d.EuWord)
				.SingleOrDefaultAsync(u => u.Login == User.Identity.Name);

			var dictToRemove = user.Dictionaries.Where(dict => dict.EuWord.Id == ruWord.EnWord.Id).SingleOrDefault();

			_context.Dictionaries.Remove(dictToRemove);

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
