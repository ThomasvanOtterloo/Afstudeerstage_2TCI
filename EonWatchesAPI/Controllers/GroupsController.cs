using EonWatchesAPI.Dtos;
using EonWatchesAPI.Services.I_Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EonWatchesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : ControllerBase
    {

        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetUserGroups(string token)
        {
            try
            {
                var result = await _groupService.GetGroups(token); // await is essentieel!
                return Ok(result); // dit moet een gewone List<GroupDto> zijn


            }
            catch (Exception ex)
            {
                return BadRequest("GetUserGroups triggered een exception: "+ex.Message);
            }
        }

        [HttpPost("WhitelistGroups")]
        public async Task<IActionResult> WhitelistGroupId(string groupId)
        {


            return Ok();
        }

        [HttpPost("DeWhitelistGroups")]
        public async Task<IActionResult> DeWhitelistGroupId(string groupId)
        {


            return Ok();
        }


    }
}
