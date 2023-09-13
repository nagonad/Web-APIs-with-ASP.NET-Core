using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;
using System.Linq.Dynamic.Core;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;
        private readonly ApplicationDbContext _context;

        public BoardGamesController(ApplicationDbContext context, ILogger<BoardGamesController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetBoardGames")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<BoardGame[]>> Get(
            int pageIndex = 0,
            int pageSize = 10,
            string? sortColumn = "Name",
            string? sortOrder = "ASC",
            string? filterQuery = null
            )
        {
            var query = _context.BoardGames.AsQueryable();
            if (!string.IsNullOrEmpty(filterQuery))
                query = query.Where(b => b.Name.Contains(filterQuery));

            var recordCount = await query.CountAsync();

            query = query.OrderBy($"{sortColumn} {sortOrder}").Skip(pageIndex * pageSize).Take(pageSize);

            return new RestDTO<BoardGame[]>()
            {
                Data = await query.ToArrayAsync(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                Url.Action(null, "BoardGames", new {pageIndex,pageSize}, Request.Scheme)!,
                "self",
                "GET"),
                }
            };
        }
        [HttpPost(Name = "UpdateBoardGame")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO model)
        {
            var DbBoardGame = await _context.BoardGames.Where(b => b.Id == model.Id).FirstOrDefaultAsync();
            if (DbBoardGame != null)
            {
                if (!string.IsNullOrEmpty(model.Name)) DbBoardGame.Name = model.Name;
                if (model.Year.HasValue && model.Year.Value > 0) DbBoardGame.Year = model.Year.Value;

                DbBoardGame.LastModifiedDate = DateTime.Now;

                _context.BoardGames.Update(DbBoardGame);

                await _context.SaveChangesAsync();
            }
            return new RestDTO<BoardGame?>()
            {
                Data = DbBoardGame,
                Links = new List<LinkDTO>
                    {
                        new LinkDTO(
                            Url.Action(null,"BoardGames",model,Request.Scheme)!,
                            "self",
                            "POST")
                    }
            };
        }
        [HttpDelete(Name = "DeleteBoardGames")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<BoardGame?>> Delete(int Id)
        {
            var DbBoardGame = await _context.BoardGames.Where(b => b.Id == Id).FirstOrDefaultAsync();
            if (DbBoardGame != null)
            {
                _context.BoardGames.Remove(DbBoardGame);
                await _context.SaveChangesAsync();
            }
            return new RestDTO<BoardGame?>()
            {
                Data = DbBoardGame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null,"BoardGames",Id,Request.Scheme)!,
                        "self",
                        "DELETE")
                }
            };
        }
    }
}
