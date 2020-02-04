using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppTest.Models;

namespace AppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandardController : ControllerBase
    {
        private readonly UserContext _context;

        public StandardController(UserContext context)
        {
            _context = context;
        }

        // GET: api/Standard
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupInfo>>> GetGroupInfo()
        {
            return await _context.GroupInfo.ToListAsync();
        }

        // GET: api/Standard/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupInfo>> GetGroupInfo(int? id)
        {
            var groupInfo = await _context.GroupInfo.FindAsync(id);

            if (groupInfo == null)
            {
                return NotFound();
            }

            return groupInfo;
        }

        // PUT: api/Standard/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroupInfo(int? id, GroupInfo groupInfo)
        {
            if (id != groupInfo.GroupId)
            {
                return BadRequest();
            }

            _context.Entry(groupInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Standard
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<GroupInfo>> PostGroupInfo(GroupInfo groupInfo)
        {
            _context.GroupInfo.Add(groupInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGroupInfo", new { id = groupInfo.GroupId }, groupInfo);
        }

        // DELETE: api/Standard/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<GroupInfo>> DeleteGroupInfo(int? id)
        {
            var groupInfo = await _context.GroupInfo.FindAsync(id);
            if (groupInfo == null)
            {
                return NotFound();
            }

            _context.GroupInfo.Remove(groupInfo);
            await _context.SaveChangesAsync();

            return groupInfo;
        }

        private bool GroupInfoExists(int? id)
        {
            return _context.GroupInfo.Any(e => e.GroupId == id);
        }
    }
}
